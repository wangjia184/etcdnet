#if NET45
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endif
using System.Security.Cryptography.X509Certificates;

namespace EtcdNet
{
    /// <summary>
    /// Options to initialize EtcdClient
    /// </summary>
    public sealed class EtcdClientOpitions
    {
        /// <summary>
        /// The urls of etcd servers (mandatory)
        /// </summary>
        public string[] Urls { get; set; }

        /// <summary>
        /// ignore invalid SSL certificate
        /// </summary>
        public bool IgnoreCertificateError { get; set; }

        /// <summary>
        /// Client certificate
        /// </summary>
        public X509Certificate X509Certificate { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Use proxy?
        /// </summary>
        public bool UseProxy { get; set; }

        /// <summary>
        /// If this field is null, default deserializer is used
        /// This parameter allows to use a different deserializer like ServiceStack.Text or Newtonsoft.Json
        /// </summary>
        public IJsonDeserializer JsonDeserializer { get; set; }
    }
}
