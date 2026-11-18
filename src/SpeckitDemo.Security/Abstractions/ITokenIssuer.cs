namespace SpeckitDemo.Security.Abstractions;

public interface ITokenIssuer
{
    (string Token, DateTime ExpiresAt) Issue(int userId, string username, string email);
}
