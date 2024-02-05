using System.Threading.Tasks;
using LegacyApp.Domain;

namespace LegacyApp.Infrastructure;

public interface IUserRepository
{
    Task AddUserAsync(User user);
}