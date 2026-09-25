# Agent Instructions

## API Contract Source

The OpenAPI 3 JSON documents in `swaggerFiles/` are the authoritative descriptions of the CCH Axcess web services. Consult them before implementing, changing, or documenting any API integration:

- `swaggerFiles/AuthenticationServices.json` describes authentication operations.
- `swaggerFiles/ClientServices.json` describes client operations.
- `swaggerFiles/TaxServicesV2.json` describes Tax Services v2 operations.

For each operation, verify the documented path and HTTP method, parameters, request and response schemas, required headers, authentication/security requirements, and relevant shared schema references. Preserve documented JSON property names, types, casing, required fields, and response handling. Do not infer API behavior from a service name, an existing code path, or a comment when the OpenAPI document specifies otherwise.

The documents may list multiple server URLs. Select a server appropriate to the task and existing configuration; do not assume the first server is always the intended environment. If a requested behavior is not described, or the specification and existing implementation disagree, make the discrepancy explicit instead of inventing or silently changing the contract.

If copies of specifications exist elsewhere in the repository, treat `swaggerFiles/` as authoritative. Do not edit these API contract files unless the task explicitly asks to update the specifications themselves.

## Implementation Choices

Do not assume a preferred programming language, framework, runtime, or user interface. First inspect the repository and the request. Follow established project conventions where they exist; otherwise choose only what the task requires, and keep the choice open when it does not. In particular, do not presume that this project should use JavaScript, Python, .NET, a command-line interface, or a graphical interface.

## Working Practices

- Read relevant source, configuration, tests, and documentation before changing behavior. Keep changes focused and preserve unrelated work already in the repository.
- Keep credentials, tokens, and other secrets out of source control, logs, examples containing real values, and generated output. Use the repository's existing secret-management approach when available.
- Do not call live CCH Axcess services or use real credentials unless the task explicitly requires it.
- Add or update focused tests when practical, then run the narrowest relevant validation available (such as tests, a build, or a schema/contract check). Report checks that could not be run.
- Update user-facing documentation when behavior or setup changes, without prescribing a language or interface style beyond the task's requirements.
