using System;
using System.Reflection;
#if NET45
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
#endif
using System.Net.Http;
#if NET45
using System.Net.Http.Headers;
using System.Net.Cache;
#endif

namespace EtcdNet
{
    internal class HttpClientEx : HttpClient
    {

        internal HttpClientEx(EtcdClientOpitions options)
            : base(CreateHandler(options))
        {
            
        }

#if NETSTANDARD2_0 
        public static bool IgnoreRemoteCertificateError(object sender
            , System.Security.Cryptography.X509Certificates.X509Certificate certificate
            , System.Security.Cryptography.X509Certificates.X509Chain chain
            , System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
#endif

        /// <summary>
        /// Because System.Net.Http.SocketsHttpHandler/WebRequestHandler are not part of .NET Standard 2.0
        /// Here we use reflection to create handler
        /// </summary>
        /// <returns></returns>
        private static HttpMessageHandler CreateHandler(EtcdClientOpitions options)
        {
#if NET45
            WebRequestHandler handler = new WebRequestHandler()
            {
                UseProxy = options.UseProxy,
                AllowAutoRedirect = false,
                AllowPipelining = true,
            };
            if (options.X509Certificate != null)
                handler.ClientCertificates.Add(options.X509Certificate);
            if (options.IgnoreCertificateError)
                handler.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => { return true; };
            return handler;
#endif
#if NETSTANDARD2_0
            HttpMessageHandler handler = null;
            Type type = typeof(HttpClient).Assembly.GetType("System.Net.Http.SocketsHttpHandler");
            if (type != null)
                handler = Activator.CreateInstance(type) as HttpMessageHandler;

            if (handler != null)
            {
                PropertyInfo pi = type.GetProperty("UseProxy");
                if( pi != null && pi.SetMethod != null )
                {
                    pi.SetMethod.Invoke(handler, new object[] { options.UseProxy });
                }

                pi = type.GetProperty("SslOptions");
                if (pi != null && pi.SetMethod != null)
                {
                    ParameterInfo[] parameters = pi.SetMethod.GetParameters();
                    if(parameters != null && parameters.Length == 1)
                    {
                        object sslOptions = Activator.CreateInstance(parameters[0].ParameterType);
                        pi.SetMethod.Invoke(handler, new object[] { sslOptions });
                        if (sslOptions != null)
                        {
                            if (options.IgnoreCertificateError)
                            {
                                var cb = parameters[0].ParameterType.GetProperty("RemoteCertificateValidationCallback");
                                if (cb != null && cb.SetMethod != null)
                                {
                                    cb.SetMethod.Invoke(sslOptions, new object[] { new System.Net.Security.RemoteCertificateValidationCallback(IgnoreRemoteCertificateError) });
                                }
                            }

                            if (options.X509Certificate != null)
                            {
                                var coll = new System.Security.Cryptography.X509Certificates.X509CertificateCollection();
                                coll.Add(options.X509Certificate);
                                var cert = parameters[0].ParameterType.GetProperty("ClientCertificates");
                                if (cert != null && cert.SetMethod != null)
                                {
                                    cert.SetMethod.Invoke(sslOptions, new object[] { options.X509Certificate });
                                }
                            }                           
                        }
                    }
                }

            }


            return handler;
#endif
        }


        /// <summary>
        /// A loop
        /// </summary>
        internal HttpClientEx Next { get; set; }
    }
}
