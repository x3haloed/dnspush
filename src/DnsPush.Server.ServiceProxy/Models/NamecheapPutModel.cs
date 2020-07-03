using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;

namespace  DnsPush.Server.ServiceProxy.Models
{
    public class NamecheapPutModel : IValidatableObject
    {
        [Required]
        [MaxLength(20)]
        public string ApiUser { get; set; }

        [Required]
        [MaxLength(50)]
        public string ApiKey { get; set; }

        [MaxLength(20)]
        public string UserName { get; set; }

        [Required]
        public string Address { get; set; }

        [MinLength(60)]
        [MaxLength(60000)]
        public int? Ttl { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            bool isValidIpAddress = IPAddress.TryParse(Address, out _);
            if (!isValidIpAddress && !Uri.TryCreate(Address, UriKind.Absolute, out _))
            {
                results.Add(new ValidationResult($"The value for \"{nameof(Address)}\" must be either a valid IP address or URL"));
            }

            if(results.Count == 0) {
                results.Add(ValidationResult.Success);
            }

            return results;
        }
    }
}