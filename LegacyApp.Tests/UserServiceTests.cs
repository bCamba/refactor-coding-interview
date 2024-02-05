using LegacyApp.Domain;
using LegacyApp.Infrastructure;
using Moq;

namespace LegacyApp.Tests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class UserServiceTests
{
    private readonly Mock<IClientRepository> clientRepository = new();
    private readonly Mock<IUserCreditService> userCreditService = new();
    private readonly Mock<IUserRepository> userRepository = new();

    private static class ValidValues
    {
        public static string FirstName => "Homer";
        public static string LastName => "Simpson";
        public static string Email => "homer.j.simpson@aol.com";
        public static DateTime DateOfBirth => DateTime.Parse("5/12/1972");
        public static int ClientId => 123;
    }

    private static IEnumerable<object?> GetEmptyValues() { return new object?[] { "", null, " ", "\n", "\t" }; }

    [SetUp]
    public void Setup()
    {
        clientRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Domain.Client
            {
                Name = "Some Client",
                ClientStatus = Domain.ClientStatus.Gold,
                Id = 123
            });
        userCreditService
            .Setup(s => s.GetCreditLimit(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>()
            ))
            .Returns(1000);
        userRepository
            .Setup(r => r.AddUserAsync(It.IsAny<Domain.User>()));
    }

    [Test]
    public void AddUser_ReturnsTrue_IfParametersAreValid()
    {
        // arrange
        UserService svc = new(
            clientRepository.Object,
            userCreditService.Object,
            userRepository.Object
        );

        // act
        bool actual = svc.AddUser(
            ValidValues.FirstName, 
            ValidValues.LastName,
            ValidValues.Email,
            ValidValues.DateOfBirth,
            ValidValues.ClientId
        );

        // assert
        Assert.That(actual, Is.EqualTo(true));
    }

    [Test, TestCaseSource(nameof(GetEmptyValues))]
    public void AddUser_ReturnsFalse_IfFirstNameIsEmpty(string? firstName)
    {
        // arrange
        UserService svc = new(
            clientRepository.Object,
            userCreditService.Object,
            userRepository.Object
        );

        // act
        bool actual = svc.AddUser(
            firstName!, 
            ValidValues.LastName,
            ValidValues.Email,
            ValidValues.DateOfBirth,
            ValidValues.ClientId
        );

        // assert
        Assert.That(actual, Is.EqualTo(false));
    }

    [Test, TestCaseSource(nameof(GetEmptyValues))]
    public void AddUser_ReturnsFalse_IfSurnameIsEmpty(string? surname)
    {
        // arrange
        UserService svc = new(
            clientRepository.Object,
            userCreditService.Object,
            userRepository.Object
        );

        // act
        bool actual = svc.AddUser(
            ValidValues.FirstName, 
            surname!,
            ValidValues.Email,
            ValidValues.DateOfBirth,
            ValidValues.ClientId
        );

        // assert
        Assert.That(actual, Is.EqualTo(false));
    }

    [Test, TestCaseSource(nameof(GetEmptyValues))]
    public void AddUser_ReturnsFalse_IfEmailIsEmpty(string? email)
    {
        // arrange
        UserService svc = new(
            clientRepository.Object,
            userCreditService.Object,
            userRepository.Object
        );

        // act
        bool actual = svc.AddUser(
            ValidValues.FirstName, 
            ValidValues.LastName,
            email!,
            ValidValues.DateOfBirth,
            ValidValues.ClientId
        );

        // assert
        Assert.That(actual, Is.EqualTo(false));
    }

    [Test]
    [TestCase("example@example.com", true)]
    [TestCase("exampleexample.com", false)]
    [TestCase("example@examplecom", false)]
    public void AddUser_ReturnsFalse_IfEmailInvalidFormat(string? email, bool expected)
    {
        // arrange
        UserService svc = new(
            clientRepository.Object,
            userCreditService.Object,
            userRepository.Object
        );

        // act
        bool actual = svc.AddUser(
            ValidValues.FirstName, 
            ValidValues.LastName,
            email!,
            ValidValues.DateOfBirth,
            ValidValues.ClientId
        );

        // assert
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void AddUser_ReturnsFalse_IfUserUnder21()
    {
        // arrange
        UserService svc = new(
            clientRepository.Object,
            userCreditService.Object,
            userRepository.Object
        );

        // act
        bool actual = svc.AddUser(
            ValidValues.FirstName, 
            ValidValues.LastName,
            ValidValues.Email,
            DateTime.Now.AddYears(-10),
            ValidValues.ClientId
        );

        // assert
        Assert.That(actual, Is.EqualTo(false));
    }

    [Test]
    public void AddUser_ReturnsFalse_IfClientNotFound()
    {
        // arrange
        clientRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Domain.Client?)null);

        UserService svc = new(
            clientRepository.Object,
            userCreditService.Object,
            userRepository.Object
        );

        // act
        bool actual = svc.AddUser(
            ValidValues.FirstName, 
            ValidValues.LastName,
            ValidValues.Email,
            ValidValues.DateOfBirth,
            ValidValues.ClientId
        );

        // assert
        Assert.That(actual, Is.EqualTo(false));
    }

    [Test]
    [TestCase(Client.VeryImportantClient, 0, true)]
    [TestCase("Test", 100, false)]
    [TestCase("Test", 1000, true)]
    [TestCase(Client.ImportantClient, 300, true)]
    [TestCase(Client.ImportantClient, 100, false)]
    public void AddUser_ReturnsFalse_IfHasCreditLimitBelow500(string clientName, int creditLimit, bool expected)
    {
        // arrange
        clientRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Client
            {
                Name = clientName,
                Id = 123,
                ClientStatus = ClientStatus.Gold
            });
        userCreditService
            .Setup(s => s.GetCreditLimit(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>()
            ))
            .Returns(creditLimit);

        UserService svc = new(
            clientRepository.Object,
            userCreditService.Object,
            userRepository.Object
        );

        // act
        bool actual = svc.AddUser(
            ValidValues.FirstName, 
            ValidValues.LastName,
            ValidValues.Email,
            ValidValues.DateOfBirth,
            ValidValues.ClientId
        );

        // assert
        Assert.That(actual, Is.EqualTo(expected));
    }
}