using SpeckitDemo.Application.Validation;
using SpeckitDemo.Contracts.Inbound;
using FluentAssertions;

namespace SpeckitDemo.Tests.Validation;

public sealed class CredentialRulesTests
{
    private readonly CredentialRules _rules = new();

    [Fact]
    public async Task EmptyUsername_Fails()
    {
        var result = await _rules.ValidateAsync(new CredentialPayload { Username = "", Password = "admin" });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.PropertyName == "Username");
    }

    [Fact]
    public async Task EmptyPassword_Fails()
    {
        var result = await _rules.ValidateAsync(new CredentialPayload { Username = "mira", Password = "" });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.PropertyName == "Password");
    }

    [Fact]
    public async Task UsernamePastOneHundredCharacters_Fails()
    {
        var result = await _rules.ValidateAsync(new CredentialPayload
        {
            Username = new string('m', 101),
            Password = "admin"
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.PropertyName == "Username");
    }

    [Fact]
    public async Task CompletePayload_Passes()
    {
        var result = await _rules.ValidateAsync(new CredentialPayload { Username = "mira", Password = "admin" });

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task BothFieldsEmpty_ReturnsTwoErrors()
    {
        var result = await _rules.ValidateAsync(new CredentialPayload());

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(error => error.PropertyName == "Username");
        result.Errors.Should().Contain(error => error.PropertyName == "Password");
    }
}
