using Microsoft.Extensions.DependencyInjection;

namespace ToastRack.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddToastRack_RegistersScopedToastService()
    {
        var services = new ServiceCollection();

        services.AddToastRack();

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IToastService));
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(ToastService), descriptor.ImplementationType);
    }

    [Fact]
    public void AddToastRack_ResolvesToastService()
    {
        using var provider = new ServiceCollection().AddToastRack().BuildServiceProvider();

        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IToastService>();

        Assert.IsType<ToastService>(service);
    }

    [Fact]
    public void AddToastRack_ReturnsSameCollectionForChaining()
    {
        var services = new ServiceCollection();

        var result = services.AddToastRack();

        Assert.Same(services, result);
    }

    [Fact]
    public void AddToastRack_DoesNotDuplicateOrReplaceExistingRegistration()
    {
        var services = new ServiceCollection();
        var existing = new ToastService();
        services.AddSingleton<IToastService>(existing);

        services.AddToastRack();
        services.AddToastRack();

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IToastService));
        Assert.Same(existing, descriptor.ImplementationInstance);
    }

    [Fact]
    public void AddToastRack_ThrowsOnNullServices()
    {
        Assert.Throws<ArgumentNullException>(() => default(IServiceCollection)!.AddToastRack());
    }
}
