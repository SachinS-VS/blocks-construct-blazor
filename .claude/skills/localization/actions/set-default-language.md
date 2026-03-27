# Action: set-default-language

## Purpose

Set a language as the default for a project. The default language is used as the fallback when a translation is missing.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uilm/v1/Language/SetDefault
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uilm/v1/Language/SetDefault" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "languageId": "<LANGUAGE_ID>",
    "projectKey": "'$VITE_X_BLOCKS_KEY'"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| languageId | string | yes | ID of the language to set as default |
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

* 400 — language not found or does not belong to the project
* 401 — run refresh-token then retry
