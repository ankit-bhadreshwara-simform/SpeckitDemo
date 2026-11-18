using SpeckitDemo.Application.Workflows;
using SpeckitDemo.Contracts.Inbound;
using SpeckitDemo.Data.Members;
using SpeckitDemo.DataAccess.Abstractions;
using SpeckitDemo.Security.Abstractions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace SpeckitDemo.Tests.Workflows;

public sealed class SignInWorkflowTests
{
    private readonly Mock<IAccountReader> _accounts = new();
    private readonly Mock<IValidator<CredentialPayload>> _rules = new();
    private readonly Mock<ISecretHasher> _hasher = new();
    private readonly Mock<ITokenIssuer> _tokens = new();
    private readonly Mock<ILogger<SignInWorkflow>> _logger = new();

    private SignInWorkflow CreateWorkflow() => new(
        _accounts.Object,
        _rules.Object,
        _hasher.Object,
        _tokens.Object,
        _logger.Object);

    [Fact]
    public async Task BlankCredentials_AreRejected()
    {
        var request = new CredentialPayload();
        _rules.Setup(validator => validator.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure("Username", "Username is required."),
                new ValidationFailure("Password", "Password is required.")
            ]));

        var result = await CreateWorkflow().LoginAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Validation failed");
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task UnknownAccount_IsRejected()
    {
        var request = new CredentialPayload { Username = "missing", Password = "admin" };
        _rules.Setup(validator => validator.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _accounts.Setup(reader => reader.FindByUsernameAsync("missing"))
            .ReturnsAsync((Member?)null);

        var result = await CreateWorkflow().LoginAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task WrongPassword_IsRejected()
    {
        var request = new CredentialPayload { Username = "mira", Password = "nope" };
        var member = new Member { Id = 1, Username = "mira", PasswordHash = "stored", Email = "mira.calder@speckitdemo.local" };

        _rules.Setup(validator => validator.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _accounts.Setup(reader => reader.FindByUsernameAsync("mira")).ReturnsAsync(member);
        _hasher.Setup(hasher => hasher.Matches("nope", "stored")).Returns(false);

        var result = await CreateWorkflow().LoginAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task MatchingPassword_ReturnsBearerToken()
    {
        var request = new CredentialPayload { Username = "mira", Password = "admin" };
        var member = new Member { Id = 1, Username = "mira", PasswordHash = "stored", Email = "mira.calder@speckitdemo.local" };
        var expiresAt = DateTime.UtcNow.AddHours(1);

        _rules.Setup(validator => validator.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _accounts.Setup(reader => reader.FindByUsernameAsync("mira")).ReturnsAsync(member);
        _hasher.Setup(hasher => hasher.Matches("admin", "stored")).Returns(true);
        _tokens.Setup(issuer => issuer.Issue(1, "mira", "mira.calder@speckitdemo.local"))
            .Returns(("signed.token.value", expiresAt));

        var result = await CreateWorkflow().LoginAsync(request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Login successful");
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().Be("signed.token.value");
        result.Data.TokenType.Should().Be("Bearer");
        result.Data.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public async Task UnknownAccountAndWrongPassword_ShareTheSameMessage()
    {
        var unknown = new CredentialPayload { Username = "missing", Password = "admin" };
        var wrong = new CredentialPayload { Username = "mira", Password = "nope" };
        var member = new Member { Id = 1, Username = "mira", PasswordHash = "stored", Email = "mira.calder@speckitdemo.local" };

        _rules.Setup(validator => validator.ValidateAsync(It.IsAny<CredentialPayload>(), default))
            .ReturnsAsync(new ValidationResult());
        _accounts.Setup(reader => reader.FindByUsernameAsync("missing")).ReturnsAsync((Member?)null);
        _accounts.Setup(reader => reader.FindByUsernameAsync("mira")).ReturnsAsync(member);
        _hasher.Setup(hasher => hasher.Matches("nope", "stored")).Returns(false);

        var workflow = CreateWorkflow();
        var unknownResult = await workflow.LoginAsync(unknown);
        var wrongResult = await workflow.LoginAsync(wrong);

        unknownResult.Message.Should().Be("Invalid credentials");
        wrongResult.Message.Should().Be(unknownResult.Message);
    }
}
