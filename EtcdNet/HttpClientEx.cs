#if NET45
using System;
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
#if NET45
        internal HttpClientEx(WebRequestHandler h)
            : base(h)
        {
        }
#endif

        internal HttpClientEx()
            : base()
        {
        }

        /// <summary>
        /// A loop
        /// </summary>
        internal HttpClientEx Next { get; set; }
    }
}
