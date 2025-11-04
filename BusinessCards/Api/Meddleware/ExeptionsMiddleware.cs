using Domain.Helper;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace Api.Meddleware;

public class ExeptionsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _rateLimitWindow = TimeSpan.FromSeconds(30);
    public ExeptionsMiddleware(RequestDelegate next, IHostEnvironment hostEnvironment, IMemoryCache memoryCache)
    {
        _next = next;
        _hostEnvironment = hostEnvironment;
        _memoryCache = memoryCache;
    }
    public async Task InvokeAsync(HttpContext context)
    {

        if (!IsRequestAllowed(context))
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";
                var response = ApiResponse<object>.FailureResponse("To many requests. Please try again later.", nameof(HttpStatusCode.TooManyRequests));
                await context.Response.WriteAsJsonAsync(response);
            }
            return;
        }

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                var response = _hostEnvironment.IsDevelopment()
                    ? ApiResponse<object>.FailureResponse($"{ex.Message}\n {ex.StackTrace}",nameof(HttpStatusCode.InternalServerError)) 
                    : ApiResponse<object>.FailureResponse("An internal server error occurred.",nameof(HttpStatusCode.InternalServerError));
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }

    private bool IsRequestAllowed(HttpContext httpContext)
    {
        var ip = httpContext.Connection.RemoteIpAddress!.ToString();
        var cacheKey = $"RateLimit_{ip}";
        var now = DateTime.Now;

        if (!_memoryCache.TryGetValue(cacheKey, out (DateTime timestamp, int count) cacheEntry))
        {
            cacheEntry = (now, 1);
            _memoryCache.Set(cacheKey, cacheEntry, _rateLimitWindow);
            return true;
        }

        if (now - cacheEntry.timestamp < _rateLimitWindow)
        {
            if (cacheEntry.count >= 8)
            {
                return false;
            }

            cacheEntry.count++;
            _memoryCache.Set(cacheKey, cacheEntry, _rateLimitWindow);
            return true;
        }
        else
        {
            cacheEntry = (now, 1);
            _memoryCache.Set(cacheKey, cacheEntry, _rateLimitWindow);
            return true;
        }
    }
}
