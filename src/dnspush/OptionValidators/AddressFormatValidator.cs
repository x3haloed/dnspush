using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace dnspush.OptionValidators
{
    public class AddressFormatValidator : IOptionValidator
    {
        public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
        {
            // This validator only runs if there is a value
            if (!option.HasValue()) return ValidationResult.Success;
            var val = option.Value();

            bool isValidIpAddress = IPAddress.TryParse(val, out _);
            if (!isValidIpAddress && !Uri.TryCreate(val, UriKind.Absolute, out _))
            {
                return new ValidationResult($"The value for --{option.LongName} must be either a valid IP address or URL");
            }

            return ValidationResult.Success;
        }
    }
}