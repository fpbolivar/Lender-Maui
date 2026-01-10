using Lender.Services.Interfaces;

namespace Lender.Services;

/// <summary>
/// Platform-agnostic wrapper for Firebase App Check token retrieval.
/// Delegates to platform-specific implementations.
/// </summary>
public partial class AppCheckService : IAppCheckService
{
    public partial Task<string?> GetAppCheckTokenAsync();
}
