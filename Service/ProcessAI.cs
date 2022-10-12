namespace TaskHandler;

public class AIProcess {

    static DateTime? startTime { get; set; }
    public bool check(MongoHelper db, string taskID) {
        if (startTime == null) {
            startTime = DateTime.Now;
            db.CreateLog(new Logs() {
                step = "start AI",
                ID = taskID
            });
            return false;
        }
        if (DateTime.Now.AddMinutes(-45) > startTime) {
            db.CreateLog(new Logs() {
                step = "AI Done",
                ID = taskID
            });
            startTime = null;
            return true;
        }
        db.CreateLog(new Logs() {
            step = "AI Pending",
            ID = taskID
        });
        return false;
    }
}