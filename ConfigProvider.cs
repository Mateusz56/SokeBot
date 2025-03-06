using Newtonsoft.Json;

namespace SokeBot
{
    public static class ConfigProvider
    {
        private static AppTokens appTokens;
        
        public static string GetRiotApiKey()
        {
            if (appTokens == null)
            {
                var configString = File.OpenText("appsettings.json").ReadToEnd();
                appTokens = JsonConvert.DeserializeObject<AppTokens>(configString);
            }

            return appTokens.RiotApiKey;
        }

        public static string GetDiscordBotToken()
        {
            if (appTokens == null)
            {
                var configString = File.OpenText("appsettings.json").ReadToEnd();
                appTokens = JsonConvert.DeserializeObject<AppTokens>(configString);
            }

            return appTokens.DiscordBotToken;
        }
    }

    public class AppTokens
    {
        public string RiotApiKey { get; set; }
        public string DiscordBotToken { get; set; }
    }
}
