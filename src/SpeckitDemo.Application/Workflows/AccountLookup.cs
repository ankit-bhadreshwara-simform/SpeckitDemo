using SpeckitDemo.Application.Abstractions;
using SpeckitDemo.Contracts.Envelope;
using SpeckitDemo.Contracts.Outbound;
using SpeckitDemo.DataAccess.Abstractions;
using Microsoft.Extensions.Logging;

namespace SpeckitDemo.Application.Workflows;

public sealed class AccountLookup : IAccountLookup
{
    private readonly IAccountReader _accounts;
    private readonly ILogger<AccountLookup> _logger;

    public AccountLookup(IAccountReader accounts, ILogger<AccountLookup> logger)
    {
        _accounts = accounts;
        _logger = logger;
    }

    public async Task<ResultEnvelope<AccountProfile>> GetCurrentUserAsync(int userId)
    {
        var member = await _accounts.FindByIdAsync(userId);
        if (member is null)
        {
            _logger.LogWarning("No account found for id {UserId}.", userId);
            return ResultEnvelope<AccountProfile>.Fail("User not found");
        }

        _logger.LogInformation("Loaded profile for account {UserId}.", userId);

        return ResultEnvelope<AccountProfile>.Ok(
            new AccountProfile
            {
                Id = member.Id,
                Username = member.Username,
                FirstName = member.FirstName,
                LastName = member.LastName,
                Email = member.Email,
                Address = member.Address,
                CreatedDate = member.CreatedDate
            },
            "User details retrieved successfully");
    }
}
