using Microsoft.AspNetCore.Authorization;

namespace Web.Auth;
public class HasFunctionalityRequirement(string code) : IAuthorizationRequirement
{ public string Code => code; }