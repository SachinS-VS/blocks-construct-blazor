# Action: validate-activation-code

## Authentication

Not required. This action is public.

---

## Purpose

Validate an activation code before submitting the full activation form. Use this to give early feedback to the user before they set a password.

---

## Endpoint

```
POST $VITE_API_BASE_URL/idp/v1/Iam/ValidateActivationCode
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/idp/v1/Iam/ValidateActivationCode" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "code": "activation_code",
    "projectKey": "'$VITE_X_BLOCKS_KEY'"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| code | string | yes | Activation code from email |
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |

---

## On Success (200)

Code is valid. Proceed to show the password-setting form and call activate-user.

---

## On Failure

* 400 — invalid or expired activation code
