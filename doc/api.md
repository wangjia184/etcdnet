# EtcdNet API Documentation

##EtcdClient Class
            
This class is used to talk with etcd service
        
###Properties

* `ClusterID` `X-Etcd-Cluster-Id` received
* `LastIndex` Lastest `X-Etcd-Index` received by this instance

###Methods


####Constructor
Constructor
> #####Parameters
> **options:** An instance of `EtcdClientOpitions` representing the options to initialize


####GetNodeAsync
Get etcd node specified by `key`
> #####Parameters
> **key:** The path of the node, must start with `/`

> **recursive:** To list the children nodes

> **sorted:** To enumerate the in-order keys as a sorted list.

> **ignoreKeyNotFoundException:** If `true`, `EtcdCommonException.KeyNotFound` exception is ignored and `null` is returned instead.

> #####Return value
> represents response; or `null` if not exist when `ignoreKeyNotFoundException` is `true`

####GetNodeValueAsync
Simplified version of `GetNodeAsync`. Get the value of the specific node
> #####Parameters
> **key:** The path of the node, must start with `/`

> **ignoreKeyNotFoundException:** If `true`, `EtcdCommonException.KeyNotFound` exception is ignored and `null` is returned instead.

> #####Return value
> A string represents a value. It could be `null`

####SetNodeAsync
Get etcd node specified by `key`
> #####Parameters
> **key:** path of the node

> **value:** value to be set

> **ttl:** time to live, in seconds

> **dir:** indicates if this is a directory

> #####Return value
> `EtcdResponse`

####DeleteNodeAsync
delete specific node
> #####Parameters
> **key:** The path of the node, must start with `/`

> **dir:** true to delete an empty directory

> **ignoreKeyNotFoundException:** If `true`, `EtcdCommonException.KeyNotFound` exception is ignored and `null` is returned instead.

> #####Return value
> `EtcdResponse` instance or `null`

####CreateInOrderNodeAsync
Create in-order node
> #####Parameters
> **key:** the path of the directory node which hosts the in-order node

> **value:** value

> **ttl:** time to live (in seconds)

> **dir:** true to create directory node

> #####Return value
> `EtcdResponse`

####CreateNodeAsync
Create a new node. If node exists, `EtcdCommonException.NodeExist` occurs
> #####Parameters
> **key:** the path of the node

> **value:** value

> **ttl:** time to live (in seconds)

> **dir:** true to create directory node

> #####Return value
> `EtcdResponse`

####CompareAndSwapNodeAsync
CAS(Compare and Swap) a node
> #####Parameters
> **key:** the path of the node

> **prevValue:** previous value to be compared

> **value:** value to be updated

> **ttl:** time to live (in seconds)

> **dir:** `true` for directory

> #####Return value
> `EtcdResponse`

####CompareAndSwapNodeAsync
CAS(Compare and Swap) a node
> #####Parameters
> **key:** the path of the node

> **prevIndex:** previous index to compare

> **value:** value to be updated

> **ttl:** time to live (in seconds)

> **dir:** `true` for directory

> #####Return value
> `EtcdResponse`

####CompareAndDeleteNodeAsync
Compare and delete specific node
> #####Parameters
> **key:** Path of the node

> **prevValue:** previous value to compare

> #####Return value
> `EtcdResponse`

####CompareAndDeleteNodeAsync
Compare and delete specific node
> #####Parameters
> **key:** path of the node

> **prevIndex:** previous index to compare

> #####Return value
> `EtcdResponse`

####WatchNodeAsync
Watch changes
> #####Parameters
> **key:** Path of the node

> **recursive:** true to monitor descendants

> **waitIndex:** Etcd Index is continue monitor from

> #####Return value
> `EtcdResponse`

