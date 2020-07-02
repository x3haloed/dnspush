using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DnsPush.Server.Models;
using DnsPush.Core.Hosts.Namecheap;

namespace DnsPush.Server.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("namecheap")]
    public class NamecheapController : ControllerBase
    {
        public NamecheapController(ILogger<NamecheapController> logger)
        {
            logger.LogDebug("{ClassName} constructor called.", nameof(NamecheapController));
            _logger = logger;
        }

        private readonly ILogger<NamecheapController> _logger;

        [HttpPatch("{sld}/{tld}/{hostName}/{recordType}")]
        public async Task PatchAsync(
            [FromRoute, Required, MaxLength(70)] string sld,
            [FromRoute, Required, MaxLength(10)] string tld,
            [FromRoute, Required] string hostName,
            [FromRoute, Required, RegularExpression("^A|CNAME$")] string recordType,
            [FromBody] NamecheapPatchModel model)
        {
            if (!ModelState.IsValid)
            {
                return ModelState;
            }

            var hostOptions = new NamecheapOptions
            {
                ApiUser = model.ApiUser,
                ApiKey = model.ApiKey,
                UserName = string.IsNullOrEmpty(model.UserName) ? model.ApiUser : model.UserName,
                ClientIp = ,
                IsSandbox = ,
            };
            _logger.LogInformation("Host options configured.");
            _logger.LogDebug("{options}", hostOptions);
            using var host = new NamecheapHost(hostOptions);
            _logger.LogInformation("Host created");

            var updateOptions = new NamecheapUpdateRecordOptions
            {
                Sld = sld,
                Tld = tld,
                HostName = hostName,
                RecordType = recordType,
                Address = model.Address,
                Ttl = model.Ttl.HasValue ? model.Ttl.Value : default,
            };
            _logger.LogInformation("Update options configured.");
            _logger.LogDebug("{options}", updateOptions);

            bool updateSuccess = await host.UpdateRecordAsync(updateOptions, cancellationToken);
            _logger.LogDebug("Update completed with status: {status}.", updateSuccess);

            int appStatusCode = updateSuccess ? 0 : 1;
            _logger.LogDebug("Patch execution complete. Exiting with status code: {status}", appStatusCode);
            return appStatusCode;
        }
    }
}
