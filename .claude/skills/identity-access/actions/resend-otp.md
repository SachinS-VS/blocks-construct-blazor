# Action: resend-otp

## Purpose

Resend an OTP to the user if the previous one expired or was not received.

---

## Endpoint

```
POST $VITE_API_BASE_URL/idp/v1/Mfa/ResendOtp
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/idp/v1/Mfa/ResendOtp" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "userId": "USER_ID",
    "projectKey": "'$VITE_X_BLOCKS_KEY'"
  }'
```

---

## Request Body

| Field | Type | Required |
|-------|------|----------|
| userId | string | yes |
| projectKey | string | yes |

---

## On Success (200)

New OTP generated and sent.

---

## On Failure

* 429 — rate limited, too many resend attempts
* 401 — run refresh-token then retry
