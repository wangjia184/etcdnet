## EtcdNet ##

# T:EtcdNet.DefaultJsonDeserializer

 DefaultJsonDeserializer takes use of DataContractJsonSerializer 



---
# T:EtcdNet.IJsonDeserializer

 This interface allows to choose alternative JSON deserializer 



---
##### M:EtcdNet.IJsonDeserializer.Deserialize``1(System.String)

 Deserialize the json string 

|Name | Description |
|-----|------|
|json: |json string|
Returns: deserialized json object



---
# T:EtcdNet.ErrorResponse

 Represent etcd error JSON 



---
##### P:EtcdNet.ErrorResponse.ErrorCode

 Error Code https://github.com/coreos/etcd/blob/master/Documentation/errorcode.md 



---
##### P:EtcdNet.ErrorResponse.Message

 Error message 



---
##### P:EtcdNet.ErrorResponse.Cause

 Cause 



---
##### P:EtcdNet.ErrorResponse.Index

 Index 



---
# T:EtcdNet.EtcdNode

 Represent a node in etcd 



---
##### M:EtcdNet.EtcdNode.GetExpirationTime

 Get expiration time of this node If none, DateTime.MaxValue is returned 

Returns: 



---
##### P:EtcdNet.EtcdNode.Key

 Path of the node 



---
##### P:EtcdNet.EtcdNode.IsDirectory

 Is directory 



---
##### P:EtcdNet.EtcdNode.Value

 Value 



---
##### P:EtcdNet.EtcdNode.CreatedIndex

 Index which creates this node 



---
##### P:EtcdNet.EtcdNode.ModifiedIndex

 Index of the modification 



---
##### P:EtcdNet.EtcdNode.TTL

 Time to live, in second 



---
##### P:EtcdNet.EtcdNode.Expiration

 Expiration time 



---
##### P:EtcdNet.EtcdNode.Nodes

 Children nodes 



---
# T:EtcdNet.EtcdResponse

 Normal response of etcd 



---
##### F:EtcdNet.EtcdResponse.ACTION_CREATE

 Create action 



---
##### F:EtcdNet.EtcdResponse.ACTION_DELETE

 Delete action 



---
##### F:EtcdNet.EtcdResponse.ACTION_SET

 Set action 



---
##### F:EtcdNet.EtcdResponse.ACTION_GET

 Get action 



---
##### F:EtcdNet.EtcdResponse.ACTION_EXPIRE

 Expire action 



---
##### F:EtcdNet.EtcdResponse.ACTION_COMPARE_AND_SWAP

 CAS action 



---
##### F:EtcdNet.EtcdResponse.ACTION_COMPARE_AND_DELETE

 CAD action 



---
##### P:EtcdNet.EtcdResponse.Action

 Represents the action 



---
##### P:EtcdNet.EtcdResponse.Node

 Changed node 



---
##### P:EtcdNet.EtcdResponse.PrevNode

 Previous node 



---
##### P:EtcdNet.EtcdResponse.EtcdServer

 The url of Etcd server which produce the response 



---
##### P:EtcdNet.EtcdResponse.EtcdClusterID

 X-Etcd-Cluster-Id 



---
##### P:EtcdNet.EtcdResponse.EtcdIndex

 X-Etcd-Index is the current etcd index as explained above. When request is a watch on key space, X-Etcd-Index is the current etcd index when the watch starts, which means that the watched event may happen after X-Etcd-Index. 



---
##### P:EtcdNet.EtcdResponse.RaftIndex

 X-Raft-Index is similar to the etcd index but is for the underlying raft protocol 



---
##### P:EtcdNet.EtcdResponse.RaftTerm

 X-Raft-Term is an integer that will increase whenever an etcd master election happens in the cluster. If this number is increasing rapidly, you may need to tune the election timeout. See the tuning section for details. 



---
# T:EtcdNet.ErrorCode

 error code in key space '/v2/keys' https://github.com/coreos/etcd/blob/master/Documentation/errorcode.md 



---
##### F:EtcdNet.ErrorCode.KeyNotFound

 Key not found 



---
##### F:EtcdNet.ErrorCode.TestFailed

 Compare failed 



---
##### F:EtcdNet.ErrorCode.NotFile

 Not a file 



---
##### F:EtcdNet.ErrorCode.NotDir

 Not a directory 



---
##### F:EtcdNet.ErrorCode.NodeExist

 Key already exists 



---
##### F:EtcdNet.ErrorCode.RootReadOnly

 Root is read only 



---
##### F:EtcdNet.ErrorCode.DirNotEmpty

 Directory not empty 



---
##### F:EtcdNet.ErrorCode.PrevValueRequired

 PrevValue is Required in POST form 



---
##### F:EtcdNet.ErrorCode.TTLNaN

 The given TTL in POST form is not a number 



---
##### F:EtcdNet.ErrorCode.IndexNaN

 The given index in POST form is not a number 



---
##### F:EtcdNet.ErrorCode.InvalidField

 Invalid field 



---
##### F:EtcdNet.ErrorCode.InvalidForm

 Invalid POST form 



---
##### F:EtcdNet.ErrorCode.RaftInternal

 Raft Internal Error 



---
##### F:EtcdNet.ErrorCode.LeaderElect

 During Leader Election 



---
##### F:EtcdNet.ErrorCode.WatcherCleared

 watcher is cleared due to etcd recovery 



---
##### F:EtcdNet.ErrorCode.EventIndexCleared

 The event in requested index is outdated and cleared 



---
# T:EtcdNet.EtcdClient

 The EtcdClient class is used to talk with etcd service 



---
##### M:EtcdNet.EtcdClient.#ctor(EtcdNet.EtcdClientOpitions)

 Constructor 

|Name | Description |
|-----|------|
|options: |options to initialize|


---
##### M:EtcdNet.EtcdClient.GetNodeAsync(System.String,System.Boolean,System.Boolean,System.Boolean)

 Get etcd node specified by `key` 

|Name | Description |
|-----|------|
|key: |The path of the node, must start with `/`|
|Name | Description |
|-----|------|
|recursive: |Represents whether list the children nodes|
|Name | Description |
|-----|------|
|sorted: |To enumerate the in-order keys as a sorted list, use the "sorted" parameter.|
|Name | Description |
|-----|------|
|ignoreKeyNotFoundException: |If `true`, `EtcdCommonException.KeyNotFound` exception is ignored and `null` is returned instead.|
Returns: represents response; or `null` if not exist



---
##### M:EtcdNet.EtcdClient.GetNodeValueAsync(System.String,System.Boolean)

 Simplified version of `GetNodeAsync`. Get the value of the specific node 

|Name | Description |
|-----|------|
|key: |The path of the node, must start with `/`|
|Name | Description |
|-----|------|
|ignoreKeyNotFoundException: |If `true`, `EtcdCommonException.KeyNotFound` exception is ignored and `null` is returned instead.|
Returns: A string represents a value. It could be `null`



---
##### M:EtcdNet.EtcdClient.SetNodeAsync(System.String,System.String,System.Nullable{System.Int32},System.Nullable{System.Boolean})

 Get etcd node specified by `key` 

|Name | Description |
|-----|------|
|key: |path of the node|
|Name | Description |
|-----|------|
|value: |value to be set|
|Name | Description |
|-----|------|
|ttl: |time to live, in seconds|
|Name | Description |
|-----|------|
|dir: |indicates if this is a directory|
Returns: SetNodeResponse



---
##### M:EtcdNet.EtcdClient.DeleteNodeAsync(System.String,System.Boolean,System.Nullable{System.Boolean})

 delete specific node 

|Name | Description |
|-----|------|
|key: |The path of the node, must start with `/`|
|Name | Description |
|-----|------|
|dir: |true to delete an empty directory|
|Name | Description |
|-----|------|
|ignoreKeyNotFoundException: |If `true`, `EtcdCommonException.KeyNotFound` exception is ignored and `null` is returned instead.|
Returns: SetNodeResponse instance or `null`



---
##### M:EtcdNet.EtcdClient.CreateInOrderNodeAsync(System.String,System.String,System.Nullable{System.Int32},System.Nullable{System.Boolean})

 Create in-order node 

|Name | Description |
|-----|------|
|key: ||
|Name | Description |
|-----|------|
|value: ||
|Name | Description |
|-----|------|
|ttl: ||
|Name | Description |
|-----|------|
|dir: ||
Returns: 



---
##### M:EtcdNet.EtcdClient.CreateNodeAsync(System.String,System.String,System.Nullable{System.Int32},System.Nullable{System.Boolean})

 Create a new node. If node exists, EtcdCommonException.NodeExist occurs 

|Name | Description |
|-----|------|
|key: ||
|Name | Description |
|-----|------|
|value: ||
|Name | Description |
|-----|------|
|ttl: ||
|Name | Description |
|-----|------|
|dir: ||
Returns: 



---
##### M:EtcdNet.EtcdClient.CompareAndSwapNodeAsync(System.String,System.String,System.String,System.Nullable{System.Int32},System.Nullable{System.Boolean})

 CAS(Compare and Swap) a node 

|Name | Description |
|-----|------|
|key: ||
|Name | Description |
|-----|------|
|prevValue: ||
|Name | Description |
|-----|------|
|value: ||
|Name | Description |
|-----|------|
|ttl: ||
|Name | Description |
|-----|------|
|dir: ||
Returns: 



---
##### M:EtcdNet.EtcdClient.CompareAndSwapNodeAsync(System.String,System.Int64,System.String,System.Nullable{System.Int32},System.Nullable{System.Boolean})

 CAS(Compare and Swap) a node 

|Name | Description |
|-----|------|
|key: |path of the node|
|Name | Description |
|-----|------|
|prevIndex: |previous index|
|Name | Description |
|-----|------|
|value: |value|
|Name | Description |
|-----|------|
|ttl: |time to live (in seconds)|
|Name | Description |
|-----|------|
|dir: |is directory|
Returns: 



---
##### M:EtcdNet.EtcdClient.CompareAndDeleteNodeAsync(System.String,System.String)

 Compare and delete specific node 

|Name | Description |
|-----|------|
|key: |Path of the node|
|Name | Description |
|-----|------|
|prevValue: |previous value|
Returns: EtcdResponse



---
##### M:EtcdNet.EtcdClient.CompareAndDeleteNodeAsync(System.String,System.Int64)

 Compare and delete specific node 

|Name | Description |
|-----|------|
|key: |path of the node|
|Name | Description |
|-----|------|
|prevIndex: |previous index|
Returns: EtcdResponse



---
##### M:EtcdNet.EtcdClient.WatchNodeAsync(System.String,System.Boolean,System.Nullable{System.Int64})

 Watch changes 

|Name | Description |
|-----|------|
|key: |Path of the node|
|Name | Description |
|-----|------|
|recursive: |true to monitor descendants|
|Name | Description |
|-----|------|
|waitIndex: |Etcd Index is continue monitor from|
Returns: EtcdResponse



---
##### P:EtcdNet.EtcdClient.ClusterID

 X-Etcd-Cluster-Id 



---
##### P:EtcdNet.EtcdClient.LastIndex

 Lastest X-Etcd-Index received by this instance 



---
# T:EtcdNet.EtcdClientOpitions

 Options to initialize EtcdClient 



---
##### P:EtcdNet.EtcdClientOpitions.Urls

 The urls of etcd servers (mandatory) 



---
##### P:EtcdNet.EtcdClientOpitions.IgnoreCertificateError

 ignore invalid SSL certificate 



---
##### P:EtcdNet.EtcdClientOpitions.X509Certificate

 Client certificate 



---
##### P:EtcdNet.EtcdClientOpitions.Username

 Username 



---
##### P:EtcdNet.EtcdClientOpitions.Password

 Password 



---
##### P:EtcdNet.EtcdClientOpitions.UseProxy

 Use proxy? 



---
##### P:EtcdNet.EtcdClientOpitions.JsonDeserializer

 If this field is null, default deserializer is used This parameter allows to use a different deserializer like ServiceStack.Text or Newtonsoft.Json 



---
# T:EtcdNet.EtcdGenericException

 Represents the generic exception from etcd https://github.com/coreos/etcd/blob/master/Documentation/errorcode.md EtcdGenericException ©À©¤©¤ EtcdCommonException | ©À©¤ KeyNotFound | ©À©¤ TestFailed | ©À©¤ NotFile | ©À©¤ NotDir | ©À©¤ NodeExist | ©À©¤ RootReadOnly | ©¸©¤ DirNotEmpty ©À©¤©¤ EtcdPostFormException | ©À©¤ PrevValueRequired | ©À©¤ TTLNaN | ©À©¤ IndexNaN | ©À©¤ InvalidField | ©¸©¤ InvalidForm ©À©¤©¤ EtcdRaftException | ©À©¤ RaftInternal | ©¸©¤ LeaderElect ©¸©¤©¤ EtcdException ©À©¤ WatcherCleared ©¸©¤ EventIndexCleared 



---
##### M:EtcdNet.EtcdGenericException.#ctor(System.String)

 Constructor 

|Name | Description |
|-----|------|
|message: ||


---
##### P:EtcdNet.EtcdGenericException.Code

 https://github.com/coreos/etcd/blob/master/Documentation/errorcode.md 



---
##### P:EtcdNet.EtcdGenericException.Cause

 Cause 



---
# T:EtcdNet.EtcdCommonException

 Command Related Error 



---
# T:EtcdNet.EtcdCommonException.KeyNotFound

 EcodeKeyNotFound 100 "Key not found" 



---
# T:EtcdNet.EtcdCommonException.TestFailed

 EcodeTestFailed 101 "Compare failed" 



---
# T:EtcdNet.EtcdCommonException.NotFile

 EcodeNotFile 102 "Not a file" 



---
# T:EtcdNet.EtcdCommonException.NotDir

 EcodeNotDir 104 "Not a directory" 



---
# T:EtcdNet.EtcdCommonException.NodeExist

 EcodeNodeExist 105 "Key already exists" 



---
# T:EtcdNet.EtcdCommonException.RootReadOnly

 EcodeRootROnly 107 "Root is read only" 



---
# T:EtcdNet.EtcdCommonException.DirNotEmpty

 EcodeDirNotEmpty 108 "Directory not empty" 



---
# T:EtcdNet.EtcdPostFormException

 Post Form Related Error 



---
# T:EtcdNet.EtcdPostFormException.PrevValueRequired

 EcodePrevValueRequired 201 "PrevValue is Required in POST form" 



---
# T:EtcdNet.EtcdPostFormException.TTLNaN

 EcodeTTLNaN 202 "The given TTL in POST form is not a number" 



---
# T:EtcdNet.EtcdPostFormException.IndexNaN

 EcodeIndexNaN 203 "The given index in POST form is not a number" 



---
# T:EtcdNet.EtcdPostFormException.InvalidField

 EcodeInvalidField 209 "Invalid field" 



---
# T:EtcdNet.EtcdPostFormException.InvalidForm

 EcodeInvalidForm 210 "Invalid POST form" 



---
# T:EtcdNet.EtcdRaftException

 Raft Related Error 



---
# T:EtcdNet.EtcdRaftException.Internal

 EcodeRaftInternal 300 "Raft Internal Error" 



---
# T:EtcdNet.EtcdRaftException.LeaderElect

 EcodeLeaderElect 301 "During Leader Election" 



---
# T:EtcdNet.EtcdException

 Etcd Related Error 



---
# T:EtcdNet.EtcdException.WatcherCleared

 EcodeWatcherCleared 400 "watcher is cleared due to etcd recovery" 



---
# T:EtcdNet.EtcdException.EventIndexCleared

 EcodeEventIndexCleared 401 "The event in requested index is outdated and cleared" 



---
##### P:EtcdNet.HttpClientEx.Next

 A loop 



---



