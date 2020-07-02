using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace DnsPush.Core.OptionValidators
{
    public class ClientIpFormatValidator : IOptionValidator
    {
        public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
        {
            // This validator only runs if there is a value
            if (!option.HasValue()) return ValidationResult.Success;
            var val = option.Value();

            if (!IPAddress.TryParse(val, out IPAddress ip) || ip.AddressFamily != AddressFamily.InterNetwork)
            {
                return new ValidationResult($"The value for --{option.LongName} must be a valid IPv4 address");
            }

            return ValidationResult.Success;
        }
    }
}