# Blocks AI Skills — Claude Instructions

## On Session Start

At the beginning of every session, follow these steps in order:

---

### Step 1 — Check Cloud Portal Prerequisites

Before anything else, inform the user:

> **Important:** The following must be completed manually in the SELISE Blocks Cloud Portal (https://cloud.seliseblocks.com) before this system can work. Claude cannot do these for you.
>
> - [ ] **Project Created** — Cloud Portal → Projects → Create Project
> - [ ] **Environment Created** — Cloud Portal → Projects → [Project] → Environments
> - [ ] **User Added with `cloudadmin` role** — Cloud Portal → Projects → [Project] → People
> - [ ] **Repository Attached** — Cloud Portal → Projects → [Project] → Repositories
>
> If any of these are not done, API calls will fail. See `skills/core/prerequisites.md` for detailed guidance and error messages.

Ask the user: **"Have all four cloud portal steps been completed?"**

- If **No** → stop and direct them to `skills/core/prerequisites.md`. Do not proceed.
- If **Yes** → continue to Step 2.

---

### Step 1b — Choose Frontend Stack

Ask the user: **"Which frontend stack would you like to use — React, Blazor (MudBlazor), or Blazor (Tailwind CSS)?"**

| Choice | Variable value | Stack |
|--------|---------------|-------|
| React | `FRONTEND_STACK=react` | React 19 + TypeScript + Vite + Tailwind + shadcn/ui |
| Blazor (MudBlazor) | `FRONTEND_STACK=blazor` | .NET 10 Blazor WASM + MudBlazor |
| Blazor (Tailwind) | `FRONTEND_STACK=blazor-tailwind` | .NET 10 Blazor WASM + Tailwind CSS (no component library) |

Store this choice in `.env` as `FRONTEND_STACK`. This determines which frontend skill files are loaded for the rest of the session.

---

### Step 2 — Check Environment Variables

Only **4 values** are required from the user. Everything else is auto-set or discovered.

> All four must come from the same SELISE Blocks project:
> `VITE_X_BLOCKS_KEY` and `VITE_BLOCKS_OIDC_CLIENT_ID` from Cloud Portal → Project settings.
> `USERNAME` and `PASSWORD` from a user added to that project with the `cloudadmin` role.

Ask for only these if missing or empty:

| Variable | Where to find it |
|----------|-----------------|
| `VITE_X_BLOCKS_KEY` | Cloud Portal → Project settings |
| `VITE_BLOCKS_OIDC_CLIENT_ID` | Cloud Portal → Project → Auth settings |
| `USERNAME` | Cloud Portal → People (must have `cloudadmin` role) |
| `PASSWORD` | Same account as USERNAME |

**Auto-set without asking:**

| Variable | Value | Reason |
|----------|-------|--------|
| `VITE_API_BASE_URL` | `https://api.seliseblocks.com` | Always the same |
| `VITE_BLOCKS_OIDC_REDIRECT_URI` | `http://localhost:5173/auth/callback` | Standard local dev default |
| `VITE_PRIMARY_COLOR` | `#15969B` | Default theme — user can change later |
| `VITE_SECONDARY_COLOR` | `#5194B8` | Default theme — user can change later |
| `GENERATE_SOURCEMAP` | `false` | Always false for production builds |

**Discovered automatically after authentication (Step 3):**

`VITE_PROJECT_SLUG` is auto-discovered after `get-token` succeeds — see `skills/identity-access/actions/discover-project-slug.md`. The user should never need to provide this manually.

**Conditional — ask only if needed:**

| Variable | When to ask |
|----------|------------|
| `VITE_CAPTCHA_SITE_KEY` | Only if captcha is enabled (confirmed by user or detected during login) |
| `VITE_CAPTCHA_TYPE` | Same — `reCaptcha` or `hCaptcha` |

Write the `.env` with all known values immediately. Include `FRONTEND_STACK` from Step 1b. Leave captcha blank until confirmed:

```
# Frontend stack selection — "react" or "blazor"
FRONTEND_STACK=<value from Step 1b>

# Vite environment variables
VITE_API_BASE_URL=https://api.seliseblocks.com
VITE_X_BLOCKS_KEY=<value>
VITE_PROJECT_SLUG=<auto-discovered in Step 3b>

VITE_CAPTCHA_SITE_KEY=
VITE_CAPTCHA_TYPE=

VITE_BLOCKS_OIDC_CLIENT_ID=<value>
VITE_BLOCKS_OIDC_REDIRECT_URI=http://localhost:5173/auth/callback

# Build configuration
GENERATE_SOURCEMAP=false

# Theme Colors - Can be in hex or hsl format (e.g., #1B9A8B or hsl(174, 69%, 41%))
VITE_PRIMARY_COLOR=#15969B
VITE_SECONDARY_COLOR=#5194B8

# CLI/Claude credentials — for direct API operations only
# Frontend gets these from the login form, NOT from here
USERNAME=<value>
PASSWORD=<value>

# Populated at runtime after authentication
ACCESS_TOKEN=
REFRESH_TOKEN=
```

---

### Step 3 — Authenticate and Discover Project Slug

**3a.** Run the get-token action (`skills/identity-access/actions/get-token.md`) to obtain `ACCESS_TOKEN` and `REFRESH_TOKEN`.

**If get-token fails, diagnose using this table:**

| HTTP Status | Meaning | Action |
|-------------|---------|--------|
| `200` ✅ | Success | Store tokens and proceed to 3b |
| `400` | Malformed request or wrong `client_id` | Check `VITE_BLOCKS_OIDC_CLIENT_ID` and `VITE_X_BLOCKS_KEY` |
| `401` | Wrong `USERNAME` or `PASSWORD` | Re-enter credentials — check the account in Cloud Portal → People |
| `403` | Account missing `cloudadmin` role | Ask admin to assign `cloudadmin` role in Cloud Portal → People |
| `404` | Environment not created or project not active | Verify the project and environment exist in the Cloud Portal |

Do **not** proceed with any task until get-token returns `200`.

**3b.** Immediately after get-token succeeds, run the discover-project-slug action (`skills/identity-access/actions/discover-project-slug.md`) to auto-detect and write `VITE_PROJECT_SLUG` to `.env`. This must happen before any other API calls.

---

### Step 4 — Load Project Context

Check if `PROJECT.md` exists in the project root.

- If it **exists** → read it and summarise loaded context: _"I can see this project uses [login methods], has roles: [roles], and [N] schemas defined."_ Skip any pre-flight questions already answered in the file.
- If it **does not exist** → note that it will be created after the first feature is built.

---

### Step 5 — Confirm Ready

Once authenticated and context loaded, confirm to the user:

> **Session ready.** Environment configured and authenticated successfully.
> You can now ask me to build features, call APIs, or generate frontend code.

---

## Skill File Paths

Skill files may be located in either `skills/` (if this repo is the project root) or `.claude/skills/` (if installed into another project via `install.sh`).

**Resolution order:** When this document references `skills/...`, check `.claude/skills/` first, then fall back to `skills/`. Inside SKILL.md files, all paths use `../core/` to reach core files — this works regardless of whether skills are in `skills/` or `.claude/skills/`.

---

## How to Use These Skills

**Skills are internal routing knowledge — never present them as a menu or list of options.**

When the user describes what they want to build, figure out what to do by reading the relevant skill files silently. Never enumerate available features, domains, or actions to the user. Never say "here's what I can build" — just build it.

---

### On Every User Request

When the user asks for something, follow this lookup chain — reading files only as needed:

**1. Read `skills/core/decision.md`**
Determines which domain the request belongs to and whether a flow exists for it. Read this file first on every new request.

**2. Read the matched flow or action**
Read only the specific flow file (e.g. `skills/identity-access/flows/login-flow.md`) or action file needed. Do not read the entire domain — read only what the request requires.

**3. Read supporting files only if the matched skill references them**
- `skills/core/context.md` — only when reading or updating PROJECT.md
- `skills/core/clarification.md` — only when unsure whether to ask or proceed
- `skills/core/runtime.md` — only when executing API calls
- `skills/core/conventions.md` — only when generating files or naming things
- `skills/core/frontend-{FRONTEND_STACK}.md` — only when generating frontend code (resolve `{FRONTEND_STACK}` from `.env`)
- `skills/core/security.md` — only when generating any code (quick checklist)
- `skills/core/app-scaffold-{FRONTEND_STACK}.md` — only when setting up a new project from scratch
- `skills/core/app-layout-{FRONTEND_STACK}.md` — only when building the app shell or layout
- `skills/{domain}/contracts.md` — only when constructing request/response types
- `skills/{domain}/frontend-{FRONTEND_STACK}.md` — only when generating domain-specific frontend code

> **Stack file resolution:** Replace `{FRONTEND_STACK}` with the value of `FRONTEND_STACK` from `.env` (either `react` or `blazor`). For example, if `FRONTEND_STACK=blazor`, read `skills/core/frontend-blazor.md` instead of `skills/core/frontend-react.md`.

**4. Read `skills/{domain}/SKILL.md` only if no flow matched**
The intent map in SKILL.md maps requests to individual actions. Use it as a fallback when no flow covers the request.

---

### What NOT to Do

- **Never read all skill files at session start** — load on demand only
- **Never list available skills, domains, or features to the user** — they are your internal knowledge
- **Never say "I have the following skills" or "here's what I can do"** — just respond to the request
- **Never ask "which skill do you want to use?"** — figure it out from the request and route internally