using Application.DTOs;
using FluentValidation;

namespace Application.Validation;

public class CreateBusinessCardValidator : AbstractValidator<CreateBusinessCardRequest>
{
    public CreateBusinessCardValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.PhotoSizeBytes)
        .LessThanOrEqualTo(1_000_000)
        .WithMessage("Photo must be <= 1MB")
        .When(x => x.PhotoSizeBytes.HasValue);
    }
}