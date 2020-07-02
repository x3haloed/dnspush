using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Serilog;

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
            Log.Debug("{ClassName} constructor called.", nameof(NamecheapHost));

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

            Log.Debug("Host options configured: {options}", options);
            Log.Debug("Host options configured.");
        }

        public async Task<bool> UpdateRecordAsync(NamecheapUpdateRecordOptions options, CancellationToken cancellationToken)
        {
            Log.Debug("{MethodName} called.", nameof(UpdateRecordAsync));

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

            Log.Debug("Update options validated: {options}", options);

            // Retrieve existing list of records
            Log.Information("Retrieving existing list of DNS records...");
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
                Log.Information("DNS records response received: {status}", response.StatusCode);
                
                if (!response.IsSuccessStatusCode)
                {
                    Log.Error("DNS records request failed with status: {status}", response.StatusCode);
                    return false;
                }

                responseDocument = XDocument.Parse(await response.Content.ReadAsStringAsync());
                Log.Information("DNS records parsed.");
                Log.Debug("{document}", responseDocument);
            }

            XElement docRoot = responseDocument.Root;

            //check for success
            string responseStatus = docRoot.Attribute("Status").Value;
            Log.Debug("DNS records response status: {status}", responseStatus);

            var defaultNs = docRoot.GetDefaultNamespace();

            if ("ERROR".Equals(responseStatus, StringComparison.OrdinalIgnoreCase))
            {
                Log.Error("DNS records request failed: {errors}", docRoot.Descendants(defaultNs.GetName("Error")));
                Log.Information("DNS records request failed. Quitting with failure status.");
                return false;
            }
            else if (!"OK".Equals(responseStatus, StringComparison.OrdinalIgnoreCase))
            {
                Log.Error("DNS records request resulted in an unexpected status: {response}", responseDocument);
                Log.Information("DNS records request failed. Quitting with failure status.");
                return false;
            }

            Log.Information("Finding host record to update...");
            var hosts = docRoot.Descendants(defaultNs.GetName("Host"));
            var hostToUpdate = hosts.Single(e =>
                options.HostName.Equals(e.Attribute("Name").Value, StringComparison.OrdinalIgnoreCase) &&
                options.RecordType.Equals(e.Attribute("Type").Value, StringComparison.OrdinalIgnoreCase));
            Log.Information("Host record found.");
            Log.Debug("{record}", hostToUpdate);
            hosts = hosts.Except(new [] { hostToUpdate });

            // update host record
            Log.Information("Composing updated host record...");
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
            Log.Information("Writing record update...");
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
            Log.Debug("{message}", formContent);
            using (HttpResponseMessage response = await _httpClient.PostAsync("", formContent, cancellationToken))
            {
                Log.Information("DNS update response received: {status}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Error("DNS update request failed with status: {status}", response.StatusCode);
                    return false;
                }

                responseDocument = XDocument.Parse(await response.Content.ReadAsStringAsync());
                Log.Information("DNS update response parsed.");
                Log.Debug("{document}", responseDocument);
            }

            docRoot = responseDocument.Root;

            responseStatus = docRoot.Attribute("Status").Value;
            Log.Debug("DNS update response status: {status}", responseStatus);

            if ("ERROR".Equals(responseStatus, StringComparison.OrdinalIgnoreCase))
            {
                Log.Error("DNS update request failed: {errors}", docRoot.Descendants(defaultNs.GetName("Error")));
                Log.Information("DNS update request failed. Quitting with failure status.");
                return false;
            }
            else if (!"OK".Equals(responseStatus, StringComparison.OrdinalIgnoreCase))
            {
                Log.Error("DNS update request resulted in an unexpected status: {response}", responseDocument);
                Log.Information("DNS update request failed. Quitting with failure status.");
                return false;
            }

            return true;
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