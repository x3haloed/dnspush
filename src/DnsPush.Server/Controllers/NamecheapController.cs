using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DnsPush.Server.ServiceProxy.Models;
using DnsPush.Core.Hosts.Namecheap;
using System.Threading;
using System.Net;
using DnsPush.Server.Models.Options;
using Microsoft.Extensions.Options;
using DnsPush.Core.Hosts;

namespace DnsPush.Server.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("namecheap")]
    public class NamecheapController : ControllerBase
    {
        public NamecheapController(IOptions<NamecheapOptionsModel> options, ILogger<NamecheapController> logger)
        {
            logger.LogDebug("{ClassName} constructor called.", nameof(NamecheapController));
            _options = options.Value;
            _logger = logger;
        }

        private readonly NamecheapOptionsModel _options;
        private readonly ILogger<NamecheapController> _logger;

        [HttpPut("{sld}/{tld}/{hostName}/{recordType}")]
        public async Task<ActionResult<PutResultModel>> PutAsync(
            [FromRoute, Required, MaxLength(70)] string sld,
            [FromRoute, Required, MaxLength(10)] string tld,
            [FromRoute, Required] string hostName,
            [FromRoute, Required, RegularExpression("^A|CNAME$")] string recordType,
            [FromBody] NamecheapPutModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var hostOptions = new NamecheapOptions
            {
                ApiUser = model.ApiUser,
                ApiKey = model.ApiKey,
                UserName = string.IsNullOrEmpty(model.UserName) ? model.ApiUser : model.UserName,
                ClientIp = _options.ClientIp,
                IsSandbox = _options.Sandbox,
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

            UpdateRecordResult updateResult = await host.UpdateRecordAsync(updateOptions, cancellationToken);
            _logger.LogDebug("Update completed with status: {status}.", updateResult.Success);

            if (updateResult.Success)
            {
                return Ok(new PutResultModel
                {
                    Success = true,
                });
            }

            return new ContentResult
            {
                Content = JsonSerializer.Serialize(new PutResultModel
                {
                    Success = false,
                    Errors = updateResult.Errors,
                }),
                ContentType = "application/json",
                StatusCode = (int)HttpStatusCode.InternalServerError,
            };
        }
    }
}
