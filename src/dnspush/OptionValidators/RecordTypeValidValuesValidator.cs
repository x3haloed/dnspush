using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace dnspush.OptionValidators
{
    public class RecordTypeValidValuesValidator : IOptionValidator
    {
        public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
        {
            // This validator only runs if there is a value
            if (!option.HasValue()) return ValidationResult.Success;
            var val = option.Value();

            if (! new[] {"A", "CNAME"}.Contains(val))
            {
                return new ValidationResult($"The value for --{option.LongName} must be one of the following: A, CNAME.");
            }

            return ValidationResult.Success;
        }
    }
}