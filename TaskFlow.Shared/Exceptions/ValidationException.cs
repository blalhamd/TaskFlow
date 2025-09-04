namespace TaskFlow.Shared.Exceptions
{
    public class ValidationException : BaseException
    {
        public ValidationException(string message)
            : base(message) { }
    }

}
