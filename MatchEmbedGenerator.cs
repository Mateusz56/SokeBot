using Discord;
using SokeBot.DataModel.RiotApi;
using System.Text;

namespace SokeBot
{
    public class MatchEmbedGenerator
    {
        private int maxDamage;
        private IEnumerable<string> puuids;
        private bool win;
        private readonly string winImage = "https://mir-s3-cdn-cf.behance.net/projects/max_808/cc5ba853065555.Y3JvcCw2MzQsNDk2LDYyOSwxNjI.png";
        private readonly string defeatImage = "https://i.pinimg.com/736x/3e/7c/04/3e7c048da4c2153fd459f35a35a7d9a4.jpg";
        private string BuildPlayerString(Participant participant)
        {
            int totalDamage = GetTotalDamage(participant);
            string playerString;
            if (puuids.Any(x => x == participant.puuid))
            {
                win = participant.win;
                playerString = participant.riotIdGameName.EmphasizeNick();
            }
            else
                playerString = participant.riotIdGameName;

            if (participant.win)
                playerString = playerString.EmphasizeWinner();

            return $"{playerString}  --  {participant.championName}  -  {participant.kills}🗡️ {participant.deaths}💀 {participant.assists}🖐️";//\n{GetDamageSquares(totalDamage)} {totalDamage} Dmg";
        }

        private int GetTotalDamage(Participant participant)
        {
            return participant.magicDamageDealtToChampions + participant.physicalDamageDealtToChampions + participant.trueDamageDealtToChampions;
        }

        public Embed BuildEmbed2(RiotMatch match, IEnumerable<string> puuids)
        {
            maxDamage = match.info.participants.Max(GetTotalDamage);
            this.puuids = puuids.ToList();
            var playerNames = match.info.participants.Where(x => puuids.Contains(x.puuid)).Select(x => x.riotIdGameName).ToList();
            var team1 = match.info.participants.Where(x => x.teamId == 100);
            var team2 = match.info.participants.Where(x => x.teamId == 200);

            return new EmbedBuilder()
                .WithTitle(QueueConfigProvider.GetQueueDescription(match.info.queueId) + " - " + string.Join(", ", playerNames))
                .AddField("Kills", match.info.teams.FirstOrDefault(x => x.teamId == 100).objectives.champion.kills + " / " + match.info.teams.FirstOrDefault(x => x.teamId == 200).objectives.champion.kills)
                .AddField("Top", BuildPlayerString(team1.GetTop()))
                .AddField("Jungle", BuildPlayerString(team1.GetJungle()))
                .AddField("Mid", BuildPlayerString(team1.GetMid()))
                .AddField("Adc", BuildPlayerString(team1.GetAdc()))
                .AddField("Supp", BuildPlayerString(team1.GetSupp()))
                .AddField("VS", "Versus")
                .AddField("Top", BuildPlayerString(team2.GetTop()))
                .AddField("Jungle", BuildPlayerString(team2.GetJungle()))
                .AddField("Mid", BuildPlayerString(team2.GetMid()))
                .AddField("Adc", BuildPlayerString(team2.GetAdc()))
                .AddField("Supp", BuildPlayerString(team2.GetSupp()))
                .WithImageUrl(win ? winImage : defeatImage)
                .Build();
        }

        public Embed BuildEmbed(RiotMatch match, IEnumerable<string> puuids)
        {
            maxDamage = match.info.participants.Max(GetTotalDamage);
            this.puuids = puuids.ToList();
            var playerNames = match.info.participants.Where(x => puuids.Contains(x.puuid)).Select(x => x.riotIdGameName).ToList();
            var team1 = match.info.participants.Where(x => x.teamId == 100);
            var team2 = match.info.participants.Where(x => x.teamId == 200);

            var team1String = new StringBuilder()
                .Append("**Top** ")
                .Append(BuildPlayerString(team1.GetTop()))
                .Append("\n")
                .Append("**Jungle** ")
                .Append(BuildPlayerString(team1.GetJungle()))
                .Append("\n")
                .Append("**Mid** ")
                .Append(BuildPlayerString(team1.GetMid()))
                .Append("\n")
                .Append("**Adc** ")
                .Append(BuildPlayerString(team1.GetAdc()))
                .Append("\n")
                .Append("**Supp** ")
                .Append(BuildPlayerString(team1.GetSupp()))
                .ToString();

            var team2String = new StringBuilder()
                .Append("**Top** ")
                .Append(BuildPlayerString(team2.GetTop()))
                .Append("\n")
                .Append("**Jungle** ")
                .Append(BuildPlayerString(team2.GetJungle()))
                .Append("\n")
                .Append("**Mid** ")
                .Append(BuildPlayerString(team2.GetMid()))
                .Append("\n")
                .Append("**Adc** ")
                .Append(BuildPlayerString(team2.GetAdc()))
                .Append("\n")
                .Append("**Supp** ")
                .Append(BuildPlayerString(team2.GetSupp()))
                .ToString();

            return new EmbedBuilder()
                .WithTitle(QueueConfigProvider.GetQueueDescription(match.info.queueId) + " - " + string.Join(", ", playerNames))
                .AddField("Kills", match.info.teams.FirstOrDefault(x => x.teamId == 100).objectives.champion.kills + " / " + match.info.teams.FirstOrDefault(x => x.teamId == 200).objectives.champion.kills)
                .AddField("Team 1", team1String)
                .AddField("Team 2", team2String)
                .WithImageUrl(win ? winImage : defeatImage)
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
