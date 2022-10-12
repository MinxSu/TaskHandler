namespace TaskHandler;

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class SendToCrawler {
    MongoHelper db { get; set; }
    public SendToCrawler(MongoHelper db) {
        this.db = db;
    }

    public async void start(string url, TaskInfo info) {
        Task.Run(() => {
            Request request = new Request() {
                taskID = info.taskID,
                subID = info.subID
            };
            send(url, request);
        });
    }

    private bool send(string url, Request request) {
        Logs log = new Logs() {
            ID = request.subID,
            step = "Send to crawler",
            message = JsonSerializer.Serialize(request)
        };
        using (HttpClient client = new HttpClient()) {
            try {
                db.CreateLog(log);
                client.Timeout = TimeSpan.FromMinutes(30); // cloud run response upper limit is 60 minutes
                HttpContent httpContent = new StringContent(log.message);
                httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = client.PostAsync(url, httpContent).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrWhiteSpace(responseBody)) {
                    log.message = $"Response:{responseBody}";
                    db.CreateLog(log); ;
                }
                return true;
            } catch (HttpRequestException e) {
                log.message = $"Exception Caught! Message :{e.Message} ";
                db.CreateLog(log);
                return false;
            }
        }
    }
}