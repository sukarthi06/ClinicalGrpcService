using ClinicalGrpcService.Infra.Interfaces;
using ClinicalGrpcService.Infra.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace ClinicalGrpcService.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("ClinicalDocumentationDb"),
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                });

            if (configuration.GetValue<bool>("Logging:LogEfSql"))
            {
                options
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging();
            }
        });

        services.AddScoped<IPhysicianNoteRecordingRepo, PhysicianNoteRecordingRepo>();

        return services;
    }
    
    public static void AddHostInfrastructure(
        this IHostBuilder hostBuilder,
        IConfiguration configuration)
    {
        var otlpEndpoint = configuration["Otlp:Endpoint"] ?? "http://localhost:4317";
        var otlpProtocol = configuration["Otlp:Protocol"] ?? "grpc";
        var otlpHeaders = configuration["Otlp:Headers"];
        var serviceName = configuration["Serilog:Properties:Application"] ?? "ClinicalGrpcService";
        var serilogProtocol = otlpProtocol.Equals("http/protobuf", StringComparison.OrdinalIgnoreCase)
            ? OtlpProtocol.HttpProtobuf
            : OtlpProtocol.Grpc;

        hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = otlpEndpoint;
                    options.Protocol = serilogProtocol;
                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = serviceName
                    };
                    if (!string.IsNullOrEmpty(otlpHeaders))
                    {
                        options.Headers = otlpHeaders
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(kv => kv.Split('=', 2))
                            .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());
                    }
                });
        });
    }

    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var otlpEndpoint = configuration["Otlp:Endpoint"] ?? "http://localhost:4317";
        var otlpProtocol = configuration["Otlp:Protocol"] ?? "grpc";
        var otlpHeaders = configuration["Otlp:Headers"];
        var serviceName = configuration["Serilog:Properties:Application"] ?? "ClinicalGrpcService";
        var exportProtocol = otlpProtocol.Equals("http/protobuf", StringComparison.OrdinalIgnoreCase)
            ? OtlpExportProtocol.HttpProtobuf
            : OtlpExportProtocol.Grpc;

        void ConfigureExporter(OtlpExporterOptions otlp)
        {
            otlp.Endpoint = new Uri(otlpEndpoint);
            otlp.Protocol = exportProtocol;
            if (!string.IsNullOrEmpty(otlpHeaders))
                otlp.Headers = otlpHeaders;
        }

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("ClinicalGrpcService.Repository")
                .AddOtlpExporter(ConfigureExporter))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(ConfigureExporter));

        return services;
    }
}
