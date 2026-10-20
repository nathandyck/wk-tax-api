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
       "StaffServiceBaseUrl": "https://api.cchaxcess.com/api/StaffService",
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
terminal. The menu offers `Compare staff.xls with Axcess Tax` and `Check auth
token`, plus `Exit` and three explicitly labeled placeholder actions that are
not implemented yet. Startup may make a live authentication request, so
configure `.env` and check the development URLs before running it.

## Command-Line Usage

The same executable supports direct commands:

```text
TaxApiSample login
TaxApiSample check-token
TaxApiSample logout
TaxApiSample list
TaxApiSample compare-staff [staff.xls] [--authorization <token>]
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
- `compare-staff [staff.xls]` compares each workbook row with the Staff Services
  `GET /v1.0/Staff?id={staffId}` operation and prints a JSON report to
  standard output. The default workbook path is `staff.xls` in the current
  directory. It matches by `Staff ID`, ignores blank workbook cells, and reports
  field-level differences as proposed values; it does not call the Staff update
  operation. The report also lists workbook staff IDs not found in Axcess.
  Workbook headings map to the Staff API fields as follows: First Name ->
  `FirstName`, Last Name -> `LastName`, Middle Name -> `MiddleName`, Suffix ->
  `Suffix`, Title -> `Honorific`, Business unit -> `BusinessUnitName`, Department
  -> `FirmDepartmentName`, Home Office -> `HomeOfficeName`, Report Name ->
  `ReportName`, Reporting Manager -> `ReportingManagerName`, Status ->
  `StaffStatusCd`, and System Email -> `SystemEmailAddress`. Blank cells are
  ignored. Axcess staff status `A` (including the trailing space returned as
  `A `) is translated to `Active` for comparison and in the `AxcessValue` field
  of the JSON report. A cached
  `Security` token is used by default; OAuth users may pass
  `--authorization <token>`. The Staff Service base URL is configured by
  `CchTaxApi:StaffServiceBaseUrl`. The checked-in Staff Services OpenAPI names
  this query parameter `STAFFID`, while the lookup request uses `id` to match
  the service URL shape expected by this environment.

## Debugging

Open the repository root in VS Code, select **TaxApiSample** in the Run and
Debug view, then press **F5**. VS Code builds the project and starts the
interactive menu in the integrated terminal. The app checks its cached session
token on startup and attempts to authenticate if the token is missing or
expired, so configure `.env` and verify the development URLs in
`appsettings.json` before launching.

### Other Examples

You must build the project before running these commands by follow the debugging steps above or run the following command:

```bash
$> dotnet build ./src/TaxApiSample/TaxApiSample.csproj
```

```bash
$> ./src/TaxApiSample/bin/Debug/net10.0/TaxApiSample login
$> TaxApiSample compare-staff staff.xls
$> ./src/TaxApiSample/bin/Debug/net10.0/TaxApiSample check-token
$> ./src/TaxApiSample/bin/Debug/net10.0/TaxApiSample Returns --method GET --query "\$filter=TaxYear eq '2025'"
# This a 2-part process to illustrate how a Batch process works
# Part 1 - Calculate a single return as a batch
$> ./src/TaxApiSample/bin/Debug/net10.0/TaxApiSample CalculateReturn --body @src/TaxApiSample/calculate-request.json
# Part 2 - check the status of that batch by calling this using the `ExecutionID` returned from part 1
$> ./src/TaxApiSample/bin/Debug/net10.0/TaxApiSample BatchStatus --method GET --query "\$filter=BatchGuid eq '<ExecutionID-from-previous-step>' and Expand eq 'Items'"
```

> PowerShell note: wrap query strings containing `$filter`/`$orderby` in
> **single** quotes, or escape the `$`, to avoid variable expansion.

Every request automatically includes the `IntegratorKey` header (from
`.env`) and either the `Security` header (cached session token from `login`)
or an `Authorization: Bearer <token>` header (via `--authorization`, for
firms using OAuth 2.0).
