using System;

namespace DnsPush.Core.Hosts.Namecheap
{
    public struct NamecheapOptions
    {
        public string ApiUser { get; set; }
        public string ApiKey { get; set; }
        public string UserName { get; set; }
        public string ClientIp { get; set; }
        public bool IsSandbox { get; set; }
    }
}