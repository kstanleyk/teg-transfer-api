using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Core.Logger.Middleware;

// based on Microsoft's standard exception middleware found here:
// https://github.com/aspnet/Diagnostics/tree/dev/src/
//         Microsoft.AspNetCore.Diagnostics/ExceptionHandler
public sealed class CustomExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly DiagnosticSource _dialogSource;
    private readonly ExceptionHandlerOptions _options;
    private readonly Func<object, Task> _clearCacheHeadersDelegate;
    private readonly string _product;
    private readonly string _layer;

    public CustomExceptionHandlerMiddleware(string product, string layer,
        RequestDelegate next,
        IOptions<ExceptionHandlerOptions> options,
        DiagnosticSource dialogSource)
    {
        _product = product;
        _layer = layer;

        _next = next;
        _dialogSource = dialogSource;
        _options = options.Value;
        _clearCacheHeadersDelegate = ClearCacheHeaders;
        _options.ExceptionHandler ??= _next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            WebHelper.LogWebError(_product, _layer, ex, context);

            var originalPath = context.Request.Path;
            if (_options.ExceptionHandlingPath.HasValue)
            {
                context.Request.Path = _options.ExceptionHandlingPath;
            }

            context.Response.Clear();
            if (originalPath.Value != null)
            {
                var exceptionHandlerFeature = new ExceptionHandlerFeature()
                {
                    Error = ex,
                    Path = originalPath.Value,
                };

                context.Features.Set<IExceptionHandlerFeature>(exceptionHandlerFeature);
                context.Features.Set<IExceptionHandlerPathFeature>(exceptionHandlerFeature);
            }

            context.Response.StatusCode = 500;
            context.Response.OnStarting(_clearCacheHeadersDelegate, context.Response);

            await _options.ExceptionHandler?.Invoke(context)!;
        }
    }

    private static Task ClearCacheHeaders(object state)
    {
        var response = (HttpResponse)state;
        response.Headers[HeaderNames.CacheControl] = "no-cache";
        response.Headers[HeaderNames.Pragma] = "no-cache";
        response.Headers[HeaderNames.Expires] = "-1";
        response.Headers.Remove(HeaderNames.ETag);
        return Task.CompletedTask;
    }
}