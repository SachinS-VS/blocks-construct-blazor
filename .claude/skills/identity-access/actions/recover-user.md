# Action: recover-user

## Purpose

Initiate password recovery — sends a reset link to the user's email.

---

## Authentication

Not required. This action is public.

---

## Endpoint

```
POST $VITE_API_BASE_URL/idp/v1/Iam/Recover
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/idp/v1/Iam/Recover" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "email": "user@example.com",
    "projectKey": "'$VITE_X_BLOCKS_KEY'"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| email | string | yes | User's registered email |
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |

---

## On Success (200)

Recovery email sent. User receives a link with a reset code.

---

## On Failure

* 404 — email not found
