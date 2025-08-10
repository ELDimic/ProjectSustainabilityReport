namespace Api.Services;
public interface IUserService
{
    Task<int> GetAppUserIdAsync(string identityUserId);
    Task<bool> HasFunctionalityAsync(string identityUserId, string functionalityCode);
    Task TouchLastAccessAsync(string identityUserId);
}