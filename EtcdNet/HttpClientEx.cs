using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Cache;

namespace EtcdNet
{
    internal class HttpClientEx : HttpClient
    {
        internal HttpClientEx(WebRequestHandler h)
            : base(h)
        {
        }

        /// <summary>
        /// A loop
        /// </summary>
        internal HttpClientEx Next { get; set; }
    }
}
