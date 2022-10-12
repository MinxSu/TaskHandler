namespace TaskHandler;

using System.Linq;

public class CrawlerProcess {

    MongoHelper db { get; set; }
    public CrawlerProcess(MongoHelper db, MessageInfo message) {
        this.db = db;
        task = db.ReadTask(message.messageId); // read task info from database
    }
    static Dictionary<string, Status> messages = new Dictionary<string, Status>();
    // static Dictionary<string, bool> crawlingStack { get; set; }
    static object msgLock = new object();

    public TaskInfo task { get; private set; }

    public string status(MessageInfo message, int limit) {

        InsertTask(message); // for test

        // check task status
        int taskAmt = messages.Count(o => !o.Value.isFinished);
        if (task.status == "pending" && taskAmt >= limit) {
            return "keep pending";
        } else if (!messages.ContainsKey(task.subID)) {
            // if cloud run rebuild
            append(message.messageId, task.createTime);
        } else if (task.status == "finished") {
            setFinished(task.subID);
        }
        Logs log = new Logs() {
            ID = message.messageId,
            step = "Got Message",
            message = $"status:{task.status}, current task:{messages.Count(o => !o.Value.isFinished)}",
        };
        db.CreateLog(log);
        return task.status;
    }

    /// <summary>
    /// 此段為模擬測試用，task資料應在 trigger/pubsub 寫入DB
    /// </summary>
    /// <param name="message"></param>
    private void InsertTask(MessageInfo message) {
        if (task == null) {
            task = new TaskInfo() {
                taskID = message.data,
                subID = message.messageId,
                status = "pending",
                createTime = DateTime.Now
            };
            db.InsertTask(task);
            append(message.messageId, DateTime.Now);
        }
    }

    public bool isFinished(string subID) {
        return messages.ContainsKey(subID) && messages[subID].isFinished;
    }

    public bool isDone() {
        if (task == null) {
            return false;
        }
        List<TaskInfo> tasks = db.ReadAllTasks(task.taskID);
        // 判斷是否所有task都已執行完成
        if (tasks != null && !tasks.Any(o => o.status == "pending" || o.status == "crawling")) {
            return true;
        }
        return false;
    }

    public void setFinished(string subID) {
        if (!messages.ContainsKey(subID)) {
            messages[subID].isFinished = true;
        }
    }

    public bool forceFinished() {
        if (messages.ContainsKey(task.subID) && overMinutes(messages[task.subID])) {
            messages[task.subID].isFinished = true;
            task.status = "force finished:timeout";
            task.updateTime = DateTime.Now;
            db.UpdateTask(task);
            setFinished(task.subID);
            return true;
        }
        return false;
    }

    private bool overMinutes(Status status) {
        DateTime dt = DateTime.Now.AddMinutes(-35);
        if (status.startTime < dt) {
            return true;
        }
        return false;
    }


    public void append(string messageID, DateTime createTime) {
        lock (msgLock) {
            messages.Add(messageID, new Status() {
                isFinished = false,
                startTime = createTime
            });
        }
    }

    class Status {
        public bool isFinished { get; set; }
        public DateTime? startTime { get; set; }
    }
}