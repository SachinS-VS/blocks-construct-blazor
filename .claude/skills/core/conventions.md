# Conventions

## Naming

### Skill files (markdown)

* Use kebab-case for folders and files
* Use descriptive names (no abbreviations unless standard)

Examples: `access-control`, `vector-database`, `create-record`

### Frontend files — stack-specific

| Convention | React | Blazor (MudBlazor) | Blazor (Tailwind) |
|-----------|-------|--------|--------|
| Folders | kebab-case (`signin-email/`) | PascalCase (`SigninEmail/`) | PascalCase (`SigninEmail/`) |
| Component files | kebab-case (`signin-email.tsx`) | PascalCase (`SigninEmail.razor`) | PascalCase (`SigninEmail.razor`) |
| Service files | kebab-case (`auth.service.ts`) | PascalCase (`AuthService.cs`) | PascalCase (`AuthService.cs`) |
| Type/model files | kebab-case (`auth.type.ts`) | PascalCase (`AuthModels.cs`) | PascalCase (`AuthModels.cs`) |
| CSS/style files | kebab-case (`auth.module.css`) | PascalCase (`SigninEmail.razor.css`) | Tailwind utilities (no CSS files) |
| Hook files | kebab-case (`use-auth.tsx`) | N/A — use service methods directly | N/A — use service methods directly |
| Namespaces | N/A | `ProjectName.Modules.Auth.Pages` | `ProjectName.Modules.Auth.Pages` |

---

## Folder Naming

* Use nouns for features (authentication, storage)
* Use plural for collections (notifications)
* Use singular for concepts (email, storage)

---

## Action Naming

* Format: verb-resource

Examples:

* create-record
* get-records
* update-user
* delete-item

---

## API Naming Conventions by Domain

Each SELISE Blocks service uses a different naming convention. **Always match the exact convention for the domain you're calling.**

| Domain | Service | Request/Response Fields | Example |
|--------|---------|------------------------|---------|
| identity-access | IDP | camelCase | `userName`, `isActive`, `roleId` |
| data-management | UDS | **PascalCase** | `ProjectKey`, `SchemaId`, `CollectionName` |
| ai-services | blocksai-api | **snake_case** | `project_key`, `agent_id`, `kb_ids` |
| communication | communication | camelCase | `templateId`, `mailTo`, `subject` |
| localization | uilm | camelCase | `languageCode`, `keyName`, `moduleName` |
| lmt | lmt | camelCase | `serviceName`, `dateFrom`, `dateTo` |

> **Critical:** Never mix conventions. Sending `projectKey` (camelCase) to UDS will fail — it expects `ProjectKey` (PascalCase). Sending `ProjectKey` to blocksai-api will fail — it expects `project_key` (snake_case). When in doubt, read the domain's `contracts.md`.

---

## API Conventions

* Use REST-style endpoints
* Use JSON for request/response (except token endpoint which uses form-encoded)
* Use consistent headers

---

## Headers

### For skill repo operations (Claude executing API calls):

```
Authorization: Bearer $ACCESS_TOKEN
x-blocks-key: $VITE_X_BLOCKS_KEY
Content-Type: application/json
```

### For generated app code — React:

```
Authorization: Bearer ${accessToken}      ← from Zustand store
x-blocks-key: ${import.meta.env.VITE_X_BLOCKS_KEY}
Content-Type: application/json
```

### For generated app code — Blazor:

```
Authorization: Bearer ${accessToken}      ← auto-attached by TokenDelegatingHandler
x-blocks-key: ${blocksKey}                ← auto-attached by TokenDelegatingHandler
Content-Type: application/json
```

> In Blazor, headers are managed automatically by `TokenDelegatingHandler` — no manual header setup needed in service classes.

---

## Environment Variables

| Context | Source | How to access |
|---------|--------|---------------|
| Claude operations (curl) | `.env` in project root | `$VITE_X_BLOCKS_KEY` |
| Generated Vite app code | `.env` in project root | `import.meta.env.VITE_X_BLOCKS_KEY` |

Never mix contexts. Never hardcode values from one context into the other.

---

## File Structure

### Skill files (markdown)

Each feature must follow:

```
feature/
├── SKILL.md
├── contracts.md
├── frontend-react.md      (optional — React frontend guide)
├── frontend-blazor.md     (optional — Blazor frontend guide)
├── actions/
│   └── verb-resource.md
└── flows/
    └── flow-name.md
```

### Generated app structure — React

```
src/
├── modules/{feature}/
│   ├── components/
│   ├── pages/
│   ├── hooks/
│   ├── services/
│   └── types/
├── components/
│   ├── ui-kit/
│   └── core/
├── state/store/
├── hooks/
└── lib/
```

### Generated app structure — Blazor

```
ProjectName/
├── Modules/{Feature}/
│   ├── Components/
│   ├── Pages/
│   ├── Services/
│   └── Models/
├── Components/
│   ├── Shared/
│   └── Core/
├── Layout/
├── Services/
├── State/
├── Models/
└── Extensions/
```

---

## Flow File Template

Every flow file must contain these sections in order:

```markdown
# Flow: flow-name

## Trigger
What user request activates this flow. Include example phrases.

## Pre-flight Questions
What Claude must ask the developer before starting.

## Flow Steps
Ordered steps. Each step includes:
- Which action to call
- Exact input fields
- Output / what to do next
- Branch conditions (if any)

## Error Handling
Table of errors per step: error | cause | action

## Frontend Output
Table of files to generate: file path | purpose
```

Rules:
- One flow = one complete user-facing scenario
- Steps must be in execution order — never reorder
- Every branch must be documented (success and failure)
- Frontend output must follow `core/frontend-{FRONTEND_STACK}.md` conventions

---

## Consistency Rule

If a pattern is used once, it must be used everywhere.
