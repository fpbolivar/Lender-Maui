namespace Lender.Services.Interfaces;

/// <summary>
/// Service to retrieve Firebase App Check tokens for secure requests.
/// </summary>
public interface IAppCheckService
{
    /// <summary>
    /// Get a valid Firebase App Check token.
    /// </summary>
    /// <returns>The App Check token, or null if unavailable.</returns>
    Task<string?> GetAppCheckTokenAsync();
}
