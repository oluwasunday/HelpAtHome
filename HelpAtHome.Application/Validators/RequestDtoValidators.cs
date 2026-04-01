using FluentValidation;
using HelpAtHome.Core.DTOs.Requests;

namespace HelpAtHome.Application.Validators
{
    public class UpdateCaregiverProfileDtoValidator : AbstractValidator<UpdateCaregiverProfileDto>
    {
        public UpdateCaregiverProfileDtoValidator()
        {
            RuleFor(x => x.Bio).MaximumLength(2000).When(x => x.Bio != null);
            RuleFor(x => x.YearsOfExperience).InclusiveBetween(0, 50).When(x => x.YearsOfExperience.HasValue);
            RuleFor(x => x.HourlyRate).GreaterThan(0).When(x => x.HourlyRate.HasValue);
            RuleFor(x => x.DailyRate).GreaterThan(0).When(x => x.DailyRate.HasValue);
            RuleFor(x => x.MonthlyRate).GreaterThan(0).When(x => x.MonthlyRate.HasValue);
        }
    }

    public class CreateTicketDtoValidator : AbstractValidator<CreateTicketDto>
    {
        public CreateTicketDtoValidator()
        {
            RuleFor(x => x.Subject).NotEmpty().MaximumLength(500);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        }
    }

    public class AddTicketMessageDtoValidator : AbstractValidator<AddTicketMessageDto>
    {
        public AddTicketMessageDtoValidator()
        {
            RuleFor(x => x.Message).NotEmpty().MaximumLength(3000);
        }
    }

    public class TriggerAlertDtoValidator : AbstractValidator<TriggerAlertDto>
    {
        public TriggerAlertDtoValidator()
        {
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90)
                .WithMessage("Latitude must be between -90 and 90");
            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180)
                .WithMessage("Longitude must be between -180 and 180");
            RuleFor(x => x.Address).MaximumLength(500).When(x => x.Address != null);
            RuleFor(x => x.Message).MaximumLength(1000).When(x => x.Message != null);
        }
    }

    public class InviteFamilyMemberDtoValidator : AbstractValidator<InviteFamilyMemberDto>
    {
        public InviteFamilyMemberDtoValidator()
        {
            RuleFor(x => x.PhoneOrEmail).NotEmpty().MaximumLength(200)
                .Must(v => v.Contains('@') || System.Text.RegularExpressions.Regex.IsMatch(v, @"^\+?[0-9]{7,15}$"))
                .WithMessage("Must be a valid email address or phone number");
        }
    }
}
