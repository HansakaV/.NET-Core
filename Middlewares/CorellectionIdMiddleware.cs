namespace StudentManagement.API.Middlewares
{
    public sealed class CorellectionIdMiddleware
    {
        private const string Headername = "X-Correlation-ID";
        private readonly RequestDelegate _next;

        public CorellectionIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext , ILogger<CorellectionIdMiddleware> logger)
        {
            var corellectionId = GetOrCreateCorellectionId(httpContext);
            httpContext.Items[Headername] = corellectionId;
            httpContext.Response.Headers[Headername] = corellectionId;

            using (logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = corellectionId
            }))
            {
                await _next(httpContext);
            }
        }
        
        private static string GetOrCreateCorellectionId(HttpContext context)
        {
            if(context.Request.Headers.TryGetValue(Headername, out var exitingCorellectionId)
            && !string.IsNullOrWhiteSpace(exitingCorellectionId))
            {
                return exitingCorellectionId.ToString();
            }
            return Guid.NewGuid().ToString();
        }
    }
}