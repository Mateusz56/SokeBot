using Microsoft.EntityFrameworkCore;
using SokeBot.DataModel.RiotApi;

namespace SokeBot
{
    public class RiotPlayerWatcher
    {
        private readonly RiotApi riotApi;
        private readonly BotDb db;
        private readonly BotMain bot;

        public RiotPlayerWatcher(RiotApi riotApi, BotDb db, BotMain bot)
        {
            this.riotApi = riotApi;
            this.db = db;
            this.bot = bot;
        }

        public async Task WatchPlayers()
        {
            while(true)
            {
                var ids = await db.RitoPlayers.Where(x => x.GameInProgress == null).ToListAsync();
                
                var games = ids.Select(async x => new RiotPlayerGameInProgress { player = x, game = await riotApi.GetGameInProgress(x.Puuid) }).Where(x => x.Result.game != null).ToList();

                games.ForEach(x =>
                {
                    GameInProgress newGame = new GameInProgress
                    {
                        PlayerId = x.Result.player.Id,
                        GameId = x.Result.game.gameId
                    };
                    db.GamesInProgress.Add(newGame);
                });

                db.SaveChanges();
                Console.WriteLine("----- FOUND NEW GAMES -----");
                games.ForEach(x => Console.WriteLine(x.Result.player.Name));

                var gamesInProgress = await db.GamesInProgress.Include(x => x.Player).ToListAsync();
                var finishedGames = gamesInProgress
                    .Select(async x => await GameInProgressToPlayerMatch(x))
                    .Select(x => x.Result)
                    .Where(x => x != null)
                    .ToList();

                var finishedGamesIds = finishedGames.Select(x => x.match.info.gameId);

                db.GamesInProgress.RemoveRange(db.GamesInProgress.Where(x => finishedGamesIds.Contains(x.GameId)));
                db.SaveChanges();

                finishedGames.ForEach(x => {
                    var player = x.match.info.participants.First(p => p.puuid == x.puuid);
                    var dbPlayer = db.RitoPlayers.AsNoTracking().Where(x => x.Puuid == player.puuid).Include(x => x.ReportChannels).First();
                    dbPlayer.ReportChannels.ToList().ForEach(
                        x => bot.SendTextMessage((ulong)x.GuildId, (ulong)x.ChannelId, $"{player.riotIdGameName} - {(player.win ? "Wygranko!" : "Przegranko :/")} - {player.championName}: {player.kills}/{player.deaths}/{player.assists}"));
                });

                Thread.Sleep(TimeSpan.FromSeconds(61));
            }
        }

        private async Task<PlayerMatch> GameInProgressToPlayerMatch(GameInProgress game)
        {
            var match = await riotApi.GetGameResult($"EUN1_{game.GameId}");

            if (match == null)
                return null;

            return new PlayerMatch { match = match, puuid = game.Player.Puuid };
        }
    }
}
