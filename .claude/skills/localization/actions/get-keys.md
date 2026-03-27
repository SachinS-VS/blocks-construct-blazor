# Action: get-keys

## Purpose

Get translation keys for a project and module with filtering and pagination.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uilm/v1/Key/Gets
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uilm/v1/Key/Gets" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "projectKey": "'$VITE_X_BLOCKS_KEY'",
    "moduleId": "<MODULE_ID>",
    "pageNumber": 1,
    "pageSize": 20,
    "filter": {
      "search": "",
      "languageCode": "",
      "untranslatedOnly": false
    }
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |
| moduleId | string | yes | Filter by module |
| pageNumber | integer | yes | Starts at 1 |
| pageSize | integer | yes | Max 100 |
| filter.search | string | no | Search by key name |
| filter.languageCode | string | no | Show translations for a specific language |
| filter.untranslatedOnly | boolean | no | `true` to show only keys missing translations |

---

## Pagination

* `pageNumber` starts at 1
* `pageSize` max is 100, default is 20
* Response includes `totalCount` for calculating total pages

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
        { "languageCode": "en", "value": "Welcome Back" },
        { "languageCode": "de", "value": "Willkommen zurück" }
      ]
    }
  ],
  "totalCount": 100,
  "success": true,
  "errorMessage": null,
  "validationErrors": []
}
```

---

## On Failure

* 401 — run refresh-token then retry
