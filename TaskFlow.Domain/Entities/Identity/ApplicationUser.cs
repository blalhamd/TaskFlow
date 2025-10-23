using Microsoft.AspNetCore.Identity;
using TaskFlow.Shared.Interfaces;

namespace TaskFlow.Domain.Entities.Identity
{
    public class ApplicationUser : IdentityUser<Guid>, IAuditableEntity
    {
        public Guid? CreatedByUserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public Guid? ModifiedByUserId { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
        public Guid? DeletedByUserId { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
        public List<ApplicationRole> Roles { get; set; } = new();
        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}
