using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dnspush.Hosts.Namecheap
{
    public class NamecheapHost : IHost<NamecheapOptions, NamecheapUpdateRecordOptions>, IDisposable
    {
        public string Key => "namecheap";
        public string DisplayName => "NameCheap";

        private readonly NamecheapOptions _options;

        private readonly HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("https://api.namecheap.com/xml.response"),
        };

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
            if (options.Username == null) {
                throw new ArgumentNullException(nameof(options.Username));
            }
            if (options.Username.Length < 1)
            {
                throw new ArgumentOutOfRangeException(
                    $"\"{nameof(options.Username)}\" must not be empty.",
                    nameof(options.Username));
            }

            // ClientIp
            if (options.ClientIp == null) {
                throw new ArgumentNullException(nameof(options.ClientIp));
            }
            if (!IPAddress.TryParse(options.ClientIp, out _))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(options.ClientIp),
                    $"\"{nameof(options.ClientIp)}\" must be a valid IP address.");
            }

            _options = options;
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
            var content = new StringContent("test", Encoding.UTF8);
            var response = await _httpClient.PostAsync("", content, cancellationToken);
            

            return false;
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