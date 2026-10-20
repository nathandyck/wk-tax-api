# TaxApiSample

An interactive .NET 10 CLI for the CCH Axcess **Tax Services v2** API
(`swaggerFiles/TaxServicesV2.json`), using a session token obtained from the
**Authentication API** (`swaggerFiles/AuthenticationServices.json`). Run it
without arguments to open the terminal menu, or pass a command for direct CLI
use.

## Setup

1. From the repository root, copy `env.template` to `.env` and fill in your values:
   ```
   INTEGRATOR_KEY=your-integrator-or-subscription-key
   CCH_USERNAME=your-username
   CCH_PASSWORD=your-password
   CCH_USER_SID=
   CCH_REALM=
   ```
   `.env` is gitignored and must never be committed.

2. Base URLs live in `appsettings.json` (not secret), one entry per API:
   ```json
   {
     "CchTaxApi": {
       "AuthBaseUrl": "https://api.cchaxcess.com/api/AuthService",
       "TaxServiceBaseUrl": "https://api.cchaxcess.com/taxservices/oiptax",
       "UseAzureAuthenticate": false
     }
   }
   ```
   Point these at whichever CCH Axcess environment/server you use. Set
   `UseAzureAuthenticate` to `true` if your firm uses Azure AD authentication
   instead of the CCH Axcess login method.

## Interactive CLI

```
$> TaxApiSample
```

With no arguments, the CLI checks the cached session token, attempts to log in
if it is missing or expired, then opens the interactive action menu in the
terminal. The menu currently offers `Check auth token`, `Exit`, and three
explicitly labeled placeholder actions that are not implemented yet. Startup
may make a live authentication request, so configure `.env` and check the
development URLs before running it.

## Command-Line Usage

The same executable supports direct commands:

```text
TaxApiSample login
TaxApiSample check-token
TaxApiSample logout
TaxApiSample list
TaxApiSample <endpoint-name> [--method GET|POST|PUT|DELETE]
                              [--query key=value ...]
                              [--body '<json>' | --body @file.json]
                              [--security <token>] [--authorization <token>]
                              [--integrator-key <key>]
TaxApiSample help
```

`help`, `-h`, and `--help` display command usage. `endpoints` is an alias for
`list`. The `list` command prints each Tax Services v2 endpoint (parsed from
the embedded `swaggerFiles/TaxServicesV2.json`), its supported HTTP methods,
query parameters, and whether it requires a request body. An endpoint name is
the final path segment, for example `Returns`, `CalculateReturn`,
`BatchStatus`, or `ReturnSections`; the method is inferred when only one is
available.

- `login` authenticates using credentials from `.env` and caches the returned
  session token under `%LOCALAPPDATA%\TaxApiSample\token.json`.
- `check-token` checks the cached token and authenticates if it is missing,
  expired, or unreadable. The application uses a local 15-minute validity
  policy after saving a token; this is not an expiry duration specified by the
  Authentication API.
- `logout` removes the cached token.

## Debugging

Open the repository root in VS Code, select **TaxApiSample** in the Run and
Debug view, then press **F5**. VS Code builds the project and starts the
interactive menu in the integrated terminal. The app checks its cached session
token on startup and attempts to authenticate if the token is missing or
expired, so configure `.env` and verify the development URLs in
`appsettings.json` before launching.

### Examples

```
$> TaxApiSample login
$> TaxApiSample Returns --query "$filter=TaxYear eq '2023'" --query "$orderby=ReturnClientName"
$> TaxApiSample CalculateReturn --body @calculate-request.json
$> TaxApiSample Returns --method DELETE --body '{"ReturnId":["2021I:TEST1:V1"],"RecoveryCopies":false,"K1ImportFiles":false}'
```

> PowerShell note: wrap query strings containing `$filter`/`$orderby` in
> **single** quotes, or escape the `$`, to avoid variable expansion.

Every request automatically includes the `IntegratorKey` header (from
`.env`) and either the `Security` header (cached session token from `login`)
or an `Authorization: Bearer <token>` header (via `--authorization`, for
firms using OAuth 2.0).
