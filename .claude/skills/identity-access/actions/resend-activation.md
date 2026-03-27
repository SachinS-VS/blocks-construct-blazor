# Action: resend-activation

## Purpose

Resend the activation email to a user who hasn't activated yet.

---

## Endpoint

```
POST $VITE_API_BASE_URL/idp/v1/Iam/ResendActivation
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/idp/v1/Iam/ResendActivation" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "email": "user@example.com",
    "projectKey": "'$VITE_X_BLOCKS_KEY'"
  }'
```

---

## Request Body

| Field | Type | Required |
|-------|------|----------|
| email | string | yes |
| projectKey | string | yes |

---

## On Success (200)

Activation email resent.

---

## On Failure

* 400 — user already activated or email not found
* 401 — run refresh-token then retry
