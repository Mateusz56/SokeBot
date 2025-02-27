namespace SokeBot.DataModel.RiotApi
{
    public class RiotGameInProgress
    {
        public long gameId { get; set; }
        public int gameQueueConfigId { get; set; }
    }

    public class RiotPlayerGameInProgress
    {
        public RitoPlayer player { get; set; }
        public RiotGameInProgress game { get; set; }
    }
}
