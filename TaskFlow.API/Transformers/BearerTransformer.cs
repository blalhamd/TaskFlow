using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace TaskFlow.API.Transformers
{
    public sealed class BearerTransformer : IOpenApiDocumentTransformer, IOpenApiOperationTransformer
    {
        private const string SchemeId = JwtBearerDefaults.AuthenticationScheme;

        /// <summary>
        /// Runs once per OpenAPI document.
        /// Adds the "Bearer" JWT security definition to the Swagger document so UI knows how to authenticate.
        /// </summary>
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {

            // Define the security scheme (Bearer token)
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",                     // The HTTP header name
                Type = SecuritySchemeType.Http,             // Type: HTTP Authentication
                Scheme = "bearer",                           // Bearer format
                BearerFormat = "JWT",                        // Format is JWT
                In = ParameterLocation.Header,               // Sent in the header
                Description = "Enter 'Bearer' followed by a space and your JWT.",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = SchemeId
                }
            };

            // Register the security scheme in the document components
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

            document.Components.SecuritySchemes[SchemeId] = securityScheme;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs for every operation (endpoint) in the document.
        /// Adds a security requirement so that Swagger UI will prompt for a Bearer token when calling this operation.
        /// </summary>
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            if (context.Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any())
            {
                operation.Security ??= new List<OpenApiSecurityRequirement>();

                // Define that this operation requires the "Bearer" scheme
                var securityRequirement = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme, // Reference an existing scheme
                            Id = SchemeId                         // The scheme name we defined above
                        }
                    },
                    Array.Empty<string>() // No specific scopes required
                }

            };

                // Attach the requirement to the operation
                operation.Security.Add(securityRequirement);
            }


            return Task.CompletedTask;
        }
    }
}
