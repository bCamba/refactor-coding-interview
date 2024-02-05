namespace LegacyApp.Domain;

public class Client
{
    public const string VeryImportantClient = "VeryImportantClient";
    public const string ImportantClient = "ImportantClient";

    public int Id { get; set; }

    public required string Name { get; set; }

    public ClientStatus ClientStatus { get; set; }
}
