using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities.Identity;
using TaskFlow.Infrastructure.Data.context;
using TaskFlow.Shared.Common;

namespace TaskFlow.Infrastructure.Data.SeedData
{
    public static class Seed
    {
        /// <summary>
        /// Seeds initial data for roles, users, developers, and tasks.
        /// </summary>
        /// <param name="context">AppDbContext instance.</param>
        /// <param name="userManager">UserManager for ApplicationUser.</param>
        /// <param name="roleManager">RoleManager for ApplicationRole.</param>
        public static async Task InitializeDataAsync(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            ArgumentNullException.ThrowIfNull(context);

            // Seed roles if none exist
            if (!await roleManager.Roles.AnyAsync())
            {
                foreach (var role in LoadRoles())
                {
                    await roleManager.CreateAsync(role);
                }
            }

            // Seed users, developers, and tasks if no users exist
            if (!await userManager.Users.AnyAsync())
            {
                foreach (var (user, password, roleName) in LoadUsers())
                {
                    var result = await userManager.CreateAsync(user, password);
                    if (!result.Succeeded) continue;

                    await userManager.AddToRoleAsync(user, roleName);

                    // Only seed developer profile and tasks for non-admin users
                    if (roleName != ApplicationConstants.Admin)
                    {
                       
                    }
                }

                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Loads initial users with their password and role.
        /// </summary>
        private static IEnumerable<(ApplicationUser user, string password, string role)> LoadUsers()
        {
            var now = DateTimeOffset.UtcNow;
            Guid creatorId = Guid.NewGuid();

            return new List<(ApplicationUser, string, string)>
        {
            (new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@system.com",
                NormalizedEmail = "ADMIN@SYSTEM.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                CreatedAt = now,
                CreatedByUserId = creatorId,
                IsDeleted = false
            }, "Admin@123", ApplicationConstants.Admin),

           
        };
        }

        /// <summary>
        /// Loads predefined application roles.
        /// </summary>
        private static ApplicationRole[] LoadRoles() =>
        [
            new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = ApplicationConstants.Admin,
            NormalizedName = ApplicationConstants.Admin.ToUpper(),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = Guid.NewGuid()
        },
        new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = ApplicationConstants.Manager,
            NormalizedName = ApplicationConstants.Manager.ToUpper(),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = Guid.NewGuid()
        },
        new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = ApplicationConstants.Developer,
            NormalizedName = ApplicationConstants.Developer.ToUpper(),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = Guid.NewGuid()
        }
        ];

       
    }
}
