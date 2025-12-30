using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Lender.Helpers;
using Lender.Services;

namespace Lender.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly IAuthenticationService _authService;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _phoneNumber = string.Empty;
    private bool _isLoading;
    private string _errorMessage = string.Empty;
    private bool _isSignUp;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set => SetProperty(ref _phoneNumber, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsSignUp
    {
        get => _isSignUp;
        set => SetProperty(ref _isSignUp, value);
    }

    public ICommand SignInCommand { get; }
    public ICommand SignUpCommand { get; }
    public ICommand GoogleSignInCommand { get; }
    public ICommand ToggleSignUpCommand { get; }

    public LoginViewModel()
    {
        // Gracefully handle missing DI at startup by falling back to a direct service instance
        _authService = ServiceHelper.GetService<IAuthenticationService>() ?? new AuthenticationService();

        SignInCommand = new RelayCommand(SignInAsync, () => CanExecuteAuth());
        SignUpCommand = new RelayCommand(SignUpAsync, () => CanExecuteAuth());
        GoogleSignInCommand = new RelayCommand(GoogleSignInAsync, () => !IsLoading);
        ToggleSignUpCommand = new RelayCommand(ToggleSignUp);
    }

    private bool CanExecuteAuth()
    {
        if (IsLoading || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
            return false;

        // For sign up, also require first and last names
        if (IsSignUp)
        {
            return !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName);
        }

        return true;
    }

    public async Task<bool> RestoreSessionAsync()
    {
        try
        {
            IsLoading = true;
            var restored = await _authService.RestoreSessionAsync();
            return restored;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Session restore error: {ex.Message} - LoginViewModel.cs:79");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ToggleSignUp()
    {
        IsSignUp = !IsSignUp;
        ErrorMessage = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        PhoneNumber = string.Empty;
    }

    private async void SignInAsync()
    {
        await HandleAuthActionAsync(() => _authService.SignInAsync(Email, Password), "Sign in");
    }

    private async void SignUpAsync()
    {
        // Validate email format
        if (!Email.Contains("@"))
        {
            ErrorMessage = "Please enter a valid email address.";
            return;
        }

        // Validate password length
        if (Password.Length < 6)
        {
            ErrorMessage = "Password must be at least 6 characters long.";
            return;
        }

        // Validate first name
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            ErrorMessage = "Please enter your first name.";
            return;
        }

        // Validate last name
        if (string.IsNullOrWhiteSpace(LastName))
        {
            ErrorMessage = "Please enter your last name.";
            return;
        }

        await HandleAuthActionAsync(
            () => _authService.SignUpAsync(Email, Password, FirstName, LastName, PhoneNumber), 
            "Sign up"
        );
    }

    private async void GoogleSignInAsync()
    {
        IsLoading = true;
        try
        {
            var success = await _authService.SignInWithGoogleAsync();
            if (success)
            {
                await NavigateToMainPage();
            }
            else
            {
                ErrorMessage = "Google sign-in failed. Please try again or use email/password.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Google sign-in error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task HandleAuthActionAsync(Func<Task<bool>> authAction, string actionName)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var success = await authAction();
            if (success)
            {
                await NavigateToMainPage();
            }
            else
            {
                ErrorMessage = $"{actionName} failed. Please check your credentials.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{actionName} error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task NavigateToMainPage()
    {
        try
        {
            // Navigate to main app after successful authentication
            await Shell.Current.GoToAsync("///mainpage", animate: true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Navigation error: {ex.Message} - LoginViewModel.cs:167");
        }
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // Re-evaluate command execution when relevant properties change
            if (propertyName == nameof(Email) || propertyName == nameof(Password) || propertyName == nameof(IsLoading) ||
                propertyName == nameof(FirstName) || propertyName == nameof(LastName) || propertyName == nameof(IsSignUp))
            {
                (SignInCommand as RelayCommand)?.NotifyCanExecuteChanged();
                (SignUpCommand as RelayCommand)?.NotifyCanExecuteChanged();
                (GoogleSignInCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }
    }
}
