using System;

namespace dnspush.Hosts.Namecheap
{
    public struct NamecheapOptions
    {
        public string ApiUser { get; set; }
        public string ApiKey { get; set; }
        public string Username { get; set; }
        public string ClientIp { get; set; }
    }
}