using FluentValidation;
using HelpAtHome.Core.DTOs.Requests.Auth;

namespace HelpAtHome.Application.Validators
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
            RuleFor(x => x.Password).NotEmpty().MaximumLength(200);
        }
    }

    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
            RuleFor(x => x.Token).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter")
                .Matches("[0-9]").WithMessage("Password must contain a digit");
            RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword)
                .WithMessage("Passwords do not match");
        }
    }

    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.OldPassword).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter")
                .Matches("[0-9]").WithMessage("Password must contain a digit");
            RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword)
                .WithMessage("Passwords do not match");
        }
    }

    public class VerifyOtpDtoValidator : AbstractValidator<VerifyOtpDto>
    {
        public VerifyOtpDtoValidator()
        {
            RuleFor(x => x.Otp).NotEmpty().Length(6)
                .Matches(@"^\d{6}$").WithMessage("OTP must be a 6-digit number");
        }
    }

    public class RegisterCaregiverDtoValidator : AbstractValidator<RegisterCaregiverDto>
    {
        public RegisterCaregiverDtoValidator()
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
            RuleFor(x => x.HourlyRate).GreaterThan(0).When(x => x.HourlyRate > 0);
            RuleFor(x => x.DailyRate).GreaterThan(0).When(x => x.DailyRate > 0);
            RuleFor(x => x.WeeklyRate).GreaterThan(0).When(x => x.WeeklyRate > 0);
            RuleFor(x => x.MonthlyRate).GreaterThan(0).When(x => x.MonthlyRate > 0);
            RuleFor(x => x.YearsOfExperience).InclusiveBetween(0, 50);
            RuleFor(x => x.Bio).MaximumLength(2000).When(x => x.Bio != null);
            RuleFor(x => x.Address).NotNull().SetValidator(new AddressUpsertDtoValidator());
        }
    }

    public class RegisterAgencyAdminDtoValidator : AbstractValidator<RegisterAgencyAdminDto>
    {
        public RegisterAgencyAdminDtoValidator()
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
            RuleFor(x => x.AgencyName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.RegistrationNumber).NotEmpty().MaximumLength(100);
            RuleFor(x => x.AgencyEmail).NotEmpty().EmailAddress().MaximumLength(200);
            RuleFor(x => x.AgencyPhone).NotEmpty()
                .Matches(@"^\+234[0-9]{10}$")
                .WithMessage("Agency phone must be a valid Nigerian number: +234XXXXXXXXXX");
            RuleFor(x => x.AgencyDescription).MaximumLength(2000).When(x => x.AgencyDescription != null);
            RuleFor(x => x.Website).Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Website must be a valid URL")
                .When(x => !string.IsNullOrEmpty(x.Website));
            RuleFor(x => x.AgencyAddress).NotNull().SetValidator(new AddressUpsertDtoValidator());
        }
    }

    public class RegisterAgencyCaregiverDtoValidator : AbstractValidator<RegisterAgencyCaregiverDto>
    {
        public RegisterAgencyCaregiverDtoValidator()
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
            RuleFor(x => x.HourlyRate).GreaterThan(0).When(x => x.HourlyRate > 0);
            RuleFor(x => x.DailyRate).GreaterThan(0).When(x => x.DailyRate > 0);
            RuleFor(x => x.MonthlyRate).GreaterThan(0).When(x => x.MonthlyRate > 0);
            RuleFor(x => x.YearsOfExperience).InclusiveBetween(0, 50);
            RuleFor(x => x.Bio).MaximumLength(2000).When(x => x.Bio != null);
            RuleFor(x => x.Address).NotNull().SetValidator(new AddressUpsertDtoValidator());
        }
    }
}
