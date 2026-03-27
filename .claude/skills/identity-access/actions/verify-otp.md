# Action: verify-otp

## Purpose

Verify a user-submitted OTP for MFA.

---

## Endpoint

```
POST $VITE_API_BASE_URL/idp/v1/Mfa/VerifyOTP
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/idp/v1/Mfa/VerifyOTP" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "userId": "USER_ID",
    "otp": "123456",
    "projectKey": "'$VITE_X_BLOCKS_KEY'"
  }'
```

---

## Request Body

| Field | Type | Required |
|-------|------|----------|
| userId | string | yes |
| otp | string | yes |
| projectKey | string | yes |

---

## On Success (200)

OTP verified. Proceed with authenticated action.

---

## On Failure

* 400 — invalid or expired OTP
* 401 — run refresh-token then retry
