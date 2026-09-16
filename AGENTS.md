# GameChanger engineering instructions

## Purpose

Build GameChanger as small, complete vertical slices. Use
`docs/business-requirements.md` as the product contract and
`docs/test-plan.md` as the verification map.

## Commands

Frontend:

- Install locked dependencies when they are missing or the lockfile changed:
  `npm run install:ci`
- Lint: `npm run lint`
- Production build: `npm run build`

API:

- Restore: `dotnet restore GameChanger.slnx --locked-mode`
- Build: `dotnet build GameChanger.slnx --configuration Release --no-restore`
- Test: `dotnet test GameChanger.slnx --configuration Release --no-build --no-restore`

The API integration tests require Docker because they run against SQL Server
through Testcontainers.

## Project structure

- `app/`, `components/`, `hooks/`, and `lib/`: Next.js/React frontend.
- `src/GameChanger.Api/`: ASP.NET Core API.
- `tests/GameChanger.Api.IntegrationTests/`: API and SQL Server integration tests.
- `docs/business-requirements.md`: business rules and API contract.
- `docs/test-plan.md`: acceptance cases and test strategy.
- `.github/workflows/`: pull-request CI and deployment automation.

## GitHub Flow

- Keep `main` deployable. Do not develop directly on `main`.
- Use one short-lived branch for one cohesive outcome. Codex-created branches use
  `codex/<work-item>-<short-description>`.
- Reference applicable business-rule and test-case IDs in the task and pull
  request.
- Open a draft pull request when early feedback is useful.
- Keep the diff focused. Do not mix unrelated refactors or dependency upgrades
  into a feature.
- Merge only after required CI passes and review findings are resolved.
- Prefer squash merge, then delete the merged branch.

## Implementation boundaries

- Preserve unrelated user changes and existing architecture.
- Put business rules in testable domain/application code rather than UI-only
  validation.
- Keep secrets, access tokens, connection strings, and personal data out of
  source, prompts, logs, fixtures, and pull-request text.
- Do not weaken or remove a failing test to make CI pass unless the documented
  product contract changed.
- Authentication and authorization must be enforced server-side before an
  internet-accessible release.

## Independent agent review

Every pull request must receive a fresh-context review by an agent that did not
author the change.

When collaboration tools are available, the authoring agent must:

1. Complete its own review and relevant validation.
2. Start an independent reviewer with a non-inherited context
   (`fork_turns="none"` or the equivalent) and the reusable prompt in
   `docs/prompts/independent-pr-review.prompt.md`.
3. Give the reviewer the repository path, base branch, pull-request goal, and
   changed diff or files. Do not give it the author's suspected issues or
   conclusions.
4. Verify every finding against the source and address valid P0/P1 findings
   before merge. Record the disposition of lower-severity findings in the pull
   request.
5. Re-run affected validation after fixes.

The reviewer reports findings and test gaps only; it does not edit the branch.
If no independent agent is available, the pull request is not ready to merge.

## Pull-request acceptance

- The pull-request template is complete.
- The change is traceable to a requirement or explains why no requirement ID
  applies.
- Relevant tests and builds pass.
- The independent review is recorded and no unresolved P0/P1 finding remains.
- User-visible behavior, data migrations, deployment impact, and rollback are
  documented when applicable.
