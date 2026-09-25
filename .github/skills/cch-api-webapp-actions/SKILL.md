---
name: cch-api-webapp-actions
description: 'Create and use guided CCH Axcess API actions in this demo application. Use when a user wants to add an API workflow, call Authentication Services, Client Services, or Tax Services v2, search or update returns, create a button/form/table action, or interact with their own .env-backed local instance.'
argument-hint: 'Describe the result you want and the API inputs, for example: find 1040 returns for a tax year and assign a status per return.'
user-invocable: true
---

# CCH API Web App Actions

Use this skill to turn a plain-language business task into a guided action in the local CCH Axcess demo. The user should be able to run the action from the browser with their own `.env` credentials and minimal command-line work.

## Safety and Contract Rules

- Treat `swaggerFiles/AuthenticationServices.json`, `swaggerFiles/ClientServices.json`, and `swaggerFiles/TaxServicesV2.json` as authoritative.
- Before writing code, verify each operation's documented path, method, query parameters, headers, body schema, property casing, and response shape.
- Never print, commit, or return credentials, integrator keys, or live tokens. Authentication responses shown in the UI must redact tokens.
- Use the root `.env` for the user's local instance. Do not replace it with hard-coded credentials or call a live service merely to explore an API.
- Keep API calls on the server. Browser code should send business inputs to local `/api/...` endpoints, never service credentials or tokens.
- Keep changes focused. Do not regenerate or edit the Swagger contract files.

## Request Handling

When the user describes an action:

1. Restate the requested result in concrete terms and identify the inputs, outputs, and whether the action reads or changes data.
2. Search the matching Swagger file and select the exact operation. If the request is not documented, say so and ask for the missing contract rather than inventing an endpoint.
3. Confirm whether the action needs authentication, client context, return context, or a prior action's output.
4. Add or update the smallest server endpoint in `src/CchApiDemo/Program.cs`.
5. Add the corresponding authenticated method to `CchApiClient`. Use the configured service base URL and `Security` header flow already used by the app.
6. Preserve documented PascalCase JSON properties. For OData filters, escape single quotes before URL encoding the complete `$filter` value.
7. Add the browser controls in `src/CchApiDemo/wwwroot/index.html` and behavior in `app.js`. Use a table for repeatable results and put row-level actions on the row when results can receive different updates.
8. Use the existing response panel for raw diagnostics, but redact secrets and cap unexpected upstream response text.
9. Update `styles.css` only as needed for the new controls and keep the layout usable on narrow screens.
10. Update `README.md` when the action changes user setup, inputs, or workflow expectations.

## Common Action Shapes

### Read-only lookup

Use this shape for finding clients, returns, statuses, or other collections:

- Add a `GET /api/...` local endpoint.
- Validate required user inputs with `400`.
- Require an in-memory token with the existing client guard.
- Call the documented upstream `GET` operation.
- Return the upstream JSON unchanged unless the UI needs a small display model.
- Render a list or table and show the total count when provided.

### Row-level update

Use this shape when each returned item can receive a different change:

- Render one status/control per row.
- Send one local `POST` or `PUT` request for the selected row.
- Use the exact documented request body casing and limits.
- Update the row only after the upstream call succeeds.
- Keep the raw response available for troubleshooting.

### Multi-step workflow

Use this shape when one result depends on several calls:

- Show the action steps in the left-side action plan.
- Authenticate first and keep the token server-side.
- Stop at the first failed step and report which operation failed.
- Pass only non-secret identifiers between steps.
- Return a combined result only after every step succeeds.

## Verification Checklist

After editing:

1. Run `dotnet build .\src\CchApiDemo\CchApiDemo.csproj`.
2. Run `node --check .\src\CchApiDemo\wwwroot\app.js` for browser behavior changes.
3. Press `F5` using the `CCH API Workflow Lab` launch profile.
4. Verify the home page loads and the new controls appear.
5. Verify unauthenticated calls return a controlled `401`.
6. With the user's own configured instance, exercise the action using a safe read-only input before attempting updates.
7. Report the exact upstream operation used and any live-service error without exposing secrets.

## Existing Endpoints and Patterns

- Authentication: `POST /v1.0/Authenticate` through `AuthBaseUrl`.
- Client lookup: `GET /v1.0/Client?id=...&subid=...` through `ClientBaseUrl`.
- Return lookup: `GET /api/v1/Returns?$filter=...` through `TaxBaseUrl`.
- Status lookup: `GET /api/v1/ReturnStatus` through `TaxBaseUrl`.
- Status update: `POST /api/v1/ReturnStatus` with `ReturnId` and `Status`.
- Local source: `src/CchApiDemo/`.
- Local credentials: root `.env`, loaded by walking up from the app content root.
