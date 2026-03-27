---
name: identity-access
description: "Use this skill for any request involving login, MFA setup, user creation, role/permission management, organization switching, SSO/OIDC configuration, session handling, password recovery, CAPTCHA, or access control on SELISE Blocks. Maps natural language intents to the correct flow or action and enforces security best practices."
user-invocable: false
blocks-version: "1.0.3"
---

# Identity & Access Skill

## Purpose

Handles all authentication, user management, role/permission management, MFA, and organization operations for SELISE Blocks via the IDP v1 API.

Must run get-token before any other action in a session.

---

## When to Use

Example prompts that should route here:
- "Build a login page with email/password and Google SSO"
- "Set up MFA with TOTP for admin users"
- "Create an admin role with full permissions"
- "Show me active user sessions with logout button"
- "Build a forgot password flow"

---

## Execution Context

Before executing any action or flow from this skill, read `../core/execution-context.md` for the required supporting files, load order, and cross-domain orchestration rules.

---

## Intent Mapping

Use this table to route user requests. Check `flows/` first — if a flow covers the request, use it. For single-action requests, go directly to the action.

| User wants to... | Use |
|------------------|-----|
| Build a login page | `flows/login-flow.md` |
| Build signup / registration | `flows/user-registration.md` |
| Build forgot password / reset password | `flows/password-recovery.md` |
| Set up MFA for a user | `flows/mfa-setup.md` |
| Create a user and assign roles (admin) | `flows/user-onboarding.md` |
| Build session list / logout UI | `flows/session-management.md` |
| Create roles and permissions / RBAC setup | `flows/role-permission-setup.md` |
| Get a specific user by ID | `actions/get-user.md` |
| Search / list users | `actions/get-users.md` |
| Update a user's profile | `actions/update-user.md` |
| Deactivate a user | `actions/deactivate-user.md` |
| Change password (authenticated user) | `actions/change-password.md` |
| Check if email is taken | `actions/check-email-available.md` |
| Validate an activation code | `actions/validate-activation-code.md` |
| Get current user's roles | `actions/get-account-roles.md` |
| Get current user's permissions | `actions/get-account-permissions.md` |
| Get a specific role | `actions/get-role.md` |
| Get a specific permission | `actions/get-permission.md` |
| List resource groups | `actions/get-resource-groups.md` |
| Create / update an organization | `actions/save-organization.md` |
| Get organization config | `actions/get-organization-config.md` |
| View audit history | `actions/get-histories.md` |
| Generate a user code | `actions/generate-user-code.md` |

---

## Flows

| Flow | File | Description |
|------|------|-------------|
| login-flow | flows/login-flow.md | Email, social, OIDC login with MFA branching |
| user-registration | flows/user-registration.md | Self-registration or admin-created with activation |
| password-recovery | flows/password-recovery.md | Forgot password → reset password → logout-all |
| mfa-setup | flows/mfa-setup.md | Email OTP or TOTP authenticator enrollment |
| user-onboarding | flows/user-onboarding.md | Admin creates user, assigns roles and org |
| session-management | flows/session-management.md | View sessions, single/all device logout |
| role-permission-setup | flows/role-permission-setup.md | Create roles, permissions, assign to users |

---

## Base Path

All endpoints are prefixed with: `$VITE_API_BASE_URL/idp/v1`

---

## Action Index

### Authentication
| Action | File | Description |
|--------|------|-------------|
| get-token | actions/get-token.md | Exchange credentials for access + refresh tokens |
| refresh-token | actions/refresh-token.md | Renew access token using refresh token |
| logout | actions/logout.md | Logout current session |
| logout-all | actions/logout-all.md | Logout all active sessions |
| get-user-info | actions/get-user-info.md | Get authenticated user's info |
| get-login-options | actions/get-login-options.md | Get available login methods for the project |
| generate-user-code | actions/generate-user-code.md | Generate a new user code |
| get-user-codes | actions/get-user-codes.md | List all user codes |

### Users
| Action | File | Description |
|--------|------|-------------|
| create-user | actions/create-user.md | Create a new user |
| update-user | actions/update-user.md | Update user details |
| get-users | actions/get-users.md | List users with pagination and filters |
| get-user | actions/get-user.md | Get a specific user by ID |
| activate-user | actions/activate-user.md | Activate a user account |
| deactivate-user | actions/deactivate-user.md | Deactivate a user account |
| change-password | actions/change-password.md | Change user password |
| recover-user | actions/recover-user.md | Initiate password recovery |
| reset-password | actions/reset-password.md | Reset password using recovery code |
| resend-activation | actions/resend-activation.md | Resend activation email |
| validate-activation-code | actions/validate-activation-code.md | Validate activation code before setting password |
| check-email-available | actions/check-email-available.md | Check if email is already taken |
| get-account | actions/get-account.md | Get current authenticated account profile |
| get-user-roles | actions/get-user-roles.md | Get roles assigned to a user |
| get-user-permissions | actions/get-user-permissions.md | Get permissions assigned to a user |
| get-account-roles | actions/get-account-roles.md | Get roles of current authenticated account |
| get-account-permissions | actions/get-account-permissions.md | Get permissions of current authenticated account |
| get-sessions | actions/get-sessions.md | Get active sessions for a user |
| get-histories | actions/get-histories.md | Get audit history for a user |

### Roles
| Action | File | Description |
|--------|------|-------------|
| create-role | actions/create-role.md | Create a new role |
| update-role | actions/update-role.md | Update an existing role |
| get-roles | actions/get-roles.md | List roles with pagination |
| get-role | actions/get-role.md | Get a specific role |
| set-roles | actions/set-roles.md | Assign roles to a user |

### Permissions
| Action | File | Description |
|--------|------|-------------|
| create-permission | actions/create-permission.md | Create a new permission |
| update-permission | actions/update-permission.md | Update an existing permission |
| get-permissions | actions/get-permissions.md | List permissions with pagination |
| get-permission | actions/get-permission.md | Get a specific permission |
| save-roles-and-permissions | actions/save-roles-and-permissions.md | Bulk assign roles and permissions |
| get-resource-groups | actions/get-resource-groups.md | List all resource groups |

### Organizations
| Action | File | Description |
|--------|------|-------------|
| save-organization | actions/save-organization.md | Create or update an organization |
| get-organizations | actions/get-organizations.md | List all organizations |
| get-organization | actions/get-organization.md | Get a specific organization |
| save-organization-config | actions/save-organization-config.md | Save configuration for an organization |
| get-organization-config | actions/get-organization-config.md | Get configuration for an organization |

### MFA
| Action | File | Description |
|--------|------|-------------|
| generate-otp | actions/generate-otp.md | Generate an OTP for MFA |
| verify-otp | actions/verify-otp.md | Verify an OTP |
| setup-totp | actions/setup-totp.md | Setup TOTP authenticator app |
| disable-user-mfa | actions/disable-user-mfa.md | Disable MFA for a user |
| resend-otp | actions/resend-otp.md | Resend OTP |

### Captcha
| Action | File | Description |
|--------|------|-------------|
| create-captcha | actions/create-captcha.md | Create a CAPTCHA challenge |
| submit-captcha | actions/submit-captcha.md | Submit CAPTCHA answer via POST |
| verify-captcha | actions/verify-captcha.md | Verify a CAPTCHA response via GET |
