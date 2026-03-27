# Flow: configure-access-policy-flow

## Trigger

User wants to set up data access control, restrict schema access, or add role-based access control (RBAC) to their data.

> "set up data access control"
> "restrict schema access"
> "add RBAC to data"
> "only admins should be able to delete products"
> "make the user schema private"
> "configure who can read and write my schemas"

---

## Pre-flight Questions

Before starting, confirm:

1. Which schema should access control be applied to?
2. What security type? `Public` (open), `Private` (authenticated users only), or `RoleBased` (fine-grained by role)?
3. If `RoleBased`:
   - Which roles need access? (provide role slugs — e.g. `admin`, `editor`, `viewer`)
   - For each role: which operations? (`Read`, `Create`, `Update`, `Delete`)
4. Are there existing access policies on this schema that should be reviewed first?

---

## Flow Steps

### Step 1 — Review Existing Policies (optional but recommended)

Before changing anything, check what policies currently exist.

```
Action: get-access-policies
Input:
  schemaName = "<SchemaName>"  (query param)
  projectKey = $VITE_PROJECT_SLUG  (query param)
```

**Branch:**
- If response has existing policies → review with user. Confirm whether to keep, update, or replace.
- If empty → no existing policies → proceed to Step 2.

---

### Step 2 — Set Security Type

```
Action: change-security
Input:
  SchemaName   = "<SchemaName>"
  SecurityType = "Public" | "Private" | "RoleBased"
  ProjectKey   = $VITE_PROJECT_SLUG
```

**Branch:**
- If SecurityType is `Public` → access is open — flow complete (no policies needed)
- If SecurityType is `Private` → authenticated users only — flow complete (no policies needed)
- If SecurityType is `RoleBased` → continue to Step 3

---

### Step 3 — Create Access Policies (only for RoleBased)

For each distinct role group and operation set, create a policy. Common patterns:

**Admin — full access:**
```
Action: create-access-policy
Input:
  SchemaName   = "<SchemaName>"
  PolicyName   = "admin-full-access"
  AllowedRoles = ["admin"]
  Operations   = ["Read", "Create", "Update", "Delete"]
  ProjectKey   = $VITE_PROJECT_SLUG
```

**Editor — read and write, no delete:**
```
Action: create-access-policy
Input:
  SchemaName   = "<SchemaName>"
  PolicyName   = "editor-write"
  AllowedRoles = ["editor"]
  Operations   = ["Read", "Create", "Update"]
  ProjectKey   = $VITE_PROJECT_SLUG
```

**Viewer — read only:**
```
Action: create-access-policy
Input:
  SchemaName   = "<SchemaName>"
  PolicyName   = "viewer-read-only"
  AllowedRoles = ["viewer"]
  Operations   = ["Read"]
  ProjectKey   = $VITE_PROJECT_SLUG
```

Repeat for each distinct role/operation combination the user specified.

---

### Step 4 — Verify Policies

Confirm the policies were created correctly.

```
Action: get-access-policies
Input:
  schemaName = "<SchemaName>"  (query param)
  projectKey = $VITE_PROJECT_SLUG  (query param)
```

Review the response with the user:
- Confirm AllowedRoles and Operations match what was requested
- If any policy is wrong → call `update-access-policy` to correct it
- If a policy should be removed → call `delete-access-policy` with the policy's `itemId`

---

## Error Handling

| Step | Error | Cause | Action |
|------|-------|-------|--------|
| Step 1 | 404 | Schema not found | Verify SchemaName from get-schemas |
| Step 2 | 400 | Invalid SecurityType | Use only: Public, Private, RoleBased |
| Step 2 | 400 | SchemaName not found | Use the exact SchemaName from define-schema |
| Step 3 | 400 | Duplicate PolicyName | Each policy name must be unique per schema — rename it |
| Step 3 | 400 | Invalid operation | Use only: Read, Create, Update, Delete |
| Step 3 | 400 | Empty AllowedRoles | Provide at least one role slug |
| Step 4 | policies missing | create-access-policy call failed silently | Re-run create-access-policy for missing policies |
| Any | 401 | Expired token | Run get-token to refresh |
| Any | 403 | Missing cloudadmin role | Add cloudadmin role in Cloud Portal → People |

---

## Important Notes

### RoleBased without policies = no access

If you set SecurityType to `RoleBased` but create no policies, ALL roles (including `cloudadmin`) will be denied access through the data API. Always create at least one policy after setting `RoleBased`.

### Role slugs must match IDP roles

The `AllowedRoles` values must exactly match the role slugs defined in the Identity & Access skill. Use `get-roles` from the identity-access skill to list available roles if you are unsure of the slug values.

### Security changes take effect immediately

Unlike schema field changes, security and policy changes do NOT require `reload-configuration`. They are applied immediately.

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/data-management/pages/schema-detail/schema-detail-page.tsx` | Access Control tab in schema detail |
| `modules/data-management/hooks/use-data-management.tsx` | `useChangeSecurity`, `useCreateAccessPolicy`, `useUpdateAccessPolicy`, `useDeleteAccessPolicy`, `useGetAccessPolicies` hooks |
| `modules/data-management/services/data-management.service.ts` | `changeSecurity()`, `createAccessPolicy()`, `getAccessPolicies()` methods |
| `modules/data-management/types/data-management.type.ts` | `SecurityType`, `Operation`, `AccessPolicy`, `CreateAccessPolicyPayload` types |
