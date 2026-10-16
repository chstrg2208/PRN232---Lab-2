using System;
using FluentValidation;
using PRN232.LMS.Services.Models.Requests;

namespace PRN232.LMS.Services.Validators
{
    public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
    {
        public CreateStudentRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("FullName is required.")
                .Length(2, 100).WithMessage("FullName must be between 2 and 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("DateOfBirth is required.")
                .LessThan(DateTime.Today.AddYears(-10))
                .WithMessage("Student must be at least 10 years old.");

            When(x => x.Credit.HasValue, () =>
            {
                RuleFor(x => x.Credit!.Value)
                    .InclusiveBetween(0, 200)
                    .WithMessage("Credit must be between 0 and 200.");
            });

            When(x => !string.IsNullOrEmpty(x.RollNumber), () =>
            {
                RuleFor(x => x.RollNumber)
                    .Matches(@"^(SE|CE|IA|HE|HS|SS|MC|GD)\d{5,}$")
                    .WithMessage("RollNumber must be in valid FPTU format (e.g. SE19886, CE18793).");
            });

            When(x => !string.IsNullOrEmpty(x.PhoneNumber), () =>
            {
                RuleFor(x => x.PhoneNumber)
                    .Matches(@"^0\d{9,10}$")
                    .WithMessage("PhoneNumber must be a valid 10-11 digit phone number starting with 0.");
            });
        }
    }
}
