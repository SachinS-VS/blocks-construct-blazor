# Action: save-module

## Purpose

Create or update a translation module. Modules group related translation keys (e.g. "auth", "dashboard", "common"). Omit `id` to create; include `id` to update.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uilm/v1/Module/Save
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uilm/v1/Module/Save" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "name": "auth",
    "projectKey": "'$VITE_X_BLOCKS_KEY'"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| id | string | no | Omit to create, include to update |
| name | string | yes | Module name (e.g. "auth", "common") |
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

* 400 — missing required fields or module name already exists in this project
* 401 — run refresh-token then retry
