using Serilog;
using StackExchange.Redis;

var logFolderPath = "Shop.Warehouse.Logger/Logs";

// Ensure the log folder exists
if (!Directory.Exists(logFolderPath))
{
    Directory.CreateDirectory(logFolderPath);
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File($"{logFolderPath}/log-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var redis = ConnectionMultiplexer.Connect("localhost:6379");
var subscriber = redis.GetSubscriber();

Log.Information("Listening for messages from main application");

await subscriber.SubscribeAsync("order_channel", (channel, message) =>
{
    Log.Information("Received message: {Message}", message);
});

Console.ReadLine();