using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtcdNet
{
    /// <summary>
    ///  error code in key space '/v2/keys'
    ///  https://github.com/coreos/etcd/blob/master/Documentation/errorcode.md
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>
        /// Key not found
        /// </summary>
        KeyNotFound = 100,

        /// <summary>
        /// Compare failed
        /// </summary>
        TestFailed = 101,

        /// <summary>
        /// Not a file
        /// </summary>
        NotFile = 102,

        /// <summary>
        /// Not a directory
        /// </summary>
        NotDir = 104,

        /// <summary>
        /// Key already exists
        /// </summary>
        NodeExist = 105,

        /// <summary>
        /// Root is read only
        /// </summary>
        RootReadOnly = 107,

        /// <summary>
        /// Directory not empty
        /// </summary>
        DirNotEmpty = 108,

        /// <summary>
        /// PrevValue is Required in POST form
        /// </summary>
        PrevValueRequired = 201,

        /// <summary>
        /// The given TTL in POST form is not a number
        /// </summary>
        TTLNaN = 202,

        /// <summary>
        /// The given index in POST form is not a number
        /// </summary>
        IndexNaN = 203,

        /// <summary>
        /// Invalid field
        /// </summary>
        InvalidField = 209, 

        /// <summary>
        /// Invalid POST form
        /// </summary>
        InvalidForm = 210,

        /// <summary>
        /// Raft Internal Error
        /// </summary>
        RaftInternal = 300,

        /// <summary>
        /// During Leader Election
        /// </summary>
        LeaderElect = 301,

        /// <summary>
        /// watcher is cleared due to etcd recovery
        /// </summary>
        WatcherCleared = 400,

        /// <summary>
        /// The event in requested index is outdated and cleared
        /// </summary>
        EventIndexCleared = 401,
    }
}
