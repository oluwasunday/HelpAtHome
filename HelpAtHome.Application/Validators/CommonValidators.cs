using FluentValidation;
using HelpAtHome.Core.DTOs.Common;

namespace HelpAtHome.Application.Validators
{
    public class AddressUpsertDtoValidator : AbstractValidator<AddressUpsertDto>
    {
        public AddressUpsertDtoValidator()
        {
            RuleFor(x => x.Line1).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Line2).MaximumLength(200).When(x => x.Line2 != null);
            RuleFor(x => x.Locality).NotEmpty().MaximumLength(100);
            RuleFor(x => x.City).MaximumLength(100).When(x => x.City != null);
            RuleFor(x => x.LGA).NotEmpty().MaximumLength(100);
            RuleFor(x => x.State).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Country).MaximumLength(100).When(x => x.Country != null);
            RuleFor(x => x.PostalCode).MaximumLength(20).When(x => x.PostalCode != null);
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90)
                .When(x => x.Latitude.HasValue)
                .WithMessage("Latitude must be between -90 and 90");
            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180)
                .When(x => x.Longitude.HasValue)
                .WithMessage("Longitude must be between -180 and 180");
        }
    }
}
