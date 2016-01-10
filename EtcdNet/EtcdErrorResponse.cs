using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace EtcdNet
{
    [DataContract]
    public class ErrorResponse
    {
        [DataMember(Name = "errorCode")]
        public int ErrorCode { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "cause")]
        public string Cause { get; set; }

        [DataMember(Name = "index")]
        public int Index { get; set; }
    }
}
