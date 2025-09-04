namespace TaskFlow.Shared.Interfaces
{
    public interface IModificationEntity
    {
        Guid? ModifiedByUserId { get; set; }
        DateTimeOffset? ModifiedAt { get; set; }
    }
}
