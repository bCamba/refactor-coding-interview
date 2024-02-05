using CommunityToolkit.Diagnostics;
using LegacyApp.Domain;
using LegacyApp.Infrastructure;
using LegacyApp.Infrastructure.SqlServer;
using System;
using System.Threading.Tasks;

namespace LegacyApp;

public class UserService : IUserService, IDisposable
{
    private bool disposed;

    private readonly IClientRepository clientRepository;
    private readonly IUserCreditService userCreditService;
    private readonly IUserRepository userRepository;

    public UserService(IClientRepository? clientRepository = null,
                       IUserCreditService? userCreditService = null,
                       IUserRepository? userRepository = null)
    {
        this.clientRepository = clientRepository ?? new ClientRepository();
        this.userCreditService = userCreditService ?? new UserCreditServiceClient();
        this.userRepository = userRepository ?? new UserRepository();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // dispose of managed resources.
            }
            disposed = true;
        }
    }

    public bool AddUser(string firstname, string surname, string email, DateTime dateOfBirth, int clientId)
    {
        return AddUserAsync(firstname, surname, email, DateOnly.FromDateTime(dateOfBirth), clientId).Result;
    }

    public async Task<bool> AddUserAsync(string firstname, string surname, string email, DateOnly dateOfBirth, int clientId)
    {
        try
        {
            Guard.IsNotNullOrWhiteSpace(firstname);
            Guard.IsNotNullOrWhiteSpace(surname);
            Guard.IsNotNullOrWhiteSpace(email);
            Guard.IsTrue(email.Contains('@') && email.Contains('.'));
            Guard.IsGreaterThanOrEqualTo(GetAge(dateOfBirth), 21);

            Client? client = await clientRepository.GetByIdAsync(clientId);
            Guard.IsNotNull(client);

            User user = new()
            {
                Client = client,
                DateOfBirth = dateOfBirth.ToDateTime(TimeOnly.MinValue),
                EmailAddress = email,
                Firstname = firstname,
                Surname = surname
            };

            await GetCreditLimitAsync(client, user);

            if (user.HasCreditLimit)
            {
                Guard.IsGreaterThanOrEqualTo(user.CreditLimit, 500);
            }

            await userRepository.AddUserAsync(user);
        }
        catch
        {
            return false;
        }

        return true;
    }

    private static int GetAge(DateOnly dateOfBirth)
    {
        DateTime now = DateTime.Now;
        int age = now.Year - dateOfBirth.Year;
        if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;
        return age;
    }

    private async Task GetCreditLimitAsync(Client client, User user)
    {
        if (client.Name == Client.VeryImportantClient)
        {
            // Skip credit check
        }
        else
        {
            // Do credit check and double credit limit
            int creditLimit = await Task.Run(() =>
            {
                return userCreditService.GetCreditLimit(user.Firstname, user.Surname, user.DateOfBirth);
            });
            if (client.Name == Client.ImportantClient)
            {
                creditLimit *= 2;
            }
            user.CreditLimit = creditLimit;
        }
    }
}
