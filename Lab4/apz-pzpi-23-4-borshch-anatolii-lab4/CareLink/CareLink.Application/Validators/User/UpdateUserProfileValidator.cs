using CareLink.Application.Dtos.User;
using FluentValidation;

namespace CareLink.Application.Validators.User
{
    public class UpdateUserProfileValidator : AbstractValidator<UpdateUserProfileRequest>
    {
        public UpdateUserProfileValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(200).WithMessage("Email cannot exceed 200 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(50).WithMessage("Phone number cannot exceed 50 characters.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required.")
                .Must(BeAValidAge).WithMessage("User must be between 18 and 120 years old.");
        }

        private bool BeAValidAge(DateTime dateOfBirth)
        {
            var age = DateTime.UtcNow.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > DateTime.UtcNow.AddYears(-age)) age--;
            return age >= 18 && age <= 120;
        }
    }
}