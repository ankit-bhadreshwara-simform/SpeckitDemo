using System.Net;
using System.Text.Json;
using SpeckitDemo.Contracts.Envelope;

namespace SpeckitDemo.Api.Pipeline;

public sealed class UnhandledErrorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UnhandledErrorMiddleware> _logger;

    public UnhandledErrorMiddleware(RequestDelegate next, ILogger<UnhandledErrorMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Request failed with an unhandled exception.");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var payload = JsonSerializer.Serialize(
                ResultEnvelope<object>.Fail("An unexpected error occurred."),
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await context.Response.WriteAsync(payload);
        }
    }
}
