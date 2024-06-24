using System;

namespace LegacyApp
{
    public class UserService(ClientRepository clientRepository, TimeProvider timeProvider, UserCreditServiceClient userCreditService)
    {
        public record struct Email(string Value){
            public bool IsValid => ValidateEmail());

            bool ValidateEmail() => Value.Contains("@") && Value.Contains(".");
        }

        public record AddUserResult()
        {
            public User? User { get; set; }

            public AddUserStatus Status { get; set; }

            public string ErrorMessage { get; set; }
        }

        public AddUserResult AddUser(string firstName, string surname, Email email, DateTime dateOfBirth, int clientId)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(firstName);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(surname);

            if (!email.IsValid)
                return new() { ErrorMessage = "Email was bad", Status = AddUserStatus.Failure };

            if (!AgeIsValid(dateOfBirth))
                return new() { ErrorMessage = "Under 21", Status = AddUserStatus.Failure };

            var client = clientRepository.GetById(clientId);

            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email.Value,
                Firstname = firstName,
                Surname = surname,
                HasCreditLimit = client.HasCreditLimit,
                CreditLimit = GetCreditLimit(firstName, surname, dateOfBirth, client.CreditMultipler)
            };

            if (HasValidCredit(user))
                return new() { ErrorMessage = "Bad credit limit", Status = AddUserStatus.Failure };

            UserDataAccess.AddUser(user);

            return new() { Status = AddUserStatus.Success, User = user };
        }

        bool AgeIsValid(DateTime dateOfBirth)
        {
            var now = timeProvider.GetUtcNow();
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;

            if (age < 21) return false;

            return true;
        }

        static bool HasValidCredit(User user) => !user.HasCreditLimit || user.CreditLimit >= 500;

        int GetCreditLimit(string firstName, string surname, DateTime dateOfBirth, int creditMultiplier)
        {
            var creditLimit = userCreditService.GetCreditLimit(firstName, surname, dateOfBirth);
            creditLimit *= creditMultiplier;
            return creditLimit;
        }
    }

                public enum AddUserStatus
            {
                Success,
                Failure,
            }
}
