using System;

namespace LegacyApp.Domain;

public class User
{
    public int Id { get; set; }

    public required string Firstname { get; set; }

    public required string Surname { get; set; }

    public DateTime DateOfBirth { get; set; }

    public required string EmailAddress { get; set; }

    public bool HasCreditLimit => CreditLimit > 0;

    public int CreditLimit { get; set; }

    public required Client Client { get; set; }
}
