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
    /// Normal response of etcd
    /// </summary>
    [DataContract]
    public class EtcdResponse
    {
        /// <summary>
        /// Create action
        /// </summary>
        public const string ACTION_CREATE = "create";

        /// <summary>
        /// Delete action
        /// </summary>
        public const string ACTION_DELETE = "delete";

        /// <summary>
        /// Set action
        /// </summary>
        public const string ACTION_SET = "set";

        /// <summary>
        /// Get action
        /// </summary>
        public const string ACTION_GET = "get";

        /// <summary>
        /// Expire action
        /// </summary>
        public const string ACTION_EXPIRE = "expire";

        /// <summary>
        /// CAS action
        /// </summary>
        public const string ACTION_COMPARE_AND_SWAP = "compareAndSwap";

        /// <summary>
        /// CAD action
        /// </summary>
        public const string ACTION_COMPARE_AND_DELETE = "compareAndDelete";

        /// <summary>
        /// Represents the action
        /// </summary>
        [DataMember(Name = "action")]
        public string Action { get; internal set; }

        /// <summary>
        /// Changed node
        /// </summary>
        [DataMember(Name = "node")]
        public EtcdNode Node { get; set; }

        /// <summary>
        /// Previous node
        /// </summary>
        [DataMember(Name = "prevNode")]
        public EtcdNode PrevNode { get; set; }

        /// <summary>
        /// The url of Etcd server which produce the response
        /// </summary>
        [IgnoreDataMember]
        public string EtcdServer { get; internal set; }

        /// <summary>
        /// X-Etcd-Cluster-Id
        /// </summary>
        [IgnoreDataMember]
        public string EtcdClusterID { get; internal set; }

        /// <summary>
        /// X-Etcd-Index is the current etcd index as explained above. When request is a watch on key space, X-Etcd-Index is the current etcd index when the watch starts, which means that the watched event may happen after X-Etcd-Index.
        /// </summary>
        [IgnoreDataMember]
        public long EtcdIndex { get; internal set; }

        /// <summary>
        /// X-Raft-Index is similar to the etcd index but is for the underlying raft protocol
        /// </summary>
        [IgnoreDataMember]
        public long RaftIndex { get; internal set; }

        /// <summary>
        /// X-Raft-Term is an integer that will increase whenever an etcd master election happens in the cluster. If this number is increasing rapidly, you may need to tune the election timeout. See the tuning section for details.
        /// </summary>
        [IgnoreDataMember]
        public long RaftTerm { get; internal set; }

    }
}
