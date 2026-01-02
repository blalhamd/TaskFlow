namespace TaskFlow.Domain.Common
{
    public class ValueResult<T> : Result
    {
        public T Value { get; }
        public ValueResult(bool isSuccess, T value, Error error) : base(isSuccess, error)
        {
            Value = value;
        }

        public static ValueResult<T> Success(T value)
        {
            return new ValueResult<T>(true, value, Error.None);
        }

        public new static ValueResult<T> Failure(Error error)
        {
            return new ValueResult<T>(false, default!, error);
        }
    }
}
