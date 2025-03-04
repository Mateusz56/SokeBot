using Newtonsoft.Json;

namespace SokeBot
{
    public class QueueConfig
    {
        public int queueId { get; set; }
        public string description { get; set; }
    }

    public static class QueueConfigProvider
    {
        private static QueueConfig[] configs;

        public static string GetQueueDescription(int id)
        {
            if(configs == null)
            {
                var configString = File.OpenText("QueueConfig.json").ReadToEnd();
                configs = JsonConvert.DeserializeObject<QueueConfig[]>(configString);
            }

            return configs.Where(x => x.queueId == id).First().description;
        }
    }
}
