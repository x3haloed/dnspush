using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;

namespace DnsPush.Server.Models.Options
{
    public class NamecheapOptionsModel : IValidatableObject
    {
        public const string Namecheap = "Namecheap";

        [Required]
        [MaxLength(15)]
        public string ClientIp { get; set; }
        public bool Sandbox { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (!IPAddress.TryParse(ClientIp, out IPAddress ip) || ip.AddressFamily != AddressFamily.InterNetwork)
            {
                results.Add(new ValidationResult($"The value for \"{nameof(ClientIp)}\" must be a valid IPv4 address"));
            }

            if(results.Count == 0) {
                results.Add(ValidationResult.Success);
            }

            return results;
        }
    }
}