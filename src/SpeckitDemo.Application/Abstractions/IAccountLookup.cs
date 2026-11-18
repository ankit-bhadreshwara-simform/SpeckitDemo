using SpeckitDemo.Contracts.Envelope;
using SpeckitDemo.Contracts.Outbound;

namespace SpeckitDemo.Application.Abstractions;

public interface IAccountLookup
{
    Task<ResultEnvelope<AccountProfile>> GetCurrentUserAsync(int userId);
}
