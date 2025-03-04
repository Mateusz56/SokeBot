using Discord;
using Newtonsoft.Json;
using SokeBot.DataModel.RiotApi;

namespace SokeBot
{
    public class RiotApi
    {
        private readonly HttpClient accountHttpClient;
        private readonly HttpClient lolHttpClient;
        private readonly string riotApiKey = "";


        public RiotApi()
        {
            accountHttpClient = new HttpClient();
            accountHttpClient.BaseAddress = new Uri("https://europe.api.riotgames.com/");
            accountHttpClient.DefaultRequestHeaders.Add("X-Riot-Token", riotApiKey);

            lolHttpClient = new HttpClient();
            lolHttpClient.BaseAddress = new Uri("https://eun1.api.riotgames.com/");
            lolHttpClient.DefaultRequestHeaders.Add("X-Riot-Token", riotApiKey);
        }

        public async Task<string> GetPuuid(string username, string tag)
        {
            var response = await accountHttpClient.GetAsync($"riot/account/v1/accounts/by-riot-id/{username}/{tag}");
            var content = await response.Content.ReadAsStringAsync();
            var acc = JsonConvert.DeserializeObject<RiotAccount>(content);

            return acc.puuid;
        }

        public async Task<RiotGameInProgress> GetGameInProgress(string puuid)
        {
            var response = await lolHttpClient.GetAsync($"lol/spectator/v5/active-games/by-summoner/{puuid}");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return null;

            var content = await response.Content.ReadAsStringAsync();

            var game = JsonConvert.DeserializeObject<RiotGameInProgress>(content);
            //Console.WriteLine($"Game found: Puuid = {puuid}; GameId = {game.gameId}");
            return game;
        }

        public async Task<RiotMatch> GetGameResult(string gameId)
        {
            var response = await accountHttpClient.GetAsync($"lol/match/v5/matches/{gameId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<RiotMatch>(content);
        }
    }
}
