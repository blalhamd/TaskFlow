namespace TaskFlow.Shared.Exceptions
{
    public class ItemNotFoundException : BaseException
    {
        public ItemNotFoundException()
            : base($"Item was not found.") { }

        public ItemNotFoundException(string message)
            : base(message) { }
    }

}
