using Microsoft.AspNetCore.Identity;
using TaskFlow.Domain.Entities.Identity;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Data.context;
using Microsoft.EntityFrameworkCore;
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
                        var developer = LoadDeveloperProfileFor(user);
                        developer.CreatedByUserId = user.Id;
                        await context.Developers.AddAsync(developer);

                        foreach (var task in LoadTasksForUser(user.Id))
                        {
                            task.CreatedByUserId = user.Id;
                            task.AssignToDeveloper(user.Id);       
                            await context.Tasks.AddAsync(task);
                        }
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

            (new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "bilal",
                NormalizedUserName = "BILAL",
                Email = "bilal@system.com",
                NormalizedEmail = "BILAL@SYSTEM.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                CreatedAt = now,
                CreatedByUserId = creatorId,
                IsDeleted = false
            }, "Bilal@123", ApplicationConstants.Developer),

            (new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "ahmed",
                NormalizedUserName = "AHMED",
                Email = "ahmed@system.com",
                NormalizedEmail = "AHMED@SYSTEM.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                CreatedAt = now,
                CreatedByUserId = creatorId,
                IsDeleted = false
            }, "Ahmed@123", ApplicationConstants.Developer)
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

        /// <summary>
        /// Creates a developer profile for a given user.
        /// </summary>
        private static Developer LoadDeveloperProfileFor(ApplicationUser user)
        {
            return new Developer(
                fullName: $"{user.UserName} Sayed",
                age: 22,
                imagePath: "WhatsApp Image 2024-04-14 at 01.35.12_56142422.jpg",
                jobTitle: ".NET Full Stack Developer",
                yearOfExperience: 1,
                jobLevel: JobLevel.Junior,
                userId: user.Id
            );
        }

        /// <summary>
        /// Loads tasks and assigns to the specified developer user ID.
        /// </summary>
        private static IEnumerable<TaskEntity> LoadTasksForUser(Guid userId)
        {
            var now = DateTimeOffset.UtcNow;
            return new List<TaskEntity>
        {
            new TaskEntity(
                startAt: now.AddDays(1),
                endAt: now.AddDays(3),
                content: "Draw UML for TaskFlow System",
                progress: TaskProgress.NotStarted
            )
            ,

            new TaskEntity(
                startAt: now.AddDays(2),
                endAt: now.AddDays(4),
                content: "Implement Repository Pattern",
                progress: TaskProgress.NotStarted
            )
        };
        }
    }
}
