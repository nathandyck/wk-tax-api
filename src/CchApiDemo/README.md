# CchApiDemo Web App


A small ASP.NET Core demo for walking through CCH Axcess API calls from a browser. The server reads credentials from `.env`; credentials and service tokens are never sent to the browser or written to logs.

The guided workflow performs these contract-defined calls in order:

1. `POST /v1.0/Authenticate` in Authentication Services
2. `GET /v1.0/Client?id=...&subid=...` in Client Services
3. `GET /api/v1/Returns` in Tax Services v2
4. `GET /api/v1/ReturnStatus` in Tax Services v2
4. `POST /api/v1/ReturnStatus` in Tax Services v2

This can be seen in the `Result Path` section at the left of the page.  
To start complete the Setup and Configuration sections in this document, hit `F5` and the application should start in a browser window.  
Then select the `Authenticate Only` button at the left of the page, then in the `Run the sequence` section, enter a ClientID that is in the account you are logged into. This will load the client response data in the window below.  
Then in the `Return actions` section, enter a valid tax year {TODO: Specify based on test data} and select the desired Return Type from the drop down. Click the green `Find Returns` button and returns should appear in a table below. You may select a `New Status` from the drop down and select `Apply` which will update the status of that return.  
This shows searching, getting data, and writing data in the API.

The paths, methods, property casing, and headers follow the OpenAPI documents in `swaggerFiles/`.

## Setup

1. Copy `.env.example` to `.env`.
2. Set `INTEGRATOR_KEY`, `CCH_USERNAME`, and `CCH_PASSWORD`. Add `CCH_USER_SID` and `CCH_REALM` when your account requires them.
3. Press `F5` in VS Code and choose **CCH API Workflow Lab**. The browser opens automatically.

The application lives in `src/CchApiDemo`. The `.vscode/launch.json` and `.vscode/tasks.json` files build and launch it, so no command-line startup is needed for the demo. The root `.env` is discovered automatically.

## Configuration

The sample application's service base URLs are configured in `src/CchApiDemo/appsettings.json` under `CchApi`. The defaults use the production DNS name `https://api.cchaxcess.com`:

- `AuthBaseUrl`: `https://api.cchaxcess.com/api/AuthService`
- `ClientBaseUrl`: `https://api.cchaxcess.com/api/ClientService`
- `TaxBaseUrl`: `https://api.cchaxcess.com/taxservices/oiptax`

Set `AUTH_BASE_URL`, `CLIENT_BASE_URL`, or `TAX_BASE_URL` in the root `.env` to override an individual URL for your own instance. Environment values take precedence over `appsettings.json`. If the configuration file is missing, cannot be loaded, or a URL is absent, the application falls back to the corresponding `api.cchaxcess.com` URL. `AUTH_OPERATION` defaults to `Authenticate` and can be set to `AzureAuthenticate`. `SECURITY_HEADER` defaults to the documented `Security` header and can be changed to `Authorization` for an OAuth bearer-token flow.

The token is held in memory for the lifetime of the running demo process. Restart the debugger to clear it. Do not commit `.env` or real credentials.

## Guided Action Skill

The repository includes the user-invocable web app skill `.github/skills/cch-api-webapp-actions/SKILL.md`. In Copilot Chat, use `/cch-api-webapp-actions` and describe the result you want in plain language. The skill guides the agent to the correct Swagger operation, adds the server and browser action, protects credentials, and validates the change in this local instance.
