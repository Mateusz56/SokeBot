using Discord;
using Microsoft.EntityFrameworkCore;
using SokeBot.DataModel.RiotApi;
using System.Diagnostics.CodeAnalysis;
using System.Timers;

namespace SokeBot
{
    public class RiotPlayerWatcher
    {
        private readonly RiotApi riotApi;
        private readonly BotMain bot;
        private readonly FileLogger logger;

        public RiotPlayerWatcher(RiotApi riotApi, BotMain bot, FileLogger logger)
        {
            this.riotApi = riotApi;
            this.bot = bot;
            this.logger = logger;
        }

        public void StartWatcher()
        {
            var timer = new System.Timers.Timer(TimeSpan.FromSeconds(61));
            timer.Elapsed += WatchPlayers;
            timer.AutoReset = true;
            timer.Start();
        }

        public async void WatchPlayers(object sender, ElapsedEventArgs e)
        {
            try
            {
                using var db = new BotDb();
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

                var gamesInProgress = await db.GamesInProgress.Include(x => x.Player).ToListAsync();

                var finishedGames = gamesInProgress
                    .Distinct(new GameInProgressComparer())
                    .Select(async x => await GameInProgressToMatch(x))
                    .Select(x => x.Result)
                    .Where(x => x != null)
                    .ToList();

                var playerIds = finishedGames.SelectMany(x => x.info.participants.Select(p => p.puuid)).Distinct().ToList();
                var finishedGamesIds = finishedGames.Select(x => x.info.gameId);

                db.GamesInProgress.RemoveRange(db.GamesInProgress.Where(x => finishedGamesIds.Contains(x.GameId)));
                db.SaveChanges();

                finishedGames.ForEach(x =>
                {
                    var gamePlayers = x.info.participants.Select(p => p.puuid).ToList();
                    var watchedPlayers = db.RitoPlayers.AsNoTracking()
                        .Where(x => gamePlayers.Contains(x.Puuid))
                        .Include(x => x.ReportChannels)
                        .ToList();

                    var reportChannels = watchedPlayers
                        .SelectMany(x => x.ReportChannels)
                        .Distinct(new ReportChannelComparer())
                        .ToList();

                    var embed = new MatchEmbedGenerator().BuildEmbed(x, watchedPlayers.Select(x => x.Puuid).ToList());
                    reportChannels.ForEach(x =>
                    {
                        bot.SendEmbedMessage((ulong)x.GuildId, (ulong)x.ChannelId, embed);
                    });
                });
            }
            catch (Exception ex)
            {
                logger.Log(ex.ToString());
            }
        }

        private async Task<PlayerMatch> GameInProgressToPlayerMatch(GameInProgress game)
        {
            var match = await riotApi.GetGameResult($"EUN1_{game.GameId}");

            if (match == null)
                return null;

            return new PlayerMatch { match = match, puuid = game.Player.Puuid };
        }

        private async Task<RiotMatch> GameInProgressToMatch(GameInProgress game)
        {
            var match = await riotApi.GetGameResult($"EUN1_{game.GameId}");

            return match;
        }

        public class GameInProgressComparer : IEqualityComparer<GameInProgress>
        {
            public bool Equals(GameInProgress? x, GameInProgress? y)
            {
                if (Object.ReferenceEquals(x, y)) return true;

                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                return x.GameId == y.GameId;
            }

            public int GetHashCode([DisallowNull] GameInProgress obj)
            {
                if (Object.ReferenceEquals(obj, null)) return 0;

                return obj.GameId.GetHashCode();
            }
        }

        public class ReportChannelComparer : IEqualityComparer<ReportChannel>
        {
            public bool Equals(ReportChannel? x, ReportChannel? y)
            {
                if (Object.ReferenceEquals(x, y)) return true;

                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                return x.GuildId == y.GuildId && x.ChannelId == y.ChannelId;
            }

            public int GetHashCode([DisallowNull] ReportChannel obj)
            {
                if (Object.ReferenceEquals(obj, null)) return 0;

                int guildIdHashCode = obj.GuildId.GetHashCode();

                int channelIdHashCode = obj.ChannelId.GetHashCode();

                return guildIdHashCode ^ channelIdHashCode;
            }
        }
    }
}
