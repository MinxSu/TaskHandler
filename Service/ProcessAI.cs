namespace TaskHandler;

public class AIProcess {
    static DateTime? startTime { get; set; }
    MongoHelper db { get; set; }
    public AIProcess(MongoHelper db) {
        this.db = db;
    }


    public bool isDone(string taskID) {
        if (startTime == null) {
            Start(taskID);
            return false;
        }
        if (CheckDone(taskID)) {
            return true;
        }
        HealthCheck();
        return false;
    }

    public void Start(string taskID) {
        // TODO: call AI start API
        db.CreateLog(new Logs() {
            step = "start AI",
            ID = taskID
        });
        startTime = DateTime.Now;
    }

    public bool CheckDone(string taskID) {
        MainTask task = db.ReadMainTask(taskID);
        return task.status == "Finished";
    }


    public void HealthCheck() {
        Task.Run(() => {
            // TODO: call AI HealthCheck
        });
    }
}