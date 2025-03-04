using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace SokeBot
{
    public class DataLogger
    {
        private readonly BotDb botDb;
        private readonly FileLogger logger;
        
        public DataLogger(BotDb botDb, FileLogger logger) 
        {
            this.botDb = botDb; 
            this.logger = logger;
        }

        public async Task<string> LogAll()
        {
            var players = await botDb.RitoPlayers.Include(x => x.ReportChannels).ToListAsync();
            var games = await botDb.GamesInProgress.ToListAsync();
            
            var playersString = string.Join('\n', players.Select(x => x.ToString()));
            var gamesString = string.Join('\n', games.Select(x => x.ToString()));

            var stringBuilder = new StringBuilder();
            stringBuilder
                .Append(DateTime.Now)
                .Append('\n')
                .Append("---------------PLAYERS---------------")
                .Append('\n')
                .Append(playersString)
                .Append('\n')
                .Append("--------------GAMES-----------------")
                .Append(gamesString)
                .Append("\n");

            var s = stringBuilder.ToString();
            logger.Log(s);
            return s;
        }
    }
}
