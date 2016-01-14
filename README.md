# EtcdNet


EtcdNet is a .NET client library to access [etcd](https://github.com/coreos/etcd) (protocol V2), which is a distributed, consistent key-value store for shared configuration and service discovery. 

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

[Here](./doc/api.md) you can find detailed api doc for `EtcdClient` class. More examples can be found below.

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

Some methods which accept `ignoreKeyNotFoundException` parameter, allows you to ignore `EtcdCommonException.KeyNotFound` exception to make the code simpler.


## Examples

#### Update by key

```csharp
await etcdClient.SetNodeAsync(key, "value to be set");
```

#### Get a key

```csharp
try {
    EtcdResponse resp = await etcdClient.GetNodeAsync(key);
}
catch(EtcdCommonException.KeyNotFound) {
    // key does not exist
}
```

#### Get a key (ignore key-not-found error)

```csharp
EtcdResponse resp = await etcdClient.GetNodeAsync(key, ignoreKeyNotFoundException: true);
```

#### Get value by key

```csharp
string value = await etcdClient.GetNodeValueAsync(key, ignoreKeyNotFoundException: true);
```

#### Create a new key

```csharp
try {
    EtcdResponse resp = await etcdClient.CreateNodeAsync(key, value);
}
catch (EtcdCommonException.NodeExist) {
    // node already exists
}
```

#### Create a in-order key

```csharp
etcdClient.CreateInOrderNodeAsync(key, value, ttl: 3);
```

#### Delete a key

```csharp
try {
    EtcdResponse resp = await etcdClient.DeleteNodeAsync(key);
}
catch(EtcdCommonException.KeyNotFound) {
    // key does not exist
}
```

#### Delete a key (ignore key-not-found error)

```csharp
await etcdClient.DeleteNodeAsync(key, ignoreKeyNotFoundException: true);
```

#### List child keys in order

```csharp
try {
	EtcdResponse resp = await etcdClient.GetNodeAsync(key, recursive: true, sorted:true);
	if (resp.Node.Nodes != null) {
	    foreach (var node in resp.Node.Nodes)
	    {
	        // child node
	    }
	}
}
catch (EtcdCommonException.KeyNotFound) {
    // key does not exist
}
```

#### Compare and Swap (by value)

```csharp
string prevValue = ...;
try {
    EtcdResponse resp = await etcdClient.CompareAndSwapNodeAsync(key, prevValue, newValue);
}
catch (EtcdCommonException.KeyNotFound) {
    // key does not exist
}
catch (EtcdCommonException.TestFailed) {
    // supplied prevValue does not match
}
```

#### Compare and Swap (by index)

```csharp
long prevIndex = ...;
try {
    EtcdResponse resp = await etcdClient.CompareAndSwapNodeAsync(key, prevIndex, newValue);
}
catch (EtcdCommonException.KeyNotFound) {
    // key does not exist
}
catch (EtcdCommonException.TestFailed) {
    // supplied prevIndex does not match
}
```

#### Compare and Delete (by value)

```csharp
string prevValue = ...;
try {
    EtcdResponse resp = await etcdClient.CompareAndDeleteNodeAsync(key, prevValue);
}
catch (EtcdCommonException.KeyNotFound) {
    // key does not exist
}
catch (EtcdCommonException.TestFailed) {
    // supplied prevValue does not match
}
```

#### Compare and Delete (by index)

```csharp
long prevIndex = ...;
try {
    EtcdResponse resp = await etcdClient.CompareAndDeleteNodeAsync(key, prevIndex);
}
catch (EtcdCommonException.KeyNotFound) {
    // key does not exist
}
catch (EtcdCommonException.TestFailed) {
    // supplied prevValue does not match
}
```


#### Keep key alive

```csharp
async void KeepAlive()
{
    string key = "/my/key";
    string value = ...;
    const int ttl = 20; // seconds

    while (_running)
    {
        try
        {
            await _etcdClient.SetNodeAsync(key, value, ttl: ttl);
            if (!_running) return;
            await Task.Delay(ttl / 2 * 1000);
            continue;
        }
        catch (EtcdGenericException ege)
        {
            // etcd returns an error code
        }
        catch (Exception ex)
        {
            // a generic error
        }

        if (!_running) return;
        // something went wrong, delay 1 second and try again
        await Task.Delay(1000);
    }
}
```


#### Watch changes

```csharp
async void WatchChanges()
{
	string key = "/my/key";
	long? waitIndex = null;
	EtcdResponse resp;
	while (_running)
	{
		try
		{
			// when waitIndex is null, get it from the ModifiedIndex
			if( !waitIndex.HasValue )
			{
				resp = await _etcdClient.GetNodeAsync( key, recursive: true);
				if( resp != null && resp.Node != null )
				{
					waitIndex = resp.Node.ModifiedIndex + 1;

					// and also check the children
					if( resp.Node.Nodes != null )
					{
						foreach( var child in resp.Node.Nodes )
						{
							if (child.ModifiedIndex >= waitIndex.Value)
								waitIndex = child.ModifiedIndex + 1;

							// child node
						}
					}
				}
			}

			// watch the changes
			resp = await _etcdClient.WatchNodeAsync(key, recursive: true, waitIndex: waitIndex);
			if (resp != null && resp.Node != null)
			{
				waitIndex = resp.Node.ModifiedIndex + 1;

				if (resp.Node.Key.StartsWith(key, StringComparison.InvariantCultureIgnoreCase))
				{
					switch(resp.Action.ToLowerInvariant())
					{
						case EtcdResponse.ACTION_DELETE:
							break;
						case EtcdResponse.ACTION_EXPIRE:
							break;
						case EtcdResponse.ACTION_COMPARE_AND_DELETE:
							break;

						case EtcdResponse.ACTION_SET:
							break;
						case EtcdResponse.ACTION_CREATE:
							break;
						case EtcdResponse.ACTION_COMPARE_AND_SWAP:
							break;
						default:
							break;
					}
				}
			}
			continue;
		}
		catch(TaskCanceledException)
		{
			// time out, try again
		}
		catch(EtcdException ee)
		{
			// reset the waitIndex
			waitIndex = null;
		}
		catch (EtcdGenericException ege)
		{
			// etcd returns an error
		}
		catch (Exception ex)
		{
			// generic error
		}

		if (!_running) return;
		// something went wrong, delay 1 second and try again
		await Task.Delay(1000);
	}
}
```