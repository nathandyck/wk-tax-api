# CchApiDemo Instructions

## Application Architecture

- This project is an ASP.NET Core 8 web app in `src/CchApiDemo/`.
- `Program.cs` currently defines the local `/api/...` routes and `CchApiClient`. Keep route handlers focused on HTTP input/output and keep upstream API behavior in the client.
- Browser assets live in `wwwroot/index.html`, `wwwroot/app.js`, and `wwwroot/styles.css`. Browser code calls only this app's local endpoints; it must never call CCH services directly or receive service credentials or tokens.
- The current workflow authenticates, looks up a client, queries returns and statuses, and can update return statuses. Confirm the requested operation against `swaggerFiles/` before changing any upstream request.

## Configuration and Security

- `CchApiOptions` is defined in `Program.cs`. Non-secret service URLs are read from the `CchApi` section of `appsettings.json`, with environment overrides such as `AUTH_BASE_URL`, `CLIENT_BASE_URL`, and `TAX_BASE_URL`.
- Credentials and integrator keys come from the root `.env`, loaded by `DotEnv.LoadFromAncestors`. Never hard-code them, commit `.env`, or expose them in browser responses or logs.
- The `CchApiClient` is a singleton and holds the session token in memory for the running process. A restart clears it. Do not persist or return the token.
- Authentication output must redact token properties. Keep response previews bounded and avoid returning secret-bearing upstream diagnostics to the browser.

## Development and Validation

- Build with `dotnet build src/CchApiDemo/CchApiDemo.csproj`.
- Run locally with `dotnet run --project src/CchApiDemo/CchApiDemo.csproj`.
- For browser script changes, check syntax with `node --check src/CchApiDemo/wwwroot/app.js`.
- Do not run workflows that call CCH services with configured credentials unless the user explicitly requests a live check. Prefer builds and local/static checks.
- Update `src/CchApiDemo/README.md` when setup, workflow steps, or user-visible behavior changes.
