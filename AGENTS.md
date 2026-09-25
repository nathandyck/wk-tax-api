# Agent Instructions

## API Contract Source

The OpenAPI 3 JSON documents in `swaggerFiles/` are the authoritative descriptions of the CCH Axcess web services. Consult them before implementing, changing, or documenting any API integration:

- `swaggerFiles/AuthenticationServices.json` describes authentication operations.
- `swaggerFiles/ClientServices.json` describes client operations.
- `swaggerFiles/TaxServicesV2.json` describes Tax Services v2 operations.

For each operation, verify the documented path and HTTP method, parameters, request and response schemas, required headers, authentication/security requirements, and relevant shared schema references. Preserve documented JSON property names, types, casing, required fields, and response handling. Do not infer API behavior from a service name, an existing code path, or a comment when the OpenAPI document specifies otherwise.

The documents may list multiple server URLs. Select a server appropriate to the task and existing configuration; do not assume the first server is always the intended environment. If a requested behavior is not described, or the specification and existing implementation disagree, make the discrepancy explicit instead of inventing or silently changing the contract.

If copies of specifications exist elsewhere in the repository, treat `swaggerFiles/` as authoritative. Do not edit these API contract files unless the task explicitly asks to update the specifications themselves.

## Project Architecture

- The application is a .NET 10 console CLI in `src/TaxApiSample/`.
- `Program.cs` owns command dispatch and the interactive menu. Keep API transport and token-cache behavior in `Services/` rather than adding more logic to command handlers.
- `Configuration/CchApiOptions.cs` binds the `CchTaxApi` settings from `appsettings.json`. Base URLs and non-secret switches belong in app settings; credentials and integrator keys belong in the root `.env`, based on `env.template`.
- `Services/AuthClient.cs` handles authentication, `TaxApiClient.cs` invokes tax operations, `TaxApiEndpointCatalog.cs` builds the runtime endpoint list, and `TokenCache.cs` persists the session token outside the repository.
- `src/TaxApiSample/Swagger/Auth.json` and `tsv2.json` are embedded runtime snapshots. The authoritative contracts remain the files in `swaggerFiles/`. When changing runtime behavior based on a contract, check the authoritative document and keep the relevant embedded snapshot synchronized when needed; do not edit `swaggerFiles/` unless asked to change the contract itself.
- The CLI currently assumes cached tokens expire 15 minutes after they are saved. This is a local application policy, not an expiry duration documented by the authentication OpenAPI contract. Keep that distinction clear in code and documentation.
- The interactive menu includes placeholder actions. Do not imply a placeholder is implemented; preserve the explicit placeholder labeling until the action has real behavior.

## Development Commands

- Build the CLI with `dotnet build src/TaxApiSample/TaxApiSample.csproj`.
- Run the interactive CLI with `dotnet run --project src/TaxApiSample/TaxApiSample.csproj`.
- Run a CLI command by appending `--` and its arguments, for example `dotnet run --project src/TaxApiSample/TaxApiSample.csproj -- list`.
- Authentication and API commands can make live requests. Do not run them with configured credentials or call CCH Axcess services unless the task explicitly requires it. Prefer builds and focused local checks for routine validation.

## Working Practices

- Read the nearby implementation, configuration, and documentation before changing behavior. Keep changes focused and preserve unrelated work already in the repository.
- Keep credentials, tokens, and other secrets out of source control, logs, examples containing real values, and generated output. Use `.env` for local secrets and never commit it or print its contents.
- Add or update focused tests when practical, then run the narrowest relevant validation available. At minimum, build the CLI for C# changes. Report checks that could not be run.
- Update `src/TaxApiSample/README.md` when CLI commands, setup, or user-visible behavior changes.
