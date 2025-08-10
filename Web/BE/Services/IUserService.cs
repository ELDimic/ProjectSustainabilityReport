namespace Web.Services;
public interface IUserService
{
    Task<Guid?> GetAppUserIdAsync(string identityUserId);
    Task<bool> HasFunctionalityAsync(string identityUserId, string functionalityCode);
    Task TouchLastAccessAsync(string identityUserId);
}