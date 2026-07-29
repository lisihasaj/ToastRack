using Microsoft.Extensions.DependencyInjection.Extensions;
using ToastRack;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering ToastRack services.
/// </summary>
public static class ToastRackServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IToastService"/> as a scoped service. In Blazor WebAssembly a scoped
    /// service behaves like a per-app singleton; in Blazor Server it is scoped to the circuit,
    /// which keeps each user's toasts isolated.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddToastRack(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<IToastService, ToastService>();
        return services;
    }
}
