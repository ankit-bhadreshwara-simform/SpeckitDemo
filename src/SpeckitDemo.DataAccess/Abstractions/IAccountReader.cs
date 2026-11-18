using SpeckitDemo.Data.Members;

namespace SpeckitDemo.DataAccess.Abstractions;

public interface IAccountReader
{
    Task<Member?> FindByUsernameAsync(string username);

    Task<Member?> FindByIdAsync(int id);
}
