using MongoDB.Bson.Serialization.Attributes;

namespace TaskHandler;

public class Request {
    public string taskID { get; set; }
    public string subID { get; set; }
}

[BsonIgnoreExtraElements]
public class TaskInfo : Request {
    public string status { get; set; }
    public DateTime createTime { get; set; }
    public DateTime? updateTime { get; set; }
}


public class Logs {
    public string service = "Task Handler";
    public string ID { get; set; }
    public string step { get; set; }
    public string message { get; set; }
    public DateTime systemTime = DateTime.Now;
}