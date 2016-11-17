#if NET45
using System;
using System.Collections.Generic;
using System.Linq;
#endif
using System.Text;
#if NET45
using System.Threading.Tasks;
using System.Runtime.Serialization;
#endif
using System.Runtime.Serialization.Json;
using System.IO;

namespace EtcdNet
{
    /// <summary>
    /// DefaultJsonDeserializer takes use of DataContractJsonSerializer
    /// </summary>
    internal class DefaultJsonDeserializer : IJsonDeserializer
    {
        public T Deserialize<T>(string json)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var deserializer = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true,
                });
                return (T)deserializer.ReadObject(ms);
            }
        }
    }
}
