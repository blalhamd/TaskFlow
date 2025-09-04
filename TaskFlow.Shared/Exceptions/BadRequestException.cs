namespace TaskFlow.Shared.Exceptions
{
    public class BadRequestException : BaseException
    {
        public BadRequestException() : base("Bad request exception")
        {
            
        }

        public BadRequestException(string message)
            : base(message) { }
    }
}
