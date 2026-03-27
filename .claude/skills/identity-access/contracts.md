# Identity & Access Contracts

## Common Headers (all authenticated requests)

```
Authorization: Bearer $ACCESS_TOKEN
x-blocks-key: $VITE_X_BLOCKS_KEY
Content-Type: application/json
```

---

## Common Response: BaseResponse

```json
{
  "isSuccess": true,
  "errors": {
    "fieldName": "error message"
  }
}
```

> `errors` is a **dictionary** (key = field name, value = error message), not an array.

---

## Authentication

### get-token Request (form-encoded)

| Field | Type | Fixed/Dynamic | Value |
|-------|------|---------------|-------|
| grant_type | string | fixed | `password` |
| client_id | string | fixed | $VITE_BLOCKS_OIDC_CLIENT_ID |
| username | string | dynamic | $USERNAME |
| password | string | dynamic | $PASSWORD |

### refresh-token Request (form-encoded)

| Field | Type | Fixed/Dynamic | Value |
|-------|------|---------------|-------|
| grant_type | string | fixed | `refresh_token` |
| client_id | string | fixed | $VITE_BLOCKS_OIDC_CLIENT_ID |
| refresh_token | string | runtime | $REFRESH_TOKEN |

### Token Response (no MFA)

```json
{
  "access_token": "eyJhbGci...",
  "token_type": "Bearer",
  "expires_in": 8000,
  "refresh_token": "538b8ede...",
  "id_token": null
}
```

### Token Response (MFA required — enable_mfa: true)

```json
{
  "enable_mfa": true,
  "mfaType": "email",
  "mfaId": "abc123",
  "message": "OTP sent to your email"
}
```

> `mfaType` values in this response are `"email"` or `"authenticator"` — these are OAuth response strings, NOT the `UserMfaType` enum values (`OTP`, `TOTP`) used in OTP generation requests.

### get-token Request — mfa_code grant (form-encoded)

| Field | Type | Required | Value |
|-------|------|----------|-------|
| grant_type | string | yes | `mfa_code` |
| client_id | string | yes | $VITE_BLOCKS_OIDC_CLIENT_ID |
| mfa_id | string | yes | `mfaId` from MFA token response |
| mfa_type | string | yes | `mfaType` from MFA token response (`"email"` or `"authenticator"`) |
| otp | string | yes | OTP entered by user |

### get-token Request — authorization_code grant (form-encoded)

| Field | Type | Required | Value |
|-------|------|----------|-------|
| grant_type | string | yes | `authorization_code` |
| client_id | string | yes | $VITE_BLOCKS_OIDC_CLIENT_ID |
| code | string | yes | authorization code from redirect URL param |
| redirect_uri | string | yes | $VITE_BLOCKS_OIDC_REDIRECT_URI |

### GetLoginOptionsResponse

```json
{
  "loginOptions": [
    {
      "type": "Email",
      "providers": []
    },
    {
      "type": "SocialLogin",
      "providers": ["Google", "Microsoft", "LinkedIn", "GitHub"]
    },
    {
      "type": "SSO",
      "providers": []
    }
  ],
  "isSuccess": true,
  "errors": {}
}
```

### LogoutRequest

```json
{
  "refreshToken": "string"
}
```

### GenerateUserCodeRequest

```json
{
  "clientId": "string",
  "codeTtlInMinute": 0,
  "note": "string"
}
```

---

## Users

### CreateUserRequest

```json
{
  "email": "string",
  "userName": "string",
  "password": "string",
  "firstName": "string",
  "lastName": "string",
  "phoneNumber": "string",
  "language": "string",
  "salutation": "string",
  "mailPurpose": "string",
  "userPassType": "Plain | Hashed",
  "userCreationType": "string",
  "varifiedType": "string",
  "platform": "string",
  "profileImageUrl": "string",
  "profileImageId": "string",
  "userMfaType": "Email | Phone | Authenticator",
  "mfaEnabled": false,
  "allowedLogInType": ["Email", "UserName", "Phone", "SocialLogin"],
  "memberships": [],
  "tags": ["string"],
  "projectKey": "string",
  "organizationId": "string"
}
```

### GetUsersRequest

```json
{
  "page": 1,
  "pageSize": 20,
  "sort": {
    "property": "string",
    "isDescending": false
  },
  "filter": {
    "email": "string",
    "name": "string",
    "userIds": ["string"],
    "status": "Status enum",
    "mfa": "MFA enum",
    "joinedOn": "date-time",
    "lastLogin": "date-time",
    "organizationId": "string"
  },
  "projectKey": "string"
}
```

### ValidateActivationCodeRequest

```json
{
  "code": "string",
  "projectKey": "string"
}
```

### ValidateActivationCodeResponse

```json
{
  "isValid": true,
  "email": "user@example.com",
  "isSuccess": true,
  "errors": {}
}
```

### ActivateUserRequest

```json
{
  "code": "string",
  "password": "string",
  "projectKey": "string",
  "mailPurpose": "string",
  "captchaCode": "string",
  "preventPostEvent": false
}
```

### DeactivateUserRequest

```json
{
  "userId": "string",
  "projectKey": "string"
}
```

### ChangePasswordRequest

```json
{
  "oldPassword": "string",
  "newPassword": "string",
  "projectKey": "string"
}
```

---

## Roles

### CreateRoleRequest

```json
{
  "name": "string",
  "description": "string",
  "slug": "string",
  "projectKey": "string"
}
```

### GetRolesRequest

```json
{
  "page": 1,
  "pageSize": 20,
  "sort": {
    "property": "string",
    "isDescending": false
  },
  "filter": {
    "search": "string"
  },
  "projectKey": "string"
}
```

### SetRolesRequest

```json
{
  "userId": "string",
  "roles": ["string"],
  "projectKey": "string"
}
```

---

## Permissions

### CreatePermissionRequest

```json
{
  "name": "string",
  "description": "string",
  "resource": "string",
  "resourceGroup": "string",
  "type": "string",
  "tags": ["string"],
  "dependentPermissions": ["string"],
  "isBuiltIn": false,
  "projectKey": "string"
}
```

### GetPermissionsRequest

```json
{
  "page": 1,
  "pageSize": 20,
  "sort": {
    "property": "string",
    "isDescending": false
  },
  "filter": {},
  "projectKey": "string"
}
```

---

## Organizations

### SaveOrganizationRequest

```json
{
  "id": "string",
  "name": "string",
  "description": "string",
  "parentId": "string",
  "projectKey": "string"
}
```

> Omit `id` to create. Include `id` to update.

### GetOrganizationsResponse

```json
{
  "organizations": [
    {
      "id": "string",
      "name": "string",
      "description": "string",
      "parentId": "string",
      "childCount": 0
    }
  ],
  "isSuccess": true,
  "errors": {}
}
```

---

## Sessions & History

### GetSessionsResponse

```json
{
  "sessions": [
    {
      "sessionId": "string",
      "device": "string",
      "browser": "string",
      "ipAddress": "string",
      "location": "string",
      "loginTime": "2024-01-01T00:00:00Z",
      "isCurrent": true
    }
  ],
  "isSuccess": true,
  "errors": {}
}
```

### GetHistoriesResponse

```json
{
  "histories": [
    {
      "id": "string",
      "action": "string",
      "description": "string",
      "ipAddress": "string",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "total": 100,
  "isSuccess": true,
  "errors": {}
}
```

---

## MFA

### SetupTotpResponse

```json
{
  "qrCodeUrl": "string",
  "secretKey": "string",
  "isSuccess": true,
  "errors": {}
}
```

> `qrCodeUrl` is a base64-encoded QR code image. Render as `<img src={qrCodeUrl} />`.
> `secretKey` is the backup manual entry code for authenticator apps.

### ResendOtpRequest

```json
{
  "mfaId": "string",
  "projectKey": "string"
}
```

### OtpGenerationRequest

```json
{
  "userId": "string",
  "projectKey": "string",
  "mfaType": "Email | Phone | Authenticator",
  "sendPhoneNumberAsEmailDomain": "string"
}
```

### VerifyOtpRequest

```json
{
  "userId": "string",
  "otp": "string",
  "projectKey": "string"
}
```

### DisableUserMfaRequest

```json
{
  "userId": "string",
  "projectKey": "string"
}
```

---

## Captcha

### CreateCaptchaRequest

```json
{
  "configurationName": "string"
}
```

### VerifyCaptchaRequest (query params)

| Param | Type | Required |
|-------|------|----------|
| VerificationCode | string | yes |
| ConfigurationName | string | yes |

---

## Enumerations

> **Note:** The Swagger spec defines these as integer enums. The string names below are the .NET enum member names. Whether the API accepts integer values or string names depends on server serialization config — use the string names shown here as they match existing action definitions.

| Enum | Values (string name — integer value) |
|------|---------------------------------------|
| UserPassType | `Plain` — 0, `Hashed` — 1, `SsoOnly` — 2 |
| UserMfaType | `None` — 0, `OTP` — 1, `TOTP` — 2, `Biometric` — 3, `Multiple` — 4 |
| UserLogInType | `Email` — 0, `SocialLogin` — 1, `SSO` — 2, `Biometric` — 3 |
| UserCreationType | `Direct` — 0, `EmailInvitation` — 1, `SocialSignUp` — 2, `API` — 3, `AdminCreated` — 4, `SelfService` — 5 |
| UserVarifiedType | `NotVerified` — 0, `EmailVerified` — 1, `PhoneVerified` — 2, `BothVerified` — 3 |
| Status | Active, Inactive (use for filter queries) |
