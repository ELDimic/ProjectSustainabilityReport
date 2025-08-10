using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;
using Web.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(o =>
{
    o.Password.RequiredLength = 8;
    o.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBlobService, BlobService>();
builder.Services.AddSingleton<IAuthorizationHandler, HasFunctionalityHandler>();

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("CanUpload", p => p.Requirements.Add(new HasFunctionalityRequirement("upload")));
    o.AddPolicy("CanDownload", p => p.Requirements.Add(new HasFunctionalityRequirement("download")));
    o.AddPolicy("IsAdmin", p => p.Requirements.Add(new HasFunctionalityRequirement("admin")));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Reports}/{action=Index}/{id?}");

// Seed: utente admin, profilo, ruoli e funzionalità
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Identity admin user
    var email = app.Configuration["AdminSeed:Email"]; var pwd = app.Configuration["AdminSeed:Password"];
    ApplicationUser? identity;
    identity = await userMgr.FindByEmailAsync(email!);
    if (identity is null)
    {
        identity = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        await userMgr.CreateAsync(identity, pwd!);
    }

    // AppUser profile
    if (!await db.UsersProfile.AnyAsync(u => u.IdentityUserId == identity!.Id))
    {
        db.UsersProfile.Add(new AppUser
        {
            Id = Guid.NewGuid(),
            FirstName = "Admin",
            LastName = "User",
            UserName = email!,
            Email = email!,
            IdentityUserId = identity!.Id,
            LastAccessAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }

    // Roles app (Admin, Consulter)
    var adminRole = await db.RolesApp.FirstOrDefaultAsync(r => r.Description == "Admin")
                    ?? db.RolesApp.Add(new Role { Id = Guid.NewGuid(), Description = "Admin" }).Entity;
    var consulterRole = await db.RolesApp.FirstOrDefaultAsync(r => r.Description == "Consulter")
                    ?? db.RolesApp.Add(new Role { Id = Guid.NewGuid(), Description = "Consulter" }).Entity;
    await db.SaveChangesAsync();

    // Functionalities già seedate in OnModelCreating (upload, download, admin)
    var fUpload = await db.Functionalities.SingleAsync(f => f.Code == "upload");
    var fDownload = await db.Functionalities.SingleAsync(f => f.Code == "download");
    var fAdmin = await db.Functionalities.SingleAsync(f => f.Code == "admin");

    var appAdmin = await db.UsersProfile.SingleAsync(u => u.IdentityUserId == identity!.Id);

    // Associazioni: Admin → upload, download, admin
    var urAdmin = db.UserRoles.FirstOrDefault(x => x.UserId == appAdmin.Id && x.RoleId == adminRole.Id)
                  ?? db.UserRoles.Add(new UserRole { Id = Guid.NewGuid(), UserId = appAdmin.Id, RoleId = adminRole.Id }).Entity;
    await db.SaveChangesAsync();

    void EnsureUrf(UserRole ur, Functionality f)
    {
        if (!db.UserRolesFunctionalities.Any(x => x.UserRoleId == ur.Id && x.FunctionalityId == f.Id))
            db.UserRolesFunctionalities.Add(new UserRoleFunctionality { Id = Guid.NewGuid(), UserRoleId = ur.Id, FunctionalityId = f.Id });
    }

    EnsureUrf(urAdmin, fUpload); EnsureUrf(urAdmin, fDownload); EnsureUrf(urAdmin, fAdmin);
    await db.SaveChangesAsync();
}

app.Run();