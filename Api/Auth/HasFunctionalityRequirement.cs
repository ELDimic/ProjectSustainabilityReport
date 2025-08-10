using Microsoft.AspNetCore.Authorization;
namespace Api.Auth;
public class HasFunctionalityRequirement(string code) : IAuthorizationRequirement { public string Code => code; }