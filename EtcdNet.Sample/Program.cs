using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using EtcdNet;


namespace EtcdNet.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DoSample().Wait();
            }
            catch (Exception ex)
            {
                int indentation = 0;
                while(ex != null) {
                    if (!(ex is AggregateException)) {
                        Console.WriteLine("{0} {1} : {2}", string.Empty.PadLeft(indentation), ex.GetType().Name, ex.Message);
                    }
                    indentation += 2;
                    ex = ex.InnerException;
                }
            }
            Console.WriteLine("Press any key to exist.");
            Console.ReadKey();
        }

        static async Task DoSample()
        {
            EtcdClientOpitions options = new EtcdClientOpitions() {
                Urls = new string[] { "https://etcd0.em", "https://etcd1.em", "https://etcd2.em" },
                Username = "root",
                Password = "654321",
                IgnoreCertificateError = true, // If the ectd server is running with self-signed SSL certificate and we can ignore the SSL error
                //X509Certificate = new X509Certificate2(@"client.p12"),  // client cerificate
                JsonDeserializer = new NewtonsoftJsonDeserializer(),
            };
            EtcdClient etcdClient = new EtcdClient(options);

            

            string key = "/my/key";
            string value;
            // query the value of the node using GetNodeValueAsync
            try {
                value = await etcdClient.GetNodeValueAsync(key);
                Console.WriteLine("The value of `{0}` is `{1}`", key, value);
            }
            catch(EtcdCommonException.KeyNotFound) {
                Console.WriteLine("Key `{0}` does not exist", key);
            }
            

            // update the value using SetNodeAsync
            EtcdResponse resp = await etcdClient.SetNodeAsync(key, "some value");
            Console.WriteLine("Key `{0}` is changed, modifiedIndex={1}", key, resp.Node.ModifiedIndex);

            // query the node using GetNodeAsync
            resp = await etcdClient.GetNodeAsync(key, ignoreKeyNotFoundException: true);
            if (resp == null || resp.Node == null)
                Console.WriteLine("Key `{0}` does not exist", key);
            else
                Console.WriteLine("The value of `{0}` is `{1}`", key, resp.Node.Value);

            //////////////////////////////////////////////////////////

            List<Task<EtcdResponse>> tasks = new List<Task<EtcdResponse>>();
            key = "/in-order-queue";

            // start monitoring the expire event
            WatchChanges(etcdClient, key);

            // create 5 in-order nodes, TTL = 3 second
            for (int i = 0; i < 5; i++) {
                tasks.Add( etcdClient.CreateInOrderNodeAsync(key, i.ToString(), ttl: 3) );
            }

            await Task.WhenAll(tasks);

            // list the in-order nodes
            resp = await etcdClient.GetNodeAsync(key, false, recursive: true, sorted:true);
            if (resp.Node.Nodes != null) {
                foreach (var node in resp.Node.Nodes)
                {
                    Console.WriteLine("`{0}` = {1}", node.Key, node.Value);
                }
            }


            /////////////////////////////////////////////////////////////
            key = "/my/cas-test";
            value = Guid.NewGuid().ToString();
            try {
                resp = await etcdClient.CreateNodeAsync(key, value, null, dir : false);
                Console.WriteLine("Key `{0}` is created with value {1}", key, value);
            }
            catch (EtcdCommonException.NodeExist) {
                Console.WriteLine("Key `{0}` already exists", key);
                
            }

            long prevIndex = 1;
            try {
                resp = await etcdClient.CompareAndSwapNodeAsync(key, value, "new value");
                Console.WriteLine("Key `{0}` is updated to `{1}`", key, resp.Node.Value);

                prevIndex = resp.Node.ModifiedIndex;
            }
            catch (EtcdCommonException.KeyNotFound) {
                Console.WriteLine("Key `{0}` does not exists", key);
            }
            catch (EtcdCommonException.TestFailed) {
                Console.WriteLine("Key `{0}` can not be updated because the supplied previous value is incorrect", key);
            }

            try {
                resp = await etcdClient.CompareAndSwapNodeAsync(key, prevIndex, "new value2");
                Console.WriteLine("Key `{0}` is updated to `{1}`", key, resp.Node.Value);
            }
            catch (EtcdCommonException.KeyNotFound) {
                Console.WriteLine("Key `{0}` does not exists", key);
            }
            catch (EtcdCommonException.TestFailed) {
                Console.WriteLine("Key `{0}` can not be updated because the supplied previous index is incorrect", key);
            }

            

            try {
                resp = await etcdClient.CompareAndDeleteNodeAsync(key, prevIndex+1);
                Console.WriteLine("Key `{0}` is deleted", key);
            }
            catch (EtcdCommonException.KeyNotFound) {
                Console.WriteLine("Key `{0}` does not exists", key);
            }
            catch (EtcdCommonException.TestFailed) {
                Console.WriteLine("Key `{0}` can not be deleted because the supplied previous index is incorrect", key);
            }

            if (prevIndex == 1) // the previous CAS failed
            {
                try {
                    resp = await etcdClient.CompareAndDeleteNodeAsync(key, "new value2");
                    Console.WriteLine("Key `{0}` is deleted", key);
                }
                catch (EtcdCommonException.KeyNotFound) {
                    Console.WriteLine("Key `{0}` does not exists", key);
                }
                catch (EtcdCommonException.TestFailed) {
                    Console.WriteLine("Key `{0}` can not be deleted because the supplied previous value is incorrect", key);
                    etcdClient.DeleteNodeAsync(key, ignoreKeyNotFoundException: true).Wait();
                }
            }
            
        }


        static async void WatchChanges(EtcdClient etcdClient, string key)
        {
            long waitIndex = etcdClient.LastIndex;
            for (; ; )
            {
                try
                {
                    EtcdResponse resp = await etcdClient.WatchNodeAsync(key, recursive: true, waitIndex: waitIndex);
                    if (resp != null && resp.Node != null)
                    {
                        if( string.Equals( resp.Action, EtcdResponse.ACTION_EXPIRE) )
                            Console.WriteLine("`{0}` is expired", resp.Node.Key);

                        waitIndex = resp.Node.ModifiedIndex + 1;
                    }
                    continue;
                }
                catch(Exception ex)
                {
                    int indentation = 0;
                    while (ex != null)
                    {
                        if (!(ex is AggregateException))
                        {
                            Console.WriteLine("{0} {1} : {2}", string.Empty.PadLeft(indentation), ex.GetType().Name, ex.Message);
                        }
                        indentation += 2;
                        ex = ex.InnerException;
                    }
                }
                // if something went wrong, wait for 1 second and try again
                await Task.Delay(1000);
            }
        }
    }
}
