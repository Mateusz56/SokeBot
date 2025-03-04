namespace SokeBot.DataModel.RiotApi
{
    public class PlayerMatch
    {
        public string puuid { get; set; }
        public RiotMatch match { get; set; }
    }

    public class RiotMatch
    {
        public MatchInfo info { get; set; }
    }

    public class MatchInfo
    {
        public long gameId { get; set; }
        public List<Participant> participants { get; set; }
        public int queueId { get; set; }
    }

    public class Participant
    {
        public string puuid { get; set; }
        public string riotIdGameName { get; set; }

        public string lane { get; set; }
        public string individualPosition { get; set; }

        public int item0 { get; set; }
        public int item1 { get; set; }
        public int item2 { get; set; }
        public int item3 { get; set; }
        public int item4 { get; set; }
        public int item5 { get; set; }
        public int item6 { get; set; }

        public int kills { get; set; }
        public int deaths { get; set; }
        public int assists { get; set; }

        public string championName { get; set; }
        public int teamId { get; set; }
        public int timePlayed { get; set; }

        public int trueDamageDealtToChampions { get; set; }
        public int magicDamageDealtToChampions { get; set; }
        public int physicalDamageDealtToChampions { get; set; }

        public string teamPosition { get; set; }
        public bool win { get; set; }
    }

    public class MatchTeam
    {
        public int teamId { get; set; }
        public bool win { get; set; }
    }
}
