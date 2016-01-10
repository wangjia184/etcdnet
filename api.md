
# EtcdNet


## DefaultJsonDeserializer

DefaultJsonDeserializer takes use of DataContractJsonSerializer


## ErrorCode

error code in key space '/v2/keys' https://github.com/coreos/etcd/blob/master/Documentation/errorcode.md


### F:EtcdNet.DirNotEmpty

Directory not empty


### F:EtcdNet.EventIndexCleared

The event in requested index is outdated and cleared


### F:EtcdNet.IndexNaN

The given index in POST form is not a number


### F:EtcdNet.InvalidField

Invalid field


### F:EtcdNet.InvalidForm

Invalid POST form


### F:EtcdNet.KeyNotFound

Key not found


### F:EtcdNet.LeaderElect

During Leader Election


### F:EtcdNet.NodeExist

Key already exists


### F:EtcdNet.NotDir

Not a directory


### F:EtcdNet.NotFile

Not a file


### F:EtcdNet.PrevValueRequired

PrevValue is Required in POST form


### F:EtcdNet.RaftInternal

Raft Internal Error


### F:EtcdNet.RootReadOnly

Root is read only


### F:EtcdNet.TestFailed

Compare failed


### F:EtcdNet.TTLNaN

The given TTL in POST form is not a number


### F:EtcdNet.WatcherCleared

watcher is cleared due to etcd recovery


## ErrorResponse

Represent etcd error JSON


### .Cause

Cause


### .ErrorCode

Error Code https://github.com/coreos/etcd/blob/master/Documentation/errorcode.md


### .Index

Index


### .Message

Error message


## EtcdClient

The EtcdClient class is used to talk with etcd service


### M:EtcdNet.#ctor(options)

Constructor

| Name | Description |
| ---- | ----------- |
| options | *EtcdNet.EtcdClientOpitions*<br>options to initialize |

### .ClusterID

X-Etcd-Cluster-Id


### M:EtcdNet.CompareAndDeleteNodeAsync(key, prevIndex)

Compare and delete specific node

| Name | Description |
| ---- | ----------- |
| key | *System.String*<br>path of the node |
| prevIndex | *System.Int64*<br>previous index |


#### Returns

EtcdResponse


### M:EtcdNet.CompareAndDeleteNodeAsync(key, prevValue)

Compare and delete specific node

| Name | Description |
| ---- | ----------- |
| key | *System.String*<br>Path of the node |
| prevValue | *System.String*<br>previous value |


#### Returns

EtcdResponse


### M:EtcdNet.CompareAndSwapNodeAsync(key, prevIndex, value, ttl, dir)

CAS(Compare and Swap) a node

| Name | Description |
| ---- | ----------- |
| key | *System.String*<br>path of the node |
| prevIndex | *System.Int64*<br>previous index |
| value | *System.String*<br>value |
| ttl | *System.Nullable{System.Int32}*<br>time to live (in seconds) |
| dir | *System.Nullable{System.Boolean}*<br>is directory |


#### Returns




### M:EtcdNet.CompareAndSwapNodeAsync(key, prevValue, value, ttl, dir)

CAS(Compare and Swap) a node

| Name | Description |
| ---- | ----------- |
| key | *System.String*<br> |
| prevValue | *System.String*<br> |
| value | *System.String*<br> |
| ttl | *System.Nullable{System.Int32}*<br> |
| dir | *System.Nullable{System.Boolean}*<br> |


#### Returns




### M:EtcdNet.CreateInOrderNodeAsync(key, value, ttl, dir)

Create in-order node

| Name | Description |
| ---- | ----------- |
| key | *System.String*<br> |
| value | *System.String*<br> |
| ttl | *System.Nullable{System.Int32}*<br> |
| dir | *System.Nullable{System.Boolean}*<br> |


#### Returns




### M:EtcdNet.CreateNodeAsync(key, value, ttl, dir)

Create a new node. If node exists, EtcdCommonException.NodeExist occurs

| Name | Description |
| ---- | ----------- |
| key | *System.String*<br> |
| value | *System.String*<br> |
| ttl | *System.Nullable{System.Int32}*<br> |
| dir | *System.Nullable{System.Boolean}*<br> |


#### Returns




### M:EtcdNet.DeleteNodeAsync(key, dir, ignoreKeyNotFoundException)

delete specific node

| Name | Description |
| ---- | ----------- |
| key | *System.String*<br>The path of the node, must start with `/` |
| dir | *System.Boolean*<br>true to delete an empty directory |
| ignoreKeyNotFoundException | *System.Nullable{System.Boolean}*<br>If `true`, `EtcdCommonException.KeyNotFound` exception is ignored and `null` is returned instead. |


#### Returns

SetNodeResponse instance or `null`


### M:EtcdNet.GetNodeAsync(key, recursive, sorted, ignoreKeyNotFoundException)

Get etcd node specified by `key`

| Name | Description |
| ---- | ----------- |
| key | *System.String*<br>The path of the node, must start with `/` |
| recursive | *System.Boolean*<br>Represents whether list the children nodes |
| sorted | *System.Boolean*<br>To enumerate the in-order keys as a sorted list, use the "sorted" parameter. |
| ignoreKeyNotFoundException | *System.Boolean*<br>If `true`, `EtcdCommonException.KeyNotFound` exception is ignored and `null` is returned instead. |


#### Returns

represents response; or `null` if not exist


### M:EtcdNet.GetNodeValueAsync(key, ignoreKeyNotFoundException)

Simplified version of `GetNodeAsync`. Get the value of the specific node

| Name | Description |
| ---- | ----------- |
| key | *System.String*<br>The path of the node, must start with `/` |
| ignoreKeyNotFoundException | *System.Boolean*<br>If `true`, `EtcdCommonException.KeyNotFound` exception is ignored and `null` is returned instead. |


#### Returns

A string represents a value. It could be `null`


### .LastIndex

Lastest X-Etcd-Index received by this instance


### M:EtcdNet.SetNodeAsync(key, value, ttl, dir)

Get etcd node specified by `key`

| Name | Description |
| ---- | ----------- |
| key | *System.String*<br>path of the node |
| value | *System.String*<br>value to be set |
| ttl | *System.Nullable{System.Int32}*<br>time to live, in seconds |
| dir | *System.Nullable{System.Boolean}*<br>indicates if this is a directory |


#### Returns

SetNodeResponse


### M:EtcdNet.WatchNodeAsync(key, recursive, waitIndex)

Watch changes

| Name | Description |
| ---- | ----------- |
| key | *System.String*<br>Path of the node |
| recursive | *System.Boolean*<br>true to monitor descendants |
| waitIndex | *System.Nullable{System.Int64}*<br>Etcd Index is continue monitor from |


#### Returns

EtcdResponse


## EtcdClientOpitions

Options to initialize EtcdClient


### .IgnoreCertificateError

ignore invalid SSL certificate


### .JsonDeserializer

If this field is null, default deserializer is used This parameter allows to use a different deserializer like ServiceStack.Text or Newtonsoft.Json


### .Password

Password


### .Urls

The urls of etcd servers (mandatory)


### .UseProxy

Use proxy?


### .Username

Username


### .X509Certificate

Client certificate


## EtcdCommonException

Command Related Error


## EtcdCommonException.DirNotEmpty

EcodeDirNotEmpty 108 "Directory not empty"


## EtcdCommonException.KeyNotFound

EcodeKeyNotFound 100 "Key not found"


## EtcdCommonException.NodeExist

EcodeNodeExist 105 "Key already exists"


## EtcdCommonException.NotDir

EcodeNotDir 104 "Not a directory"


## EtcdCommonException.NotFile

EcodeNotFile 102 "Not a file"


## EtcdCommonException.RootReadOnly

EcodeRootROnly 107 "Root is read only"


## EtcdCommonException.TestFailed

EcodeTestFailed 101 "Compare failed"


## EtcdException

Etcd Related Error


## EtcdException.EventIndexCleared

EcodeEventIndexCleared 401 "The event in requested index is outdated and cleared"


## EtcdException.WatcherCleared

EcodeWatcherCleared 400 "watcher is cleared due to etcd recovery"


## EtcdGenericException

Represents the generic exception from etcd https://github.com/coreos/etcd/blob/master/Documentation/errorcode.md EtcdGenericException ©À©¤©¤ EtcdCommonException | ©À©¤ KeyNotFound | ©À©¤ TestFailed | ©À©¤ NotFile | ©À©¤ NotDir | ©À©¤ NodeExist | ©À©¤ RootReadOnly | ©¸©¤ DirNotEmpty ©À©¤©¤ EtcdPostFormException | ©À©¤ PrevValueRequired | ©À©¤ TTLNaN | ©À©¤ IndexNaN | ©À©¤ InvalidField | ©¸©¤ InvalidForm ©À©¤©¤ EtcdRaftException | ©À©¤ RaftInternal | ©¸©¤ LeaderElect ©¸©¤©¤ EtcdException ©À©¤ WatcherCleared ©¸©¤ EventIndexCleared


### M:EtcdNet.#ctor(message)

Constructor

| Name | Description |
| ---- | ----------- |
| message | *System.String*<br> |

### .Cause

Cause


### .Code

https://github.com/coreos/etcd/blob/master/Documentation/errorcode.md


## EtcdNode

Represent a node in etcd


### .CreatedIndex

Index which creates this node


### .Expiration

Expiration time


### M:EtcdNet.GetExpirationTime

Get expiration time of this node If none, DateTime.MaxValue is returned


#### Returns




### .IsDirectory

Is directory


### .Key

Path of the node


### .ModifiedIndex

Index of the modification


### .Nodes

Children nodes


### .TTL

Time to live, in second


### .Value

Value


## EtcdPostFormException

Post Form Related Error


## EtcdPostFormException.IndexNaN

EcodeIndexNaN 203 "The given index in POST form is not a number"


## EtcdPostFormException.InvalidField

EcodeInvalidField 209 "Invalid field"


## EtcdPostFormException.InvalidForm

EcodeInvalidForm 210 "Invalid POST form"


## EtcdPostFormException.PrevValueRequired

EcodePrevValueRequired 201 "PrevValue is Required in POST form"


## EtcdPostFormException.TTLNaN

EcodeTTLNaN 202 "The given TTL in POST form is not a number"


## EtcdRaftException

Raft Related Error


## EtcdRaftException.Internal

EcodeRaftInternal 300 "Raft Internal Error"


## EtcdRaftException.LeaderElect

EcodeLeaderElect 301 "During Leader Election"


## EtcdResponse

Normal response of etcd


### .Action

Represents the action


### F:EtcdNet.ACTION_COMPARE_AND_DELETE

CAD action


### F:EtcdNet.ACTION_COMPARE_AND_SWAP

CAS action


### F:EtcdNet.ACTION_CREATE

Create action


### F:EtcdNet.ACTION_DELETE

Delete action


### F:EtcdNet.ACTION_EXPIRE

Expire action


### F:EtcdNet.ACTION_GET

Get action


### F:EtcdNet.ACTION_SET

Set action


### .EtcdClusterID

X-Etcd-Cluster-Id


### .EtcdIndex

X-Etcd-Index is the current etcd index as explained above. When request is a watch on key space, X-Etcd-Index is the current etcd index when the watch starts, which means that the watched event may happen after X-Etcd-Index.


### .EtcdServer

The url of Etcd server which produce the response


### .Node

Changed node


### .PrevNode

Previous node


### .RaftIndex

X-Raft-Index is similar to the etcd index but is for the underlying raft protocol


### .RaftTerm

X-Raft-Term is an integer that will increase whenever an etcd master election happens in the cluster. If this number is increasing rapidly, you may need to tune the election timeout. See the tuning section for details.


### .HttpClientEx.Next

A loop


## IJsonDeserializer

This interface allows to choose alternative JSON deserializer


### M:EtcdNet.Deserialize``1(json)

Deserialize the json string

| Name | Description |
| ---- | ----------- |
| json | *System.String*<br>json string |


#### Returns

deserialized json object


