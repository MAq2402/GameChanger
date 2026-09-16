---
title: Independent pull-request review
version: 1.0.0
owner: GameChanger
status: active
---

# Independent pull-request review

## Goal

Review a proposed GameChanger change independently for defects, regressions,
security problems, and missing tests before it is merged.

## Context

GameChanger has a Next.js/React frontend and an ASP.NET Core API backed by SQL
Server. Product behavior is defined in `docs/business-requirements.md`, and
expected verification is described in `docs/test-plan.md`.

## Inputs

- Repository path: `{{repository_path}}`
- Base branch or revision: `{{base_revision}}`
- Pull-request goal and acceptance criteria: `{{pull_request_goal}}`
- Changed diff, commit, or file list: `{{changes}}`
- Validation already reported by the author: `{{validation_report}}`

## Constraints

- Review only; do not edit files, commit, push, merge, or rewrite the change.
- Establish your own view from the diff and relevant source. Do not rely on the
  author's conclusions.
- Focus on behavior and risk. Skip subjective style comments unless they expose
  a correctness or maintainability problem.
- Do not report speculative issues without a concrete failure mode.
- Treat credentials, authorization boundaries, destructive data changes, and
  incorrect business rules as high-risk areas.

## Process

1. Inspect repository status and compare the change with the base revision.
2. Read the changed code and the smallest set of related requirements, tests,
   and call sites needed to understand its behavior.
3. Check correctness, API contracts, security and privacy, data integrity,
   concurrency, migrations, error handling, and user-visible regressions where
   applicable.
4. Assess whether tests cover the important success, boundary, and failure
   paths.
5. Verify reported validation when the environment permits it.

## Output contract

Return only actionable findings, ordered from highest to lowest severity.

For each finding, provide:

- Severity: `P0`, `P1`, `P2`, or `P3`.
- A concise title.
- Exact file and line reference.
- The concrete failure mode and impact.
- The smallest reasonable correction.

Then provide a `Test gaps` section containing only material missing coverage.
If there are no findings, write `No findings.` and state any residual risk or
validation that could not be completed.

## Failure behavior

If the base revision, diff, or goal is unavailable, identify the missing input
and review the available material without inventing context. If a command cannot
run, report the command and reason; do not treat an environmental failure as a
product defect.

## Examples

```text
P1 — Duplicate active cycles can be created concurrently
src/GameChanger.Api/Cycles/CreateCycle.cs:42
The application-level existence check is not protected by a database constraint,
so two simultaneous requests can both create an active cycle. Add a database
constraint or transaction that enforces the invariant and cover it with a
concurrency integration test.

Test gaps
- Concurrent create requests for the same owner are not covered.
```

## Tests

A successful review:

- Reports only issues introduced or exposed by the proposed change.
- Connects every finding to a concrete file, line, and failure mode.
- Checks applicable business-rule IDs instead of substituting personal taste.
- Separates product defects from missing validation or environmental limits.
