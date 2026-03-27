# Execution Context

This file defines the supporting files that must be loaded when executing any action or flow from any skill domain. Every domain SKILL.md references this file instead of duplicating the list.

---

## Required Files — Load In Order

When a skill is activated and you're about to execute actions or generate code, load these supporting files. All paths are relative to the skill's own directory — use `../core/` to reach core files.

1. **Before executing API calls:** Read `../core/runtime.md` — token handling, header construction, error retry logic
2. **Before generating any code:** Read `../core/security.md` — SAST checklist, never hardcode credentials
3. **Before generating frontend code:** Read `../core/frontend-{FRONTEND_STACK}.md` — stack-specific component patterns and conventions
4. **Before generating files:** Read `../core/conventions.md` — naming rules, file structure, module layout
5. **When constructing request/response types:** Read `contracts.md` (in this skill's own directory) — request/response schemas for this domain
6. **When generating domain-specific UI:** Read `frontend-{FRONTEND_STACK}.md` (in this skill's own directory) — component patterns for this domain
7. **When unsure whether to ask or proceed:** Read `../core/clarification.md` — decision rules for pre-flight questions
8. **When reading/updating PROJECT.md:** Read `../core/context.md` — project context format

---

## API Response Formats by Domain

Each SELISE Blocks service uses a different response wrapper and naming convention. Match the exact format for the domain you're working with.

| Domain | Service | Naming Convention | Response Wrapper |
|--------|---------|-------------------|-----------------|
| identity-access | IDP | camelCase | `{ isSuccess, errors }` or raw token response `{ access_token, token_type, expires_in, refresh_token }` |
| data-management | UDS | **PascalCase** | `{ IsSuccess, Message, HttpStatusCode, Data, Errors }` |
| ai-services | blocksai-api | **snake_case** | `{ is_success, detail, item_id, error }` |
| communication | communication | camelCase | `{ isSuccess, errors }` |
| localization | uilm | camelCase | `{ isSuccess, data, errors }` |
| lmt | lmt | camelCase | `{ isSuccess, data }` |

**Critical:** Never mix naming conventions across domains. If you're calling UDS, use `ProjectKey` (PascalCase). If you're calling blocksai-api, use `project_key` (snake_case). Read the domain's `contracts.md` for exact field names.

### Special Cases

- **get-token and refresh-token** use `Content-Type: application/x-www-form-urlencoded` (NOT JSON). All other endpoints use `application/json`.
- **GraphQL endpoint** includes the project slug in the URL path: `POST $VITE_API_BASE_URL/uds/v1/$VITE_PROJECT_SLUG/graphql`

---

## Cross-Domain Requests

If a user request spans multiple domains (e.g., "create a user and send them a welcome email"):

1. **Identify all domains involved** — list them in dependency order
2. **Always start with `identity-access`** — authentication must come first if any auth action is needed
3. **Execute one domain at a time** — complete all actions/flows for domain A before moving to domain B
4. **Share context between domains** — tokens, user IDs, and other outputs from domain A become inputs to domain B
5. **Common cross-domain patterns:**

| Request pattern | Domain order |
|----------------|-------------|
| Create user + send welcome email | `identity-access` → `communication` |
| Define schema + set access policies | `data-management` (single domain, use `configure-access-policy-flow`) |
| Create AI agent + upload KB files | `ai-services` (single domain, use `create-agent-flow`) |
| Build feature + add translations | Any domain → `localization` (always last) |
| Build any page | Any domain → `localization` (localization is mandatory for all frontend output) |

6. **If unsure about order** — read `../core/decision.md` for the full routing table
