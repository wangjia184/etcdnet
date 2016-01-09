# EtcdNet


EtcdNet is a .NET client library to access [etcd](https://github.com/coreos/etcd), which is a distributed, consistent key-value store for shared configuration and service discovery. 

* Provides API for key space operations
* Support authentication & client-certificate
* Support etcd cluster & failover
* Lightweight & zero dependency on other assembly
* Task-based Asynchronous Pattern (TAP) API
* Structured Exceptions
* .NET Framework minimal requirement : v4.5


## Get Started

### Installation
To install etcdnet, run the following command in the Package Manager Console of Visual Studio. Or you can search `etcdnet` in NuGet

```
Install-Package etcdnet
```

### Initialization

To use the client, the first step is to instantiate `EtcdClient` as below.

```csharp
using EtcdNet;
using EtcdNet.DTO;

var options = new EtcdClientOpitions() {
    Urls = new string[] { "http://etcd0.em:2379" },
    //...
};
EtcdClient etcdClient = new EtcdClient(options);
```



##### Options

`EtcdClientOpitions` allows one to customize the `EtcdClient`.

* `Urls` The url of the etcd service. If you are running a etcd cluster, more then one urls here.
* `IgnoreCertificateError`
* `X509Certificate`
* `Username`
* `Password`
* `UseProxy`
* `JsonDeserializer`

### Make the call

`EtcdClient` class provides APIs like below

##### Thread Safety

The implementation of `EtcdClient` class is guaranteed to be thread-safe, which means the methods of the same instance can be called from different threads without synchronization. 

Further, it is recommended to use only one `EtcdClient` instance to talk to the same etcd cluster. `System.Net.Http.HttpClient` class, which emits HTTP requests internally, uses its own connection pool, isolating its requests from requests executed by other HttpClient instances. Sharing the same `EtcdClient` instance helps to utilize features like [HTTP pipelining](https://en.wikipedia.org/wiki/HTTP_pipelining), which is good for performance.