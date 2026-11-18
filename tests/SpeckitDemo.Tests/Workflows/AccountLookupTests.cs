using SpeckitDemo.Application.Workflows;
using SpeckitDemo.Data.Members;
using SpeckitDemo.DataAccess.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace SpeckitDemo.Tests.Workflows;

public sealed class AccountLookupTests
{
    private readonly Mock<IAccountReader> _accounts = new();
    private readonly Mock<ILogger<AccountLookup>> _logger = new();

    private AccountLookup CreateLookup() => new(_accounts.Object, _logger.Object);

    [Fact]
    public async Task KnownAccount_ReturnsProfileWithoutSecret()
    {
        var openedOn = new DateTime(2026, 3, 14, 0, 0, 0, DateTimeKind.Utc);
        _accounts.Setup(reader => reader.FindByIdAsync(1)).ReturnsAsync(new Member
        {
            Id = 1,
            Username = "mira",
            FirstName = "Mira",
            LastName = "Calder",
            Email = "mira.calder@speckitdemo.local",
            Address = "18 Harbor Lane, Port Merrin",
            PasswordHash = "must-stay-inside-the-database",
            CreatedDate = openedOn
        });

        var result = await CreateLookup().GetCurrentUserAsync(1);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("User details retrieved successfully");
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(1);
        result.Data.Username.Should().Be("mira");
        result.Data.FirstName.Should().Be("Mira");
        result.Data.LastName.Should().Be("Calder");
        result.Data.Email.Should().Be("mira.calder@speckitdemo.local");
        result.Data.Address.Should().Be("18 Harbor Lane, Port Merrin");
        result.Data.CreatedDate.Should().Be(openedOn);
        result.Data.GetType().GetProperty("PasswordHash").Should().BeNull();
    }

    [Fact]
    public async Task MissingAccount_ReturnsNotFound()
    {
        _accounts.Setup(reader => reader.FindByIdAsync(44)).ReturnsAsync((Member?)null);

        var result = await CreateLookup().GetCurrentUserAsync(44);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found");
        result.Data.Should().BeNull();
    }
}
