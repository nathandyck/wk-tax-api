---
name: cch-api-cli-actions
description: 'Add and use CCH Axcess API workflows in this .NET console CLI. Use when a user wants a CLI command or interactive menu action for Authentication Services, Client Services, or Tax Services v2, including looking up or updating returns with their local .env-backed instance.'
argument-hint: 'Describe the command or action and its inputs, for example: add a read-only command to list returns for a tax year.'
user-invocable: true
---

# CCH API CLI Actions

Use this skill to turn a plain-language business task into a CLI command or interactive menu action in `src/TaxApiSample/`. Follow the existing .NET console application patterns and use the user's local configuration; do not introduce a web app or browser UI.

## Safety and Contract Rules

- Treat `swaggerFiles/AuthenticationServices.json`, `swaggerFiles/ClientServices.json`, and `swaggerFiles/TaxServicesV2.json` as authoritative.
- Before writing code, verify each operation's documented path, method, query parameters, headers, body schema, property casing, and response shape.
- Never print, commit, or return credentials, integrator keys, or live tokens. Redact tokens from any terminal output.
- Use the root `.env` for local credentials and `src/TaxApiSample/appsettings.json` for base URLs and non-secret options. Do not hard-code secrets or call a live service merely to explore an API.
- Authentication and API commands can make live requests. Do not run them with configured credentials or call CCH Axcess unless the user explicitly requests it. Prefer builds and local read-only commands for routine validation.
- Keep changes focused. Do not regenerate or edit the Swagger contract files.
- Preserve the menu's explicit placeholder labels until those actions have real behavior.

## Request Handling

When the user describes an action:

1. Identify the desired command or menu action, its inputs and output, and whether it reads or changes data. Ask only if important behavior is unclear.
2. Search the matching Swagger file and select the exact operation. If the request is not documented, say so and ask for the missing contract rather than inventing an endpoint.
3. Determine the required authentication and inputs from the contract and existing CLI flow. Keep any new command's arguments explicit and consistent with the current CLI syntax.
4. Put command dispatch and interactive menu wiring in `src/TaxApiSample/Program.cs`, keeping handlers thin. Put HTTP transport and token-cache behavior in `src/TaxApiSample/Services/`.
5. Reuse `AuthClient`, `TaxApiClient`, `TaxApiEndpointCatalog`, and `TokenCache` where appropriate. There is no Client Services client yet; add one under `Services/` if the requested operation requires it, following the contract and existing configuration patterns.
6. Bind non-secret settings through `Configuration/CchApiOptions.cs` and `appsettings.json`; read credentials and integrator keys from environment variables loaded from the root `.env`.
7. Preserve documented JSON property names, casing, types, required fields, and response handling. Encode query parameters correctly; for OData filters, escape embedded single quotes and URL-encode the filter value.
8. Present results and failures clearly in the terminal, use a nonzero exit code for command failures, and never display credentials, integrator keys, or tokens.
9. Update `src/TaxApiSample/README.md` when command syntax, setup, or user-visible behavior changes.

## Common Action Shapes

### Read-only command

Use this shape for documented lookups and collections:

- Add a command or endpoint invocation option with clear input validation.
- Call the documented operation through an appropriate service class.
- Print the response in a readable form without changing the documented data shape unnecessarily.
- Keep the command read-only; do not silently perform updates as a side effect.

### Update command

Use this shape for a documented update:

- Require the identifiers and values needed by the operation; validate them before sending a request.
- Use the exact documented method, headers, and request body casing and limits.
- Report the upstream result or error without claiming success before the request succeeds.
- Do not add confirmation prompts or retries that change behavior unless the user requests them or existing conventions require them.

### Interactive menu action

Use this when the user specifically wants a menu workflow rather than a command:

- Add a real action to the menu in `Program.cs`; leave unrelated placeholder actions labeled as placeholders.
- Use Spectre.Console patterns already present in the CLI for prompts and status messages.
- Stop at a failed operation and identify the failed step. Keep credentials and tokens out of menu output.

### Multi-step workflow

Use this when later operations depend on earlier results:

- Keep orchestration readable and put reusable API behavior in service classes.
- Stop at the first failed step and identify the operation that failed.
- Pass only the non-secret identifiers and values required by later steps.
- Report a completed result only after all required operations succeed.

## Verification Checklist

After editing:

1. Build with `dotnet build src/TaxApiSample/TaxApiSample.csproj`.
2. For command changes, check `help` and run safe local commands such as `dotnet run --project src/TaxApiSample/TaxApiSample.csproj -- list` when relevant.
3. For menu changes, verify compilation and menu flow without initiating authentication or API calls unless explicitly authorized.
4. The VS Code `TaxApiSample` F5 profile launches the no-argument interactive flow, which can authenticate when the token is missing or expired. Do not launch it during routine verification with configured credentials unless live authentication is explicitly requested.
5. If the user authorizes a live check, prefer a safe read-only operation before any update.
6. Report the exact upstream operation used and any live-service error without exposing secrets.

## Existing CLI Patterns

- Project: `src/TaxApiSample/`, targeting .NET 10.
- Command dispatch and interactive menu: `Program.cs`; `list` reads the embedded tax endpoint catalog without a live request.
- Authentication: `Services/AuthClient.cs`; token persistence and the 15-minute local validity policy: `Services/TokenCache.cs`.
- Tax API invocation: `Services/TaxApiClient.cs`; operation catalog: `Services/TaxApiEndpointCatalog.cs`.
- The tax catalog is built from the authoritative `swaggerFiles/TaxServicesV2.json`; consult `swaggerFiles/` for all operation contracts.
- Base URLs and non-secret switches: `appsettings.json` via `Configuration/CchApiOptions.cs`. Credentials and integrator keys: root `.env`.
- Add or update CLI usage documentation in `src/TaxApiSample/README.md`.
