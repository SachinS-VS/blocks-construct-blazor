# Action: get-uilm-file

## Purpose

Download the compiled translation JSON file for a specific language and module. Always call `generate-uilm-file` first to ensure the file is up to date before downloading.

---

## Endpoint

```
GET $VITE_API_BASE_URL/uilm/v1/Key/GetUilmFile?language=<CODE>&moduleId=<ID>&projectKey=$VITE_X_BLOCKS_KEY
```

---

## curl

```bash
curl --location \
  "$VITE_API_BASE_URL/uilm/v1/Key/GetUilmFile?language=en&moduleId=<MODULE_ID>&projectKey=$VITE_X_BLOCKS_KEY" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| language | string | yes | ISO 639-1 language code (e.g. "en") |
| moduleId | string | yes | ID of the module |
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |

---

## On Success (200)

Returns a flat JSON object of key-value pairs:

```json
{
  "login.title": "Welcome Back",
  "login.button.submit": "Sign In",
  "login.placeholder.email": "Enter your email"
}
```

---

## On Failure

* 401 — run refresh-token then retry
* 404 — no compiled file found; call `generate-uilm-file` first
