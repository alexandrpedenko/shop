using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Shop.Core.Middlewares
{
    public class AuthorizationLoggerMiddleware(RequestDelegate next, ILogger<AuthorizationLoggerMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<AuthorizationLoggerMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString();
                var endpoint = context.GetEndpoint()?.DisplayName;

                _logger.LogWarning("Unauthorized access attempt. IP: {IpAddress}, Endpoint: {Endpoint}", ipAddress, endpoint);
            }

            if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString();
                var endpoint = context.GetEndpoint()?.DisplayName;

                _logger.LogWarning(
                    "Forbidden access. Role mismatch or insufficient permissions. IP: {IpAddress}, Endpoint: {Endpoint}",
                    ipAddress,
                    endpoint);
            }
        }
    }
}