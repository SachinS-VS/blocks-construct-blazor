# Action: get-token

## Purpose

Authenticate using username and password to obtain an access token and refresh token.
Run this at the start of every session before calling any other API.

---

## Endpoint

```
POST $VITE_API_BASE_URL/idp/v1/Authentication/Token
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/idp/v1/Authentication/Token" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/x-www-form-urlencoded" \
  --data-urlencode "grant_type=password" \
  --data-urlencode "username=$USERNAME" \
  --data-urlencode "password=$PASSWORD" \
  --data-urlencode "client_id=$VITE_BLOCKS_OIDC_CLIENT_ID"
```

---

## Fixed vs Dynamic

| Parameter | Value | Type |
|-----------|-------|------|
| x-blocks-key | $VITE_X_BLOCKS_KEY | fixed |
| grant_type | password | fixed |
| client_id | $VITE_BLOCKS_OIDC_CLIENT_ID | fixed |
| username | $USERNAME | dynamic |
| password | $PASSWORD | dynamic |

---

## On Success (200)

Store values from the response:
* `access_token` → $ACCESS_TOKEN
* `refresh_token` → $REFRESH_TOKEN

All subsequent API calls must use:
```
Authorization: Bearer $ACCESS_TOKEN
```

### Sample Response

```json
{
  "access_token": "eyJhbGci...",
  "token_type": "Bearer",
  "expires_in": 8000,
  "refresh_token": "538b8ede...",
  "id_token": null
}
```

---

## On Failure

* 400 — malformed request, wrong client_id, or invalid grant_type
* 401 — wrong USERNAME or PASSWORD
* 403 — account missing `cloudadmin` role — assign it in Cloud Portal → People
* 404 — environment not created or project not active — verify in Cloud Portal → Environments
