using QIMy.Core.Interfaces;

namespace QIMy.API.Middleware;

/// <summary>
/// Resolves BusinessId (tenant) for API requests.
/// Strategy:
/// - Prefer header: X-Business-Id
/// - Fallback to query: businessId
/// If no BusinessId is provided, tenant-scoped entities will be filtered out by EF global query filters.
/// </summary>
public sealed class BusinessIdAccessorMiddleware
{
    private const string HeaderName = "X-Business-Id";
    private const string QueryName = "businessId";

    private readonly RequestDelegate _next;

    public BusinessIdAccessorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentBusinessIdAccessor accessor)
    {
        if (!accessor.BypassTenantFilter)
        {
            if (TryGetBusinessIdFromHeader(context, out var businessId) || TryGetBusinessIdFromQuery(context, out businessId))
            {
                accessor.CurrentBusinessId = businessId;
            }
        }

        await _next(context);
    }

    private static bool TryGetBusinessIdFromHeader(HttpContext context, out int businessId)
    {
        businessId = 0;
        if (!context.Request.Headers.TryGetValue(HeaderName, out var values))
            return false;

        var raw = values.FirstOrDefault();
        return int.TryParse(raw, out businessId) && businessId > 0;
    }

    private static bool TryGetBusinessIdFromQuery(HttpContext context, out int businessId)
    {
        businessId = 0;
        if (!context.Request.Query.TryGetValue(QueryName, out var values))
            return false;

        var raw = values.FirstOrDefault();
        return int.TryParse(raw, out businessId) && businessId > 0;
    }
}
