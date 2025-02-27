using Microsoft.EntityFrameworkCore;
namespace SokeBot
{
    public class BotDb : DbContext
    {
        public DbSet<RitoPlayer> RitoPlayers { get; set; }
        public DbSet<GameInProgress> GamesInProgress { get; set; }
        public DbSet<ReportChannel> ReportChannels { get; set; }

        public string DbPath { get; }

        public BotDb()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = System.IO.Path.Join(path, "soke.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }

    public class RitoPlayer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Tag { get; set; }
        public string Puuid { get; set; }
        public string? Server { get; set; }
        public virtual GameInProgress GameInProgress { get; set; }
        public virtual List<ReportChannel> ReportChannels { get; set; }

        public override string ToString()
        {
            return $"[Id: {Id}, Name: {Name}, Tag: {Tag}, Puuid: {Puuid}, GameInProgress: {GameInProgress?.GameId}\n{string.Join('\n', ReportChannels.Select(x => $"[{x.GuildId} {x.ChannelId}]"))}]";
        }
    }

    public class GameInProgress
    {
        public int Id { get; set; }
        public long GameId { get; set; }
        public int PlayerId { get; set; }
        public virtual RitoPlayer Player { get; set; }

        public override string ToString()
        {
            return $"[Id: {Id}, GameId: {GameId}, PlayerId: {PlayerId}]";
        }
    }

    public class ReportChannel
    {
        public int Id { get; set; }
        public ulong? GuildId { get; set; }
        public ulong? ChannelId { get; set; }
    }
}