# Decision Guide (Claude Code)

This is the first file to read when receiving a user request.
It routes the request to the correct skill domain and defines what to clarify before starting.

---

## How to Use This File

Read this file on every user request. Use the tables below to determine internally which skill to load next. **Do not present these tables or options to the user** — route silently and proceed.

---

## Step 1 — Route to Skill Domain

| If the user wants to... | Read |
|-------------------------|------|
| Set up a new project, install dependencies, create entry point / http client | `core/app-scaffold-{FRONTEND_STACK}.md` |
| Set up the app shell, sidebar, layout, header, profile menu, permissions hook | `core/app-layout-{FRONTEND_STACK}.md` |
| Login, register, activate account, reset password, MFA, roles, permissions, users, organizations, sessions, CAPTCHA | `identity-access/SKILL.md` |
| Send email, push notification, in-app notification, messaging, email templates | `communication/SKILL.md` |
| Define data schemas, manage collections, upload/manage files, data sources, access policies, validation rules | `data-management/SKILL.md` |
| Translate content, manage languages, locale keys, import/export translations, auto-translate | `localization/SKILL.md` |
| AI agents, knowledge base, RAG, vector search, LLM queries, streaming chat, AI models, tools | `ai-services/SKILL.md` |
| View service logs, distributed traces, API performance analytics, live log streaming | `lmt/SKILL.md` |
| CI/CD, infrastructure, security scanning | `devsecops` *(not implemented)* |

If the request spans multiple domains, read `core/execution-context.md` for cross-domain orchestration rules and dependency ordering.

> **Stack file resolution:** Throughout this file, `{FRONTEND_STACK}` refers to the value of `FRONTEND_STACK` from `.env` — either `react` or `blazor`. Always resolve this before reading any frontend file. For example, `core/frontend-{FRONTEND_STACK}.md` becomes `core/frontend-blazor.md` when `FRONTEND_STACK=blazor`.

---

## Step 2 — Check for a Flow

Before picking individual actions, check if the request maps to an existing flow in the skill's `flows/` folder.

| If the user wants to build... | Use flow |
|-------------------------------|----------|
| Login page | `identity-access/flows/login-flow.md` |
| Registration / signup page | `identity-access/flows/user-registration.md` |
| Forgot password / reset password | `identity-access/flows/password-recovery.md` |
| MFA setup page | `identity-access/flows/mfa-setup.md` |
| Admin user creation + role assignment | `identity-access/flows/user-onboarding.md` |
| Session management / logout UI | `identity-access/flows/session-management.md` |
| Role and permission configuration | `identity-access/flows/role-permission-setup.md` |
| Send email | `communication/flows/send-email-flow.md` |
| In-app notifications | `communication/flows/notification-flow.md` |
| Email template management | `communication/flows/manage-templates-flow.md` |
| Data schema definition | `data-management/flows/define-schema-flow.md` |
| Query / insert / update / delete records in a schema | `data-management/flows/query-data-flow.md` |
| Modify an existing schema (add/change fields) | `data-management/flows/migrate-schema-flow.md` |
| Database connection setup | `data-management/flows/setup-data-source-flow.md` |
| File upload / document management | `data-management/flows/upload-file-flow.md` |
| Data access policies | `data-management/flows/configure-access-policy-flow.md` |
| Add languages / set default language | `localization/flows/language-setup.md` |
| Manage translation keys | `localization/flows/key-management.md` |
| Import / export translations | `localization/flows/import-export.md` |
| Create AI agent | `ai-services/flows/create-agent-flow.md` |
| Set up knowledge base | `ai-services/flows/setup-knowledge-base.md` |
| Chat with an AI agent | `ai-services/flows/chat-flow.md` |
| Query an LLM directly / send a prompt / stream a response | `ai-services/flows/query-lmt-flow.md` |
| Configure AI models | `ai-services/flows/manage-models.md` |
| View service logs | `lmt/flows/view-logs-flow.md` |
| Analyze traces and performance | `lmt/flows/view-traces-flow.md` |

If no flow matches, fall back to the intent mapping table in `SKILL.md`.

---

## Step 3 — Ask Pre-flight Questions

**Before asking anything, check `PROJECT.md`** — see `core/context.md` for how to read it. Any question already answered there must be skipped.

For questions not covered by `PROJECT.md`, ask the developer:

### Always ask:
1. **What is the feature name / module name?** *(used for folder and file naming)*
2. **Is this a new page or an addition to an existing page?**

### Ask for auth-related features (skip if answered in PROJECT.md):
3. **Which login methods are enabled?** `email/password`, `social login`, `OIDC`, or multiple?
4. **Is MFA required?** If yes, which type — `email OTP`, `authenticator app (TOTP)`, or both?
5. **Is CAPTCHA enabled?** If yes, which provider — `reCaptcha` or `hCaptcha`?
6. **Is this self-registration (user signs up themselves) or admin-created (admin creates users)?**

### Ask for user management features (skip if answered in PROJECT.md):
7. **Which roles exist in this project?** *(needed for set-roles calls)*
8. **Are organizations used?** If yes, should users be assigned to one on creation?

Do not proceed until these questions are answered. Update `PROJECT.md` with any new answers after the feature is built.

---

## Step 4 — Define Output

Every request must produce both:

| Output | What it means |
|--------|---------------|
| **Backend** | API calls (curl via action files), token handling, error responses |
| **Frontend** | Pages, components, services, routing — following `core/frontend-{FRONTEND_STACK}.md` |

If the user only wants backend or only wants frontend, confirm before limiting scope.

### Localization is mandatory for every frontend output

Before generating any component:

1. List every user-visible string in the planned output
2. Call `get-keys-by-names` to check which keys already exist
3. Reuse existing keys — do not create duplicates
4. Call `save-keys` to create any missing keys
5. Use `t('key.name')` for every string in the generated component — no hardcoded text

If this is the first feature in the project, also generate the localization infrastructure for the chosen stack:

**React (`FRONTEND_STACK=react`):**
- `src/hooks/use-translation.tsx` — the app-level translation hook
- `src/state/store/language/index.tsx` — persisted language store
- `src/components/core/language-switcher/language-switcher.tsx` — the switcher component
- Mount `<LanguageSwitcher />` in the app header/layout

**Blazor (`FRONTEND_STACK=blazor`):**
- `Services/LocalizationService.cs` — `ILocalizationService` implementation (from `app-scaffold-blazor.md`)
- `Components/Core/LanguageSwitcher.razor` — MudBlazor language select dropdown
- Mount `<LanguageSwitcher />` in the `AppHeader.razor`

**Blazor + Tailwind (`FRONTEND_STACK=blazor-tailwind`):**
- `Services/LocalizationService.cs` — `ILocalizationService` implementation (from `app-scaffold-blazor-tailwind.md`)
- `Components/Core/LanguageSwitcher.razor` — plain `<select>` with Tailwind styling
- Mount `<LanguageSwitcher />` in the `AppHeader.razor`

See `core/frontend-{FRONTEND_STACK}.md` for the exact implementation of each.

---

## Step 5 — Follow Execution Order

1. Read `core/context.md` — load PROJECT.md if it exists; create/update it after execution
2. Read `core/clarification.md` — when to ask, what to ask, how to ask before and during execution
3. Read `core/security.md` — SAST rules that apply to all generated code
4. Read `core/runtime.md` — execution rules and token flow
5. Read `core/conventions.md` — naming conventions and flow file template format
6. Read the matched flow file in full
7. Read the action files referenced by the flow
8. Read `contracts.md` for request/response schemas
9. **Check the reference implementation** — For React: `https://github.com/SELISEdigitalplatforms/blocks-construct-react`. Verify component names, module structure, and auth patterns against the actual codebase. (No reference implementation for Blazor yet.)
10. **Use the shadcn/ui MCP** (React only) — `https://ui.shadcn.com/docs/mcp` — to confirm correct import paths and props for any shadcn/ui component used in generated code. For Blazor, use MudBlazor documentation.
11. **Localization** — before writing any component: list strings → `get-keys-by-names` → reuse or `save-keys` → use `t()` (React) or `Localizer["key"]` (Blazor) for every string. If first feature: generate localization infrastructure per stack.
12. Execute or generate in the order defined by the flow
13. Never skip steps or reorder them — the flow defines the correct sequence
