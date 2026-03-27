# Action: get-login-options

## Purpose

Get available login methods configured for the project (e.g. email/password, social login, SSO).

---

## Endpoint

```
GET $VITE_API_BASE_URL/idp/v1/Authentication/GetLoginOptions
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/idp/v1/Authentication/GetLoginOptions" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Request Body

None.

---

## On Success (200)

Returns list of enabled login methods for the project. Use this to dynamically show login options in the UI.

---

## On Failure

* 400 — invalid or missing x-blocks-key
