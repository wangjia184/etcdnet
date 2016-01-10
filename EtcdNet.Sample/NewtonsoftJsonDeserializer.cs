using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtcdNet.Sample
{
    class NewtonsoftJsonDeserializer : EtcdNet.IJsonDeserializer
    {
        public T Deserialize<T>(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
    }
}
