using SpeckitDemo.Contracts.Inbound;
using FluentValidation;

namespace SpeckitDemo.Application.Validation;

public sealed class CredentialRules : AbstractValidator<CredentialPayload>
{
    public CredentialRules()
    {
        RuleFor(request => request.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(100).WithMessage("Username must not exceed 100 characters.");

        RuleFor(request => request.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
