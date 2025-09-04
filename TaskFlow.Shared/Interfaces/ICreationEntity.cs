namespace TaskFlow.Shared.Interfaces
{
    public interface ICreationEntity
    {
        Guid? CreatedByUserId { get; set; }
        DateTimeOffset CreatedAt { get; set; }
    }    
}
