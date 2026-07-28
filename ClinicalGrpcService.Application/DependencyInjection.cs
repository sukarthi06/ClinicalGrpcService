using ClinicalGrpcService.Application.Interfaces;
using ClinicalGrpcService.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicalGrpcService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAppService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IPhysicianNoteRecordingService, PhysicianNoteRecordingService>();
        return services;
    }
}
