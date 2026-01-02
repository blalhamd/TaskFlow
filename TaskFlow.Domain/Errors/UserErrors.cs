using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Errors
{
    public class UserErrors
    {
        public static readonly Error EmptyPassword = new("ChangePassword.Empty", "Passwords cannot be empty.", ErrorType.Validation);
        public static readonly Error NotFound = new("User.Errors.NotFound", "User not found", ErrorType.NotFound);
    }
}
