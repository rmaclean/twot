namespace twot
{
    using System;
    using Microsoft.Extensions.Configuration;
    using System.Reflection;
    using System.Linq;

    class Config
    {
        [ConfigInfo(DisplayName = "API Key", ConfigProperty = "apikey")]
        public string APIKey { get; private set; } = "";

        [ConfigInfo(DisplayName = "API Secret", ConfigProperty = "apisecret")]
        public string APISecret { get; private set; } = "";

        [ConfigInfo(DisplayName = "Access Token", ConfigProperty = "accesstoken")]
        public string AccessToken { get; private set; } = "";

        [ConfigInfo(DisplayName = "Access Secret", ConfigProperty = "accesssecret")]
        public string AccessSecret { get; private set; } = "";

        private Config() { }

        public static (Boolean Success, Config Config) Load()
        {
            var configuration = new ConfigurationBuilder()
                                .AddUserSecrets(typeof(Program).Assembly)
                                .AddJsonFile("./secrets.json", true, false)
                                .Build();

            var section = configuration.GetSection("twot");
            var result = new Config();

            var propertiesToSet = result.GetType().GetProperties()
                .Select(propertyInfo =>
                    (PropertyInfo: propertyInfo, Config: propertyInfo.GetCustomAttribute<ConfigInfoAttribute>()))
                .Where(property => property.Config != null);

            foreach (var propInfo in propertiesToSet)
            {
                var configValue = section.GetValue<string>(propInfo.Config!.ConfigProperty);
                if (string.IsNullOrWhiteSpace(configValue))
                {
                    Console.WriteLine($"{propInfo.Config.DisplayName} not set");
                    return (false, new Config());
                }

                propInfo.PropertyInfo.SetValue(result, configValue);
            }

            return (true, result);
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, this.GetType().GetProperties()
                .Select(propertyInfo =>
                    (PropertyInfo: propertyInfo, Config: propertyInfo.GetCustomAttribute<ConfigInfoAttribute>()))
                .Where(property => property.Config != null)
                .Select(property => $"{property.Config!.DisplayName} = {property.PropertyInfo.GetValue(this)}"));
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class ConfigInfoAttribute : System.Attribute
    {
        public string DisplayName { get; set; } = "";
        public string ConfigProperty { get; set; } = "";
    }
}
