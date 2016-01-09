using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Globalization;

namespace EtcdNet.DTO
{
    /// <summary>
    /// Represent a node in etcd
    /// </summary>
    [DataContract]
    public class EtcdNode
    {
        [DataMember(Name = "key")]
        public string Key { get; set; }

        [DataMember(Name = "dir")]
        public bool IsDirectory { get; set; }

        [DataMember(Name = "value")]
        public string Value { get; set; }

        [DataMember(Name = "createdIndex")]
        public long CreatedIndex { get; internal set; }

        [DataMember(Name = "modifiedIndex")]
        public long ModifiedIndex { get; internal set; }

        [DataMember(Name = "ttl")]
        public int TTL { get; internal set; }

        [DataMember(Name = "expiration")]
        public string Expiration { get; internal set; }

        /// <summary>
        /// Children nodes
        /// </summary>
        [DataMember(Name = "nodes")]
        public EtcdNode [] Nodes { get; set; }


        static readonly Regex TIME_REGEX = new Regex(
            @"^(?<year>\d{4,4})\-(?<month>\d{2,2})\-(?<day>\d{2,2})T(?<hour>\d{2,2})\:(?<minute>\d{2,2})\:(?<second>\d{2,2})"
            , RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.CultureInvariant);
        public DateTime GetExpirationTime()
        {
            if( !string.IsNullOrWhiteSpace(this.Expiration) )
            {
                bool isUtc = this.Expiration.EndsWith("Z");
                // 2016-01-09T06:34:56.168680746Z
                // 2013-12-04T12:01:21.874888581-08:00
                Match m = TIME_REGEX.Match(this.Expiration);
                if( m.Success )
                {
                    return new DateTime(int.Parse(m.Groups["year"].Value, CultureInfo.InvariantCulture)
                        , int.Parse(m.Groups["month"].Value, CultureInfo.InvariantCulture)
                        , int.Parse(m.Groups["day"].Value, CultureInfo.InvariantCulture)
                        , int.Parse(m.Groups["hour"].Value, CultureInfo.InvariantCulture)
                        , int.Parse(m.Groups["minute"].Value, CultureInfo.InvariantCulture)
                        , int.Parse(m.Groups["second"].Value, CultureInfo.InvariantCulture)
                        , isUtc ? DateTimeKind.Utc : DateTimeKind.Local
                        );
                }
            }
            return DateTime.MaxValue;
        }
    }
}
