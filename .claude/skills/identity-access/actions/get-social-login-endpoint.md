# Action: get-social-login-endpoint

## Purpose

Get the authorization URL for a specific social login provider (Google, Microsoft, LinkedIn, GitHub, etc.).
Redirect the user to this URL to begin the OAuth flow.

---

## Endpoint

```
POST $VITE_API_BASE_URL/idp/v1/Authentication/GetSocialLogInEndPoint
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/idp/v1/Authentication/GetSocialLogInEndPoint" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "provider": "Google",
    "redirectUri": "'$VITE_BLOCKS_OIDC_REDIRECT_URI'",
    "projectKey": "'$VITE_X_BLOCKS_KEY'"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| provider | string | yes | Social provider name (e.g. `Google`, `Microsoft`, `LinkedIn`, `GitHub`) |
| redirectUri | string | yes | Use $VITE_BLOCKS_OIDC_REDIRECT_URI |
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |

---

## On Success (200)

Returns the provider's authorization URL. Redirect the user to this URL.

```json
{
  "url": "https://accounts.google.com/o/oauth2/auth?...",
  "isSuccess": true,
  "errors": {}
}
```

After the user authenticates with the provider, they are redirected back to `redirectUri` with a `code` parameter in the URL.
Exchange that code using `get-token` with `grant_type=authorization_code`.

---

## On Failure

* 400 — invalid provider name or missing redirectUri
* 404 — provider not configured for this project

> Use `get-login-options` first to confirm which social providers are enabled before calling this action.
