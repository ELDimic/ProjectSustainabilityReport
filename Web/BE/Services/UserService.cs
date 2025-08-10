using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public class UserService(ApplicationDbContext db) : IUserService
{
	public async Task<Guid?> GetAppUserIdAsync(string identityUserId)
		=> await db.UsersProfile.Where(u => u.IdentityUserId == identityUserId)
								.Select(u => (Guid?)u.Id).FirstOrDefaultAsync();

	public async Task<bool> HasFunctionalityAsync(string identityUserId, string functionalityCode)
	{
		return await db.UserRoles
			.Where(ur => ur.User.IdentityUserId == identityUserId)
			.SelectMany(ur => ur.UserRoleFunctionalities)
			.AnyAsync(urf => urf.Functionality.Code == functionalityCode);
	}

	public async Task TouchLastAccessAsync(string identityUserId)
	{
		var u = await db.UsersProfile.FirstOrDefaultAsync(x => x.IdentityUserId == identityUserId);
		if (u is null) return;
		u.LastAccessAt = DateTimeOffset.UtcNow;
		await db.SaveChangesAsync();
	}
}