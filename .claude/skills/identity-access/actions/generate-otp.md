# Action: generate-otp

## Purpose

Generate a one-time password (OTP) for MFA verification.

---

## Endpoint

```
POST $VITE_API_BASE_URL/idp/v1/Mfa/GenerateOTP
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/idp/v1/Mfa/GenerateOTP" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "userId": "USER_ID",
    "projectKey": "'$VITE_X_BLOCKS_KEY'",
    "mfaType": "Email"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| userId | string | yes | |
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |
| mfaType | string | yes | Email \| Phone \| Authenticator |
| sendPhoneNumberAsEmailDomain | string | no | Optional |

---

## On Success (200)

OTP generated and sent via the specified channel (email/SMS).

---

## On Failure

* 400 — MFA not enabled for user
* 401 — run refresh-token then retry
