# EtcdNet


EtcdNet is a .NET client library to access [etcd](https://github.com/coreos/etcd), which is a distributed, consistent key-value store for shared configuration and service discovery. 

* Provides API for all key space operations
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

### Basic Usage

Instantiate `EtcdClient` class, then make the call.

```csharp
using EtcdNet;

var options = new EtcdClientOpitions() {
    Urls = new string[] { "http://etcd0.em:2379" },
    //...
};
EtcdClient etcdClient = new EtcdClient(options);

string value = await etcdClient.GetNodeValueAsync("/some-key");
//...
```

[Here](./doc/api.md) you can find detailed api doc for `EtcdClient` class. More examples can be found in [sample](https://github.com/wangjia184/etcdnet/blob/master/EtcdNet.Sample/Program.cs).

### EtcdClientOpitions

`EtcdClientOpitions` allows to customize the `EtcdClient`.

```csharp
EtcdClientOpitions options = new EtcdClientOpitions() {
    Urls = new string[] { "https://server1", "https://server2", "https://server3" },
    Username = "username",
    Password = "password",
    UseProxy = false,
    IgnoreCertificateError = true, 
    X509Certificate = new X509Certificate2(@"client.p12"),
    JsonDeserializer = new NewtonsoftJsonDeserializer(),
};
```

* `Urls` If you are running a etcd cluster, more then one urls here.

* `Username` & `Password` are required when etcd enables basic authentication

* `UseProxy` controls if use system proxy

* `IgnoreCertificateError` ignores untrusted server SSL certificates. This is useful if you are using a self-signed SSL cert.

* `X509Certificate` is required when etcd enabled client certification.

* `JsonDeserializer` allows you to choose a different JSON deserializer. EtcdNet aims to avoid dependency on other 3rd-party assembly. Hence it takes use of the built-in `DataContractJsonSerializer` to deserialize JSON. This parameter allows you to use other JSON deserializer like Newtonsoft.Json or ServiceStack.Text.

```csharp
class NewtonsoftJsonDeserializer : EtcdNet.IJsonDeserializer
{
    public T Deserialize<T>(string json)
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
    }
}
```


### Thread Safety

The implementation of `EtcdClient` class is guaranteed to be thread-safe, which means the methods of the same instance can be called from different threads without synchronization. 

Further, it is recommended to use only one `EtcdClient` instance to talk to the same etcd cluster. `System.Net.Http.HttpClient` class, which emits HTTP requests internally, uses its own connection pool, isolating its requests from requests executed by other HttpClient instances. Sharing the same `EtcdClient` instance helps to utilize features like [HTTP pipelining](https://en.wikipedia.org/wiki/HTTP_pipelining).


### Exception Handling

Each of the [error code](https://github.com/coreos/etcd/blob/master/Documentation/errorcode.md) defined by etcd is mapped to an individual exception class.

```
 EtcdGenericException
  ├── EtcdCommonException
  |    ├─ KeyNotFound
  |    ├─ TestFailed
  |    ├─ NotFile
  |    ├─ NotDir
  |    ├─ NodeExist
  |    ├─ RootReadOnly
  |    └─ DirNotEmpty
  ├── EtcdPostFormException
  |    ├─ PrevValueRequired
  |    ├─ TTLNaN
  |    ├─ IndexNaN
  |    ├─ InvalidField
  |    └─ InvalidForm
  ├── EtcdRaftException
  |    ├─ RaftInternal
  |    └─ LeaderElect
  └── EtcdException
       ├─ WatcherCleared
       └─ EventIndexCleared
```

Hence you have the choice to handle a specific error, or a group errors.
```csharp
try {
    //...
}
catch (EtcdCommonException.KeyNotFound) {
    // 100 error
}
catch (EtcdCommonException.NodeExist) {
    // 105 error
}
catch (EtcdCommonException) {
    // 100-199 errors
}
catch (EtcdGenericException) {
    // all etcd errors
}
```