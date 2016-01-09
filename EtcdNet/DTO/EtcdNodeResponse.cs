using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace EtcdNet.DTO
{
    [DataContract]
    public class EtcdNodeResponse : EtcdResponseBase
    {
        [DataMember(Name = "node")]
        public EtcdNode Node { get; set; }

        [DataMember(Name = "prevNode")]
        public EtcdNode PrevNode { get; set; }
    }
}
