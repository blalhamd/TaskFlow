namespace TaskFlow.Shared.Interfaces
{
    public interface IDeletionEntity
    {
        Guid? DeletedByUserId { get; set; }
        DateTimeOffset? DeletedAt { get; set; }
        bool IsDeleted { get; set; }
    }
}
