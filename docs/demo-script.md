# Polaris demo for Acme Corp: runbook and talk track

Audience: developers, security, engineering leadership. Acme builds software on GitHub with Actions and VS Code, and is early in its AppSec journey.
Format: live, 30 minutes. The exercise brief says 60 minutes, so a 60 minute version is at the bottom.

## What is verified and what is not

Verified in Black Duck documentation:
- The action is `blackduck-inc/black-duck-security-scan@v2`. It runs a full scan on pushes to listed branches and a pull request scan on PRs.
- PR comments only report issues that are new relative to a baseline full scan on the target branch, and they require a GitHub token.
- C# up to C# 14 and ASP.NET Core MVC are supported, and .NET 10 support arrived with Coverity on Polaris 2026.3.0. Your tenant's Coverity version matters.
- Compiled languages need build and clean commands for SAST. This workflow supplies them.
- Code Sight is the IDE plugin, and it needs build information in a Coverity configuration file for C#.

NOT verified, so prove these in rehearsal and do not claim them live until they work:
1. That a Polaris policy violation fails the action step. The docs mention failing the build on policy violations in the GitHub App options, but do not explain how policy results map to the action's exit status.
2. Which findings Polaris actually reports for the seeded code. The expected findings below are what the code is designed to trigger, not guaranteed results.
3. Scan duration. Never quote a number you have not measured on this repo.
4. Code Sight connection to your Polaris tenant in VS Code.

Fallback for item 1 if policy does not fail the step: use the action's returned status (`steps.polaris-scan.outputs.status`) or the SARIF report in a small extra workflow step that fails on Critical or High. Tell the audience honestly which mechanism you use.

## Setup runbook (do this once, before rehearsal)

1. **Compile check.** I could not run .NET in my environment, so run these first and fix anything that fails:
   `dotnet build src/AcmePortal.Web` then `dotnet test tests/AcmePortal.Tests`, then `dotnet run --project src/AcmePortal.Web` and click through sign in and orders.
2. **Commit and push the baseline to main.** This triggers the first full Polaris scan, which is the baseline that PR scans compare against.
3. **Confirm secrets.** Repository secrets `POLARIS_ACCESS_TOKEN` (token only) and `POLARIS_SERVER_URL` (for example `https://poc.polaris.blackduck.com`).
4. **Watch the first run.** If it fails with "No concurrent entitlements" or an application error, create the application `acme-customer-portal` in Polaris manually and rerun.
5. **Review baseline findings in Polaris.** Note the real counts and severities. Triage anything you want to explain as legacy. These real numbers replace every placeholder in the script below.
6. **Create the policy.** In Polaris use Settings, then Policies. The goal is Critical and High block, everything else is tracked. Then test whether a violating PR fails the check (item 1 above).
7. **Branch ruleset.** In GitHub use Settings, Rules, Rulesets. Target the default branch, require a pull request, and require the status checks **Build and test** and **Polaris SAST + SCA**.
8. **Create demo branches.** Run `powershell -ExecutionPolicy Bypass -File .\.demo-kit\create-demo-branches.ps1`. This creates four local branches from the kit.
9. **Rehearse with throwaway names.** Push `demo/order-search-vulnerable` as `feature/rehearsal-1`, open a PR, and watch it. Close the PR without merging. For the real demo use fresh branch names and merge only once.

Live sequence for a scenario: push the vulnerable branch, open the PR, show the blocked check and comments, then push the fixed branch to the same remote branch. The fixed branch contains the vulnerable commit plus a fix commit, so it is a fast forward push.
Example: `git push origin demo/order-search-vulnerable:feature/order-search-demo` then later `git push origin demo/order-search-fixed:feature/order-search-demo`.

## Run of show (30 minutes)

| Time | Segment | Show |
|---|---|---|
| 0:00 to 3:00 | Acme's situation and goals | Slide or talk only |
| 3:00 to 8:00 | How it plugs into GitHub | `polaris.yml`, secrets, ruleset |
| 8:00 to 17:00 | Scenario 1 live: order search | PR, blocked check, comment, Polaris finding, fix, green |
| 17:00 to 21:00 | Developer experience in VS Code | Code Sight on the same flaw before commit |
| 21:00 to 26:00 | Scenario 2: dependency and path traversal | SCA finding, upgrade guidance, fix |
| 26:00 to 30:00 | Rolling it out and value | Policy phasing, recap, questions |

### 0:00 to 3:00 Acme's situation
- Say: "You ship on GitHub with Actions and VS Code, and AppSec is just starting. Three goals: scan automatically, make scans required before merge, and give developers feedback early so there is less rework."
- Say what you will not do: "I will not show every feature. I will show one change going from risky to merged."

### 3:00 to 8:00 GitHub integration
- Open `polaris.yml`. Point at the triggers: pushes to main run a full scan, pull requests run a PR scan.
- Explain that the PR scan reports only new issues against the main baseline. This is the early stage adoption story: legacy findings do not block developers on day one.
- Show that SAST and SCA run in one step, and that the build capture lines exist because C# is compiled.
- Show the ruleset: PR required, both checks required. "Nobody merges without a scan."

### 8:00 to 17:00 Scenario 1: order search (SAST)
- Open the PR "Add order search". Say what the developer intended: a harmless search box.
- While the check runs, explain what is happening (build capture, analysis, comparison to baseline). Do not promise a duration.
- Show the blocked merge button and the PR comment. Then open the finding in Polaris.
- Expected findings (confirm in rehearsal): SQL injection from string concatenation in the search query, and reflected cross site scripting from raw output of the search term.
- Walk through: severity and why it is prioritized, the data flow from the request parameter to the query (root cause), and the remediation guidance.
- Push the fixed branch. Show the fix: a parameterized query and default output encoding. Show the check turn green.
- Value line: the developer fixed this minutes after writing it, with the code still fresh. Do not quote a cost multiplier unless you can cite a source.

### 17:00 to 21:00 Developer experience
- Open the vulnerable file in VS Code with Code Sight and show the same issue before any commit or push.
- Message: the PR check is the safety net, the IDE is the fast feedback. Both use the same engine and findings.
- If Code Sight is not working in rehearsal, play a short recording instead and say so.

### 21:00 to 26:00 Scenario 2: invoice export (SCA plus SAST)
- Show the PR that adds an old Newtonsoft.Json and an invoice download endpoint.
- Expected findings (confirm in rehearsal): a vulnerable dependency version with a known CVE and available upgrade, and a path traversal in the download endpoint.
- Show component details, severity, and the upgrade guidance. Show the fix: upgrade the package and constrain file access to the invoice folder.
- Mention Fix PRs as an option that can raise dependency upgrade PRs automatically. It is off in this workflow, so describe it as available, not as shown.

### 26:00 to 30:00 Rollout and value
- Phasing for an early stage team: scan everything, block only new Critical and High, let the legacy backlog shrink over time, then tighten the policy.
- Recap in their words: required scans before merge, early feedback, scalable adoption across repos.
- Leadership message: risk is caught before production and rework happens while the change is small. Keep claims qualitative.
- Be explicit about limits: results depend on code, build setup, and policy tuning, and some findings will need triage.

## If you get 60 minutes
- Add: baseline triage in Polaris, a policy walk through, the GitHub Security tab with SARIF upload, Fix PRs live, and a rollout plan across many repositories.
- Run scenario 2 fully live instead of narrating it.

## Backup plan for a live demo
- Have both PRs pre-run on rehearsal branches with their results open in tabs.
- Have screenshots or a short recording of each key screen.
- If a scan is slow, narrate the pipeline, then switch to the pre-run PR for the findings.
