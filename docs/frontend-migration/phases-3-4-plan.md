# React Frontend Migration — Phases 3 & 4 Plan

Reference spec: [template-api-generator/specs.md](https://github.com/alequeshow/template-api-generator/blob/main/src/Template.Frontend/Template.Frontend.React/docs/specs.md)

---

## Context

This document maps the workloads and gaps for **Phase 3 (Auth + BFF)** and **Phase 4 (Feature Migration)** of the React frontend migration applied to the `habitica-task-handler` backend.

Key facts about the backend that shape both phases:

- Azure Isolated-process Functions (C#). No traditional REST API server — only an HTTP-triggered webhook and a Timer-triggered function.
- Authentication to Habitica is **API key-based** (`HABITICA_USER_ID` + `HABITICA_USER_TOKEN` env vars). There are no login/logout/refresh backend endpoints.
- All Habitica interactions go through `IHabiticaApiClient` (Refit, calls `https://habitica.com/api/v3`).
- Current deployment is **single-tenant** (one Habitica user per deployment).
- No existing frontend to migrate from — this is a **greenfield UI** over an existing backend.

---

## Phase 3 — Auth + BFF

### Goal (from spec)

> Implement BFF auth endpoints and cookie persistence. Integrate login/logout/refresh flows with backend. Add auth guards and session recovery behavior.

### Adaptation for this backend

The spec assumes a credential-based login flow (`POST /auth/login → session cookie`). This backend has no such endpoint — it uses pre-configured API keys. The BFF must be adapted to:

1. **Validate** the server-side API keys against Habitica on first request.
2. **Issue a session cookie** to the browser after validation rather than after a credential exchange.
3. **Proxy** all Habitica API calls through Next.js route handlers so the API keys never reach the browser.

There is no refresh flow required (Habitica API tokens do not expire by default).

---

### Workloads

#### 3.1 — BFF Infrastructure Setup

| Item | Description |
|---|---|
| Route handler skeleton | Create `app/api/` route handlers in Next.js for auth and proxy paths. |
| Environment config | Map `HABITICA_URL`, `HABITICA_USER_ID`, `HABITICA_USER_TOKEN` as server-only env vars. Validate presence at startup. |
| HTTP client | Configure a typed server-side client to call Habitica API (e.g. `axios` or native `fetch` with a base URL factory). |

#### 3.2 — Session Initiation and Cookie Persistence

| Item | Description |
|---|---|
| `POST /api/auth/session` | BFF calls Habitica `/api/v3/user` to verify the configured credentials. On success, issues an HTTP-only `SameSite=Strict; Secure` session cookie containing an opaque session token (not the raw API key). |
| Server-side session store | Maintain a lightweight in-memory or edge-compatible session map (session token → validated state + timestamp). Consider `iron-session` or similar for encrypted cookie payload. |
| `DELETE /api/auth/session` | Clears the session cookie and invalidates the session token in the store. Acts as logout. |
| Session validation middleware | Next.js middleware that checks the session cookie on every protected route. Redirects to a setup/error page when no valid session exists. |

#### 3.3 — BFF Proxy Routes for Habitica API

| Item | Description |
|---|---|
| `GET /api/habitica/tasks` | Forwards `?type=` to Habitica `GET /api/v3/tasks/user`. Returns the filtered task list. |
| `POST /api/habitica/tasks` | Forwards task creation payload to Habitica `POST /api/v3/tasks/user`. |
| Generic proxy pattern | Consider a catch-all route handler (`/api/habitica/[...path]`) to forward arbitrary Habitica API paths, injecting credentials server-side. Evaluate trade-offs (surface area vs flexibility). |

#### 3.4 — Auth Guards and Session Recovery

| Item | Description |
|---|---|
| Protected layout | `AuthGuard` component (or server layout check) that redirects unauthenticated users. |
| Session expiry | Define a TTL for the session cookie. On expiry, BFF re-validates credentials automatically (since keys are static) or forces a manual re-trigger of session init. |
| Error boundary | Centralized handling for `401` responses from the BFF proxy — triggers session recovery flow. |

#### 3.5 — Security Requirements

| Item | Description |
|---|---|
| CSRF mitigation | Use `SameSite=Strict` cookies + double-submit CSRF token for state-mutating BFF endpoints. |
| Cookie hardening | `HttpOnly`, `Secure`, `SameSite=Strict`. `__Host-` prefix in production. |
| No token leakage | API keys must never appear in server logs, client responses, or browser storage. |
| Rate limiting | Basic rate limiting on `/api/auth/session` to prevent brute-force credential probing. |

---

### Phase 3 Gaps

| Gap | Impact | Suggested Resolution |
|---|---|---|
| **No login/logout backend endpoints** | The spec's BFF pattern assumes a credential exchange. This backend has none. | BFF validates API keys directly against Habitica. The "login" UX is implicit (credentials pre-configured, session starts automatically). Evaluate whether a manual "connect" step is desirable. |
| **Single-tenant architecture** | One set of API keys per deployment; no multi-user model. | Acceptable for personal use. Document the constraint. If multi-user is ever needed, it requires a database and separate credential store — out of scope for these phases. |
| **No token refresh mechanism** | Habitica tokens don't expire, so no refresh loop is needed. | Document the assumption. If Habitica ever introduces token expiry, the BFF session layer already provides an extension point. |
| **Session store choice** | Edge/serverless deployments (Vercel, Cloudflare) have no persistent memory. | For phase 3, use encrypted cookie payload (`iron-session`) to avoid a separate store. Flag as a production hardening item. |

---

## Phase 4 — Feature Migration

### Goal (from spec)

> Migrate high-priority pages first (dashboard/status/auth-related). Use feature-by-feature rollout and parity checks.

### Feature inventory

Since there is no existing frontend, "migration" here means **exposing existing backend capabilities through a UI** and adding visibility that currently requires direct API or log inspection.

---

### Workloads

#### 4.1 — Task Dashboard (highest priority)

| Item | Description |
|---|---|
| Daily tasks list | Call `GET /api/habitica/tasks?type=dailys` and render a list with completion status, due indicator, and snooze-eligible badge. |
| Todo tasks list | Call `GET /api/habitica/tasks?type=todos` and render with due date and tag display. |
| Snoozeable tag filter | Highlight tasks carrying the snooze tag (`HABITICA_SNOOZE_TAG_ID`). |
| Refresh / polling | Periodically refresh task lists (TanStack Query `refetchInterval`) to reflect state changes made in Habitica. |

#### 4.2 — Snooze Task Management

| Item | Description |
|---|---|
| Snooze status panel | Show which dailies are snooze-eligible today (due + not completed + has tag). |
| Manual snooze trigger | `POST /api/habitica/tasks/snooze` — BFF endpoint that replicates the `HandleDailyTasks` logic on demand. Requires a new Azure Function HTTP endpoint or direct BFF execution of the same logic. |
| Existing snoozed todos | List currently active snoozed todos (todos with snooze tag, not completed). |

#### 4.3 — Function Status and Activity Log

| Item | Description |
|---|---|
| Last run display | Show last execution time of `TimedEventFunction`. Requires a new storage-backed log entry on each cron run. |
| Next scheduled run | Compute next cron trigger from `TIMED_FUNCTION_CRON` value and display it. |
| Execution history | Table of recent cron executions (timestamp, tasks processed, todos created). Requires a new persistence layer (Azure Table Storage or CosmosDB). |

#### 4.4 — Configuration Page

| Item | Description |
|---|---|
| Credential status | Show whether configured Habitica credentials are valid (call session validation). Never expose raw keys. |
| Cron schedule viewer | Parse and display `TIMED_FUNCTION_CRON` in human-readable form. |
| Timezone toggle | UI representation of `DUE_TASK_COMPARE_YESTERDAY` setting. |
| Tag ID display | Show the snooze tag ID currently configured (read-only, sourced from BFF). |

---

### Phase 4 Gaps

| Gap | Impact | Suggested Resolution |
|---|---|---|
| **Backend exposes no UI-friendly endpoints** | Azure Functions only expose a webhook trigger and a timer trigger. The frontend has no way to call `HandleDailyTasks` or read logs. | Add new HTTP-triggered Azure Functions for: `GET /tasks/dailys`, `GET /tasks/todos`, `GET /tasks/snooze-candidates`, `POST /tasks/snooze`, `GET /function/status`. These must reuse existing service logic without duplicating it. |
| **No execution history persistence** | `TimedEventFunction` logs to Application Insights but has no queryable store. | Instrument `TimedEventFunction` to write a compact execution record to Azure Table Storage on each run. Define the record schema now so the UI can consume it in phase 4. |
| **Manual snooze trigger scope** | Running snooze logic from the UI duplicates the timed trigger path. | Extract snooze logic from `TimedEventFunction` into a shared `ISnoozeService` and expose it through both the timer function and a new HTTP function. This refactor belongs in phase 4.1 before building the UI for it. |
| **No real-time task state** | Task completion state in Habitica changes outside the app. | Polling via TanStack Query is sufficient for phase 4. Server-Sent Events or WebSocket integration is deferred to phase 5. |
| **No deployment plan defined** | Frontend (Next.js) and backend (Azure Functions) are separate runtimes. | Define hosting topology: Azure Static Web Apps + API proxying, or separate deployments with CORS. CORS policy for the Azure Function must be configured to allow the Next.js origin. |
| **No feature flags / parity checks** | Spec calls for "parity checks" but there's no existing UI baseline to compare against. | Define parity as "matches Habitica API state" rather than "matches existing UI." Add integration tests in phase 4 that verify BFF responses against direct Habitica API responses. |

---

## Cross-cutting Concerns

| Concern | Phase | Notes |
|---|---|---|
| TypeScript types for Habitica API | 3 | Generate or hand-write types from `IHabiticaApiClient` contracts. Consider `openapi-typescript` if a spec is available. |
| Error handling convention | 3 | Define a consistent BFF error response shape and React error boundary strategy before phase 4 pages are built. |
| TanStack Query setup | 3/4 | Configure query client, default stale times, and global error handler in phase 3 so phase 4 features can adopt it directly. |
| Vitest + RTL tests per workload | 3 & 4 | Each BFF handler and UI feature should ship with unit/component tests as part of the same workload — not deferred to phase 5. |
| E2E tests | 4 | Playwright tests for login flow (phase 3) and task dashboard (phase 4.1) as the first E2E coverage milestone. |

---

## Suggested Execution Order

```
Phase 3
  └─ 3.1 BFF infrastructure + environment config
  └─ 3.2 Session initiation + cookie persistence
  └─ 3.3 Habitica proxy routes (GET tasks only first)
  └─ 3.4 Auth guards + session recovery
  └─ 3.5 Security hardening

  ↓ [Phase 3 gate: protected page loads and BFF returns task data]

Phase 4
  └─ New Azure Function HTTP endpoints (backend work, unblocks UI)
  └─ 4.1 Task dashboard (dailys + todos)
  └─ 4.2 Snooze status panel + manual trigger
  └─ 4.3 Function status page (requires persistence instrumentation)
  └─ 4.4 Configuration page
```

---

## Open Questions for Iteration

1. **Single-session vs multi-session**: Should the app support multiple simultaneous browser sessions for the same deployment, or is a single-session-at-a-time model acceptable?
2. **Credential input UI**: Should the frontend ever allow entering API keys through a setup wizard, or are credentials always pre-configured at deployment time?
3. **Manual snooze scope**: Is a manual "run snooze now" button in scope for phase 4, or is it phase 5 material?
4. **Execution history TTL**: How long should execution records be retained in Azure Table Storage?
5. **Hosting topology**: Next.js on Azure Static Web Apps + Azure Functions backend, or a different deployment model?
