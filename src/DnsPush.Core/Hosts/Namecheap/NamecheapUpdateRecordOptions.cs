using System;

namespace DnsPush.Core.Hosts.Namecheap
{
    public struct NamecheapUpdateRecordOptions
    {
        public string Sld {get; set; }
        public string Tld { get; set; }
        public string HostName { get; set; }
        public string RecordType { get; set; }
        public string Address { get; set; }
        public int? Ttl { get; set; }
    }
}