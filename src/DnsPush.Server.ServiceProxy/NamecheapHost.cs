using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using DnsPush.Server.ServiceProxy.Models;

namespace DnsPush.Server.ServiceProxy
{
    public class NamecheapHost : IDisposable
    {
        public NamecheapHost()
        {
        }

        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<PutResultModel> UpdateRecordAsync(
            string apiUser,
            string apiKey,
            string sld,
            string tld,
            string hostName,
            string recordType,
            string address,
            string userName = null,
            int? ttl = null,
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PutAsJsonAsync(
                $"{sld}/{tld}/{hostName}/{recordType}",
                new NamecheapPutModel
                {
                    ApiUser = apiUser,
                    ApiKey = apiKey,
                    UserName = userName,
                    Address = address,
                    Ttl = ttl,
                },
                cancellationToken);

            return await response.Content.ReadFromJsonAsync<PutResultModel>(cancellationToken: cancellationToken);
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
