namespace TaskFlow.Shared.Exceptions
{
    public abstract class BaseException : ApplicationException
    {
        protected BaseException() { }

        protected BaseException(string message)
            : base(message) { }

        protected BaseException(string message, Exception innerException)
            : base(message, innerException) { }

    }
}
