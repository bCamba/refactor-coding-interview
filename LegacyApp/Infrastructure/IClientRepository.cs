using System.Threading.Tasks;
using LegacyApp.Domain;

namespace LegacyApp.Infrastructure;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(int id);
}