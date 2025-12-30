namespace Lender.Helpers;

/// <summary>
/// Helper class for accessing services from the dependency injection container.
/// </summary>
public static class ServiceHelper
{
    /// <summary>
    /// Gets a service from the MAUI service provider.
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve.</typeparam>
    /// <returns>The service instance or null if not found.</returns>
    public static T? GetService<T>() where T : class
    {
        return MauiProgram.ServiceProvider?.GetService(typeof(T)) as T;
    }
}
