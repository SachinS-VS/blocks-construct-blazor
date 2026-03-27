# Action: generate-uilm-file

## Purpose

Regenerate the compiled translation JSON file for a language and module. Must be called before `get-uilm-file` to ensure the downloaded file reflects the latest key changes.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uilm/v1/Key/GenerateUilmFile
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uilm/v1/Key/GenerateUilmFile" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "projectKey": "'$VITE_X_BLOCKS_KEY'",
    "moduleId": "<MODULE_ID>",
    "languageCode": "en"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |
| moduleId | string | yes | ID of the module to regenerate |
| languageCode | string | yes | ISO 639-1 language code |

---

## On Success (200)

```json
{
  "success": true,
  "errorMessage": null,
  "validationErrors": []
}
```

After success, call `get-uilm-file` to download the regenerated file.

---

## On Failure

* 400 — invalid moduleId or languageCode not configured for the project
* 401 — run refresh-token then retry
