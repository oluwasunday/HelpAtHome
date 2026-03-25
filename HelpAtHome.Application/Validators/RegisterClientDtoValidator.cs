using FluentValidation;
using HelpAtHome.Core.DTOs.Requests;
using HelpAtHome.Core.DTOs.Requests.Auth;
using HelpAtHome.Core.DTOs.Responses;

namespace HelpAtHome.Application.Validators
{
    public class RegisterClientDtoValidator : AbstractValidator<RegisterClientDto>
    {
        public RegisterClientDtoValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
            RuleFor(x => x.PhoneNumber).NotEmpty()
                .Matches(@"^\+234[0-9]{10}$")
                .WithMessage("Phone must be a valid Nigerian number: +234XXXXXXXXXX");
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter")
                .Matches("[0-9]").WithMessage("Password must contain a digit");
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password)
                .WithMessage("Passwords do not match");
        }
    }

    // CreateBookingDtoValidator.cs
    public class CreateBookingDtoValidator : AbstractValidator<CreateBookingDto>
    {
        public CreateBookingDtoValidator()
        {
            RuleFor(x => x.CaregiverProfileId).NotEmpty();
            RuleFor(x => x.ServiceCategoryId).NotEmpty();
            RuleFor(x => x.ScheduledStartDate)
                .GreaterThan(DateTime.UtcNow.AddHours(1))
                .WithMessage("Booking must start at least 1 hour from now");
            RuleFor(x => x.ScheduledEndDate)
                .GreaterThan(x => x.ScheduledStartDate)
                .WithMessage("End date must be after start date");
            RuleFor(x => x.SpecialInstructions).MaximumLength(2000);
        }
    }

    // CreateReviewDtoValidator.cs
    public class CreateReviewDtoValidator : AbstractValidator<CreateReviewDto>
    {
        public CreateReviewDtoValidator()
        {
            RuleFor(x => x.BookingId).NotEmpty();
            RuleFor(x => x.Rating).InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5");
            RuleFor(x => x.Comment).MaximumLength(2000);
        }
    }

    // WithdrawalDtoValidator.cs
    public class WithdrawalDtoValidator : AbstractValidator<WithdrawalDto>
    {
        public WithdrawalDtoValidator()
        {
            RuleFor(x => x.Amount).GreaterThanOrEqualTo(2000)
                .WithMessage("Minimum withdrawal amount is ₦2,000");
            RuleFor(x => x.AccountNumber).NotEmpty().Length(10)
                .WithMessage("Account number must be exactly 10 digits");
            RuleFor(x => x.BankName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.AccountName).NotEmpty().MaximumLength(200);
        }
    }

}
