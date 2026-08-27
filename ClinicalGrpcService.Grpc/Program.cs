using ClinicalGrpcService.Application;
using ClinicalGrpcService.Grpc.Common;
using ClinicalGrpcService.Grpc.Mappers;
using ClinicalGrpcService.Grpc.Services;
using ClinicalGrpcService.Infra;
using ClinicalGrpcService.Infra.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc(options => options.Interceptors.Add<ExceptionInterceptor>());

builder.Host.AddHostInfrastructure(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
if (!builder.Environment.IsEnvironment("Local"))
{
    builder.Services.AddObservability(builder.Configuration);
}
builder.Services.AddAppService(builder.Configuration);

if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Local"))
{
    builder.Services.AddGrpcReflection();
}

builder.Services.AddSingleton<PhysicianNoteRecordingMapper>();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
    app.MapGrpcReflectionService();
}

await app.Services.InitialiseDatabaseAsync();

// Configure the HTTP request pipeline.
app.MapGrpcService<PhysicianNoteRecordingService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
