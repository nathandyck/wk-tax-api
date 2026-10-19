# CCH Axcess API Lab: Copilot in GitHub Codespaces

A 50-minute, browser-based lab for accountants and professionals. Participants use GitHub Copilot to add a client-search action to a starter application, try it, and export client contacts to Excel. No software development background or desktop VS Code installation is expected.

> GitHub Codespaces opens a development editor in the browser. It looks like VS Code, but nothing is installed on the participant's computer.

## What participants will make

- **Search clients:** enter one Client ID; capitalization should not matter.
- Show each matching client's addresses, phone numbers, and email addresses.
- Create one Excel sheet with exactly these columns, in order: **Client ID**, **Sub ID**, **Client Name**, **Addresses**, **Phone Numbers**, **Email Addresses**.

Prompts aim for this behavior, not identical code or identical live data. Copilot responses, API records, credentials, and workbook timestamps vary.


## 50-minute schedule

| Elapsed (minutes) | Activity |
| --- | --- |
| 0-8 | Sign in, open the starter in Codespaces, create `.env`, enter instructor-provided test details, and get the app ready to run. |
| 8-13 | Run the starter as-is, inspect its menu, and check the authentication session. |
| 13-22 | Add and try **Search clients**. |
| 22-31 | Add contact details to client search and try it. |
| 31-39 | Add Excel export and download the workbook. |
| 39-46 | Ask Copilot to check the completed experience; repeat any failed test. |
| 46-50 | Exit the app, stop the Codespace, and review privacy reminders. |

If setup takes longer, skip the optional final discussion rather than rushing participants through credential handling or the live-data warning.

## Participant setup

1. Sign up (or sign in) to GitHub and open the instructor's **starter** repository.
2. Fork the starter app using the repo-link given by instructor.
3. Choose **Code**, then **Codespaces**, then create a Codespace in the browser from the forked branch. When VS Code asks whether you trust the folder, choose **Trust the folder**.
4. In the file Explorer, make a copy of `env.template` in the repository's top-level folder and name the copy `.env` exactly. Open `.env` and replace the blank/example values with the details the instructor gives you. Leave a value blank only when the instructor says it should be blank. Do not paste any secret into Copilot chat. If the browser blocks pasting the integrator key into `.env`, allow the browser's clipboard permission; if it still does not work, try another browser. There is no required browser preference.
5. Open Copilot Chat inside the Codespaces browser editor. Set **Default Permissions** to **Allow All** in the chat, and click **Use AI Features** if prompted. Then paste the setup prompt below. Copilot may take a few minutes to prepare the workspace. Do not ask it to display `.env`.

### Setup prompt

```text
I'm new to software development, working in GitHub Codespaces in a browser. Please get this workspace ready to run the existing application. If anything needed to run it is missing, set it up for this Codespace. Do not change what the application does. Tell me in plain language when I can try the app.
```

If Copilot cannot prepare the workspace, ask the instructor for help during this first 8-minute setup period. Do not spend the feature time guessing at environment settings.

## Run the starter first

Before asking Copilot to add anything, run the existing application and look at its current menu. Paste this prompt:

```text
Rebuild and run my app.
```

When the menu appears, take a moment to see which actions are available and which are labeled as placeholders. Choose **Check auth token** to check the session. If there is no valid saved session, the app authenticates using the values in `.env`; the app also performs this check at startup, so it may already have authenticated before showing the menu. Do not share credentials or tokens in chat.

## Prompt-and-try sequence

Use one prompt at a time in the same Copilot conversation. Let Copilot finish, then use the app before moving to the next feature. Copilot can handle technical checks; participants do not need to type build commands or inspect code differences.

### 1. Search for one client

Go back to Copilot Chat and paste:

```text
Please add a menu choice called "Search clients." Ask me for one Client ID and show matching clients in a table. Uppercase and lowercase letters in the ID must be treated as the same. Search only for the ID I enter; do not download every client in the firm. Keep the search read-only and preserve all other existing menu choices.
When asking the client service to search, use its required spelling "Search" with a capital S. Lowercase "search" is rejected. Use the service's documented client-ID search and check returned IDs without regard to capitalization. Do not show or change any credentials. Make the app ready for me to try.
```

Then paste:

```text
;
```

Choose **Search clients** and enter the instructor-provided test Client ID. Try a different capitalization if time allows. Confirm the intended client appears, and that unrelated client IDs do not.

### 2. Add contact information

Go back to Copilot Chat and paste:

```text
Please enhance "Search clients" so it also fetches every address, phone number, and email address for each matching client, including clients with a Sub ID. Show all of those details in the results, and show (none) when a client has no details of a particular type. If a contact lookup fails, show that it failed instead of pretending the client has no contact information. Keep the existing Client ID search working. Do not change any client data. Make the app ready for me to try.
```

Then paste:

```text
Rebuild and run my app.
```

Choose **Search clients** again and enter the test Client ID. Check that the table shows the contact details. The application will make several read-only service calls for each matching client, so do not repeatedly search the whole client population.

### 3. Export to Excel

Go back to Copilot Chat and paste:

```text
Please make "Search clients" save its displayed results and contact details as an Excel workbook after a successful search. The workbook must have exactly one sheet and exactly these six columns, in this order: Client ID, Sub ID, Client Name, Addresses, Phone Numbers, Email Addresses. Put every contact value in its matching cell; if there are multiple values, put them on separate lines in that cell.
Save the file in the Codespace's Documents/TaxApiSample folder with a timestamped name based on the Client ID, and show me the saved path after the file is written. Do not make a live service request while editing. Make the app ready for me to try.
```

Then paste:

```text
Rebuild and run my app.
```

Choose **Search clients**, enter the test Client ID, and wait for the workbook path. To download the file from Codespaces, ask Copilot:

```text
Please help me download the Excel workbook from this Codespace to my computer using the browser. Do not put it in the project or commit it. Explain each step simply and do not show any client contact information in chat.
```

Treat the workbook as confidential. Do not commit or push it; delete it when the lab's data-handling policy says it is no longer needed.

### 4. Finish

Paste this prompt:

```text
Please check that the app has the single-Client-ID search, all contact details, and the Excel workbook with exactly my six requested columns. Check that the app is ready to run, but do not contact CCH or show private values. If something is missing, fix it and explain in plain language how I can try it.
```

The participant's check is to try each menu choice and see the expected result. No one needs to type technical commands, inspect source code, or run a manual code review.

## Simple recovery prompts

Paste the relevant sentence into Copilot Chat; do not paste credentials or client contact data.

| What you see | Prompt to give Copilot |
| --- | --- |
| Client search says “Method not allowed” | `The client search was rejected. Please check the service's required capitalization: it must say Search with a capital S, not search. Correct it and tell me how to try again.` |
| It says the CCH session has expired | `The app says the saved CCH session has expired. Please sign out of this app, clear only its saved session, and let me sign in again. Do not display or repeat any credentials or token.` |
| App cannot start in Codespaces | `Please prepare this Codespace so the existing application can run. Do not change its features or contact CCH. Explain simply when it is ready.` |
| The Excel file is not visible in the project | `The workbook is saved in this Codespace, not on my computer. Please help me download it through the browser without copying it into the project or committing it.` |

If a recovery attempt makes the app worse, use the instructor's known-good reference or ask the instructor for help; don't keep prompting against an unknown state.

## Instructor notes: timing, privacy, and capacity

- Reserve the first 8 minutes for account access, Codespaces startup, `.env` entry, and the workspace setup prompt. This is the participant's readiness check within the 50 minutes; the instructor does not need to verify every person's repository beforehand.
- A single optional rehearsal against the prepared starter is useful, but not a requirement to test each participant's fork. Keep the known-good reference available.
- Put the approved non-production service addresses in the starter before the lab. The current app settings point to the regular CCH host; do not use it for class testing.
- Fifty people may authenticate and call CCH at once. Contact CCH ahead of time about test-account licensing, simultaneous sessions, throttling, network access, and test-data availability. Stagger live searches if necessary. Prefer individual test accounts.
- The app checks its token at startup, but CCH may expire a token before the app's local 15-minute timer does. Be ready for the session-expired recovery prompt.
- A client lookup can issue a client search plus up to three contact lookups per matching client. The search should be limited to the entered Client ID; avoid firm-wide searches during the class.
- The Excel file contains addresses, phone numbers, and email addresses. Use safe test records, do not screen-share real client details, do not commit the workbook, and follow the organization's retention rules.
- Tell participants to stop Codespaces when they finish and delete them if they will not reuse them. They own their Codespace usage; check the “who pays” message when creating it.

### GitHub Copilot Free usage

GitHub's current public plan information lists 2,000 code completions per month and limited Copilot chat for Free accounts. Chat/agent usage is metered separately using AI credits, but GitHub does not publish a stable numeric Free-plan chat allowance that can support a guaranteed percentage estimate. The model is automatically selected on Free, and a request's cost varies with the repository context and response length.

This sequence uses about **9-11 participant prompts** including setup, four run requests, and final checks (about **450-550 chat turns in aggregate** for 50 people). Copilot does not expose a reliable token count for this workflow, and GitHub does not publish a stable numeric Free-plan chat-credit allowance, so a precise credit estimate or percentage would be misleading. Free accounts have separate allowances, not one shared allowance for all 50 attendees. Do not promise that usage is a specific percentage or that nobody will hit a limit.

If you need a planning percentage, use a pilot account: record its displayed remaining Free chat/AI allowance before and after completing the same 9-11 prompts. Calculate `(before - after) / before * 100`. Repeat with a second account if practical; repository context and Copilot responses can change usage. The separate 2,000 monthly code-completion allowance is not a measure of chat usage. If Copilot is unavailable or a participant reaches a limit, use the reference version for the demonstration rather than asking them to enable paid usage.

### GitHub Codespaces Free usage

GitHub currently lists 120 compute hours and 15 GB-month storage for personal Free accounts. At two cores, a 50-minute Codespace session is about 1.7 core-hours, or roughly 1.4% of 120 core-hours per participant. This is an estimate; extra active time and repeated sessions increase usage. Fifty separate personal accounts each use their own quota. If Codespaces are billed to an organization, the personal quota may not apply.

## Official references

- [GitHub Copilot plans](https://docs.github.com/en/copilot/get-started/plans-for-github-copilot)
- [GitHub AI Credits and individual usage-based billing](https://docs.github.com/en/copilot/concepts/billing-and-usage/individuals/billing)
- [GitHub Codespaces billing and Free quotas](https://docs.github.com/en/billing/concepts/product-billing/github-codespaces)
- [Creating a Codespace for a repository](https://docs.github.com/en/codespaces/developing-in-a-codespace/creating-a-codespace-for-a-repository)
- [Asking Copilot questions on GitHub](https://docs.github.com/en/copilot/how-tos/chat-with-copilot/chat-in-github)
- [CCH Axcess Developer Portal](https://developers.cchaxcess.com/)
