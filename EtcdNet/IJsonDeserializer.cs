#if NET45
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endif

namespace EtcdNet
{
    /// <summary>
    /// This interface allows to choose alternative JSON deserializer
    /// </summary>
    public interface IJsonDeserializer
    {
        /// <summary>
        /// Deserialize the json string
        /// </summary>
        /// <typeparam name="T">Expected type</typeparam>
        /// <param name="json">json string</param>
        /// <returns>deserialized json object</returns>
        T Deserialize<T>(string json);
    }
}
