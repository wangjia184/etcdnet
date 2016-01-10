using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtcdNet.Sample
{
    class ServiceStackJsonDeserializer : EtcdNet.IJsonDeserializer
    {
        public T Deserialize<T>(string json)
        {
            return ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(json);
        }
    }
}
