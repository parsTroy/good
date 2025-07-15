# GoodMaster

A Blazor WebAssembly app for debt payoff planning and financial management.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- (Optional) Visual Studio 2022+ or VS Code with C# extension
- Node.js (if using frontend tooling)

## Project Structure

```
good/
  .github/workflows/      # GitHub Actions CI/CD workflows
  Components/            # Blazor components
  Layout/                # Layout components
  Models/                # Data models (e.g., Debt, Payment)
  Pages/                 # Blazor pages
  Properties/            # Launch settings
  Services/              # Service classes (e.g., LocalStorageService)
  Tests/                 # Unit tests for backend logic
  Utils/                 # Utility and calculation logic
  wwwroot/               # Static assets
  good.sln               # Solution file
  good.csproj            # Main project file
```

## Setup & Local Development

1. **Clone the repository:**
   ```sh
   git clone https://github.com/YOUR-USERNAME/goodmaster.git
   cd goodmaster/good
   ```
2. **Restore dependencies:**
   ```sh
   dotnet restore
   ```
3. **Build the project:**
   ```sh
   dotnet build
   ```
4. **Run the app locally:**
   ```sh
   dotnet run
   ```
   - The app will be available at the URL shown in the terminal (e.g., http://localhost:5243)

## Running Tests

- **Run all backend unit tests:**
  ```sh
  dotnet test
  ```
- **CI/CD:**
  - All pushes and pull requests to `main` will trigger the GitHub Actions workflow, which builds and tests the project automatically.
  - Builds will fail if any tests do not pass.

## Deployment

- This project is designed for static web hosting (e.g., GitHub Pages, Vercel, Netlify, Azure Static Web Apps).
- To publish a production build:
  ```sh
  dotnet publish -c Release
  ```
- Deploy the contents of the `bin/Release/net8.0/publish/wwwroot` directory to your static host.

## Contributing

- Please ensure all tests pass before submitting a pull request.
- Follow the project structure and naming conventions.
- For major changes, open an issue to discuss your proposal first.

## License

[MIT](LICENSE) 