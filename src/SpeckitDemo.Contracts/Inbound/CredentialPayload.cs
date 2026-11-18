namespace SpeckitDemo.Contracts.Inbound;

public sealed class CredentialPayload
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
