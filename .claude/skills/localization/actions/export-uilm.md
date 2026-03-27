# Action: export-uilm

## Purpose

Export translation modules as downloadable files. Pass one or more module IDs to export their compiled translations.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uilm/v1/Key/UilmExport
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uilm/v1/Key/UilmExport" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "projectKey": "'$VITE_X_BLOCKS_KEY'",
    "moduleIds": ["<MODULE_ID_1>", "<MODULE_ID_2>"]
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |
| moduleIds | array of strings | yes | IDs of modules to export |

---

## On Success (200)

```json
{
  "success": true,
  "errorMessage": null,
  "validationErrors": []
}
```

After triggering export, call `get-exported-files` to list available downloads.

---

## On Failure

* 400 — invalid or empty moduleIds array
* 401 — run refresh-token then retry
