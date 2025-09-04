namespace TaskFlow.API.Models
{
    public class CustomErrorResponse
    {
        public CustomErrorResponse(int statusCode, string errorMessage, string errorDetail, string traceId)
        {
            StatusCode = statusCode;
            ErrorMessage = errorMessage;
            ErrorDetail = errorDetail;
            TraceId = traceId;
        }

        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorDetail { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"Status Code: {StatusCode}\n" +
                   $"Error Message: {ErrorMessage}\n" +
                   $"Error Details: {ErrorDetail}\n"+
                   $"Trace Id: {TraceId}\n";
        }
    }
}
