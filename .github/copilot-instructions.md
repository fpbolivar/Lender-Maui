# .NET MAUI Project Setup Instructions

## Project Overview
This is a .NET MAUI (Multi-platform App UI) application targeting iOS and Android from a single codebase.

## Environment Setup

### Prerequisites
- .NET 10.0 SDK or later
- MAUI workload installed (`dotnet workload install maui`)
- Xcode (for iOS/macOS development on Mac)
- Android SDK (for Android development)

### Installation

1. Install MAUI workload (if not already installed):
   ```bash
   dotnet workload install maui
   ```

2. Restore project dependencies:
   ```bash
   cd Lender
   dotnet restore Lender.csproj
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

## Project Structure

```
Lender/
├── App.xaml              # Application root XAML
├── App.xaml.cs           # Application code-behind
├── AppShell.xaml         # Shell navigation structure
├── AppShell.xaml.cs      # Shell code-behind
├── MainPage.xaml         # Main application page
├── MainPage.xaml.cs      # Main page code-behind
├── MauiProgram.cs        # Application startup configuration
├── Lender.csproj        # Project file
├── Platforms/            # Platform-specific code
├── Properties/           # Project properties
├── Resources/            # Application resources
│   ├── Fonts/           # Custom fonts
│   ├── Colors/          # Color definitions
│   ├── Styles/          # Style definitions
│   └── Strings/         # String resources (localization)
└── obj/                 # Build output (ignored)
```

## Running the Application

### Development on macOS

For iOS simulator:
```bash
dotnet build -f net10.0-ios
dotnet run -f net10.0-ios
```

### Android
```bash
dotnet build -f net10.0-android
dotnet run -f net10.0-android
```

## Development Workflow

### Key Files
- **MauiProgram.cs**: Configure services, styles, and platform-specific setup
- **AppShell.xaml**: Define navigation routes and structure
- **MainPage.xaml**: Create UI layouts using XAML
- **Code-behind files**: Handle events and logic in C#

### Adding New Pages
1. Create a new `.xaml` file for the UI
2. Create a corresponding `.xaml.cs` code-behind file
3. Register the page in `AppShell.xaml`
4. Add navigation routes as needed

### Resources
- Store application-wide resources in the `Resources/` folder
- Define colors, styles, and fonts for consistency
- Use resource keys for maintainability

## Debugging

### Enable Debug Mode
VS Code should automatically detect .NET launch configurations. To debug:
1. Open the `.vscode/launch.json` file (created by VS Code)
2. Set breakpoints in your C# code
3. Press F5 to start debugging

## Common Commands

```bash
# Restore dependencies
dotnet restore

# Build for a specific platform
dotnet build Lender.csproj -f net10.0-ios
dotnet build Lender.csproj -f net10.0-android

# Clean build artifacts
dotnet clean

# Run tests (if any)
dotnet test
```

## Next Steps

1. Familiarize yourself with the MAUI documentation: https://learn.microsoft.com/dotnet/maui/
2. Build and test on your target platform
3. Add custom pages, services, and features as needed
4. Configure platform-specific settings in `Platforms/` folder

## Useful Resources

- [MAUI Documentation](https://learn.microsoft.com/dotnet/maui/)
- [MAUI Samples](https://github.com/dotnet/maui-samples)
- [XAML Documentation](https://learn.microsoft.com/dotnet/maui/xaml/)
