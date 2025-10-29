namespace TaskFlow.Shared.Interfaces
{
    public interface IEntityIdentity<T>
    {
        T Id { get; set; }
    }
}
