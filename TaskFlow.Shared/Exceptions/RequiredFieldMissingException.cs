namespace TaskFlow.Shared.Exceptions
{
    public class RequiredFieldMissingException : BaseException
    {
        public RequiredFieldMissingException(string fieldName)
            : base($"The field '{fieldName}' is required and cannot be null.") { }
    }

}
