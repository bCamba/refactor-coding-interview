namespace LegacyApp
{
    public class Client
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ClientStatus ClientStatus { get; set; }

        public virtual int CreditMultipler => 1;
        public virtual bool HasCreditLimit => true;
    }

    public class ImportantClient : Client{

        public override int CreditMultipler => 2;
    }

    public class VeryImportantClient : Client{

        public override bool HasCreditLimit => false;

    }
}
