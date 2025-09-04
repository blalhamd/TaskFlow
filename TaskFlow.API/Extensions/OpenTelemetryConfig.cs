using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace TaskFlow.API.Extensions
{
    public static class OpenTelemetryConfig
    {
        public static IServiceCollection RegisterOpenTelemetryConfig(this IServiceCollection services)
        {
            var serviceName = "DeveloperService";

            services.AddOpenTelemetry()
                .ConfigureResource(resource =>
                    resource.AddService(serviceName: serviceName))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddSource(serviceName) 
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/traces");
                        });
                });

            return services;
        }
    }
}
