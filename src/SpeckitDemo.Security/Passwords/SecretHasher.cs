using SpeckitDemo.Security.Abstractions;

namespace SpeckitDemo.Security.Passwords;

public sealed class SecretHasher : ISecretHasher
{
    private const int WorkFactor = 12;

    public string Protect(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Matches(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
