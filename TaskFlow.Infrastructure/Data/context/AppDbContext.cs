using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Claims;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Entities.Base;
using TaskFlow.Domain.Entities.Identity;
using TaskFlow.Shared.Interfaces;

namespace TaskFlow.Infrastructure.Data.context
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public DbSet<Developer> Developers { get; set; }
        public DbSet<TaskEntity> Tasks { get; set; }

        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor contextAccessor) : base(options)
        {
            _httpContextAccessor = contextAccessor;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Users
            builder.Entity<ApplicationUser>()
                   .ToTable("Users", schema: "Security");

            // Roles
            builder.Entity<ApplicationRole>()
                   .ToTable("Roles", schema: "Security");

            // UserRoles (junction)
            builder.Entity<IdentityUserRole<Guid>>()
                   .ToTable("UserRoles", schema: "Security");

            // UserClaims
            builder.Entity<IdentityUserClaim<Guid>>()
                .ToTable("UserClaims", schema: "Security");

            // UserLogins
            builder.Entity<IdentityUserLogin<Guid>>()
                .ToTable("UserLogins", "Security");

            // RoleClaims
            builder.Entity<IdentityRoleClaim<Guid>>()
                   .ToTable("RoleClaims", schema: "Security");

            // UserTokens
            builder.Entity<IdentityUserToken<Guid>>()
                .ToTable("UserTokens", schema: "Security");


            builder.Entity<ApplicationUser>()
                .OwnsMany(x => x.RefreshTokens)
                .ToTable("RefreshTokens")
                .WithOwner()
                .HasForeignKey("UserId");

            builder.Entity<Developer>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<TaskEntity>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<ApplicationUser>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<ApplicationRole>().HasQueryFilter(x => !x.IsDeleted);

            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }


        public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var currentUserId = _httpContextAccessor.HttpContext?
                                .User?
                                .FindFirstValue(ClaimTypes.NameIdentifier);

            var ip = _httpContextAccessor.HttpContext?
                     .Connection
                     .RemoteIpAddress?
                     .ToString();

            // 🗒️  collect BaseEntity changes
            var tracked = ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.State is EntityState.Added
                                    or EntityState.Modified
                                    or EntityState.Deleted);

            Guid? userGuid = null;
            if (Guid.TryParse(currentUserId, out Guid guid))
                userGuid = guid;

            foreach (var e in tracked)
            {
                // ---------- stamps ----------
                switch (e.State)
                {
                    case EntityState.Added:
                        e.Property(p => p.CreatedAt).CurrentValue = now;
                        e.Property(p => p.CreatedByUserId).CurrentValue = userGuid;
                        break;

                    case EntityState.Modified:
                        if (e.Properties.Any(p => p.IsModified))
                        {
                            e.Property(p => p.ModifiedAt).CurrentValue = now;
                            e.Property(p => p.ModifiedByUserId).CurrentValue = userGuid;
                        }
                        break;

                    case EntityState.Deleted when e.Entity is IDeletionEntity soft:
                        e.State = EntityState.Modified; // soft-delete
                        e.Entity.Delete(userGuid ?? Guid.Empty);
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}


