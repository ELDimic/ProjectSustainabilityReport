using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Web.Data;

public class ApplicationUser : IdentityUser { }

public class AppUser
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTimeOffset? LastAccessAt { get; set; }
    public string IdentityUserId { get; set; } = null!;
    public ApplicationUser IdentityUser { get; set; } = null!;
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class Role
{
    public Guid Id { get; set; }
    public string Description { get; set; } = null!; // es. "Admin", "Consulter"
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class UserRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public AppUser User { get; set; } = null!;
    public Role Role { get; set; } = null!;
    public ICollection<UserRoleFunctionality> UserRoleFunctionalities { get; set; } = new List<UserRoleFunctionality>();
}

public class Functionality
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!; // upload | download | admin
    public string Description { get; set; } = null!;
    public ICollection<UserRoleFunctionality> UserRoleFunctionalities { get; set; } = new List<UserRoleFunctionality>();
}

public class UserRoleFunctionality
{
    public Guid Id { get; set; }
    public Guid UserRoleId { get; set; }
    public Guid FunctionalityId { get; set; }
    public UserRole UserRole { get; set; } = null!;
    public Functionality Functionality { get; set; } = null!;
}

public class Document
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!; // nome visibile
    public string BlobName { get; set; } = null!; // chiave blob
    public string? Year { get; set; }
    public string? Summary { get; set; }
    public long Size { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
    public Guid UploadedByUserId { get; set; }
    public AppUser UploadedBy { get; set; } = null!;
}

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<AppUser> UsersProfile => Set<AppUser>();
    public DbSet<Role> RolesApp => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Functionality> Functionalities => Set<Functionality>();
    public DbSet<UserRoleFunctionality> UserRolesFunctionalities => Set<UserRoleFunctionality>();
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<AppUser>(e =>
        {
            e.ToTable("Users");
            e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            e.Property(x => x.UserName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.HasIndex(x => x.UserName).IsUnique();
            e.HasOne(x => x.IdentityUser)
             .WithMany()
             .HasForeignKey(x => x.IdentityUserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Role>(e =>
        {
            e.ToTable("Roles");
            e.Property(x => x.Description).HasMaxLength(100).IsRequired();
        });

        b.Entity<UserRole>(e =>
        {
            e.ToTable("UserRoles");
            e.HasOne(x => x.User).WithMany(x => x.UserRoles).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Role).WithMany(x => x.UserRoles).HasForeignKey(x => x.RoleId);
            e.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
        });

        b.Entity<Functionality>(e =>
        {
            e.ToTable("Functionalities");
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Description).HasMaxLength(200).IsRequired();
        });

        b.Entity<UserRoleFunctionality>(e =>
        {
            e.ToTable("UserRolesFunctionalities");
            e.HasOne(x => x.UserRole).WithMany(x => x.UserRoleFunctionalities).HasForeignKey(x => x.UserRoleId);
            e.HasOne(x => x.Functionality).WithMany(x => x.UserRoleFunctionalities).HasForeignKey(x => x.FunctionalityId);
            e.HasIndex(x => new { x.UserRoleId, x.FunctionalityId }).IsUnique();
        });

        b.Entity<Document>(e =>
        {
            e.ToTable("Documents");
            e.Property(x => x.FileName).HasMaxLength(260).IsRequired();
            e.Property(x => x.BlobName).HasMaxLength(260).IsRequired();
            e.Property(x => x.Year).HasMaxLength(4);
            e.HasOne(x => x.UploadedBy).WithMany().HasForeignKey(x => x.UploadedByUserId);
        });

        // Seed Functionalities
        var fUpload = new Functionality { Id = Guid.NewGuid(), Code = "upload", Description = "Caricamento documenti" };
        var fDownload = new Functionality { Id = Guid.NewGuid(), Code = "download", Description = "Download documenti" };
        var fAdmin = new Functionality { Id = Guid.NewGuid(), Code = "admin", Description = "Amministrazione" };
        b.Entity<Functionality>().HasData(fUpload, fDownload, fAdmin);
    }
}