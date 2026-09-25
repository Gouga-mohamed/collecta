using CollectA.Application.Common.Interfaces;
using CollectA.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace CollectA.Infrastructure.Services;

public class UserLookupService : IUserLookupService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserLookupService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string?> GetUserFullNameAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.FullName;
    }
}
