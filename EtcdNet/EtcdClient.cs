using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
#if NET45
using System.Net.Cache;
#endif
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
#if NET45
using System.Security.Cryptography.X509Certificates;
#endif

namespace EtcdNet
{
    /// <summary>
    /// The EtcdClient class is used to talk with etcd service
    /// </summary>
    public class EtcdClient
    {

        HttpClientEx _currentClient;
        readonly IJsonDeserializer _jsonDeserializer;
        long _lastIndex;

        /// <summary>
        /// X-Etcd-Cluster-Id
        /// </summary>
        public string ClusterID { get; private set; }

        /// <summary>
        /// Lastest X-Etcd-Index received by this instance
        /// </summary>
        public long LastIndex { get; private set; }

        #region constructor EtcdClient(EtcdClientOpitions options)

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">options to initialize</param>
        public EtcdClient(EtcdClientOpitions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");
            if (options.Urls == null || options.Urls.Length == 0)
                throw new ArgumentException("`EtcdClientOpitions.Urls` does not contain valid url");
#if NET45
            WebRequestHandler handler = new WebRequestHandler()
            {
                UseProxy = options.UseProxy,
                AllowAutoRedirect = false,
                AllowPipelining = true,
                CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore),
            };
            if (options.X509Certificate != null)
                handler.ClientCertificates.Add(options.X509Certificate);
#else
            /*
             * To be implemented when .NET Standard 2.1 is available
            SocketsHttpHandler handler = new SocketsHttpHandler()
            {
                UseProxy = options.UseProxy,
            };
            */
#endif
            AuthenticationHeaderValue authenticationHeaderValue = null;
            if( !string.IsNullOrWhiteSpace(options.Username) &&
                !string.IsNullOrWhiteSpace(options.Password) )
            {
                string auth = string.Format("{0}:{1}", options.Username, options.Password);
                authenticationHeaderValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(auth)));
            }

            _jsonDeserializer = options.JsonDeserializer == null ? new DefaultJsonDeserializer() : options.JsonDeserializer;
#if NET45
            if (options.IgnoreCertificateError)
                handler.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => { return true; };
#endif
            HttpClientEx [] httpClients = options.Urls.Select(u =>
                {
                    if (string.IsNullOrWhiteSpace(u))
                        throw new ArgumentNullException("`urls` array contains empty url");
#if NET45
                    HttpClientEx httpClient = new HttpClientEx(handler);
#else
                    HttpClientEx httpClient = new HttpClientEx();
#endif
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
        private async Task<EtcdResponse> SendRequest(HttpMethod method, string requestUri, IEnumerable<KeyValuePair<string, string>> formFields = null)
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

                            if( !string.IsNullOrWhiteSpace(json) )
                            {
                                EtcdResponse resp = _jsonDeserializer.Deserialize<EtcdResponse>(json);
                                resp.EtcdServer = currentClient.BaseAddress.OriginalString;
                                resp.EtcdClusterID = GetStringHeader(responseMessage, "X-Etcd-Cluster-Id");
                                resp.EtcdIndex = GetLongHeader(responseMessage, "X-Etcd-Index");
                                resp.RaftIndex = GetLongHeader(responseMessage, "X-Raft-Index");
                                resp.RaftTerm = GetLongHeader(responseMessage, "X-Raft-Term");

                                long previousIndex = _lastIndex;
                                if (resp.EtcdIndex > previousIndex)
                                    Interlocked.CompareExchange(ref _lastIndex, resp.EtcdIndex, previousIndex);
                                this.ClusterID = resp.EtcdClusterID;
                                return resp;
                            }
                            return null;
                        }
                    }
                }
                catch(EtcdRaftException)
                {
                    currentClient = currentClient.Next;
                    if (currentClient != startClient)
                        continue; // try the next
                    else
                        throw; // tried all clients, all failed
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


        long GetLongHeader(HttpResponseMessage responseMessage, string name)
        {

            if (responseMessage.Headers != null)
            {
                IEnumerable<string> headerValues;
                long longValue;
                if (responseMessage.Headers.TryGetValues(name, out headerValues) && headerValues != null)
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

        string GetStringHeader(HttpResponseMessage responseMessage, string name)
        {

            if (responseMessage.Headers != null)
            {
                IEnumerable<string> headerValues;
                if (responseMessage.Headers.TryGetValues(name, out headerValues) && headerValues != null)
                {
                    foreach (string headerValue in headerValues)
                    {
                        if (!string.IsNullOrWhiteSpace(headerValue))
                            return headerValue;
                    }
                }
            }
            return null;
        }
        #endregion

        /// <summary>
        /// Get etcd node specified by `key`
        /// </summary>
        /// <param name="key">The path of the node, must start with `/`</param>
        /// <param name="recursive">Represents whether list the children nodes</param>
        /// <param name="sorted">To enumerate the in-order keys as a sorted list, use the "sorted" parameter.</param>
        /// <param name="ignoreKeyNotFoundException">If `true`, `EtcdCommonException.KeyNotFound` exception is ignored and `null` is returned instead.</param>
        /// <returns>represents response; or `null` if not exist</returns>
        public async Task<EtcdResponse> GetNodeAsync(string key
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
#if NET45
                    , recursive.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()
                    , sorted.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()
#else
                    , recursive.ToString().ToLowerInvariant()
                    , sorted.ToString().ToLowerInvariant()
#endif
                    );
                EtcdResponse getNodeResponse = await SendRequest( HttpMethod.Get, url);
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
        /// <param name="key">The path of the node, must start with `/`</param>
        /// <param name="ignoreKeyNotFoundException">If `true`, `EtcdCommonException.KeyNotFound` exception is ignored and `null` is returned instead.</param>
        /// <returns>A string represents a value. It could be `null`</returns>
        public async Task<string> GetNodeValueAsync(string key, bool ignoreKeyNotFoundException = false)
        {
            EtcdResponse getNodeResponse = await this.GetNodeAsync(key, ignoreKeyNotFoundException);
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
        public Task<EtcdResponse> SetNodeAsync(string key, string value, int? ttl = null, bool? dir = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            if (!key.StartsWith("/"))
                throw new ArgumentException("The value of `key` must start with `/`.");

            string url = string.Format(CultureInfo.InvariantCulture, "/v2/keys{0}", key);
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            if( value != null)
                list.Add(new KeyValuePair<string, string>("value", value));
            if (ttl.HasValue)
                list.Add(new KeyValuePair<string, string>("ttl", ttl.Value.ToString(CultureInfo.InvariantCulture)));
            if( dir.HasValue)
#if NET45
                list.Add(new KeyValuePair<string, string>("dir", dir.Value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
#else
                list.Add(new KeyValuePair<string, string>("dir", dir.Value.ToString().ToLowerInvariant()));
#endif

            return SendRequest(HttpMethod.Put, url, list);
        }

        /// <summary>
        /// delete specific node
        /// </summary>
        /// <param name="key">The path of the node, must start with `/`</param>
        /// <param name="dir">true to delete an empty directory</param>
        /// <param name="ignoreKeyNotFoundException">If `true`, `EtcdCommonException.KeyNotFound` exception is ignored and `null` is returned instead.</param>
        /// <returns>SetNodeResponse instance or `null`</returns>
        public async Task<EtcdResponse> DeleteNodeAsync(string key, bool ignoreKeyNotFoundException = false, bool? dir = null)
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
                return await SendRequest(HttpMethod.Delete, url);
            }
            catch(EtcdCommonException.KeyNotFound)
            {
                if (ignoreKeyNotFoundException)
                    return null;
                throw;
            }
        }

        /// <summary>
        /// Create in-order node
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ttl"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public Task<EtcdResponse> CreateInOrderNodeAsync(string key, string value, int? ttl = null, bool? dir = null)
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
#if NET45
                list.Add(new KeyValuePair<string, string>("dir", dir.Value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
#else
                list.Add(new KeyValuePair<string, string>("dir", dir.Value.ToString().ToLowerInvariant()));
#endif

            return SendRequest(HttpMethod.Post, url, list);
        }

        /// <summary>
        /// Create a new node. If node exists, EtcdCommonException.NodeExist occurs
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ttl"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public Task<EtcdResponse> CreateNodeAsync(string key, string value, int? ttl = null, bool? dir = null)
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
#if NET45
                list.Add(new KeyValuePair<string, string>("dir", dir.Value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
#else
                list.Add(new KeyValuePair<string, string>("dir", dir.Value.ToString().ToLowerInvariant()));
#endif

            return SendRequest(HttpMethod.Put, url, list);
        }

        /// <summary>
        /// CAS(Compare and Swap) a node
        /// </summary>
        /// <param name="key"></param>
        /// <param name="prevValue"></param>
        /// <param name="value"></param>
        /// <param name="ttl"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public Task<EtcdResponse> CompareAndSwapNodeAsync(string key, string prevValue, string value, int? ttl = null, bool? dir = null)
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
#if NET45
                list.Add(new KeyValuePair<string, string>("dir", dir.Value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
#else
                list.Add(new KeyValuePair<string, string>("dir", dir.Value.ToString().ToLowerInvariant()));
#endif

            return SendRequest(HttpMethod.Put, url, list);
        }

        /// <summary>
        /// CAS(Compare and Swap) a node
        /// </summary>
        /// <param name="key">path of the node</param>
        /// <param name="prevIndex">previous index</param>
        /// <param name="value">value</param>
        /// <param name="ttl">time to live (in seconds)</param>
        /// <param name="dir">is directory</param>
        /// <returns></returns>
        public Task<EtcdResponse> CompareAndSwapNodeAsync(string key, long prevIndex, string value, int? ttl = null, bool? dir = null)
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
#if NET45
                list.Add(new KeyValuePair<string, string>("dir", dir.Value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
#else
                list.Add(new KeyValuePair<string, string>("dir", dir.Value.ToString().ToLowerInvariant()));
#endif

            return SendRequest(HttpMethod.Put, url, list);
        }

        /// <summary>
        /// Compare and delete specific node
        /// </summary>
        /// <param name="key">Path of the node</param>
        /// <param name="prevValue">previous value</param>
        /// <returns>EtcdResponse</returns>
        public Task<EtcdResponse> CompareAndDeleteNodeAsync(string key, string prevValue)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            if (!key.StartsWith("/"))
                throw new ArgumentException("The value of `key` must start with `/`.");

            string url = string.Format(CultureInfo.InvariantCulture, "/v2/keys{0}?prevValue={1}", key, Uri.EscapeDataString(prevValue));
            return SendRequest(HttpMethod.Delete, url);
        }

        /// <summary>
        /// Compare and delete specific node
        /// </summary>
        /// <param name="key">path of the node</param>
        /// <param name="prevIndex">previous index</param>
        /// <returns>EtcdResponse</returns>
        public Task<EtcdResponse> CompareAndDeleteNodeAsync(string key, long prevIndex)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            if (!key.StartsWith("/"))
                throw new ArgumentException("The value of `key` must start with `/`.");

            string url = string.Format(CultureInfo.InvariantCulture, "/v2/keys{0}?prevValue={1}", key, prevIndex);
            return SendRequest(HttpMethod.Delete, url);
        }


        /// <summary>
        /// Watch changes
        /// </summary>
        /// <param name="key">Path of the node</param>
        /// <param name="recursive">true to monitor descendants</param>
        /// <param name="waitIndex">Etcd Index is continue monitor from</param>
        /// <returns>EtcdResponse</returns>
        public async Task<EtcdResponse> WatchNodeAsync(string key, bool recursive = false, long? waitIndex = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            if (!key.StartsWith("/"))
                throw new ArgumentException("The value of `key` must start with `/`.");

            string requestUri = string.Format(CultureInfo.InvariantCulture
                , "/v2/keys{0}?wait=true&recursive={1}"
                , key
#if NET45
                , recursive.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()
#else
                , recursive.ToString().ToLowerInvariant()
#endif
                );
            if (waitIndex.HasValue)
            {
                requestUri = string.Format(CultureInfo.InvariantCulture
                    , "{0}&waitIndex={1}"
                    , requestUri
                    , waitIndex.Value
                    );
            }

            for (; ; )
            {
                try
                {
                    EtcdResponse resp = await SendRequest(HttpMethod.Get, requestUri);
                    if (resp != null)
                        return resp;
                }
                catch (TaskCanceledException)
                {
                    // no changes detected and the connection idles for too long, try again
                }
                catch (HttpRequestException hrex)
                {
                    // server closed connection
                    WebException webException = hrex.InnerException as WebException;
                    if (webException == null || webException.Status != WebExceptionStatus.ConnectionClosed)
                        throw;
                }
            }

        }
    }
}
