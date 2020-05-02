
using System;
using Microsoft.Extensions.Configuration;

namespace twot
{
    class Config
    {
        public string APIKey { get; private set; } = "";
        public string APISecret { get; private set; } = "";
        public string AccessToken { get; private set; } = "";
        public string AccessSecret { get; private set; } = "";

        private Config() { }

        public static (Boolean Success, Config Config) Load()
        {
            var configuration = new ConfigurationBuilder()
                                .AddUserSecrets(typeof(Program).Assembly)
                                .AddJsonFile("./secrets.json", true, false)
                                .Build();

            var section = configuration.GetSection("twot");
            var result = new Config
            {
                AccessSecret = section.GetValue<string>("accesssecret"),
                APISecret = section.GetValue<string>("apisecret"),
                AccessToken = section.GetValue<string>("accesstoken"),
                APIKey = section.GetValue<string>("apikey")
            };

            if (string.IsNullOrWhiteSpace(result.AccessSecret))
            {
                Console.WriteLine("Access Secret not set");
                return (false, new Config());
            }

            if (string.IsNullOrWhiteSpace(result.APISecret))
            {
                Console.WriteLine("API Secret not set");
                return (false, new Config());
            }

            if (string.IsNullOrWhiteSpace(result.AccessToken))
            {
                Console.WriteLine("Access Token not set");
                return (false, new Config());
            }

            if (string.IsNullOrWhiteSpace(result.APIKey))
            {
                Console.WriteLine("API Key not set");
                return (false, new Config());
            }

            return (true, result);
        }

        public override string ToString()
        {
            return $"APIKey: {APIKey}\nAPISecret: {APISecret}\nAccessToken: {AccessToken}\nAccessSecret: {AccessSecret}";
        }
    }
}