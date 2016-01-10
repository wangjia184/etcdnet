#EtcdNet


##DefaultJsonDeserializer
            
DefaultJsonDeserializer takes use of DataContractJsonSerializer
        

##IJsonDeserializer
            
This interface allows to choose alternative JSON deserializer
        
###Methods


####Deserialize``1(System.String)
Deserialize the json string
> #####Parameters
> **json:** json string

> #####Return value
> deserialized json object

##ErrorResponse
            
Represent etcd error JSON
        
###Properties

####ErrorCode
Error Code https://github.com/coreos/etcd/blob/master/Documentation/errorcode.md
####Message
Error message
####Cause
Cause
####Index
Index

##EtcdNode
            
Represent a node in etcd
        
###Properties

####Key
Path of the node
####IsDirectory
Is directory
####Value
Value
####CreatedIndex
Index which creates this node
####ModifiedIndex
Index of the modification
####TTL
Time to live, in second
####Expiration
Expiration time
####Nodes
Children nodes
###Methods


####GetExpirationTime
Get expiration time of this node If none, DateTime.MaxValue is returned
> #####Return value
> 

##EtcdResponse
            
Normal response of etcd
        
###Fields

####ACTION_CREATE
Create action
####ACTION_DELETE
Delete action
####ACTION_SET
Set action
####ACTION_GET
Get action
####ACTION_EXPIRE
Expire action
####ACTION_COMPARE_AND_SWAP
CAS action
####ACTION_COMPARE_AND_DELETE
CAD action
###Properties

####Action
Represents the action
####Node
Changed node
####PrevNode
Previous node
####EtcdServer
The url of Etcd server which produce the response
####EtcdClusterID
X-Etcd-Cluster-Id
####EtcdIndex
X-Etcd-Index is the current etcd index as explained above. When request is a watch on key space, X-Etcd-Index is the current etcd index when the watch starts, which means that the watched event may happen after X-Etcd-Index.
####RaftIndex
X-Raft-Index is similar to the etcd index but is for the underlying raft protocol
####RaftTerm
X-Raft-Term is an integer that will increase whenever an etcd master election happens in the cluster. If this number is increasing rapidly, you may need to tune the election timeout. See the tuning section for details.

##ErrorCode
            
error code in key space '/v2/keys' https://github.com/coreos/etcd/blob/master/Documentation/errorcode.md
        
###Fields

####KeyNotFound
Key not found
####TestFailed
Compare failed
####NotFile
Not a file
####NotDir
Not a directory
####NodeExist
Key already exists
####RootReadOnly
Root is read only
####DirNotEmpty
Directory not empty
####PrevValueRequired
PrevValue is Required in POST form
####TTLNaN
The given TTL in POST form is not a number
####IndexNaN
The given index in POST form is not a number
####InvalidField
Invalid field
####InvalidForm
Invalid POST form
####RaftInternal
Raft Internal Error
####LeaderElect
During Leader Election
####WatcherCleared
watcher is cleared due to etcd recovery
####EventIndexCleared
The event in requested index is outdated and cleared

##EtcdClient
            
The EtcdClient class is used to talk with etcd service
        
###Properties

####ClusterID
X-Etcd-Cluster-Id
####LastIndex
Lastest X-Etcd-Index received by this instance
####
The urls of etcd servers (mandatory)
####
ignore invalid SSL certificate
####
Client certificate
####
Username
####
Password
####
Use proxy?
####
If this field is null, default deserializer is used This parameter allows to use a different deserializer like ServiceStack.Text or Newtonsoft.Json
###Methods


####Constructor
Constructor
> #####Parameters
> **options:** options to initialize


####GetNodeAsync(System.String,System.Boolean,System.Boolean,System.Boolean)
Get etcd node specified by `key`
> #####Parameters
> **key:** The path of the node, must start with `/`

> **recursive:** Represents whether list the children nodes

> **sorted:** To enumerate the in-order keys as a sorted list, use the "sorted" parameter.

> **ignoreKeyNotFoundException:** If `true`, `EtcdCommonException.KeyNotFound` exception is ignored and `null` is returned instead.

> #####Return value
> represents response; or `null` if not exist

####GetNodeValueAsync(System.String,System.Boolean)
Simplified version of `GetNodeAsync`. Get the value of the specific node
> #####Parameters
> **key:** The path of the node, must start with `/`

> **ignoreKeyNotFoundException:** If `true`, `EtcdCommonException.KeyNotFound` exception is ignored and `null` is returned instead.

> #####Return value
> A string represents a value. It could be `null`

####SetNodeAsync(System.String,System.String,System.Nullable{System.Int32},System.Nullable{System.Boolean})
Get etcd node specified by `key`
> #####Parameters
> **key:** path of the node

> **value:** value to be set

> **ttl:** time to live, in seconds

> **dir:** indicates if this is a directory

> #####Return value
> SetNodeResponse

####DeleteNodeAsync(System.String,System.Boolean,System.Nullable{System.Boolean})
delete specific node
> #####Parameters
> **key:** The path of the node, must start with `/`

> **dir:** true to delete an empty directory

> **ignoreKeyNotFoundException:** If `true`, `EtcdCommonException.KeyNotFound` exception is ignored and `null` is returned instead.

> #####Return value
> SetNodeResponse instance or `null`

####CreateInOrderNodeAsync(System.String,System.String,System.Nullable{System.Int32},System.Nullable{System.Boolean})
Create in-order node
> #####Parameters
> **key:** 

> **value:** 

> **ttl:** 

> **dir:** 

> #####Return value
> 

####CreateNodeAsync(System.String,System.String,System.Nullable{System.Int32},System.Nullable{System.Boolean})
Create a new node. If node exists, EtcdCommonException.NodeExist occurs
> #####Parameters
> **key:** 

> **value:** 

> **ttl:** 

> **dir:** 

> #####Return value
> 

####CompareAndSwapNodeAsync(System.String,System.String,System.String,System.Nullable{System.Int32},System.Nullable{System.Boolean})
CAS(Compare and Swap) a node
> #####Parameters
> **key:** 

> **prevValue:** 

> **value:** 

> **ttl:** 

> **dir:** 

> #####Return value
> 

####CompareAndSwapNodeAsync(System.String,System.Int64,System.String,System.Nullable{System.Int32},System.Nullable{System.Boolean})
CAS(Compare and Swap) a node
> #####Parameters
> **key:** path of the node

> **prevIndex:** previous index

> **value:** value

> **ttl:** time to live (in seconds)

> **dir:** is directory

> #####Return value
> 

####CompareAndDeleteNodeAsync(System.String,System.String)
Compare and delete specific node
> #####Parameters
> **key:** Path of the node

> **prevValue:** previous value

> #####Return value
> EtcdResponse

####CompareAndDeleteNodeAsync(System.String,System.Int64)
Compare and delete specific node
> #####Parameters
> **key:** path of the node

> **prevIndex:** previous index

> #####Return value
> EtcdResponse

####WatchNodeAsync(System.String,System.Boolean,System.Nullable{System.Int64})
Watch changes
> #####Parameters
> **key:** Path of the node

> **recursive:** true to monitor descendants

> **waitIndex:** Etcd Index is continue monitor from

> #####Return value
> EtcdResponse

##EtcdClientOpitions
            
Options to initialize EtcdClient
        
###Properties

####Urls
The urls of etcd servers (mandatory)
####IgnoreCertificateError
ignore invalid SSL certificate
####X509Certificate
Client certificate
####Username
Username
####Password
Password
####UseProxy
Use proxy?
####JsonDeserializer
If this field is null, default deserializer is used This parameter allows to use a different deserializer like ServiceStack.Text or Newtonsoft.Json

##EtcdGenericException
            
Represents the generic exception from etcd https://github.com/coreos/etcd/blob/master/Documentation/errorcode.md EtcdGenericException ├── EtcdCommonException | ├─ KeyNotFound | ├─ TestFailed | ├─ NotFile | ├─ NotDir | ├─ NodeExist | ├─ RootReadOnly | └─ DirNotEmpty ├── EtcdPostFormException | ├─ PrevValueRequired | ├─ TTLNaN | ├─ IndexNaN | ├─ InvalidField | └─ InvalidForm ├── EtcdRaftException | ├─ RaftInternal | └─ LeaderElect └── EtcdException ├─ WatcherCleared └─ EventIndexCleared
        
###Properties

####Code
https://github.com/coreos/etcd/blob/master/Documentation/errorcode.md
####Cause
Cause
###Methods


####Constructor
Constructor
> #####Parameters
> **message:** 


##EtcdCommonException
            
Command Related Error
        

##EtcdCommonException.KeyNotFound
            
EcodeKeyNotFound 100 "Key not found"
        

##EtcdCommonException.TestFailed
            
EcodeTestFailed 101 "Compare failed"
        

##EtcdCommonException.NotFile
            
EcodeNotFile 102 "Not a file"
        

##EtcdCommonException.NotDir
            
EcodeNotDir 104 "Not a directory"
        

##EtcdCommonException.NodeExist
            
EcodeNodeExist 105 "Key already exists"
        

##EtcdCommonException.RootReadOnly
            
EcodeRootROnly 107 "Root is read only"
        

##EtcdCommonException.DirNotEmpty
            
EcodeDirNotEmpty 108 "Directory not empty"
        

##EtcdPostFormException
            
Post Form Related Error
        

##EtcdPostFormException.PrevValueRequired
            
EcodePrevValueRequired 201 "PrevValue is Required in POST form"
        

##EtcdPostFormException.TTLNaN
            
EcodeTTLNaN 202 "The given TTL in POST form is not a number"
        

##EtcdPostFormException.IndexNaN
            
EcodeIndexNaN 203 "The given index in POST form is not a number"
        

##EtcdPostFormException.InvalidField
            
EcodeInvalidField 209 "Invalid field"
        

##EtcdPostFormException.InvalidForm
            
EcodeInvalidForm 210 "Invalid POST form"
        

##EtcdRaftException
            
Raft Related Error
        

##EtcdRaftException.Internal
            
EcodeRaftInternal 300 "Raft Internal Error"
        

##EtcdRaftException.LeaderElect
            
EcodeLeaderElect 301 "During Leader Election"
        

##EtcdException
            
Etcd Related Error
        

##EtcdException.WatcherCleared
            
EcodeWatcherCleared 400 "watcher is cleared due to etcd recovery"
        

##EtcdException.EventIndexCleared
            
EcodeEventIndexCleared 401 "The event in requested index is outdated and cleared"
        