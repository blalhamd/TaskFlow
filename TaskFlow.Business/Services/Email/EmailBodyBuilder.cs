using TaskFlow.Core.IServices.Email;
using Microsoft.AspNetCore.Hosting;

namespace TaskFlow.Business.Services.Email
{
    public class EmailBodyBuilder : IEmailBodyBuilder
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailBodyBuilder(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> GenerateEmailBody(string templateName, string imageUrl, string header, string textBody, string link, string linkTitle)
        {
            var templatePath = $"{_webHostEnvironment.WebRootPath}/templates/{templateName}";

            var body = await File.ReadAllTextAsync(templatePath);

            body = body.Replace("[imageUrl]", imageUrl)
                       .Replace("[header]", header)
                       .Replace("[TextBody]", textBody)
                       .Replace("[link]", link)
                       .Replace("[Title]", linkTitle);

            return body;
        }
    }
}
