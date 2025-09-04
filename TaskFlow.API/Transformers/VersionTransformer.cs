using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace TaskFlow.API.Transformers
{
    public sealed class VersionTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            var version = context.DocumentName;
            document.Info.Version = version;
            document.Info.Title = $"Project API {version}";

            return Task.CompletedTask;
        }
    }
}
