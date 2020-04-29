
using Microsoft.Extensions.Configuration;

namespace twot
{
    class Config
    {
        public string APIKey { get; private set; }
        public string APISecret { get; private set; }
        public string AccessToken { get; private set; }
        public string AccessSecret { get; private set; }

        private Config(){}

        public static Config Load()
        {
            var configuration = new ConfigurationBuilder()
                                .AddUserSecrets(typeof(Program).Assembly)
                                .Build();

            var section = configuration.GetSection("twot");
            return new Config() {
                AccessSecret = section.GetValue<string>("accesssecret"),
                APISecret = section.GetValue<string>("apisecret"),
                AccessToken = section.GetValue<string>("accesstoken"),
                APIKey = section.GetValue<string>("apikey")
            };            
        }

        public override string ToString()
        {
            return $"APIKey: {APIKey}\nAPISecret: {APISecret}\nAccessToken: {AccessToken}\nAccessSecret: {AccessSecret}";
        }
    }
}