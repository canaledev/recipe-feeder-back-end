namespace Feedy.Api.Validators;

using Feedy.Application.UseCases.LoginUser;
using FluentValidation;

/// <summary>
/// Validates the LoginUserRequest at the presentation boundary.
/// Format and completeness checks only — password min-length is intentionally omitted
/// to avoid hinting whether an account exists (enumeration vector).
/// </summary>
public class LoginUserRequestValidator : AbstractValidator<LoginUserRequest>
{
    public LoginUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
