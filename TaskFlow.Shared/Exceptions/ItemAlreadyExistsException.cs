namespace TaskFlow.Shared.Exceptions
{
    public class ItemAlreadyExistsException : BaseException
    {
        public ItemAlreadyExistsException()
            : base($"Item already exists.") { }

        public ItemAlreadyExistsException(string message)
            : base(message) { }
    }

}
