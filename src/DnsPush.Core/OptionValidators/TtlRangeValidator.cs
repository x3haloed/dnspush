using System;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace DnsPush.Core.OptionValidators
{
    public class TtlRangeValidator : IOptionValidator
    {
        public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
        {
            // This validator only runs if there is a value
            if (!option.HasValue()) return ValidationResult.Success;
            int val = ((CommandOption<int>)option).ParsedValue;

            if (val < 60 || val > 60000)
            {
                return new ValidationResult($"The value for --{option.LongName} must be at least 60 and no greater than 60000");
            }

            return ValidationResult.Success;
        }
    }
}