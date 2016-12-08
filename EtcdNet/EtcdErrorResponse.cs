#if NET45
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endif
using System.Runtime.Serialization;

namespace EtcdNet
{
    /// <summary>
    /// Represent etcd error JSON
    /// </summary>
    [DataContract]
    public class ErrorResponse
    {
        /// <summary>
        /// Error Code
        /// https://github.com/coreos/etcd/blob/master/Documentation/errorcode.md
        /// </summary>
        [DataMember(Name = "errorCode")]
        public int ErrorCode { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        [DataMember(Name = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Cause
        /// </summary>
        [DataMember(Name = "cause")]
        public string Cause { get; set; }

        /// <summary>
        /// Index
        /// </summary>
        [DataMember(Name = "index")]
        public int Index { get; set; }
    }
}
