# Action: get-keys-by-names

## Purpose

Retrieve specific translation keys by their key name strings. Useful when you know the exact key names and want to fetch their current translations.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uilm/v1/Key/GetsByKeyNames
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uilm/v1/Key/GetsByKeyNames" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "projectKey": "'$VITE_X_BLOCKS_KEY'",
    "moduleId": "<MODULE_ID>",
    "keyNames": ["login.title", "login.button.submit"]
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |
| moduleId | string | yes | Module the keys belong to |
| keyNames | array of strings | yes | List of key name strings to fetch |

---

## On Success (200)

```json
{
  "data": [
    {
      "id": "string",
      "keyName": "login.title",
      "moduleId": "string",
      "projectKey": "string",
      "translations": [
        { "languageCode": "en", "value": "Welcome Back" }
      ]
    }
  ],
  "success": true,
  "errorMessage": null,
  "validationErrors": []
}
```

---

## On Failure

* 401 — run refresh-token then retry
