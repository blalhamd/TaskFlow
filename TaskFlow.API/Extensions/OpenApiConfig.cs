using TaskFlow.API.Transformers;

namespace TaskFlow.API.Extensions
{
    public static class OpenApiConfig
    {
        public static IServiceCollection RegisterOpenAPI(this IServiceCollection services)
        {
            string[] versions = ["v1", "v2"];

            foreach (string version in versions)
            {
                services.AddOpenApi(version, options =>
                {
                    // for versioning
                    options.AddDocumentTransformer<VersionTransformer>();

                    // for bearer config
                    options.AddDocumentTransformer<BearerTransformer>();
                    options.AddOperationTransformer<BearerTransformer>();
                });
            }
            return services;
        }
    }
}
