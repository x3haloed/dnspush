using System;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace DnsPush.Core.OptionValidators
{
    public class SldLengthValidator : IOptionValidator
    {
        public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
        {
            // This validator only runs if there is a value
            if (!option.HasValue()) return ValidationResult.Success;
            var val = option.Value();

            if (val.Length < 1)
            {
                return new ValidationResult($"The value for --{option.LongName} must not be empty");
            }
            if (val.Length > 70)
            {
                return new ValidationResult($"The value for --{option.LongName} must be 70 characters in length or less");
            }

            return ValidationResult.Success;
        }
    }
}