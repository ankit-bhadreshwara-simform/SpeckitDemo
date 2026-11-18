using SpeckitDemo.Data;
using SpeckitDemo.Data.Members;
using SpeckitDemo.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace SpeckitDemo.DataAccess.Stores;

public sealed class AccountReader : IAccountReader
{
    private readonly SpeckitDemoContext _context;

    public AccountReader(SpeckitDemoContext context)
    {
        _context = context;
    }

    public Task<Member?> FindByUsernameAsync(string username) =>
        _context.Users.FirstOrDefaultAsync(member => member.Username == username);

    public Task<Member?> FindByIdAsync(int id) =>
        _context.Users.FirstOrDefaultAsync(member => member.Id == id);
}
