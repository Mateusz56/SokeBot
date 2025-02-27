using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace SokeBot
{
    public class BotMain
    {
        private readonly BotDb db;
        private readonly RiotApi riotApi;
        private readonly DataLogger dataLogger;
        private readonly DatabaseOperations databaseOperations;

        private Action startedCallback;
        private static DiscordSocketClient client;
        string token = "";

        public BotMain(RiotApi riotApi, BotDb botDb, DataLogger dataLogger, DatabaseOperations databaseOperations)
        {
            client = new DiscordSocketClient();
            db = botDb;
            this.riotApi = riotApi;
            this.dataLogger = dataLogger;
            this.databaseOperations = databaseOperations;
        }

        public async void Start(Action onStarted)
        {
            startedCallback = onStarted;
            client.Ready += Client_Ready;
            client.SlashCommandExecuted += Client_SlashCommandExecuted;
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task Client_SlashCommandExecuted(SocketSlashCommand arg)
        {
            switch(arg.CommandName)
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

            await client.CreateGlobalApplicationCommandAsync(watchCommand.Build());
            await client.CreateGlobalApplicationCommandAsync(logCommand.Build());
            await client.CreateGlobalApplicationCommandAsync(wipeCommand.Build());
            startedCallback?.Invoke();
        }

        private async Task HandleWatchPlayer(SocketSlashCommand command)
        {
            var username = command.Data.Options.Where(x => x.Name == "username").First().Value.ToString();
            var tag = command.Data.Options.Where(x => x.Name == "tag").First().Value.ToString();

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

            await command.RespondAsync("Sending logs to server console");
            await dataLogger.LogAll();
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

        public void SendTextMessage(ulong guild, ulong channel, string text)
        {
            client.GetGuild(guild).GetTextChannel(channel).SendMessageAsync(text);
        }
    }
}
