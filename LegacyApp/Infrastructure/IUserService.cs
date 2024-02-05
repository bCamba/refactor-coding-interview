using System;
using System.Threading.Tasks;

namespace LegacyApp.Infrastructure;

public interface IUserService
{
    Task<bool> AddUserAsync(string firname, string surname, string email, DateOnly dateOfBirth, int clientId);
}