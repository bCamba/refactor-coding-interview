using System.Threading.Tasks;
using LegacyApp.Domain;

namespace LegacyApp.Infrastructure.SqlServer;

public class UserRepository : IUserRepository
{
    public Task AddUserAsync(User user)
    {
        UserDataAccess.AddUser(user);
        return Task.CompletedTask;
    }
}