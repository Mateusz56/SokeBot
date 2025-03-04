using Discord;
using SokeBot.DataModel.RiotApi;

namespace SokeBot
{
    public class MatchEmbedGenerator
    {
        private int maxDamage;
        private IEnumerable<string> puuids;

        private string BuildPlayerString(Participant participant)
        {
            int totalDamage = GetTotalDamage(participant);
            string playerString;
            if (puuids.Any(x => x == participant.puuid))
                playerString = participant.riotIdGameName.EmphasizeNick();
            else
                playerString = participant.riotIdGameName;

            if (participant.win)
                playerString = playerString.EmphasizeWinner();

            return $"{playerString} ⛰️ {participant.championName} ⛰️  {participant.kills}🗡️ {participant.deaths}💀 {participant.assists}🖐️\n{GetDamageSquares(totalDamage)} {totalDamage} Dmg";
        }

        private int GetTotalDamage(Participant participant)
        {
            return participant.magicDamageDealtToChampions + participant.physicalDamageDealtToChampions + participant.trueDamageDealtToChampions;
        }

        public Embed BuildEmbed(RiotMatch match, IEnumerable<string> puuids)
        {
            maxDamage = match.info.participants.Max(GetTotalDamage);
            this.puuids = puuids.ToList();
            var playerNames = match.info.participants.Where(x => puuids.Contains(x.puuid)).Select(x => x.riotIdGameName).ToList();
            var team1 = match.info.participants.Where(x => x.teamId == 100);
            var team2 = match.info.participants.Where(x => x.teamId == 200);

            return new EmbedBuilder()
                .WithTitle(QueueConfigProvider.GetQueueDescription(match.info.queueId) + " - " + string.Join(", ", playerNames))
                .AddField("Top", BuildPlayerString(team1.GetTop()) + "\nVS\n" + BuildPlayerString(team2.GetTop()))
                .AddField("Jungle", BuildPlayerString(team1.GetJungle()) + "\nVS\n" + BuildPlayerString(team2.GetJungle()))
                .AddField("Mid", BuildPlayerString(team1.GetMid()) + "\nVS\n" + BuildPlayerString(team2.GetMid()))
                .AddField("Adc", BuildPlayerString(team1.GetAdc()) + "\nVS\n " + BuildPlayerString(team2.GetAdc()))
                .AddField("Supp", BuildPlayerString(team1.GetSupp()) + "\nVS\n" + BuildPlayerString(team2.GetSupp()))
                .Build();
        }

        private Participant GetTestParticipant()
        {
            return new Participant
            {
                assists = 7,
                championName = "Kai'Sa",
                deaths = 2,
                kills = 3,
                magicDamageDealtToChampions = 2613,
                physicalDamageDealtToChampions = 18467,
                trueDamageDealtToChampions = 256,
                riotIdGameName = "Seko",
                win = true,
                individualPosition = "Top"
            };
        }

        private string GetDamageSquares(int damage)
        {
            int fullSquares = (int)Math.Round((float)damage * 10 / maxDamage);
            var full = "⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛";
            var empty = "⬜⬜⬜⬜⬜⬜⬜⬜⬜⬜";

            return string.Join("", full.Take(fullSquares)) + string.Join("", empty.Take(10 - fullSquares));
        }
    }

    public static class Extensions
    {
        public static string EmphasizeNick(this string nick)
        {
            return $":bison: **{nick}** :bison: ";
        }

        public static string EmphasizeWinner(this string nick)
        {
            return $"👑 {nick} 👑 ";
        }

        public static Participant GetTop(this IEnumerable<Participant> participants)
        {
            return participants.Where(x => x.teamPosition == top).FirstOrDefault();
        }

        public static Participant GetJungle(this IEnumerable<Participant> participants)
        {
            return participants.Where(x => x.teamPosition == jungle).FirstOrDefault();
        }

        public static Participant GetMid(this IEnumerable<Participant> participants)
        {
            return participants.Where(x => x.teamPosition == mid).FirstOrDefault();
        }

        public static Participant GetAdc(this IEnumerable<Participant> participants)
        {
            return participants.Where(x => x.teamPosition == adc).FirstOrDefault();
        }

        public static Participant GetSupp(this IEnumerable<Participant> participants)
        {
            return participants.Where(x => x.teamPosition == supp).FirstOrDefault();
        }

        private static string top = "TOP";
        private static string jungle = "JUNGLE";
        private static string mid = "MIDDLE";
        private static string adc = "BOTTOM";
        private static string supp = "UTILITY";
    }
}
