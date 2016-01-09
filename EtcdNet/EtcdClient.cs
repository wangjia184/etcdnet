using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Cache;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace EtcdNet
{
    using DTO;

    /// <summary>
    /// 
    /// </summary>
    public class EtcdClient
    {
        HttpClientEx _currentClient;
        readonly IJsonDeserializer _jsonDeserializer;

        #region constructor EtcdClient(EtcdClientOpitions options)
        public EtcdClient(EtcdClientOpitions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");
            if (options.Urls == null || options.Urls.Length == 0)
                throw new ArgumentException("`EtcdClientOpitions.Urls` does not contain valid url");

            WebRequestHandler handler = new WebRequestHandler()
            {
                UseProxy = options.UseProxy,
                AllowAutoRedirect = false,
                AllowPipelining = true,
                CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore),
            };
            if (options.X509Certificate != null)
                handler.ClientCertificates.Add(options.X509Certificate);

            AuthenticationHeaderValue authenticationHeaderValue = null;
            if( !string.IsNullOrWhiteSpace(options.Username) &&
                !string.IsNullOrWhiteSpace(options.Password) )
            {
                string auth = string.Format("{0}:{1}", options.Username, options.Password);
                authenticationHeaderValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(auth)));
            }

            _jsonDeserializer = options.JsonDeserializer == null ? new DefaultJsonDeserializer() : options.JsonDeserializer;

            if (options.IgnoreCertificateError)
                handler.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => { return true; };

            HttpClientEx [] httpClients = options.Urls.Select(u =>
                {
                    if (string.IsNullOrWhiteSpace(u))
                        throw new ArgumentNullException("`urls` array contains empty url");

                    HttpClientEx httpClient = new HttpClientEx(handler);
                    httpClient.BaseAddress = new Uri(u);
                    httpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
                    return httpClient;
                }).ToArray();

            // make the clients as a ring, so that we can try the next one when one fails
            if( httpClients.Length > 1 )
            {
                for( int i = httpClients.Length - 2; i >= 0; i--)
                {
                    httpClients[i].Next = httpClients[i + 1];
                }
            }
            httpClients[httpClients.Length - 1].Next = httpClients[0];

            // pick a client randomly
            _currentClient = httpClients[DateTime.UtcNow.Ticks % httpClients.Length];
        }
        #endregion

        #region Task<T> SendRequest<T>(HttpRequestMessage requestMessage, IEnumerable<KeyValuePair<string, string>> formFields)
        private async Task<T> SendRequest<T>(HttpMethod method, string requestUri, IEnumerable<KeyValuePair<string, string>> formFields = null) where T : EtcdResponseBase
        {
            HttpClientEx startClient = _currentClient;
            HttpClientEx currentClient = _currentClient;

            for (; ;)
            {
                try
                {
                    using (HttpRequestMessage requestMessage = new HttpRequestMessage(method, requestUri))
                    {
                        if (formFields != null)
                            requestMessage.Content = new FormUrlEncodedContent(formFields);

                        using (HttpResponseMessage responseMessage = await currentClient.SendAsync(requestMessage))
                        {
                            string json = null;

                            if (responseMessage.Content != null)
                                json = await responseMessage.Content.ReadAsStringAsync();

                            if (!responseMessage.IsSuccessStatusCode)
                            {
                                if (!string.IsNullOrWhiteSpace(json))
                                {
                                    ErrorResponse errorResponse = null;
                                    try
                                    {
                                        errorResponse = _jsonDeserializer.Deserialize<ErrorResponse>(json);
                                    }
                                    catch { }

                                    if (errorResponse != null)
                                        throw EtcdGenericException.Create(requestMessage, errorResponse);
                                }


                                currentClient = currentClient.Next;
                                if (currentClient != startClient)
                                {
                                    // try the next
                                    continue;
                                }
                                else
                                {
                                    responseMessage.EnsureSuccessStatusCode();
                                }
                            }

                            // if currentClient != _currentClient, update _currentClient
                            if (currentClient != startClient)
                                Interlocked.CompareExchange(ref _currentClient, currentClient, startClient);

                            T t = _jsonDeserializer.Deserialize<T>(json);
                            t.EtcdServer = currentClient.BaseAddress.OriginalString;
                            t.EtcdIndex = GetLongValue(responseMessage, "X-Etcd-Index");
                            t.RaftIndex = GetLongValue(responseMessage, "X-Raft-Index");
                            t.RaftTerm = GetLongValue(responseMessage, "X-Raft-Term");
                            return t;
                        }
                    }
                }
                catch(EtcdGenericException)
                {
                    throw;
                }
                catch(Exception)
                {
                    currentClient = currentClient.Next;
                    if (currentClient != startClient)
                        continue; // try the next
                    else
                        throw; // tried all clients, all failed
                }
            }
        }

        long GetLongValue(HttpResponseMessage responseMessage, string name)
        {
            if( responseMessage.Content.Headers != null )
            {
                IEnumerable<string> headerValues;
                long longValue;
                if( responseMessage.Content.Headers.TryGetValues("X-Etcd-Index", out headerValues) && headerValues != null )
                {
                    foreach( string headerValue in headerValues )
                    {
                        if (!string.IsNullOrWhiteSpace(headerValue) && long.TryParse(headerValue, out longValue))
                            return longValue;
                    }
                }
            }
            return 0;
        }
        #endregion

        /// <summary>
        /// Get etcd node specified by `key`
        /// </summary>
        /// <param name="key">The path of the node, must starts with `/`</param>
        /// <param name="recursive">Represents whether list the children nodes</param>
        /// <param name="sorted">To enumerate the in-order keys as a sorted list, use the "sorted" parameter.</param>
        /// <param name="ignoreKeyNotFoundException">If `true`, `EtcdCommonException.KeyNotFound` exception is ignored and `null` is returned instead.</param>
        /// <returns>represents response; or `null` if not exist</returns>
        public async Task<EtcdNodeResponse> GetNodeAsync(string key
            , bool ignoreKeyNotFoundException = false
            , bool recursive = false
            , bool sorted = false
            )
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            if (!key.StartsWith("/"))
                throw new ArgumentException("The value of `key` must start with `/`.");

            try
            {
                string url = string.Format( CultureInfo.InvariantCulture
                    , "/v2/keys{0}?recursive={1}&sorted={2}"
                    , key
                    , recursive.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()
                    , sorted.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()
                    );
                EtcdNodeResponse getNodeResponse = await SendRequest<EtcdNodeResponse>( HttpMethod.Get, url);
                return getNodeResponse;
            }
            catch(EtcdCommonException.KeyNotFound)
            {
                if (ignoreKeyNotFoundException)
                    return null;
                throw;
            }
        }

        /// <summary>
        /// Simplified version of `GetNodeAsync`.
        /// Get the value of the specific node
        /// </summary>
        /// <param name="key">The path of the node, must starts with `/`</param>
        /// <param name="ignoreKeyNotFoundException">If `true`, `EtcdCommonException.KeyNotFound` exception is ignored and `null` is returned instead.</param>
        /// <returns>A string represents a value. It could be `null`</returns>
        public async Task<string> GetNodeValueAsync(string key, bool ignoreKeyNotFoundException = false)
        {
            EtcdNodeResponse getNodeResponse = await this.GetNodeAsync(key, ignoreKeyNotFoundException);
            if (getNodeResponse != null && getNodeResponse.Node != null)
                return getNodeResponse.Node.Value;
            return null;
        }

        /// <summary>
        /// Get etcd node specified by `key`
        /// </summary>
        /// <param name="key">path of the node</param>
        /// <param name="value">value to be set</param>
        /// <param name="ttl">time to live, in seconds</param>
        /// <param name="dir">indicates if this is a directory</param>
        /// <returns>SetNodeResponse</returns>
        public Task<EtcdNodeResponse> SetNodeAsync(string key, string value, int? ttl = null, bool? dir = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            if (value == null)
                throw new ArgumentNullException("value");
            if (!key.StartsWith("/"))
                throw new ArgumentException("The value of `key` must start with `/`.");

            string url = string.Format(CultureInfo.InvariantCulture, "/v2/keys{0}", key);
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string,string>>()
            {
                new KeyValuePair<string,string>( "value", value)
            };
            if (ttl.HasValue)
                list.Add(new KeyValuePair<string, string>("ttl", ttl.Value.ToString(CultureInfo.InvariantCulture)));
            if( dir.HasValue)
                list.Add(new KeyValuePair<string, string>("dir", dir.Value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));

            return SendRequest<EtcdNodeResponse>(HttpMethod.Put, url, list);
        }

        /// <summary>
        /// delete specific node
        /// </summary>
        /// <param name="key">The path of the node, must starts with `/`</param>
        /// <param name="ignoreKeyNotFoundException">If `true`, `EtcdCommonException.KeyNotFound` exception is ignored and `null` is returned instead.</param>
        /// <returns>SetNodeResponse instance or `null`</returns>
        public async Task<EtcdNodeResponse> DeleteNodeAsync(string key, bool ignoreKeyNotFoundException = false, bool? dir = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            if (!key.StartsWith("/"))
                throw new ArgumentException("The value of `key` must start with `/`.");

            string url = string.Format(CultureInfo.InvariantCulture, "/v2/keys{0}", key);
            if (dir == true)
                url += "?dir=true";

            try
            {
                return await SendRequest<EtcdNodeResponse>(HttpMethod.Delete, url);
            }
            catch(EtcdCommonException.KeyNotFound)
            {
                if (ignoreKeyNotFoundException)
                    return null;
                throw;
            }
        }

        public Task<EtcdNodeResponse> CreateInOrderNodeAsync(string key, string value, int? ttl = null, bool? dir = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            if (value == null)
                throw new ArgumentNullException("value");
            if (!key.StartsWith("/"))
                throw new ArgumentException("The value of `key` must start with `/`.");

            string url = string.Format(CultureInfo.InvariantCulture, "/v2/keys{0}", key);
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string,string>( "value", value)
            };
            if (ttl.HasValue)
                list.Add(new KeyValuePair<string, string>("ttl", ttl.Value.ToString(CultureInfo.InvariantCulture)));
            if (dir.HasValue)
                list.Add(new KeyValuePair<string, string>("dir", dir.Value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));

            return SendRequest<EtcdNodeResponse>(HttpMethod.Post, url, list);
        }

        public Task<EtcdNodeResponse> CreateNodeAsync(string key, string value, int? ttl = null, bool? dir = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            if (value == null)
                throw new ArgumentNullException("value");
            if (!key.StartsWith("/"))
                throw new ArgumentException("The value of `key` must start with `/`.");

            string url = string.Format(CultureInfo.InvariantCulture, "/v2/keys{0}", key);
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string,string>( "value", value),
                new KeyValuePair<string,string>( "prevExist", "false")
            };
            if (ttl.HasValue)
                list.Add(new KeyValuePair<string, string>("ttl", ttl.Value.ToString(CultureInfo.InvariantCulture)));
            if (dir.HasValue)
                list.Add(new KeyValuePair<string, string>("dir", dir.Value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));

            return SendRequest<EtcdNodeResponse>(HttpMethod.Put, url, list);
        }

        public Task<EtcdNodeResponse> CompareAndSwapNodeAsync(string key, string prevValue, string value, int? ttl = null, bool? dir = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            if (value == null)
                throw new ArgumentNullException("value");
            if (!key.StartsWith("/"))
                throw new ArgumentException("The value of `key` must start with `/`.");

            string url = string.Format(CultureInfo.InvariantCulture, "/v2/keys{0}", key);
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string,string>( "value", value),
                new KeyValuePair<string,string>( "prevValue", prevValue)
            };
            if (ttl.HasValue)
                list.Add(new KeyValuePair<string, string>("ttl", ttl.Value.ToString(CultureInfo.InvariantCulture)));
            if (dir.HasValue)
                list.Add(new KeyValuePair<string, string>("dir", dir.Value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));

            return SendRequest<EtcdNodeResponse>(HttpMethod.Put, url, list);
        }

        public Task<EtcdNodeResponse> CompareAndSwapNodeAsync(string key, long prevIndex, string value, int? ttl = null, bool? dir = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            if (value == null)
                throw new ArgumentNullException("value");
            if (!key.StartsWith("/"))
                throw new ArgumentException("The value of `key` must start with `/`.");

            string url = string.Format(CultureInfo.InvariantCulture, "/v2/keys{0}", key);
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string,string>( "value", value),
                new KeyValuePair<string,string>( "prevIndex", prevIndex.ToString(CultureInfo.InvariantCulture))
            };
            if (ttl.HasValue)
                list.Add(new KeyValuePair<string, string>("ttl", ttl.Value.ToString(CultureInfo.InvariantCulture)));
            if (dir.HasValue)
                list.Add(new KeyValuePair<string, string>("dir", dir.Value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));

            return SendRequest<EtcdNodeResponse>(HttpMethod.Put, url, list);
        }


        public Task<EtcdNodeResponse> CompareAndDeleteNodeAsync(string key, string prevValue)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            if (!key.StartsWith("/"))
                throw new ArgumentException("The value of `key` must start with `/`.");

            string url = string.Format(CultureInfo.InvariantCulture, "/v2/keys{0}?prevValue={1}", key, Uri.EscapeDataString(prevValue));
            return SendRequest<EtcdNodeResponse>(HttpMethod.Delete, url);
        }

        public Task<EtcdNodeResponse> CompareAndDeleteNodeAsync(string key, long prevIndex)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            if (!key.StartsWith("/"))
                throw new ArgumentException("The value of `key` must start with `/`.");

            string url = string.Format(CultureInfo.InvariantCulture, "/v2/keys{0}?prevValue={1}", key, prevIndex);
            return SendRequest<EtcdNodeResponse>(HttpMethod.Delete, url);
        }
    }
}
