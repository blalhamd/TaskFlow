namespace TaskFlow.Domain.Enums
{
    public enum ErrorType
    {
        None = 0,
        Failure = 1,               // general
        Validation = 2,            // 400 BadRequest
        NotFound = 3,              // 404 NotFound
        Conflict = 4,              // 409 Confilict
        InternalServerError = 5,   // 500 Confilict
    }
}
