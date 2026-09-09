# GameChanger test plan

## Purpose

The test suite protects the core product loop: create a cycle, group goals into categories, complete weekly reviews, and finish the cycle. Category-specific modules extend that loop without changing or losing the core data.

## Test levels

| Level | Purpose | Dependencies |
| --- | --- | --- |
| Unit | Business rules, calculations, validation, and date boundaries | None |
| Integration | HTTP contract, EF Core mappings, migrations, SQL behavior, and transactions | SQL Server through Testcontainers |
| Contract | Generated OpenAPI document and frontend client compatibility | Running API |
| End-to-end | Complete user journeys through React and the API | Deployed or locally composed application |

Integration tests use the same SQL Server provider as Azure SQL. One disposable SQL Server container is shared by a test collection and destroyed after the run. When migrations and mutable domain data are introduced, each test will begin from a known schema and reset data between scenarios.

## Automated foundation

The initial executable suite proves the test infrastructure before domain endpoints are added:

| Test | What it proves |
| --- | --- |
| API information is available | The complete ASP.NET Core application starts and handles HTTP requests |
| Health endpoint reports healthy with SQL Server | The API health check reaches the Testcontainers database |
| EF Core can query the container database | The production SQL Server provider and dependency injection are wired correctly |

To run locally, start Docker Desktop and execute:

```powershell
dotnet test GameChanger.slnx --configuration Release
```

The first run may take longer while Docker downloads the pinned SQL Server 2022 image.

## Core acceptance and test cases

### Cycles

| ID | Scenario | Expected result | Primary level |
| --- | --- | --- | --- |
| CYC-001 | Create a cycle with a name, start date, and valid length | Cycle is created in draft state | Integration |
| CYC-002 | Use a length below 1 or above 52 weeks | Request is rejected with validation details | Unit + integration |
| CYC-003 | Start a draft cycle containing categories and goals | Cycle becomes active and week 1 is available | Integration |
| CYC-004 | Start a second cycle while another is active | Request is rejected | Integration |
| CYC-005 | Calculate the current week from the cycle start date | Correct week is returned at every boundary | Unit |
| CYC-006 | Shorten a cycle below an already reviewed week | Request is rejected without data loss | Integration |
| CYC-007 | Complete the final week | Cycle becomes ready for the final review | Integration |
| CYC-008 | Archive a completed cycle | History remains readable and cannot be changed accidentally | Integration |
| CYC-009 | Duplicate a completed cycle | New draft contains copied categories and goals but no old reviews | Integration |

### Categories

| ID | Scenario | Expected result | Primary level |
| --- | --- | --- | --- |
| CAT-001 | Add a category to a draft cycle | Category is created in the requested position | Integration |
| CAT-002 | Add categories with the same display name | Defined duplicate-name rule is enforced | Unit + integration |
| CAT-003 | Reorder categories | New order is preserved | Integration |
| CAT-004 | Archive a category used in previous reviews | Historical reviews retain the category | Integration |
| CAT-005 | Promote a generic category to a module | Goals and review history remain unchanged | Integration |
| CAT-006 | Disable a category module | Module data is retained but generic category behavior remains available | Integration |
| CAT-007 | Request an unsupported module type | Request is rejected without changing the category | Integration |

### Goals

| ID | Scenario | Expected result | Primary level |
| --- | --- | --- | --- |
| GOL-001 | Add a goal to a category | Goal is created under that category | Integration |
| GOL-002 | Move a goal between categories before the cycle starts | Goal appears only in the destination category | Integration |
| GOL-003 | Move or archive a goal after reviews exist | Previous review entries retain their original context | Integration |
| GOL-004 | Create a goal without a title | Request is rejected with validation details | Unit + integration |
| GOL-005 | Reorder goals within a category | New order is preserved | Integration |

### Weekly reviews

| ID | Scenario | Expected result | Primary level |
| --- | --- | --- | --- |
| REV-001 | Save a partial review | Draft is persisted and can be resumed | Integration |
| REV-002 | Rate a goal from 1 to 5 | Rating is accepted | Unit + integration |
| REV-003 | Rate a goal outside 1 to 5 | Request is rejected | Unit + integration |
| REV-004 | Mark a goal not applicable | No numeric score is required | Integration |
| REV-005 | Submit a complete weekly review | Review becomes completed | Integration |
| REV-006 | Submit a review with an unrated applicable goal | Request is rejected with the missing goal identified | Integration |
| REV-007 | Calculate a category average | Applicable goal ratings are averaged; N/A goals are excluded | Unit |
| REV-008 | Calculate an overall weekly average | Category and goal rules produce the agreed result | Unit |
| REV-009 | Attempt a duplicate review for the same cycle week | Uniqueness is enforced by the API and database | Integration |
| REV-010 | Edit a completed review | Allowed changes follow the agreed audit/history rule | Integration |
| REV-011 | Read review history | Reviews are ordered by week and grouped by category | Integration |
| REV-012 | Detect low or falling scores | Review analysis identifies the correct goals and categories | Unit |

### Final review

| ID | Scenario | Expected result | Primary level |
| --- | --- | --- | --- |
| FIN-001 | Start a final review before all required weeks are complete | Request is rejected | Integration |
| FIN-002 | Complete all final-review questions | Cycle is marked completed | Integration |
| FIN-003 | Produce a cycle summary | Ratings, completed goals, and recurring low scores are correct | Unit + integration |

### Module contract

Every category module must pass the same contract tests:

| ID | Scenario | Expected result |
| --- | --- | --- |
| MOD-001 | Enable the module | Existing category goals and reviews remain available |
| MOD-002 | Save module-specific data | Data is scoped to the correct category and cycle |
| MOD-003 | Disable and re-enable the module | Module data is restored |
| MOD-004 | Archive the category | Module data remains in historical views |
| MOD-005 | Delete or modify another category | Module data is unaffected |

## Cross-cutting cases

- A user cannot read or modify another user's cycles.
- Repeated create requests with the same idempotency key do not create duplicates.
- Concurrent weekly-review submissions cannot bypass uniqueness constraints.
- Dates and week boundaries remain correct across time zones and daylight-saving changes.
- Invalid identifiers return `404`; invalid state transitions return `409`; invalid input returns `400` with problem details.
- API logs do not expose review notes, connection strings, or authentication tokens.

## Pipeline policy

- Every pull request restores, builds, and runs all tests.
- Integration tests require a Docker-capable runner; GitHub-hosted Ubuntu runners satisfy this requirement.
- A failed container startup, migration, health check, or test blocks deployment.
- Only the already-tested publish artifact is passed to the deployment job.
- Tests must not depend on execution order or an existing developer database.
