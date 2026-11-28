using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace AffiliateMessages.Api;

public static class HostLifecycleEndpoints
{
    public static void Map(WebApplication application)
    {
        application.MapPost("/api/host/shutdown", HandleShutdownAsync);
    }

    private static Task HandleShutdownAsync(
        HttpContext context,
        ConversionState state,
        IHostApplicationLifetime lifetime)
    {
        if (!state.TryBeginShutdown())
        {
            return ApiProblemDetails.WriteAsync(
                context,
                StatusCodes.Status409Conflict,
                "conversion_in_progress",
                "Não é possível encerrar agora",
                "Aguarde a conversão atual terminar antes de encerrar a aplicação.");
        }

        context.Response.StatusCode = StatusCodes.Status202Accepted;
        context.Response.OnCompleted(() =>
        {
            lifetime.StopApplication();
            return Task.CompletedTask;
        });
        return Task.CompletedTask;
    }
}
