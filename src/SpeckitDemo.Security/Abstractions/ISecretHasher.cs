namespace SpeckitDemo.Security.Abstractions;

public interface ISecretHasher
{
    string Protect(string password);

    bool Matches(string password, string hash);
}
