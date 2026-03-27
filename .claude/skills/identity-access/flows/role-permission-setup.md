# Flow: role-permission-setup

## Trigger

Admin wants to define roles, create permissions, and assign them.

> "build role management"
> "create permissions for my app"
> "set up RBAC"
> "assign permissions to roles"
> "build access control"

---

## Pre-flight Questions

Before starting, confirm:

1. Are roles already defined, or do they need to be created?
2. Are permissions already defined, or do they need to be created?
3. What resource groups exist? *(required for creating permissions)*
4. Is this a UI for admins to manage roles/permissions, or a one-time setup via API?

---

## Concepts

| Concept | Definition |
|---------|-----------|
| **Permission** | A named capability tied to a resource (e.g. `user:read`, `report:export`) |
| **Resource Group** | A category permissions belong to (e.g. `users`, `reports`) |
| **Role** | A named collection of permissions (e.g. `admin`, `viewer`) |
| **Role assignment** | Linking a role to a user via `set-roles` |

Always create permissions before roles if starting from scratch.
Always assign roles to users after roles are created.

---

## Flow Steps

### Step 1 — Get Existing Resource Groups

Before creating permissions, fetch available resource groups.

```
Action: get-resource-groups
Input: ACCESS_TOKEN only
```

Use the response to populate the `resourceGroup` field when creating permissions.

---

### Step 2 — Create Permissions

Create one permission per discrete capability.

```
Action: create-permission
Input:
  name          = human-readable name (e.g. "Read Users")
  resource      = resource name (e.g. "users")
  resourceGroup = resource group from Step 1
  description   = optional
  projectKey    = VITE_X_BLOCKS_KEY
```

Repeat for each permission needed.

```
On 400 (duplicate) → permission name already exists, skip or update
```

---

### Step 3 — Create Role

```
Action: create-role
Input:
  name        = human-readable name (e.g. "Admin")
  slug        = kebab-case unique identifier (e.g. "admin")
  description = optional
  projectKey  = VITE_X_BLOCKS_KEY
```

```
On 400 (duplicate slug) → slug already taken, choose a different slug
```

---

### Step 4 — Assign Permissions to Role

Link the permissions created in Step 2 to the role created in Step 3.

```
Action: save-roles-and-permissions
Input:
  roleId      = role ID from Step 3
  permissions = array of permission names created in Step 2
  projectKey  = VITE_X_BLOCKS_KEY
```

---

### Step 5 — Assign Role to User

```
Action: set-roles
Input:
  userId     = target user's ID
  roles      = array of role slugs (e.g. ["admin"])
  projectKey = VITE_X_BLOCKS_KEY
```

> `set-roles` **replaces** all existing role assignments.
> To add a role without removing others: call `get-user-roles` first, merge the new slug into the array, then call `set-roles` with the merged array.

---

## Viewing and Updating

### List all roles
```
Action: get-roles
Input: page, pageSize, sort, filter, projectKey
```

### Get single role (with its permissions)
```
Action: get-role
Input: roleId (query param)
```

### Update role name/description
```
Action: update-role
Input: roleId + name + description + projectKey
```

### List all permissions
```
Action: get-permissions
Input: page, pageSize, sort, filter, projectKey
```

### Get user's current roles
```
Action: get-user-roles
Input: userId (query param)
```

### Get user's effective permissions
```
Action: get-user-permissions
Input: userId (query param)
```

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| `create-permission` 400 | Duplicate name | Skip if exists, or update via update-permission |
| `create-role` 400 | Duplicate slug | Choose a unique slug |
| `set-roles` 400 | Invalid role slug | Verify slug exists via get-roles |
| `save-roles-and-permissions` 401 | Token expired | Run refresh-token then retry |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/roles/pages/roles-page.tsx` | Role list with create/edit/delete |
| `modules/roles/components/role-form/role-form.tsx` | Create/edit role form |
| `modules/roles/components/role-permissions/role-permissions.tsx` | Permission assignment UI per role |
| `modules/permissions/pages/permissions-page.tsx` | Permission list |
| `modules/permissions/components/permission-form/permission-form.tsx` | Create/edit permission form |
| `modules/users/components/user-roles/user-roles.tsx` | Role assignment component in user detail |
| `modules/roles/hooks/use-roles.tsx` | `useCreateRole`, `useGetRoles`, `useGetRole`, `useUpdateRole`, `useSetRoles` hooks |
| `modules/permissions/hooks/use-permissions.tsx` | `useCreatePermission`, `useGetPermissions`, `useSaveRolesAndPermissions` hooks |
