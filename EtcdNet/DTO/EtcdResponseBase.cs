using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace EtcdNet.DTO
{
    [DataContract]
    public class EtcdResponseBase
    {
        [DataMember(Name = "action")]
        public string Action { get; internal set; }

        /// <summary>
        /// The url of Etcd server which produce the response
        /// </summary>
        public string EtcdServer { get; internal set; }

        /// <summary>
        /// X-Etcd-Index is the current etcd index as explained above. When request is a watch on key space, X-Etcd-Index is the current etcd index when the watch starts, which means that the watched event may happen after X-Etcd-Index.
        /// </summary>
        public long EtcdIndex { get; internal set; }

        /// <summary>
        /// X-Raft-Index is similar to the etcd index but is for the underlying raft protocol
        /// </summary>
        public long RaftIndex { get; internal set; }

        /// <summary>
        /// X-Raft-Term is an integer that will increase whenever an etcd master election happens in the cluster. If this number is increasing rapidly, you may need to tune the election timeout. See the tuning section for details.
        /// </summary>
        public long RaftTerm { get; internal set; }
    }
}
