using ClinicalGrpcService.Application;
using ClinicalGrpcService.Infra;
using ClinicalService.Api.Common;
using ClinicalService.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
if (builder.Environment.IsEnvironment("Local"))
    builder.Configuration.AddUserSecrets<Program>(optional: true);

builder.AddGatewayKeyVault();
builder.AddServiceDefaults();

builder.Host.AddHostInfrastructure(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
if (!builder.Environment.IsEnvironment("Local"))
{
    builder.Services.AddObservability(builder.Configuration);
}

builder.Services.AddAppService(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<GatewaySecretMiddleware>();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

app.MapGet("/", () => "ClinicalService.Api is running.");

app.MapPatientEndpoints();

app.Run();
