using TaskFlow.Shared.Interfaces;

namespace TaskFlow.Domain.Entities.Base
{
    public abstract class BaseEntity : IEntityIdentity, IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid? ModifiedByUserId { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
        public Guid? DeletedByUserId { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }

        public void Delete(Guid deletedByUserId)
        {
            DeletedByUserId = deletedByUserId;
            IsDeleted = true;
            DeletedAt = DateTimeOffset.UtcNow;
        }

        public void UndoDelete()
        {
            DeletedByUserId = null;
            IsDeleted = false;
            DeletedAt = null;
        }
    }
}
