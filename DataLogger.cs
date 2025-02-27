using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace SokeBot
{
    public class DataLogger
    {
        private readonly BotDb botDb;
        
        public DataLogger(BotDb botDb) 
        {
            this.botDb = botDb; 
        }

        public async Task LogAll()
        {
            var players = await botDb.RitoPlayers.Include(x => x.ReportChannels).ToListAsync();
            var games = await botDb.GamesInProgress.ToListAsync();
            
            var playersString = string.Join('\n', players.Select(x => x.ToString()));
            var gamesString = string.Join('\n', games.Select(x => x.ToString()));

            Console.WriteLine("---------------PLAYERS---------------");
            Console.WriteLine(playersString);
            Console.WriteLine("--------------GAMES-----------------");
            Console.WriteLine(gamesString);
        }
    }
}
