namespace Blog
{
    public class Configuration
    {
        public static string JwtKey = "Minha chave secreta era muito pequena fica a duvida";
        public static string ApiKeyName = "api_key";
        public static string ApiKey = "curso_api_dsajoiJEwdsaomw==";

        public static SmtpConfiguration Smtp = new();

        public class SmtpConfiguration
        {
            public string Host { get; set; }
            public int Port { get; set; } = 25;
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}