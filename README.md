# ProductManagement

Small ASP.NET Core API solution targeting .NET 8.

## Projects
- `ProductManagement.Api` — main Web API project
- `ProductManagement.Tests.Unit` — unit tests
- `ProductManagement.Tests.Integration` — integration tests

Solution file: `ProductManagement.Api.slnx`

## Prerequisites
- .NET 8 SDK (install from https://dotnet.microsoft.com)
- Visual Studio 2026 (Community) or any editor that supports .NET 8
- Recommended shell: `powershell.exe`

## Quickstart (CLI)
1. Clone:
   - git clone <repo>
2. Restore and build:
   - `dotnet restore`
   - `dotnet build`
3. Configure connection string:
   - Edit `ProductManagement.Api/appsettings.json` and update `ConnectionStrings:DefaultConnection`:
     - `"Host=SERVER;Port=PORT;Database=DATABASE;Username=USER_NAME;Password=PASSWORD;"`
4. Run the API:
   - `cd ProductManagement.Api`
   - `dotnet run`
5. Run tests:
   - From solution root: `dotnet test`

## Quickstart (Visual Studio)
1. Open solution: `ProductManagement.Api.slnx`
2. Use __Build Solution__ to compile
3. Set `ProductManagement.Api` as startup project and use __Start Debugging__
4. Run tests from the __Test Explorer__

## Configuration
Default `appsettings.json` contains logging and a `DefaultConnection` placeholder. Use environment-specific `appsettings.{Environment}.json` or environment variables for secrets.

## Folder layout
- `ProductManagement.Api/` — API sources and configuration
- `ProductManagement.Tests.Unit/` — unit tests
- `ProductManagement.Tests.Integration/` — integration tests

## Contributing
- Follow branch strategy used in repo (current branch: `featureBranch`)
- Run unit and integration tests locally before pushing
