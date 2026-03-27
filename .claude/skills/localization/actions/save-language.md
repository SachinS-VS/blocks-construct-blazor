# Action: save-language

## Purpose

Create or update a language for a project. Omit `id` to create; include `id` to update.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uilm/v1/Language/Save
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uilm/v1/Language/Save" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "name": "English",
    "code": "en",
    "projectKey": "'$VITE_X_BLOCKS_KEY'"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| id | string | no | Omit to create, include to update |
| name | string | yes | Display name (e.g. "English") |
| code | string | yes | ISO 639-1 language code (e.g. "en") |
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |

---

## On Success (200)

```json
{
  "success": true,
  "errorMessage": null,
  "validationErrors": []
}
```

---

## On Failure

* 400 — missing required fields or language code already exists
* 401 — run refresh-token then retry
