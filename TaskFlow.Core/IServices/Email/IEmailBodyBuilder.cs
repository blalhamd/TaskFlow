namespace TaskFlow.Core.IServices.Email
{
    public interface IEmailBodyBuilder
    {
        Task<string> GenerateEmailBody(string templateName, string imageUrl, string header, string textBody, string link, string linkTitle);
    }
}
