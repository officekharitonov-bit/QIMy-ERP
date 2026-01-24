using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QIMy.Core.Entities;
using QIMy.Infrastructure.Data;

namespace QIMy.Web.Services;

/// <summary>
/// Tracks the current business for the logged-in user and provides their business list.
/// </summary>
public class BusinessContext
{
    private readonly ApplicationDbContext _db;
    private readonly AuthenticationStateProvider _auth;
    private readonly UserManager<AppUser> _userManager;

    public int? CurrentBusinessId { get; private set; }
    public string? CurrentBusinessName { get; private set; }

    public event Action? Changed;

    public BusinessContext(ApplicationDbContext db, AuthenticationStateProvider auth, UserManager<AppUser> userManager)
    {
        _db = db;
        _auth = auth;
        _userManager = userManager;
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
        var user = await GetCurrentUserAsync();
        if (user == null) return;
        if (user.BusinessId.HasValue)
            await SetBusinessAsync(user.BusinessId.Value, saveDefault:false);
        else
        {
            // Set first available business as default
            var first = await _db.UserBusinesses
                .Include(ub => ub.Business)
                .Where(ub => ub.UserId == user.Id)
                .Select(ub => ub.Business)
                .OrderBy(b => b!.Name)
                .FirstOrDefaultAsync();
            if (first != null)
                await SetBusinessAsync(first.Id, saveDefault:false);
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
        var b = await _db.Businesses.FirstOrDefaultAsync(x => x.Id == businessId);
        CurrentBusinessId = b?.Id;
        CurrentBusinessName = b?.Name;

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