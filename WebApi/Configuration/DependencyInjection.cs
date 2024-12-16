using WebApi.Common;
using WebApi.Constants;

namespace WebApi.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClients(configuration);
        services.AddOptions(configuration);
        return services;
    }

    private static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ILlamaService, LlamaService>();
        services.AddHttpClient(Constant.LlamaAI, x => x.BaseAddress = new Uri("http://desa-llm:9091/api/generate"));

        return services;
    }
    private static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var name = configuration["InitialResponseWith"];
        services.AddSingleton(new InitialResponse() { Name = name! });
        return services;
    }
}
