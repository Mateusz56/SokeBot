using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace SokeBot
{
    public class BotMain
    {
        private readonly RiotApi riotApi;
        private readonly DataLogger dataLogger;
        private readonly DatabaseOperations databaseOperations;

        private Action startedCallback;
        private static DiscordSocketClient client;
        string token = "";

        public BotMain(RiotApi riotApi, DataLogger dataLogger, DatabaseOperations databaseOperations)
        {
            client = new DiscordSocketClient();
            this.riotApi = riotApi;
            this.dataLogger = dataLogger;
            this.databaseOperations = databaseOperations;
        }

        public async void Start(Action onStarted)
        {
            startedCallback = onStarted;
            client.Log += Log;
            client.Ready += Client_Ready;
            client.SlashCommandExecuted += Client_SlashCommandExecuted;
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private static Task Log(LogMessage msg)
        {
            //Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task Client_SlashCommandExecuted(SocketSlashCommand arg)
        {
            try
            {
                switch (arg.CommandName)
                {
                    case "watch-player":
                        await HandleWatchPlayer(arg);
                        break;
                    case "log-db":
                        await HandleLogDb(arg);
                        break;
                    case "wipe-db":
                        await HandleWipeDb(arg);
                        break;
                    case "test-result":
                        await HandleTestResult(arg);
                        break;
                    case "clean-db":
                        await HandleCleanDb(arg);
                        break;
                    case "insert-game":
                        await HandleInsertGameInProgress(arg);
                        break;
                    case "test-embed":
                        await HandleTestEmbed(arg);
                        break;
                }
            }
            catch (Exception e)
            {
                SendTextMessage(1343689770101903400, 1345022596575658035, e.Message);
            }
        }

        private async Task Client_Ready()
        {
            var watchCommand = new SlashCommandBuilder();
            watchCommand.WithName("watch-player");
            watchCommand.WithDescription("Add player to watch list");
            watchCommand.AddOption("username", ApplicationCommandOptionType.String, "username", true);
            watchCommand.AddOption("tag", ApplicationCommandOptionType.String, "tag", true);
            watchCommand.AddOption("server", ApplicationCommandOptionType.String, "server", false);

            var logCommand = new SlashCommandBuilder();
            logCommand.WithName("log-db");
            logCommand.WithDescription("Log data to server console");

            var wipeCommand = new SlashCommandBuilder();
            wipeCommand.WithName("wipe-db");
            wipeCommand.WithDescription("Remove all data from database");

            var testReportResultCommand = new SlashCommandBuilder();
            testReportResultCommand.WithName("test-result");
            testReportResultCommand.WithDescription("Send notification for user by puuid");
            testReportResultCommand.AddOption("puuid", ApplicationCommandOptionType.String, "puuid", true);

            var cleanDbCommand = new SlashCommandBuilder();
            cleanDbCommand.WithName("clean-db");
            cleanDbCommand.WithDescription("Clean bad data from db");

            var insertGameInProgressCommand = new SlashCommandBuilder();
            insertGameInProgressCommand.WithName("insert-game");
            insertGameInProgressCommand.WithDescription("Insert game to db");
            insertGameInProgressCommand.AddOption("puuid", ApplicationCommandOptionType.String, "playerId", true);
            insertGameInProgressCommand.AddOption("gameid", ApplicationCommandOptionType.String, "gameId", true);

            var sendEmberCommand = new SlashCommandBuilder()
                .WithName("test-embed")
                .WithDescription("Test embed")
                .AddOption("gameid", ApplicationCommandOptionType.String, "gameid", true)
                .AddOption("puuid1", ApplicationCommandOptionType.String, "puuid1", false)
                .AddOption("puuid2", ApplicationCommandOptionType.String, "puuid2", false)
                .AddOption("puuid3", ApplicationCommandOptionType.String, "puuid3", false)
                .AddOption("puuid4", ApplicationCommandOptionType.String, "puuid4", false);

            await client.CreateGlobalApplicationCommandAsync(watchCommand.Build());
            await client.CreateGlobalApplicationCommandAsync(logCommand.Build());
            await client.CreateGlobalApplicationCommandAsync(wipeCommand.Build());
            await client.CreateGlobalApplicationCommandAsync(testReportResultCommand.Build());
            await client.CreateGlobalApplicationCommandAsync(cleanDbCommand.Build());
            await client.CreateGlobalApplicationCommandAsync(insertGameInProgressCommand.Build());
            await client.CreateGlobalApplicationCommandAsync(sendEmberCommand.Build());
            startedCallback?.Invoke();
        }

        private async Task HandleWatchPlayer(SocketSlashCommand command)
        {
            var username = command.Data.Options.Where(x => x.Name == "username").First().Value.ToString();
            var tag = command.Data.Options.Where(x => x.Name == "tag").First().Value.ToString();
            using var db = new BotDb();
            var channelId = command.ChannelId;
            var guildId = command.GuildId;

            var newPlayer = new RitoPlayer
            {
                Name = username,
                Tag = tag,
                Server = "eune",
                Puuid = await riotApi.GetPuuid(username, tag)
            };

            if (newPlayer.Puuid == null) {
                await command.RespondAsync("Nie znaleziono użytkownika.");
                return;
            }

            var player = await db.RitoPlayers.Where(x => x.Puuid == newPlayer.Puuid).Include(x => x.ReportChannels).FirstOrDefaultAsync();

            if (player == null)
            {
                newPlayer.ReportChannels = [new ReportChannel { ChannelId = command.ChannelId, GuildId = command.GuildId }];
                db.RitoPlayers.Add(newPlayer);

                await db.SaveChangesAsync();
                await command.RespondAsync("Dodano użytkownika.");
                return;
            }

            if(player.ReportChannels != null && player.ReportChannels.Any(x => x.ChannelId == channelId && x.GuildId == guildId))
            {
                await command.RespondAsync("Już był dodany.");
                return;
            }

            var reportChannel = new ReportChannel { ChannelId = channelId, GuildId = guildId };

            if (player.ReportChannels != null)
                player.ReportChannels.Add(reportChannel);
            else
                player.ReportChannels = [reportChannel];
            
            db.ReportChannels.Add(reportChannel);
            await db.SaveChangesAsync();
            await command.RespondAsync("Dodano użytkownika.");
        }

        private async Task HandleLogDb(SocketSlashCommand command)
        {
            if (command.GuildId != 1343689770101903400)
            {
                await command.RespondAsync("Nie.");
                return;
            }

            var s = await dataLogger.LogAll();
            await command.RespondAsync(s);
        }

        private async Task HandleWipeDb(SocketSlashCommand command)
        {
            if(command.GuildId != 1343689770101903400)
            {
                await command.RespondAsync("Nie.");
                return;
            }

            await databaseOperations.WipeDb();
            await command.RespondAsync("Wipe request sent");
        }

        private async Task HandleTestResult(SocketSlashCommand command)
        {
            if (command.GuildId != 1343689770101903400)
            {
                await command.RespondAsync("Nie.");
                return;
            }

            await command.RespondAsync("Przyjęto.");
            var puuid = command.Data.Options.Where(x => x.Name == "puuid").First().Value.ToString();

            var player = await databaseOperations.GetUserReportChannels(puuid);
            player.ReportChannels.ForEach(x => SendTextMessage((ulong)x.GuildId, (ulong)x.ChannelId, $"Wiadomość testowa dla użytkownika {player.Name}"));
        }

        private async Task HandleCleanDb(SocketSlashCommand command)
        {
            if (command.GuildId != 1343689770101903400)
            {
                await command.RespondAsync("Nie.");
                return;
            }

            await command.RespondAsync("Przyjęto.");
            databaseOperations.RemoveBadData();
        }

        public async Task HandleInsertGameInProgress(SocketSlashCommand command)
        {
            if (command.GuildId != 1343689770101903400)
            {
                await command.RespondAsync("Nie.");
                return;
            }

            await command.RespondAsync("Przyjęto.");
            using var db = new BotDb();

            var gameId = command.Data.Options.Where(x => x.Name == "gameid").First().Value.ToString();
            var playerId = command.Data.Options.Where(x => x.Name == "puuid").First().Value.ToString();

            db.GamesInProgress.Add(new GameInProgress
            {
                GameId = long.Parse(gameId),
                PlayerId = int.Parse(playerId)
            });
            await db.SaveChangesAsync();
        }

        public async Task HandleTestEmbed(SocketSlashCommand command)
        {
            var gameid = command.Data.Options.Where(x => x.Name == "gameid").First().Value.ToString();
            var puuids = command.Data.Options.Where(x => x.Name.Contains("puuid")).Select(x => x.Value.ToString()).ToList();
            var game = await riotApi.GetGameResult("EUN1_" + gameid);
            var embed = new MatchEmbedGenerator().BuildEmbed(game, puuids);

            await command.RespondAsync(embed: embed);
        }

        public void SendTextMessage(ulong guild, ulong channel, string text)
        {
            client.GetGuild(guild)?.GetTextChannel(channel)?.SendMessageAsync(text);
        }

        public void SendEmbedMessage(ulong guild, ulong channel, Embed embed)
        {
            client.GetGuild(guild)?.GetTextChannel(channel)?.SendMessageAsync(embed: embed);
        }
    }
}
