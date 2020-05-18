namespace twot
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;
    using Newtonsoft.Json;
    using static ConsoleHelper;
    using static System.ConsoleColor;

    class InitCommand : twot.ICommand
    {
        public void AddCommand(Command rootCommand)
        {
            var cmd = new Command("Init", "Initialises the specified configuration file to make it easier to setup.");
            cmd.AddAlias("init");
            cmd.AddAlias("i");

            var scoreInit = new Command("Score", "Initialises a score file for use with the Clean command.");
            scoreInit.AddAlias("score");
            scoreInit.AddOption(new Option<string>("--file", () => "score.json", "This allows you to specify where you output " +
                "the scoring config to. Defaults to score.json"));
            scoreInit.Handler = CommandHandler.Create<string>(InitScoreConfig);

            cmd.AddCommand(scoreInit);

            var secretsInit = new Command("Secrets", "Initialises a secrets.json file for your Twitter keys.");
            secretsInit.AddAlias("secrets");
            secretsInit.AddOption(new Option<string>("--file", () => "secrets.json", "This allows you to specify where you output " +
                "the secrets config to. Defaults to secrets.json"));
            secretsInit.Handler = CommandHandler.Create<string>(InitSecretsConfig);

            cmd.AddCommand(secretsInit);


            rootCommand.Add(cmd);
        }

        private void InitSecretsConfig(string file)
        {
            Writeln(Cyan, $"Init secrets config to {file}");
            var content = JsonConvert.SerializeObject(new SecretsConfig(), Formatting.Indented);
            File.WriteAllText(file, content);
        }

        private void InitScoreConfig(string file)
        {
            Writeln(Cyan, $"Init score config to {file}");
            var content = new ScoreConfig().ToString();
            File.WriteAllText(file, content);
        }
    }

    class SecretsConfig
    {
        [JsonProperty(PropertyName = "twot:apikey")]
        public string APIKey { get; set; } = "";

        [JsonProperty(PropertyName = "twot:apisecret")]
        public string APISecret { get; set; } = "";

        [JsonProperty(PropertyName = "twot:accesstoken")]
        public string AccessToken { get; set; } = "";

        [JsonProperty(PropertyName = "twot:accesssecret")]
        public string AccessSecret { get; set; } = "";
    }
}
