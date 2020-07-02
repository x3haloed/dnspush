using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace dnspush.Hosts.Namecheap
{
    public class NamecheapHost : IHost<NamecheapOptions, NamecheapUpdateRecordOptions>, IDisposable
    {
        public string Key => "namecheap";
        public string DisplayName => "NameCheap";

        private readonly NamecheapOptions _options;

        private readonly HttpClient _httpClient = new HttpClient();

        public NamecheapHost(NamecheapOptions options)
        {
            // validation guards
            // ApiUser
            if (options.ApiUser == null) {
                throw new ArgumentNullException(nameof(options.ApiUser));
            }
            if (options.ApiUser.Length < 1)
            {
                throw new ArgumentOutOfRangeException(
                    $"\"{nameof(options.ApiUser)}\" must not be empty.",
                    nameof(options.ApiUser));
            }

            // ApiKey
            if (options.ApiKey == null) {
                throw new ArgumentNullException(nameof(options.ApiKey));
            }
            if (options.ApiKey.Length < 1)
            {
                throw new ArgumentOutOfRangeException(
                    $"\"{nameof(options.ApiKey)}\" must not be empty.",
                    nameof(options.ApiKey));
            }

            // Username
            if (options.UserName == null) {
                throw new ArgumentNullException(nameof(options.UserName));
            }
            if (options.UserName.Length < 1)
            {
                throw new ArgumentOutOfRangeException(
                    $"\"{nameof(options.UserName)}\" must not be empty.",
                    nameof(options.UserName));
            }

            // ClientIp
            if (options.ClientIp == null) {
                throw new ArgumentNullException(nameof(options.ClientIp));
            }
            if (!IPAddress.TryParse(options.ClientIp, out IPAddress ip) || ip.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(options.ClientIp),
                    $"\"{nameof(options.ClientIp)}\" must be a valid IPv4 address.");
            }

            _options = options;

            // set up API address
            _httpClient.BaseAddress = options.IsSandbox
                ? new Uri("https://api.sandbox.namecheap.com/xml.response")
                : new Uri("https://api.namecheap.com/xml.response");
        }

        public async Task<bool> UpdateRecordAsync(NamecheapUpdateRecordOptions options, CancellationToken cancellationToken)
        {
            // validation guards
            // SLD
            if (options.Sld == null) {
                throw new ArgumentNullException(nameof(options.Sld));
            }
            if (options.Sld.Length < 1 || options.Sld.Length > 70)
            {
                throw new ArgumentException(
                    $"\"{nameof(options.Sld)}\" must be at least one character and no more than 70 characters in length.",
                    nameof(options.Sld));
            }

            // TLD
            if (options.Tld == null) {
                throw new ArgumentNullException(nameof(options.Tld));
            }
            if (options.Tld.Length < 1 || options.Tld.Length > 10)
            {
                throw new ArgumentException(
                    $"\"{nameof(options.Tld)}\" must be at least 1 character and no more than 10 characters in length.",
                    nameof(options.Tld));
            }

            // HostName
            if (options.HostName == null) {
                throw new ArgumentNullException(nameof(options.HostName));
            }
            if (options.HostName.Length < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(options.HostName),
                    $"\"{nameof(options.HostName)}\" must not be empty.");
            }

            // RecordType
            if (options.RecordType == null) {
                throw new ArgumentNullException(nameof(options.RecordType));
            }
            if (! new[] {"A", "CNAME"}.Contains(options.RecordType))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(options.RecordType),
                    $"\"{nameof(options.RecordType)}\" must be one of the following: A, CNAME.");
            }

            // Address
            if (options.Address == null) {
                throw new ArgumentNullException(nameof(options.Address));
            }
            if (!Uri.TryCreate(options.Address, UriKind.Absolute, out _))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(options.Address),
                    $"\"{nameof(options.Address)}\" must be either a valid IP address or URL.");
            }

            // Retrieve existing list of records
            var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "ApiUser", _options.ApiUser },
                { "ApiKey", _options.ApiKey },
                { "UserName", _options.UserName },
                { "Command", "namecheap.domains.dns.getHosts" },
                { "ClientIp", _options.ClientIp },
                { "SLD", options.Sld },
                { "TLD", options.Tld },
            });
            XDocument responseDocument;
            using (HttpResponseMessage response = await _httpClient.PostAsync("", formContent, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                responseDocument = XDocument.Parse(await response.Content.ReadAsStringAsync());
            }
            var hosts = responseDocument.Root.Descendants("Host");
            var hostToUpdate = hosts.Single(e =>
                options.HostName.Equals(e.Attribute("Name").Value, StringComparison.OrdinalIgnoreCase) &&
                options.RecordType.Equals(e.Attribute("Type").Value, StringComparison.OrdinalIgnoreCase));
            hosts = hosts.Except(new [] { hostToUpdate });

            // update host record
            hostToUpdate.SetAttributeValue("Address", options.Address);
            if (options.Ttl.HasValue)
            {
                hostToUpdate.SetAttributeValue("TTL", options.Ttl.Value);
            }

            var newHosts = hosts
                .Append(hostToUpdate)
                .SelectMany(h => new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("HostName", h.Attribute("Name").Value),
                    new KeyValuePair<string, string>("RecordType", h.Attribute("Type").Value),
                    new KeyValuePair<string, string>("Address", h.Attribute("Address").Value),
                    new KeyValuePair<string, string>("TTL", h.Attribute("TTL").Value),
                });

            // send update request
            formContent = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("ApiUser", _options.ApiUser),
                new KeyValuePair<string, string>("ApiKey", _options.ApiKey),
                new KeyValuePair<string, string>("UserName", _options.UserName),
                new KeyValuePair<string, string>("Command", "namecheap.domains.dns.setHosts"),
                new KeyValuePair<string, string>("ClientIp", _options.ClientIp),
                new KeyValuePair<string, string>("SLD", options.Sld),
                new KeyValuePair<string, string>("TLD", options.Tld),
            }.Concat(newHosts));
            using (HttpResponseMessage response = await _httpClient.PostAsync("", formContent, cancellationToken))
            {
                return response.IsSuccessStatusCode;
            }
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient.Dispose();
            }
        }
    }
}