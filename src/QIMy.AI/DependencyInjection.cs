using Microsoft.Extensions.DependencyInjection;
using QIMy.AI.Services;

namespace QIMy.AI;

public static class DependencyInjection
{
    public static IServiceCollection AddAiServices(this IServiceCollection services)
    {
        // AI Services
        services.AddScoped<IAiEncodingDetectionService, AiEncodingDetectionService>();
        
        // TODO: Add more AI services as they are implemented
        // services.AddScoped<IAiOcrService, AiOcrService>();
        // services.AddScoped<IAiClassificationService, AiClassificationService>();
        // services.AddScoped<IAiMatchingService, AiMatchingService>();
        // services.AddScoped<IAiApprovalRouter, AiApprovalRouter>();
        
        return services;
    }
}
