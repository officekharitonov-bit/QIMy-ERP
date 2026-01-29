using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;
using QIMy.Core.Interfaces;
using QIMy.Infrastructure.Data;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;

namespace QIMy.Web.Services;

/// <summary>
/// Tracks the current business for the logged-in user and provides their business list.
/// CRITICAL: Each business is a separate isolated database - ALL queries MUST filter by BusinessId
/// </summary>
public class BusinessContext
{
    private readonly ApplicationDbContext _db;
    private readonly AuthenticationStateProvider _auth;
    private readonly UserManager<AppUser> _userManager;
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ILogger<BusinessContext> _logger;
    private readonly ICurrentBusinessIdAccessor _businessIdAccessor;
    private const string SESSION_KEY = "CurrentBusinessId";

    public int? CurrentBusinessId { get; private set; }
    public string? CurrentBusinessName { get; private set; }

    public event Action? Changed;

    public BusinessContext(
        ApplicationDbContext db,
        AuthenticationStateProvider auth,
        UserManager<AppUser> userManager,
        ProtectedSessionStorage sessionStorage,
        ILogger<BusinessContext> logger,
        ICurrentBusinessIdAccessor businessIdAccessor)
    {
        _db = db;
        _auth = auth;
        _userManager = userManager;
        _sessionStorage = sessionStorage;
        _logger = logger;
        _businessIdAccessor = businessIdAccessor;
    }

    private async Task<AppUser?> GetCurrentUserAsync()
    {
        var state = await _auth.GetAuthenticationStateAsync();
        var user = state.User;
        if (user?.Identity?.IsAuthenticated != true)
            return null;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return null;
        return await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("🔍 InitializeAsync called");

        // 1. Try to load from session (priority #1 - user's current choice)
        try
        {
            var result = await _sessionStorage.GetAsync<int>(SESSION_KEY);
            _logger.LogInformation("📦 Session storage result: Success={Success}, Value={Value}", result.Success, result.Value);

            if (result.Success && result.Value > 0)
            {
                var business = await _db.Businesses.FirstOrDefaultAsync(b => b.Id == result.Value);
                if (business != null)
                {
                    CurrentBusinessId = business.Id;
                    CurrentBusinessName = business.Name;
                    _businessIdAccessor.CurrentBusinessId = business.Id;
                    _logger.LogInformation("✅ Loaded from SESSION: BusinessId={Id}, Name={Name}", CurrentBusinessId, CurrentBusinessName);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Session not available yet, continuing to user default");
        }

        // 2. Load from user's saved default
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            _logger.LogWarning("⚠️ No authenticated user found");
            return;
        }

        _logger.LogInformation("👤 User BusinessId={BusinessId}", user.BusinessId);

        if (user.BusinessId.HasValue)
        {
            await SetBusinessAsync(user.BusinessId.Value, saveDefault: false);
        }
        else
        {
            // 3. Set first available business as fallback
            var first = await _db.UserBusinesses
                .Include(ub => ub.Business)
                .Where(ub => ub.UserId == user.Id)
                .Select(ub => ub.Business)
                .OrderBy(b => b!.Name)
                .FirstOrDefaultAsync();
            if (first != null)
                await SetBusinessAsync(first.Id, saveDefault: false);
        }
    }

    public async Task<List<Business>> GetUserBusinessesAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return new List<Business>();
        return await _db.UserBusinesses
            .Include(ub => ub.Business)
            .Where(ub => ub.UserId == user.Id)
            .Select(ub => ub.Business)
            .Where(b => b != null)!
            .OrderBy(b => b!.Name)
            .ToListAsync() as List<Business>;
    }

    public async Task SetBusinessAsync(int businessId, bool saveDefault = true)
    {
        _logger.LogInformation("🔄 SetBusinessAsync called: BusinessId={BusinessId}, SaveDefault={SaveDefault}", businessId, saveDefault);

        var b = await _db.Businesses.FirstOrDefaultAsync(x => x.Id == businessId);
        if (b == null)
        {
            _logger.LogWarning("⚠️ Business {BusinessId} not found", businessId);
            return;
        }

        CurrentBusinessId = b.Id;
        CurrentBusinessName = b.Name;

        // Ensure EF Core query filters / guards see the new tenant immediately.
        _businessIdAccessor.CurrentBusinessId = b.Id;

        // Avoid accidental cross-business writes from previously tracked entities.
        _db.ChangeTracker.Clear();

        // CRITICAL: Save to session for isolation between businesses
        try
        {
            await _sessionStorage.SetAsync(SESSION_KEY, b.Id);
            _logger.LogInformation("✅ Saved to SESSION: BusinessId={Id}, Name={Name}", b.Id, b.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to save to session storage");
        }

        if (saveDefault)
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                user.BusinessId = CurrentBusinessId;
                _db.Update(user);
                await _db.SaveChangesAsync();
            }
        }

        Changed?.Invoke();
    }
}
