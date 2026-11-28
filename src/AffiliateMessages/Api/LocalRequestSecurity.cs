using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace AffiliateMessages.Api;

public sealed class LocalRequestSecurity
{
    private readonly RequestDelegate next;
    private readonly LocalRequestSecurityOptions options;

    public LocalRequestSecurity(RequestDelegate next, LocalRequestSecurityOptions options)
    {
        this.next = next;
        this.options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var expectedHost = options.ExpectedHost;
        var csrfToken = Encoding.UTF8.GetBytes(options.CsrfToken);
        if (!string.Equals(context.Request.Host.Value, expectedHost, StringComparison.OrdinalIgnoreCase))
        {
            await RejectAsync(context, "A solicitação não pertence a este host local.");
            return;
        }

        if (context.Request.Path.Equals("/api/session", StringComparison.OrdinalIgnoreCase) &&
            HttpMethods.IsGet(context.Request.Method))
        {
            var fetchSite = context.Request.Headers["Sec-Fetch-Site"];
            if (fetchSite.Count > 0 && !string.Equals(fetchSite, "same-origin", StringComparison.Ordinal))
            {
                await RejectAsync(context, "A origem da solicitação não é permitida.");
                return;
            }

            context.Response.Headers.CacheControl = "no-store";
            context.Response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        }
        else if (context.Request.Path.StartsWithSegments("/api") &&
                 (HttpMethods.IsPost(context.Request.Method) ||
                  HttpMethods.IsPut(context.Request.Method) ||
                  HttpMethods.IsPatch(context.Request.Method) ||
                  HttpMethods.IsDelete(context.Request.Method)))
        {
            var origin = context.Request.Headers.Origin.ToString();
            var fetchSite = context.Request.Headers["Sec-Fetch-Site"].ToString();
            var suppliedToken = Encoding.UTF8.GetBytes(context.Request.Headers["X-CSRF-Token"].ToString());

            if (!string.Equals(origin, "http://" + expectedHost, StringComparison.Ordinal) ||
                !string.Equals(fetchSite, "same-origin", StringComparison.Ordinal) ||
                !CryptographicOperations.FixedTimeEquals(suppliedToken, csrfToken))
            {
                await RejectAsync(context, "A origem ou o token de segurança da solicitação é inválido.");
                return;
            }
        }

        await next(context);
    }

    private static Task RejectAsync(HttpContext context, string detail)
    {
        return ApiProblemDetails.WriteAsync(
            context,
            StatusCodes.Status403Forbidden,
            "request_forbidden",
            "Solicitação não permitida",
            detail,
            context.RequestAborted);
    }
}

public sealed class LocalRequestSecurityOptions
{
    public string ExpectedHost { get; set; } = string.Empty;
    public string CsrfToken { get; set; } = string.Empty;
}
