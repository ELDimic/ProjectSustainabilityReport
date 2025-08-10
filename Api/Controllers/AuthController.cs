using Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(UserManager<ApplicationUser> userMgr, ApplicationDbContext db, IConfiguration cfg) : ControllerBase
{
    public record LoginReq(string Email, string Password);
    public record LoginRes(string Token, string Email, string? FullName, string[] Functionalities);

    [HttpPost("login")]
    public async Task<ActionResult<LoginRes>> Login([FromBody] LoginReq req)
    {
        var identity = await userMgr.FindByEmailAsync(req.Email);
        if (identity is null) return Unauthorized();
        if (!await userMgr.CheckPasswordAsync(identity, req.Password)) return Unauthorized();

        var appUser = await db.UsersProfile.FirstOrDefaultAsync(u => u.IdentityUserId == identity.Id);
        if (appUser is null) return Unauthorized();

        var fun = await db.UserRoles.Where(ur => ur.UserId == appUser.Id)
                          .SelectMany(ur => ur.UserRoleFunctionalities)
                          .Select(urf => urf.Functionality.Code).Distinct().ToArrayAsync();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, identity.Id),
            new Claim(ClaimTypes.Email, identity.Email ?? req.Email),
            new Claim("fullName", $"{appUser.FirstName} {appUser.LastName}")
        };
        claims.AddRange(fun.Select(f => new Claim("func", f)));
        var token = new JwtSecurityToken(
            issuer: cfg["Jwt:Issuer"], audience: cfg["Jwt:Audience"], claims: claims,
            expires: DateTime.UtcNow.AddHours(8), signingCredentials: creds);
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return new LoginRes(jwt, identity.Email!, $"{appUser.FirstName} {appUser.LastName}", fun);
    }
}