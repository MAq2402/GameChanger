# GameChanger business requirements — generic MVP

Related document: [GameChanger test plan](./test-plan.md).

## 1. Product outcome

GameChanger helps one person run a focused improvement cycle without turning the process into a complicated project-management system. The first version must make it easy to:

1. Create a cycle and choose its length.
2. Group goals into categories.
3. Start the cycle.
4. Complete a short review every week using ratings from 1 to 5.
5. See progress during the cycle and a summary at the end.

The generic category and goal model is the foundation. A category may later enable a dedicated module with custom functionality, but the generic workflow and historical data must continue to work.

## 2. MVP scope

### Included

- One personal owner and one active cycle at a time.
- Configurable cycle length from 1 to 52 weeks; the UI defaults to 10 weeks.
- Draft, active, completed, and archived cycle states.
- User-defined categories and goals.
- Ordered categories and ordered goals within each category.
- Weekly review drafts and explicit submission.
- A rating of 1–5 or `Not applicable` for every reviewed goal.
- Optional weekly notes: wins, challenges, and focus for the next week.
- Cycle progress and final summary.
- Stable category identifiers so a generic category can later become a custom module.

### Not included in the generic MVP

- Category-specific modules.
- Shared cycles, teams, coaches, or social features.
- Notifications and calendar integrations.
- AI-generated goals or recommendations.
- Native mobile applications.
- File attachments.
- Multiple simultaneous active cycles.

## 3. Main user flow

1. **Create cycle** — enter a name, start date, time zone, and number of weeks.
2. **Create categories** — for example Health, Relationships, Learning, or Finance.
3. **Create goals** — add one or more goals under each category.
4. **Review setup** — confirm cycle dates, categories, and goals.
5. **Start cycle** — the draft becomes active and week 1 is available.
6. **Weekly review** — rate each applicable goal, add optional notes, save a draft, and submit it.
7. **Track progress** — see completed reviews, missing weeks, averages, and score trends.
8. **Complete cycle** — after the final week and all required reviews, complete the cycle and view its summary.
9. **Archive cycle** — keep the history read-only and start planning another cycle.

## 4. Business concepts

| Concept | Description | Important fields |
| --- | --- | --- |
| Cycle | A fixed-length personal improvement period | ID, name, start date, time zone, length, status |
| Category | A user-defined grouping of goals | ID, cycle ID, name, color/icon, order, optional module key |
| Goal | A measurable or assessable intention within a category | ID, category ID, title, description, order, active state |
| Cycle week | A calculated seven-day window within a cycle | Cycle ID, week number, start date, end date |
| Weekly review | The user's review for one cycle week | ID, cycle ID, week number, status, notes, submitted time |
| Review entry | The assessment of one goal in a weekly review | Review ID, goal ID, rating or N/A, note, goal/category snapshot |

## 5. Business rules

### Cycles

- **BR-CYC-01:** Cycle length must be between 1 and 52 weeks.
- **BR-CYC-02:** A new cycle begins in `Draft` status.
- **BR-CYC-03:** Only one cycle may have `Active` status for the owner.
- **BR-CYC-04:** A cycle can start only when it contains at least one category and one active goal.
- **BR-CYC-05:** Week 1 starts on the cycle start date. Every week is a consecutive seven-day window calculated in the cycle's time zone.
- **BR-CYC-06:** Cycle dates and length may be changed only while the cycle is a draft.
- **BR-CYC-07:** A cycle can be completed only after its final week has started and every required weekly review has been submitted.
- **BR-CYC-08:** Only drafts may be permanently deleted. Completed cycles are archived, not deleted.

### Categories and goals

- **BR-CAT-01:** Category names must be present and unique within a cycle, ignoring letter case.
- **BR-CAT-02:** Goal titles must be present.
- **BR-CAT-03:** Categories and goals have an explicit display order.
- **BR-CAT-04:** Categories and goals may be freely changed in a draft cycle.
- **BR-CAT-05:** During an active cycle, new categories or goals affect the current and future unsubmitted reviews only.
- **BR-CAT-06:** Archiving a category or goal must not remove it from previously submitted reviews.
- **BR-CAT-07:** Review entries store goal and category name snapshots so history remains understandable after later edits.
- **BR-CAT-08:** `ModuleKey` is empty for a generic category. Enabling a future module must not change the category ID or remove its goals and review history.

### Weekly reviews

- **BR-REV-01:** A cycle can have at most one weekly review for each week number.
- **BR-REV-02:** A review can be created only after that week has started; future weeks cannot be reviewed.
- **BR-REV-03:** Saving a review creates or replaces its draft without submitting it.
- **BR-REV-04:** Each applicable goal rating must be an integer from 1 to 5.
- **BR-REV-05:** `Not applicable` entries do not require a rating and are excluded from averages.
- **BR-REV-06:** Submission requires an entry for every goal applicable to that review.
- **BR-REV-07:** Submitting the same already-submitted review is safe and does not create a duplicate.
- **BR-REV-08:** A submitted review is read-only until the owner explicitly reopens it. A review cannot be reopened after the cycle is completed or archived.
- **BR-REV-09:** A reopened review retains its identity and submission history.

### Progress and scoring

- **BR-PRG-01:** A category's weekly score is the arithmetic mean of its applicable goal ratings.
- **BR-PRG-02:** The overall weekly score is the arithmetic mean of all applicable goal ratings, so categories with different numbers of goals do not receive artificial equal weighting.
- **BR-PRG-03:** Progress shows elapsed weeks, submitted reviews, missing reviews, category averages, and rating trends.
- **BR-PRG-04:** The final summary uses submitted reviews only.

## 6. API conventions

- Base path: `/api/v1`.
- Resource identifiers are UUIDs; week numbers are integers starting at 1.
- Dates use ISO 8601. Cycle start dates are date-only values, and timestamps are UTC.
- The API never accepts an owner/user ID from the client; ownership comes from the authenticated identity.
- Successful creates return `201 Created` and a `Location` header.
- Successful reads and state-changing commands return `200 OK`; updates without a body may return `204 No Content`.
- Validation errors return `400 Bad Request` using Problem Details.
- Missing or inaccessible resources return `404 Not Found`.
- Invalid state transitions and uniqueness conflicts return `409 Conflict`.
- Create and command endpoints accept an `Idempotency-Key` header so client retries do not duplicate data.
- List responses use a consistent `{ "items": [...] }` envelope, leaving room for pagination later.

Authentication is required before an internet-accessible production release. Authentication may be provided by Azure App Service Authentication, so the MVP does not need custom registration, login, password, or token endpoints.

## 7. Required API endpoints

### Platform

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `GET` | `/api/v1` | Return API name and version information |
| `GET` | `/health` | Report application and SQL Server health |

### Cycles

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `POST` | `/api/v1/cycles` | Create a draft cycle |
| `GET` | `/api/v1/cycles` | List cycles, optionally filtered by status |
| `GET` | `/api/v1/cycles/active` | Return the active cycle, or `404` when none exists |
| `GET` | `/api/v1/cycles/{cycleId}` | Return cycle details, categories, and goals |
| `PATCH` | `/api/v1/cycles/{cycleId}` | Change draft cycle name, dates, time zone, or length |
| `DELETE` | `/api/v1/cycles/{cycleId}` | Permanently delete a draft cycle |
| `POST` | `/api/v1/cycles/{cycleId}/start` | Validate the setup and activate the cycle |
| `POST` | `/api/v1/cycles/{cycleId}/complete` | Complete the cycle after all required reviews |
| `POST` | `/api/v1/cycles/{cycleId}/archive` | Archive a completed cycle |

### Categories

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `GET` | `/api/v1/cycles/{cycleId}/categories` | List categories with their goals |
| `POST` | `/api/v1/cycles/{cycleId}/categories` | Add a generic category |
| `PATCH` | `/api/v1/categories/{categoryId}` | Change category name, appearance, or active state |
| `DELETE` | `/api/v1/categories/{categoryId}` | Delete an unused draft category or archive a historically used category |
| `PUT` | `/api/v1/cycles/{cycleId}/categories/order` | Replace the category display order |

### Goals

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `GET` | `/api/v1/categories/{categoryId}/goals` | List goals in display order |
| `POST` | `/api/v1/categories/{categoryId}/goals` | Add a goal to a category |
| `PATCH` | `/api/v1/goals/{goalId}` | Change title, description, category, order, or active state |
| `DELETE` | `/api/v1/goals/{goalId}` | Delete an unused draft goal or archive a historically used goal |
| `PUT` | `/api/v1/categories/{categoryId}/goals/order` | Replace the goal display order within a category |

### Cycle weeks and weekly reviews

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `GET` | `/api/v1/cycles/{cycleId}/weeks` | List calculated weeks and review status for each week |
| `GET` | `/api/v1/cycles/{cycleId}/weeks/current` | Return the current week, goals to review, and draft/submitted status |
| `GET` | `/api/v1/cycles/{cycleId}/weeks/{weekNumber}/review` | Return the review and its entries; return an empty review shape if none exists |
| `PUT` | `/api/v1/cycles/{cycleId}/weeks/{weekNumber}/review` | Create or replace a weekly-review draft |
| `DELETE` | `/api/v1/cycles/{cycleId}/weeks/{weekNumber}/review` | Delete a review draft; submitted reviews cannot be deleted |
| `POST` | `/api/v1/cycles/{cycleId}/weeks/{weekNumber}/review/submit` | Validate and submit the weekly review |
| `POST` | `/api/v1/cycles/{cycleId}/weeks/{weekNumber}/review/reopen` | Explicitly reopen a submitted review while the cycle is active |

### Progress and summary

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `GET` | `/api/v1/cycles/{cycleId}/progress` | Return completion counts, missing reviews, averages, and trends |
| `GET` | `/api/v1/cycles/{cycleId}/summary` | Return the final or current-to-date cycle summary |

## 8. Minimum request contracts

### Create cycle

```json
{
  "name": "My 10-week reset",
  "startDate": "2026-09-14",
  "timeZoneId": "Atlantic/Reykjavik",
  "lengthInWeeks": 10
}
```

### Create category

```json
{
  "name": "Health",
  "color": "#2F855A",
  "icon": "heart"
}
```

### Create goal

```json
{
  "title": "Exercise three times per week",
  "description": "Strength or cardio sessions of at least 30 minutes"
}
```

### Save weekly-review draft

```json
{
  "wins": "Completed all planned sessions.",
  "challenges": "Sleep was inconsistent.",
  "nextWeekFocus": "Keep a fixed bedtime.",
  "entries": [
    {
      "goalId": "00000000-0000-0000-0000-000000000001",
      "rating": 4,
      "notApplicable": false,
      "note": "Good consistency, with room to improve."
    }
  ]
}
```

Exactly one of `rating` or `notApplicable: true` must be supplied for every entry.

## 9. Implementation plan

### Phase 1 — Contracts and domain foundation

- Define cycle, category, goal, review, and review-entry domain models.
- Define statuses and state transitions.
- Add API request/response contracts and Problem Details conventions.
- Add EF Core mappings and the first SQL Server migration.
- Add integration-test database reset between mutable scenarios.

### Phase 2 — Cycle setup vertical slice

- Implement cycle create, list, detail, update, and draft delete.
- Implement category and goal CRUD plus ordering.
- Implement the start-cycle command.
- Automate test cases `CYC-001`–`CYC-006`, `CAT-001`–`CAT-004`, and `GOL-001`–`GOL-005` from the test plan.

### Phase 3 — Weekly review vertical slice

- Calculate cycle weeks and the current week.
- Implement review draft, delete, submit, and reopen operations.
- Snapshot goal/category labels in review entries.
- Enforce database uniqueness for cycle and week number.
- Automate test cases `REV-001`–`REV-011`.

### Phase 4 — Progress and completion

- Implement category, weekly, and overall score calculations.
- Implement progress and summary endpoints.
- Implement complete-cycle and archive-cycle commands.
- Automate remaining review, final-review, and lifecycle cases.

### Phase 5 — Production readiness

- Generate and verify OpenAPI documentation.
- Add production authentication and ownership isolation.
- Add structured logging without personal review content.
- Add rate limiting, observability, backups, and recovery checks.
- Add the React onboarding, weekly-review, and progress screens.

## 10. Category-module extension boundary

Custom modules are deliberately deferred, but the MVP must preserve a clean extension point:

- Every category continues to support generic goals and weekly-review entries.
- A module is enabled by assigning a recognized `ModuleKey` to a category.
- Module-specific data lives separately from the generic category tables.
- Future module endpoints use `/api/v1/categories/{categoryId}/modules/{moduleKey}/...`.
- Disabling a module hides its custom behavior but does not delete module data.
- Module contract tests `MOD-001`–`MOD-005` must pass for every future module.

## 11. MVP completion criteria

The generic MVP is complete when a user can perform the entire main flow through the frontend, all required endpoints are documented in OpenAPI, all implemented business rules have unit or integration coverage, and the GitHub Actions pipeline passes before deployment.
