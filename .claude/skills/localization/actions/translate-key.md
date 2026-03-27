# Action: translate-key

## Purpose

AI-translate a specific key into a target language. Use when you want to translate a single key rather than all untranslated keys in a module.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uilm/v1/Key/TranslateKey
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uilm/v1/Key/TranslateKey" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "keyId": "<KEY_ID>",
    "projectKey": "'$VITE_X_BLOCKS_KEY'",
    "languageCode": "de"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| keyId | string | yes | ID of the key to translate |
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |
| languageCode | string | yes | Target language code (e.g. "de", "fr") |

---

## On Success (200)

```json
{
  "success": true,
  "errorMessage": null,
  "validationErrors": []
}
```

After success, call `get-key` to retrieve the updated translation value.

---

## On Failure

* 400 — invalid keyId or languageCode not configured for the project
* 401 — run refresh-token then retry
* 500 — AI translation service error; retry the request
