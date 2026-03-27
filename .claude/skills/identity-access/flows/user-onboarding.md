# Flow: user-onboarding

## Trigger

Admin wants to create a user and configure their access — roles, permissions, organization.

> "create a user and assign roles"
> "onboard a new team member"
> "add a user with specific permissions"
> "build user management / admin panel"

---

## Pre-flight Questions

Before starting, confirm:

1. Is this admin-created (admin does it) or self-registration (user does it)? *(if self-registration, use `user-registration` flow)*
2. Which roles should be assigned? *(get existing role list first)*
3. Should the user be assigned to an organization?
4. Is MFA required for this user?
5. Should an activation email be sent? *(always yes unless userPassType=Plain and password is pre-set)*

---

## Flow Steps

### Step 1 — Get Available Roles (pre-fill role selector)

Before showing the create-user form, fetch the list of available roles.

```
Action: get-roles
Input:
  page       = 1
  pageSize   = 100
  projectKey = VITE_X_BLOCKS_KEY
```

Use the response to populate a role multi-select in the form.

---

### Step 2 — Get Organizations (if applicable)

If the project uses organizations, fetch the list to populate an org selector.

```
Action: get-organizations
```

---

### Step 3 — Create User

```
Action: create-user
Input:
  email            = user's email (required, must be unique)
  firstName        = optional
  lastName         = optional
  userCreationType = "AdminCreated"
  userPassType     = "Plain" (if setting password) or omit (activation email will be sent)
  password         = only if userPassType is Plain
  mfaEnabled       = true/false based on pre-flight answer
  allowedLogInType = ["Email"] or as configured
  organizationId   = org ID if applicable
  projectKey       = VITE_X_BLOCKS_KEY
```

```
On success (isSuccess: true) → continue to Step 4
On 400 (duplicate email)    → show "This email is already registered"
On 401                      → run refresh-token then retry
```

---

### Step 4 — Assign Roles

Assign selected roles to the newly created user.

```
Action: set-roles
Input:
  userId     = ID of the user created in Step 3
  roles      = array of role slugs selected in form
  projectKey = VITE_X_BLOCKS_KEY
```

```
On success → roles assigned
On 400     → invalid role slugs — verify roles exist via get-roles
```

> Note: `set-roles` **replaces** all existing roles. If adding to existing roles, first call `get-user-roles` and merge the arrays.

---

### Step 5 — Confirm and Notify

After user is created and roles assigned:
- Show success state in the UI
- The backend automatically sends an activation email to the user
- The user activates their account via the `user-registration` flow (Step 3 onward)

---

## Viewing and Managing Users

### List users
```
Action: get-users
Input:
  page            = 1
  pageSize        = 20
  sort.property   = "createdDate"
  sort.isDescending = true
  filter.name     = search term (optional)
  filter.status   = "Active" | "Inactive" (optional)
  projectKey      = VITE_X_BLOCKS_KEY
```

### Get single user
```
Action: get-user
Input: userId (query param)
```

### Update user details
```
Action: update-user
Input: userId + fields to update + projectKey
```

### Deactivate user
```
Action: deactivate-user
Input: userId + projectKey
```

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| `create-user` 400 | Duplicate email | Show "Email already registered" |
| `create-user` 400 | Weak password | Show password strength requirements |
| `set-roles` 400 | Invalid role slug | Verify against get-roles response |
| `get-roles` 401 | Token expired | Run refresh-token then retry |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/users/pages/users-page.tsx` | User list with search, filter, pagination |
| `modules/users/components/user-table/user-table.tsx` | Data table of users |
| `modules/users/components/create-user-form/create-user-form.tsx` | Create user form with role multi-select |
| `modules/users/components/user-detail/user-detail.tsx` | User profile view with roles and permissions |
| `modules/users/hooks/use-users.tsx` | `useCreateUser`, `useGetUsers`, `useGetUser`, `useUpdateUser`, `useDeactivateUser`, `useSetRoles` hooks |
| `modules/users/services/user.service.ts` | API calls for user management |
| `modules/users/types/user.type.ts` | `CreateUserPayload`, `UserListResponse`, `UserDetailResponse` |
| `routes/users.route.tsx` | `/users`, `/users/:id` routes |
