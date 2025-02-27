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
    }
}
