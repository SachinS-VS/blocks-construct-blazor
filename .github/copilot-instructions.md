# Claude Instructions — Blocks Construct Blazor

## Project Overview

This is a SELISE Blocks Blazor WASM application with the following architecture:

- **Client** (.NET 10 Blazor WASM): SPA frontend components and pages
- **Server** (.NET 10 Blazor Server): Backend hosting and API routes
- **Services** (.NET 10 class library): Shared business logic and service layer
- **Worker** (.NET 10 worker service): Background job processor
- **Test** (.NET 10 xUnit): Unit test projects

## Technology Stack

- **Frontend**: Blazor WASM (.NET 10) with MudBlazor or Tailwind CSS
- **Backend**: ASP.NET Core 10, GraphQL API
- **Authentication**: OIDC (SELISE Blocks identity)
- **Data**: GraphQL queries/mutations, S3 file uploads
- **Deployment**: Docker (worker service)


## Using CLI/Claude for Development

Claude can help with:

- Building new features (CRUD operations, forms, reports)
- Generating components and pages
- Setting up GraphQL queries and mutations
- API integration and error handling
- Unit tests and component tests
- Debugging authentication and token issues

**Before starting**: Ensure `.env` is configured with:

```
VITE_API_BASE_URL=https://api.seliseblocks.com
VITE_X_BLOCKS_KEY=<your-key>
VITE_BLOCKS_OIDC_CLIENT_ID=<your-client-id>
VITE_PROJECT_SLUG=<auto-discovered>
USERNAME=<for-cli-only>
PASSWORD=<for-cli-only>
```

## Common Tasks

| Task | Related Skill / Workflow |
|------|----------|
| Create new data schema | `data-management` skill |
| Add login / MFA / SSO | `identity-access` skill |
| Send notifications / emails | `communication` skill |
| Query data via GraphQL | `data-management` skill |
| Set up AI agents / LLMs | `ai-services` skill |
| Configure translations | `localization` skill |
| View logs / traces | `lmt` skill |

See `.claude/skills/` for detailed workflows.

## Additional Resources

- CLAUDE.md — On-session setup and prerequisites
- PROJECT.md — Auto-generated project context (login methods, roles, schemas)
- `.claude/skills/` — Domain-specific workflows and actions
