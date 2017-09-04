namespace Fileharbor.Common.Configuration
{
    public class AuthenticationConfiguration
    {
        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string SigningCredentials { get; set; }

        public int TokenLifeTimeInHours { get; set; }

        public int HashIterations { get; set; }

        public bool EnableMailValidation { get; set; }
    }
}
