using Microsoft.AspNetCore.Mvc;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;

namespace TaskFlow.API.Controllers.Base
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        [NonAction]
        public IActionResult Success(Result result)
        {
            return result.IsSuccess? Ok() : CreateProblem(result.Error);
        }

        [NonAction]
        public IActionResult Success<T>(ValueResult<T> result)
        {
            return result.IsSuccess ? Ok(result.Value) : CreateProblem(result.Error);
        }

        [NonAction]
        private IActionResult CreateProblem(Error error)
        {
            var statusCode = StatusCodes.Status500InternalServerError;
            switch (error.ErrorType)
            {
                case ErrorType.Validation:
                    statusCode = StatusCodes.Status400BadRequest;
                    break;
                case ErrorType.NotFound:
                    statusCode = StatusCodes.Status404NotFound;
                    break;
                case ErrorType.Conflict:
                    statusCode = StatusCodes.Status409Conflict;
                    break;
                default:
                    break;
            }

            return Problem(statusCode: statusCode, title: error.Code, detail: error.Description);
        }
    }
}
