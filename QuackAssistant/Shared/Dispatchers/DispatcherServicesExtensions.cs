namespace QuackAssistant.Shared.Dispatchers;

public static class DispatcherServicesExtensions
{
    public static IServiceCollection AddDispatcher(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();
        Dispatcher.RegisterHandlers(services);
        return services;
    }
}