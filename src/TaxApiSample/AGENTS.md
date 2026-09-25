# TaxApiSample Instructions

## Application Architecture

- This project is a .NET 10 console CLI in `src/TaxApiSample/`.
- `Program.cs` owns command dispatch and the Spectre.Console interactive menu. Keep command handling there and API transport/token persistence in `Services/`.
- `Services/AuthClient.cs` handles authentication, `TaxApiClient.cs` invokes Tax Services operations, `TaxApiEndpointCatalog.cs` loads the endpoint catalog, and `TokenCache.cs` stores the token outside the repository.
- `TaxApiSample.csproj` embeds the authoritative documents from `swaggerFiles/` using logical resource names. Do not restore project-local Swagger copies or edit `swaggerFiles/` unless asked to change the contract itself.

## Configuration and Behavior

- `Configuration/CchApiOptions.cs` binds non-secret settings from `appsettings.json`.
- Credentials and integrator keys are read from the root `.env`, based on `env.template`. Never commit or print secrets.
- The no-argument flow checks the cached token and may authenticate before showing the menu. Authentication, token-check, and endpoint commands may make live requests; do not run them with configured credentials unless the user explicitly requests it.
- `TokenCache` applies a local 15-minute validity policy after saving a token. This is application policy, not an expiry period specified by the Authentication API.
- Keep the menu's unimplemented actions explicitly labeled as placeholders until implemented.

## Development and Validation

- Build with `dotnet build src/TaxApiSample/TaxApiSample.csproj`.
- Run the interactive menu with `dotnet run --project src/TaxApiSample/TaxApiSample.csproj`.
- Run a direct command by appending `--` and its arguments, for example `dotnet run --project src/TaxApiSample/TaxApiSample.csproj -- list`.
- `help` and `list` are local checks that do not call CCH services.
- The VS Code `TaxApiSample` F5 profile starts the no-argument interactive flow, which may authenticate. Do not launch it for routine validation when credentials are configured unless a live request is authorized.
- Update `src/TaxApiSample/README.md` when commands, setup, or user-visible behavior changes.
