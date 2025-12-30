namespace Lender.Services;

public partial class GoogleSignInService
{
    public static partial Task<GoogleSignInResult?> SignInAsync();
}

public class GoogleSignInResult
{
    public string? IdToken { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
}
