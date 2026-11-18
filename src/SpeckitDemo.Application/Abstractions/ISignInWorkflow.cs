using SpeckitDemo.Contracts.Envelope;
using SpeckitDemo.Contracts.Inbound;
using SpeckitDemo.Contracts.Outbound;

namespace SpeckitDemo.Application.Abstractions;

public interface ISignInWorkflow
{
    Task<ResultEnvelope<IssuedToken>> LoginAsync(CredentialPayload request);
}
