using SpeckitDemo.Application.Abstractions;
using SpeckitDemo.Contracts.Envelope;
using SpeckitDemo.Contracts.Inbound;
using SpeckitDemo.Contracts.Outbound;
using SpeckitDemo.DataAccess.Abstractions;
using SpeckitDemo.Security.Abstractions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace SpeckitDemo.Application.Workflows;

public sealed class SignInWorkflow : ISignInWorkflow
{
    private readonly IAccountReader _accounts;
    private readonly IValidator<CredentialPayload> _rules;
    private readonly ISecretHasher _hasher;
    private readonly ITokenIssuer _tokens;
    private readonly ILogger<SignInWorkflow> _logger;

    public SignInWorkflow(
        IAccountReader accounts,
        IValidator<CredentialPayload> rules,
        ISecretHasher hasher,
        ITokenIssuer tokens,
        ILogger<SignInWorkflow> logger)
    {
        _accounts = accounts;
        _rules = rules;
        _hasher = hasher;
        _tokens = tokens;
        _logger = logger;
    }

    public async Task<ResultEnvelope<IssuedToken>> LoginAsync(CredentialPayload request)
    {
        var validation = await _rules.ValidateAsync(request);
        if (!validation.IsValid)
        {
            _logger.LogInformation("Sign-in payload rejected by validation.");
            return ResultEnvelope<IssuedToken>.Fail(
                "Validation failed",
                validation.Errors.Select(error => error.ErrorMessage));
        }

        var member = await _accounts.FindByUsernameAsync(request.Username);
        if (member is null || !_hasher.Matches(request.Password, member.PasswordHash))
        {
            _logger.LogWarning("Sign-in failed for username {Username}.", request.Username);
            return ResultEnvelope<IssuedToken>.Fail("Invalid credentials");
        }

        var (token, expiresAt) = _tokens.Issue(member.Id, member.Username, member.Email);
        _logger.LogInformation("Account {UserId} signed in.", member.Id);

        return ResultEnvelope<IssuedToken>.Ok(
            new IssuedToken
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresAt = expiresAt
            },
            "Login successful");
    }
}
