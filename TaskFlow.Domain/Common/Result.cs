namespace TaskFlow.Domain.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public Error Error { get; } = Error.None;

        public Result(bool isSuccess, Error error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success()
        {
            return new Result(true, Error.None);
        }

        public static Result Failure(Error error)
        {
            return new Result(false, error);
        }
    }
}
