# Action: get-key

## Purpose

Get a single translation key by its ID, including all language translations.

---

## Endpoint

```
GET $VITE_API_BASE_URL/uilm/v1/Key/Get?itemId=<KEY_ID>&projectKey=$VITE_X_BLOCKS_KEY
```

---

## curl

```bash
curl --location \
  "$VITE_API_BASE_URL/uilm/v1/Key/Get?itemId=<KEY_ID>&projectKey=$VITE_X_BLOCKS_KEY" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| itemId | string | yes | ID of the key to retrieve |
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |

---

## On Success (200)

```json
{
  "data": {
    "id": "string",
    "keyName": "login.title",
    "moduleId": "string",
    "projectKey": "string",
    "translations": [
      { "languageCode": "en", "value": "Welcome Back" },
      { "languageCode": "de", "value": "Willkommen zurück" }
    ]
  },
  "success": true,
  "errorMessage": null,
  "validationErrors": []
}
```

---

## On Failure

* 401 — run refresh-token then retry
* 404 — key not found
