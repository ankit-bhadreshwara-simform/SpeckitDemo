namespace SpeckitDemo.Contracts.Outbound;

public sealed class IssuedToken
{
    public string Token { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    public DateTime ExpiresAt { get; set; }
}
