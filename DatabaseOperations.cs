using Microsoft.EntityFrameworkCore;

namespace SokeBot
{
    public class DatabaseOperations
    {
        private readonly BotDb botDb;

        public DatabaseOperations(BotDb botDb)
        { 
            this.botDb = botDb;
        }

        public async Task WipeDb()
        {
            botDb.RitoPlayers.RemoveRange(botDb.RitoPlayers);
            botDb.GamesInProgress.RemoveRange(botDb.GamesInProgress);
            botDb.SaveChanges();
        }

        public async Task<RitoPlayer> GetUserReportChannels(string puuid)
        {
            return await botDb.RitoPlayers.Where(x => x.Puuid == puuid).Include(x => x.ReportChannels).FirstOrDefaultAsync();
        }

        public async Task RemoveBadData()
        {
            await botDb.GamesInProgress.ExecuteDeleteAsync();
        }
    }
}
