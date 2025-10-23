using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace TaskFlow.API.Filters
{
    public class RequestTimeCalculationFilter : IAsyncActionFilter
    {
        private readonly ILogger<RequestTimeCalculationFilter> _logger;

        public RequestTimeCalculationFilter(ILogger<RequestTimeCalculationFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            await next();
            stopWatch.Stop();
            _logger.LogInformation("Time took by request is: {ElapsedMilliseconds}ms", stopWatch.ElapsedMilliseconds);

        }
    }
}
