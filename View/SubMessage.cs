/*{
    "message": {
        "attributes": {
            "key": "value"
        },
        "data": "SGVsbG8gQ2xvdWQgUHViL1N1YiEgSGVyZSBpcyBteSBtZXNzYWdlIQ==",
        "messageId": "2070443601311540",
        "message_id": "2070443601311540",
        "publishTime": "2021-02-26T19:13:55.749Z",
        "publish_time": "2021-02-26T19:13:55.749Z"
    },
   "subscription": "projects/myproject/subscriptions/mysubscription"
}*/

using System.Collections.Generic;

namespace TaskHandler;
public class SubscribeMessage {
    public MessageInfo message { get; set; }
    public string subscription { get; set; }
}

public class MessageInfo {
    public Dictionary<string, string> attributes { get; set; }
    public string data { get; set; }
    public string messageId { get; set; }
    public string message_id { get; set; }
    public string publishTime { get; set; }
    public string publish_time { get; set; }
}