using TaskHandler;

var builder = WebApplication.CreateBuilder(args);

IConfiguration config = builder.Configuration;

var port = "8080";
var url = $"http://0.0.0.0:{port}";

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MongoHelper>();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// doc upload
app.MapGet("/trigger/pubsub", (string message) => {
    // TODO: download file from bucket
    // TODO: check file content
    // TODO: insert database
    // send pub/sub message
    MongoHelper db = app.Services.GetService<MongoHelper>();
    db.SimpleLog("start pubsub", $"topic message:{message}");
    PubSubService service = new PubSubService(db, config);
    int amount = Convert.ToInt32(config.GetValue<int>("TestAmount"));
    service.sendCrawler(message, amount);
    return Results.StatusCode(200);
})
.WithName("Pubsub");

// receive pub/sub message - crawler
app.MapPost("/trigger/crawler", (SubscribeMessage info) => {
    MongoHelper db = app.Services.GetService<MongoHelper>();
    CrawlerProcess model = new CrawlerProcess(db, info.message);
    try {
        if (model.isFinished(info.message.messageId)) {
            return Results.StatusCode(200);
        }
        int limit = Convert.ToInt32(config.GetValue<int>("TaskLimit"));
        switch (model.status(info.message, limit)) {
            case "keep pending":
                return Results.StatusCode(500);
            case "pending":
                SendToCrawler crawler = new SendToCrawler(db);
                crawler.start(config["Crawler"], model.task);
                return Results.StatusCode(102);
            case "crawling":
                if (model.forceFinished()) {
                    return Results.StatusCode(200);
                }
                return Results.StatusCode(102);
            case "finished":
            case "failed":
                return Results.StatusCode(200); // ack message
            default:
                return Results.StatusCode(500);
        }
    } finally {
        if (model.isDone()) {
            PubSubService service = new PubSubService(db, config);
            service.sendAI("trigger AI");
        }
    }
})
.WithName("Crawler");

// receive pub/sub message - AI
app.MapPost("/trigger/ai", (SubscribeMessage info) => {
    MongoHelper db = app.Services.GetService<MongoHelper>();
    AIProcess model = new AIProcess(db);
    if (model.isDone(info.message.messageId)) {
        return Results.StatusCode(200);
    }
    return Results.StatusCode(500);
})
.WithName("AI");

app.Run(url);