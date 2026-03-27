# Action: save-key

## Purpose

Create or update a single translation key with its translations. Omit `id` to create; include `id` to update.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uilm/v1/Key/Save
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uilm/v1/Key/Save" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "keyName": "login.button.submit",
    "moduleId": "<MODULE_ID>",
    "projectKey": "'$VITE_X_BLOCKS_KEY'",
    "translations": [
      { "languageCode": "en", "value": "Sign In" },
      { "languageCode": "de", "value": "Anmelden" }
    ]
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| id | string | no | Omit to create, include to update |
| keyName | string | yes | Dot-notation key (e.g. "login.button.submit") |
| moduleId | string | yes | ID of the module this key belongs to |
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |
| translations | array | yes | Array of `{ languageCode, value }` objects |
| translations[].languageCode | string | yes | ISO 639-1 code matching a configured language |
| translations[].value | string | yes | Translated string for this language |

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

* 400 — key name already exists in this module, or invalid moduleId
* 401 — run refresh-token then retry
