using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
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
