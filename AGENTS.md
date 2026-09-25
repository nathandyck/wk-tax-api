# Agent Instructions

## API Contract Source

The OpenAPI 3 JSON documents in `swaggerFiles/` are the authoritative descriptions of the CCH Axcess web services. Consult them before implementing, changing, or documenting any API integration:

- `swaggerFiles/AuthenticationServices.json` describes authentication operations.
- `swaggerFiles/ClientServices.json` describes client operations.
- `swaggerFiles/TaxServicesV2.json` describes Tax Services v2 operations.

For each operation, verify the documented path and HTTP method, parameters, request and response schemas, required headers, authentication/security requirements, and relevant shared schema references. Preserve documented JSON property names, types, casing, required fields, and response handling. Do not infer API behavior from a service name, an existing code path, or a comment when the OpenAPI document specifies otherwise.

The documents may list multiple server URLs. Select a server appropriate to the task and existing configuration; do not assume the first server is always the intended environment. If a requested behavior is not described, or the specification and existing implementation disagree, make the discrepancy explicit instead of inventing or silently changing the contract.

If copies of specifications exist elsewhere in the repository, treat `swaggerFiles/` as authoritative. Do not edit these API contract files unless the task explicitly asks to update the specifications themselves.

## Applications

This repository contains the ASP.NET Core web app in `src/CchApiDemo/` and the .NET console CLI in `src/TaxApiSample/`.
Read the nearest project-level `AGENTS.md` before changing either application. Keep framework-specific architecture, commands, and user experience guidance in those files.

## Development Commands

Use the build, run, and validation commands in the relevant project-level `AGENTS.md`.

## Working Practices

Read the nearby implementation, configuration, project instructions, and documentation before changing behavior. Keep changes focused and preserve unrelated work.
Keep credentials, integrator keys, tokens, and other secrets out of source control, logs, examples containing real values, and generated output. Use the root `.env` for local secrets; never commit it or print its contents.
Authentication and API operations can make live requests. Do not call CCH Axcess unless the task explicitly requires it; prefer builds and local checks.
Add or update focused tests when practical, run relevant checks, and report anything that could not be verified.
Update the README belonging to the application whose setup or user-visible behavior changes.
