using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PRN232.LMS.Services.Validators
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class FptuRollNumberAttribute : ValidationAttribute
    {
        private static readonly Regex FptuRegex = new(@"^(SE|CE|IA|HE|HS|SS|MC|GD)\d{5,}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public FptuRollNumberAttribute() : base("Student roll number must be in FPTU format (e.g. SE19886, CE18793).")
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success; // [Required] will handle empty/null
            }

            var strValue = value.ToString()!.Trim();
            if (!FptuRegex.IsMatch(strValue))
            {
                return new ValidationResult(ErrorMessageString);
            }

            return ValidationResult.Success;
        }
    }
}
