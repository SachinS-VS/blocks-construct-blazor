# Flow: manage-templates-flow

## Trigger

User wants to create, view, edit, clone, or delete email templates.

> "create an email template"
> "manage templates"
> "edit an email template"
> "I need a welcome email template"
> "clone an existing template"
> "delete a template"
> "view all templates"

---

## Pre-flight Questions

Before starting, confirm:

1. Are you creating a new template or modifying an existing one?
2. What is the `purpose` slug for this template (e.g. `"welcome"`, `"password-reset"`, `"invoice"`)? This must match the `purpose` used in `send-email-with-template`.
3. What language is this template for? (default: `"en"`)
4. Does the body contain dynamic placeholders? If yes, what are the variable names (e.g. `{{firstName}}`, `{{activationLink}}`)?
5. Is there an existing template to clone as a starting point?

---

## Flow Steps

### Step 1 — List Existing Templates

Always fetch the current template list first to check what already exists.

```
Action: get-templates
Input:
  search     = (optional, to narrow results)
  projectKey = $VITE_PROJECT_SLUG
Output:
  templates[]  → list of existing templates with name, purpose, language, itemId
  totalCount   → total number of templates
```

- Template with the desired `purpose` already exists → offer to edit (Step 3) or clone (Step 4)
- No match found → proceed to Step 2 to create

---

### Step 2 — Create a New Template

```
Action: save-template
Input:
  name            = "Welcome Email"
  templateSubject = "Welcome to {{appName}}!"
  templateBody    = "<h1>Hello {{firstName}},</h1><p>Welcome to {{appName}}. Click <a href=\"{{activationLink}}\">here</a> to activate your account.</p>"
  language        = "en"
  purpose         = "welcome"
  projectKey      = $VITE_PROJECT_SLUG
  (omit itemId to create)
```

On `isSuccess: true` → template created. Run `get-templates` to confirm and retrieve the new `itemId`.
On `isSuccess: false` → inspect `errors` and correct the request.

---

### Step 3 — Edit an Existing Template

First, load the full template to populate the editor:

```
Action: get-template
Input:
  itemId     = "template-id-123"
  projectKey = $VITE_PROJECT_SLUG
Output:
  template.templateSubject
  template.templateBody
  template.purpose
  template.language
```

Then save the updated version:

```
Action: save-template
Input:
  itemId          = "template-id-123"    (include to update)
  name            = "Welcome Email v2"
  templateSubject = "Welcome to {{appName}}!"
  templateBody    = "<h1>Hi {{firstName}},</h1><p>Updated message here.</p>"
  language        = "en"
  purpose         = "welcome"
  projectKey      = $VITE_PROJECT_SLUG
```

On `isSuccess: true` → changes saved. Invalidate template cache.
On `isSuccess: false` → inspect `errors`, especially for duplicate `purpose`/`language` conflicts.

---

### Step 4 — Clone a Template

Use cloning when you need a template that is similar to an existing one but for a different purpose or language.

```
Action: clone-template
Input:
  itemId     = "template-id-123"
  newName    = "Welcome Email (French)"
  projectKey = $VITE_PROJECT_SLUG
```

On `isSuccess: true` → clone created. Run `get-templates` to find the new `itemId`, then edit it (Step 3) to update `purpose`, `language`, and body.

---

### Step 5 — Preview Template (Frontend)

In the template editor, render a live HTML preview of `templateBody` in a sandboxed `<iframe>`:

```tsx
<iframe
  sandbox="allow-same-origin"
  srcDoc={templateBody}
  className="w-full h-full border rounded-md"
  title="Template preview"
/>
```

Placeholders like `{{firstName}}` will render as literal text in the preview — this is expected. The actual substitution happens server-side at send time.

---

### Step 6 — Delete a Template

Show a confirmation dialog before proceeding.

```
Action: delete-template
Input:
  itemId     = "template-id-123"
  projectKey = $VITE_PROJECT_SLUG
```

On `isSuccess: true` → template deleted. Invalidate template list cache and navigate back to the templates list.
On `isSuccess: false` → template not found, or already deleted.

---

## Template Variable Guide

| Syntax | Example | How it resolves |
|--------|---------|-----------------|
| `{{variableName}}` | `{{firstName}}` | Top-level key in `bodyDataContext` when sending |
| `{{.FieldName}}` | `{{.ActivationLink}}` | Object field access in `bodyDataContext` |

Template body is full HTML. Best practices:
- Use inline CSS for email client compatibility
- Test across email clients (Gmail, Outlook) before deploying
- Keep image assets hosted externally — do not embed base64 images in the body

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| `isSuccess: false` with `purpose` error | Duplicate `purpose`/`language` combination already exists | Change `purpose` or update the existing template instead of creating |
| `isSuccess: false` with `itemId` error | Template not found | Verify `itemId` from `get-templates` |
| `isSuccess: false` with `newName` error on clone | Name already taken | Choose a unique `newName` |
| `isSuccess: false` with `templateBody` error | Body is empty | Supply a valid HTML body |
| `401` | `ACCESS_TOKEN` expired | Re-run `get-token` |
| `403` | Missing `cloudadmin` role | Verify role in Cloud Portal → People |
| `404` | Wrong `VITE_API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/communication/pages/templates/templates-page.tsx` | List all templates with search, sort, and row actions (Edit, Clone, Delete) |
| `modules/communication/pages/templates/template-editor-page.tsx` | Two-panel create/edit form with live HTML preview in sandboxed iframe |
| `modules/communication/pages/templates/template-clone-dialog.tsx` | Dialog to input new name when cloning a template |
| `modules/communication/hooks/use-communication.tsx` | `useGetTemplates`, `useGetTemplate`, `useSaveTemplate`, `useCloneTemplate`, `useDeleteTemplate` hooks |
| `modules/communication/services/communication.service.ts` | `getTemplates()`, `getTemplate()`, `saveTemplate()`, `cloneTemplate()`, `deleteTemplate()` |
| `modules/communication/types/communication.type.ts` | `EmailTemplate`, `SaveTemplatePayload`, `CloneTemplatePayload` interfaces |
