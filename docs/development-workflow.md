# GameChanger development workflow

GameChanger uses GitHub Flow: `main` stays deployable, and each cohesive change
is developed on a short-lived branch and merged through a pull request.

## Development cycle

1. Choose one user outcome and identify its business-rule and test-case IDs.
2. Create a branch from the latest `main`, such as
   `codex/cyc-001-create-cycle`.
3. Implement the smallest complete vertical slice and validate it locally.
4. Push the branch and open a draft pull request using the repository template.
5. Let the `CI / Required CI` check validate the frontend and API.
6. Ask an independent fresh-context agent to review the pull request with
   `docs/prompts/independent-pr-review.prompt.md`.
7. Verify and resolve review findings, then re-run affected checks.
8. Squash-merge the pull request and delete its branch.

Production API deployment runs only for API changes merged into `main`. Pull
requests never deploy.

## Recommended GitHub settings

After this local repository is pushed to GitHub, create a branch ruleset for
`main` with these settings:

- Require a pull request before merging.
- Require the `CI / Required CI` status check.
- Require conversation resolution.
- Block force pushes and branch deletion.
- Require branches to be up to date before merging.
- Allow squash merging and automatically delete head branches.

For a solo repository, do not require an approving human review because the
author cannot approve their own pull request. Record the independent agent
review in the pull-request template. Add a required human approval when another
maintainer joins the project.

## Review responsibilities

The authoring agent owns implementation, validation, and fixing confirmed
findings. The reviewing agent is intentionally read-only and owns only an
independent risk assessment. The human owner decides whether remaining P2/P3
tradeoffs are acceptable.
