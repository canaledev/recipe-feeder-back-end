namespace Feedy.Api.Validators;

using Feedy.Application.UseCases.RegisterUser;
using FluentValidation;

/// <summary>
/// Validates the RegisterUserRequest at the presentation boundary.
/// Format and completeness checks only — business rules live in RegisterUserService.
/// </summary>
public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.InitialFlavorTags)
            .NotNull().WithMessage("InitialFlavorTags must not be null.");
    }
}
