# CCH Connections 2026: session outline

**Status:** Draft v0.1 for discussion, October 9, 2026
**Presenters:** Nathan Dyck (Wolters Kluwer) and Sergey Chernykh (Microsoft)
**Format:** One 100-minute session, with a separate hands-on lab

## Purpose and status

This file records the flow we discussed on October 8, suggests content for each section and lists the decisions that are still open. Everything here is a proposal, so edit it directly. Each section is written as a set of beats that can be expanded into slides.

The repository is public. The outline contains no credentials, account identifiers or customer data, and it should stay that way as it grows.

## Session message

Firms already know their workflows. CCH Axcess APIs make the data and actions behind those workflows reachable from other tools, and AI coding tools reduce the effort of building on them. Testing and review remain the firm's responsibility, and any change to client or return data needs a human approval step.

## Principles from our discussion

- Describe capabilities and keep endpoints, parameters and variables out of audience-facing material.
- Say "CCH Axcess Tax" on first mention and "Axcess Tax" afterward. The audience knows the product, so it needs no explanation.
- Show a recognizable result before explaining how it was built.
- This session covers what is possible and why it matters. Setup steps and click-by-click instructions belong in the lab.
- Lead with GitHub Copilot and say that other agentic tools work in a similar way.
- Do not pad. If a section finishes early, the session finishes early and questions use the remaining time.
- Audience: users who know Axcess Tax, a mix of small and larger firms, many of whom have not used the APIs.

## Run of show

| # | Minutes | Section | Lead | Purpose |
|---|---|---|---|---|
| 1 | 0–10 | Welcome and framing | Both | State the problem, the plan for the session and how it relates to the lab |
| 2 | 10–25 | What the CCH Axcess APIs make possible | Nathan | Explain in business terms what firms can connect to and where to start |
| 3 | 25–40 | An application built with the APIs and agentic coding | Nathan, with Sergey | Show a working result and explain how it was built |
| 4 | 40–75 | AI tools that work with APIs | Sergey | Compare the ways AI tools use APIs and where Microsoft platforms fit |
| 5 | 75–85 | Lab preview | Both | Describe what attendees build in the lab and how to prepare |
| 6 | 85–100 | Questions | Both | Answer questions and route workflow ideas to the lab |

Sections 2 and 3 can run 10 to 15 minutes each, and section 4 can run 30 to 45. Questions have a 10-minute minimum and absorb any time that remains.

## 1. Welcome and framing (0–10)

Lead: both.

- Introductions and a one-sentence statement of what the session covers.
- The problem: repetitive work that moves information between CCH Axcess and spreadsheets, CRMs and other tools. Nathan to add an example the audience will recognize.
- The plan: what the CCH Axcess APIs can do, a working application, how AI tools work with APIs, and the lab.
- Optional show of hands on who has used the APIs and who has used an AI coding tool. The answers help calibrate the rest of the session.

## 2. What the CCH Axcess APIs make possible (10–25)

Lead: Nathan. Audience-facing language only.

Suggested slides:

1. **An API in one slide.** A way for software to do what a person does in the application, such as looking up a client or finding returns. Access requires sign-in and follows authorization policies. Nathan to confirm how permissions apply.
2. **What is newly available to this audience.** Nathan to confirm the wording, including what changed and who can use it now.
3. **Capabilities by business area,** with one example for each:
   - Clients, staff and firm structure (Common APIs)
   - Tax returns: find returns, see and update status, check history (Tax APIs)
   - Advanced tax: e-file status, tax data transfer, printing and roll-forward (additional licensing)
   - Engagement, firm management (practice, documents, workstream, iQ) and workflow
4. **Included and additional capabilities.** A simple table of what comes with the tax license and what needs additional licensing. Nathan to confirm the packaging.
5. **How a request works,** without field names: sign in, ask, receive structured information, use it.
6. **Examples from firms.** Nathan to choose. Published examples include checking e-file status before roll-forward, batch-printing organizers, syncing client data with other systems and importing K-1 data from Excel.
7. **Integration possibilities.** Spreadsheets, CRMs, document tools and dashboards, as a one-time task or an ongoing process.
8. **Where to learn more.** The Developer Portal, API training and consulting, and the CCH Marketplace.

Transition: that covers what is possible. If you are not a developer, how do you build something? Here is an example.

## 3. An application built with the APIs and agentic coding (25–40)

Lead: Nathan, with Sergey commenting on the AI-assisted build.

Frame the demonstration with the guided-journey structure from the planning notes: the workflow is the business problem, the journey steps are the sequence and the expected output is the result.

- The business problem in the audience's words, in about one minute.
- The finished application running against the test firm, from input to result.
- How it was built: the request given to Copilot, the guidance it worked from (the OpenAPI files, `AGENTS.md` and the skill in this repository), what it produced, what was corrected and how the result was tested.
- Division of work. AI interprets the request, reads the API definitions, builds the requests, sequences the calls and shapes the results. The API handles authentication, data access, updates and security. The firm decides the workflow, reviews the output and approves changes.
- Cost clarification: once built, the application runs without AI. Section 4 covers when AI is used at run time.
- Other ideas built on the same APIs, shown as screenshots or sample output without live work.

Demonstration mode is an open decision. The proposed default is the finished application live, the build shown through screenshots or a short recording, and a live change only if it has been rehearsed. A recorded fallback covers unexpected responses. Run against the designated test firm and never against real client data.

Transition: building an application is one way to use the APIs. AI tools can work with them in other ways too.

## 4. AI tools that work with APIs (40–75)

Lead: Sergey. Detail to follow in the next draft.

| Minutes | Topic | Content |
|---|---|---|
| 5 | From chat to agents | Chat assistants, agents that use tools and coding agents that write and test software. Kept short. |
| 8 | Two ways AI uses an API | Build time: AI helps write an application that later runs on its own. Run time: an AI agent calls the API and reasons over the response, with usage-based AI cost. When each fits. |
| 10 | Microsoft platforms and their roles | GitHub Copilot and Codespaces for building applications. Microsoft 365 Copilot and Copilot Studio for agents in tools such as Outlook, Teams, Word and Excel. Microsoft Foundry for building and operating custom agents. Agent 365 for observing and governing agents across an organization. One example per platform of where the CCH Axcess APIs could act as a tool. |
| 12 | Back to GitHub Copilot | The development loop of request, API definitions, build, test, review and revise. What the repository contains to guide the agent. A short demonstration or recording. Other agentic tools follow a similar pattern. Test data, limited access and review before any update. |

Present the platform examples as illustrations of what is possible, and keep them separate from released Wolters Kluwer or Microsoft product features.

Transition: if you want to try this yourself, that is what the lab is for.

## 5. Lab preview (75–85)

Lead: both.

- What attendees do: describe a workflow in plain language, use GitHub Copilot to build an application on the CCH Axcess APIs, test it against a demo firm and leave with a working prototype and a starter project.
- The lab is a standalone experience. Align this slide with the lab description.
- A preview of the setup: GitHub Copilot, the code that connects to the API and one example application.
- Take-home path: this repository is the starting point. Add your own API credentials to the local settings file, which is excluded from source control, and begin. Nathan to confirm what attendees need on their license to run it against their own firm.
- Lab dates, times and advance preparation are confirmed by the lab team and added here.
- Close by inviting attendees to bring a workflow idea.

## 6. Questions (85–100)

Lead: both. Starting points for likely questions:

| Question | Direction for the answer |
|---|---|
| Do I pay for AI every time I use the API? | Not necessarily. An application built with AI runs without AI. A tool that uses AI at run time incurs usage cost. See section 4. |
| Do I need a developer? | Nathan: what Wolters Kluwer offers through the Developer Portal, utilities, training and consulting. Sergey: how far agentic tools go and where review is needed. |
| Is it safe? | Access requires sign-in and follows authorization policies. Use test data first, limit access and approve changes before applying them. |
| Which AI tool should I use? | The approach works with several tools. Start with one the firm has already approved. |
| What does it cost, and which license do we need? | Nathan: core and additional licensing. |
| Can it do my workflow? | Talk it through: which data, which APIs, which tool. Point the idea to the lab. |

If few questions come up, invite two or three workflow ideas from the audience and discuss how each would be approached. End early rather than add material.

## Demo candidates

Scale: 1 to 5. Higher usefulness is better. Higher complexity means more effort to produce a repeatable, rehearsed result, and it does not measure production readiness. The last column repeats the planning notes (Demo / Lab / Complexity) for comparison.

| Candidate | Usefulness | Complexity | Suggested role | Planning notes |
|---|---:|---:|---|---|
| Client address synchronization | 5 | 3 | Main demonstration: compare a spreadsheet with client records, review the proposed changes and apply them to the test firm | Yes / Yes / Medium |
| Client contact export | 3 | 3 (2 for a few fields and clients) | Lab starter exercise | Yes / Yes / Low |
| Return status reporting | 4 | 2 | Lab core exercise and backup demonstration | Yes / Yes / Medium |
| Contact export with return status | 5 | 4 | Lab stretch exercise | No / Optional / High |
| Workpaper-to-return automation | 5 | 5 | Illustration only, not live | Optional / No / High |

Notes:

- Return status reporting is closest to existing code. The `1-web-app-demo` branch lists returns by tax year and return type and can apply a status.
- Address synchronization needs operations that neither the main branch nor the web demo uses yet: the client address list and the address update. It also needs a compare-and-apply step. The main branch has no Client Services client.
- The full contact export needs separate lookups for address, email and phone for each client, so it is more work than it first appears.
- Address synchronization changes client records. Run it against the test firm, show a preview before applying anything and read the records back afterward.
- Define "all my returns" before building: one client, one signer or the whole firm.
- Return status is separate from e-file status and from workflow status.
- The return list matches Client ID with a contains search, so confirm exact IDs when joining it with client data.
- Workpaper-to-return automation requires the Advanced Tax Transfer license and independent review of the results.

## Open items

| Item | Owner | Target |
|---|---|---|
| Choose the demonstration application and the lab scenario | Nathan and Sergey | Working session on October 15 |
| Specify what is newly available to this audience (section 2) | Nathan | Next draft |
| Share test access so the demonstration path can be run end to end | Nathan | Before October 15 (proposed) |
| Choose the demonstration mode: live, recorded or hybrid | Nathan and Sergey | October 15 |
| Draft section 4 detail and one platform example for each product | Sergey | Next draft |
| Confirm lab dates, times and expected attendance | Nathan and lab team | Open |
| Align the session title and description with this outline | Nathan | Open |
| Schedule a full rehearsal, including the demonstration on the conference setup | Nathan and Sergey | Before the conference |

## Summary for conference organizers

Copy-ready text.

**Title:** Use the title listed in the conference portal.

**Description:** This session shows how CCH Axcess APIs connect firm data and workflows to other tools, demonstrates a working application built with AI-assisted development and compares the ways AI tools and Microsoft platforms work with APIs. Attendees leave with ideas for their own firm and a path to try them in the hands-on lab.

**Learning objectives:**

- Describe what the CCH Axcess APIs can do for tax and accounting workflows.
- Recognize how agentic AI tools help build applications on those APIs.
- Compare the ways AI tools and Microsoft platforms work with APIs.
- Identify a workflow in your firm to improve and know where to start.

**Agenda (100 minutes):** Welcome and framing, 10. CCH Axcess API capabilities, 15. Demonstration of an application built with agentic coding, 15. AI tools and Microsoft platforms, 35. Lab preview, 10. Questions, 15.

## References

- [CCH Axcess Open Integration APIs](https://www.wolterskluwer.com/en/solutions/cch-axcess/open-integration), Wolters Kluwer
- [API Spotlight: Tax Transfer APIs](https://www.wolterskluwer.com/en/expert-insights/api-spotlight-harnessing-the-power-of-tax-transfer-apis), Wolters Kluwer
- [Frequently asked questions about tax preparation APIs](https://www.wolterskluwer.com/en/expert-insights/frequently-asked-questions-about-tax-preparation-apis), Wolters Kluwer
- OpenAPI definitions: `swaggerFiles/` in this repository
- [Microsoft Agent 365 overview](https://learn.microsoft.com/en-us/microsoft-agent-365/overview), Microsoft Learn
