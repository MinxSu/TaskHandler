using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using TaskHandler;

public class PubSubService {
    IConfiguration config { get; set; }
    MongoHelper db { get; set; }

    public PubSubService(MongoHelper db, IConfiguration config) {
        this.config = config;
        this.db = db;
    }

    public bool sendCrawler(string message, int amount) {
        try {
            string project = config["GCP:Project"];
            string topic = config["GCP:TOPICS:CRAWLER"];
            db.SimpleLog("send to pub/sub Crawler", $"Project={project}, Topic={topic}");
            List<string> messageList = new List<string>();
            for (int i = 0; i < amount; i++) {
                messageList.Add(message);
            }
            int res = Task.Run(() => {
                return PublishMessagesAsync(project, topic, messageList);
            }).Result;
            return amount == res;
        } catch (Exception ex) {
            db.SimpleLog("Send Crawler Failed", ex.ToString());
            return false;
        }
    }

    public bool sendAI(string message) {
        try {
            List<string> messageList = new List<string>() { message };
            string project = config["GCP:Project"];
            string topic = config["GCP:TOPICS:AI"];
            db.SimpleLog("send to pub/sub AI", $"Project={project}, Topic={topic}");
            int res = Task.Run(() => {
                return PublishBatchMessagesAsync(config["GCP:Project"], config["GCP:TOPICS:AI"], messageList);
            }).Result;
            return res == 1;
        } catch (Exception ex) {
            db.SimpleLog("Send AI Failed", ex.ToString());
            return false;
        }
    }

    public async Task<int> PublishMessagesAsync(string projectId, string topicId, IEnumerable<string> messageTexts) {
        TopicName topicName = TopicName.FromProjectTopic(projectId, topicId);
        PublisherClient publisher = await PublisherClient.CreateAsync(topicName);

        int publishedMessageCount = 0;
        var publishTasks = messageTexts.Select(async text => {
            try {
                string message = await publisher.PublishAsync(text);
                Console.WriteLine($"Published message {message}");
                Interlocked.Increment(ref publishedMessageCount);
            } catch (Exception exception) {
                Console.WriteLine($"An error ocurred when publishing message {text}: {exception.Message}");
            }
        });
        await Task.WhenAll(publishTasks);
        return publishedMessageCount;
    }

    public async Task<int> PublishBatchMessagesAsync(string projectId, string topicId, IEnumerable<string> messageTexts) {
        TopicName topicName = TopicName.FromProjectTopic(projectId, topicId);

        // Default Settings:
        // byteCountThreshold: 1000000
        // elementCountThreshold: 100
        // delayThreshold: 10 milliseconds
        var customSettings = new PublisherClient.Settings {
            BatchingSettings = new BatchingSettings(
                elementCountThreshold: 50,
                byteCountThreshold: 10240,
                delayThreshold: TimeSpan.FromMilliseconds(100))
        };

        PublisherClient publisher = await new PublisherClientBuilder {
            TopicName = topicName,
            Settings = customSettings
        }.BuildAsync();

        int publishedMessageCount = 0;
        var publishTasks = messageTexts.Select(async text => {
            try {
                string message = await publisher.PublishAsync(text);
                Console.WriteLine($"Published message {message}");
                Interlocked.Increment(ref publishedMessageCount);
            } catch (Exception exception) {
                Console.WriteLine($"An error occurred when publishing message {text}: {exception.Message}");
            }
        });
        await Task.WhenAll(publishTasks);
        return publishedMessageCount;
    }
}