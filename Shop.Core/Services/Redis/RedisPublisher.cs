using StackExchange.Redis;

namespace Shop.Core.Services.Redis
{
    public sealed class RedisPublisher(IConnectionMultiplexer redis)
    {
        private readonly IConnectionMultiplexer _redis = redis;

        public async Task PublishAsync(string channel, string message)
        {
            var pub = _redis.GetSubscriber();
            await pub.PublishAsync(channel, message);
        }
    }
}