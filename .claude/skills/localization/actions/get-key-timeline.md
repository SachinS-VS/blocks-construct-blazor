# Action: get-key-timeline

## Purpose

Get the edit history (version timeline) for a translation key. Each entry represents a change to any translation value for the key.

---

## Endpoint

```
GET $VITE_API_BASE_URL/uilm/v1/Key/GetTimeline?keyId=<KEY_ID>&pageNumber=1&pageSize=20
```

---

## curl

```bash
curl --location \
  "$VITE_API_BASE_URL/uilm/v1/Key/GetTimeline?keyId=<KEY_ID>&pageNumber=1&pageSize=20" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| keyId | string | yes | ID of the key |
| pageNumber | integer | yes | Starts at 1 |
| pageSize | integer | yes | Number of entries per page |

---

## On Success (200)

```json
{
  "data": [
    {
      "id": "string",
      "keyId": "string",
      "languageCode": "en",
      "value": "Welcome Back",
      "changedAt": "2024-01-01T00:00:00Z",
      "changedBy": "user@example.com"
    }
  ],
  "totalCount": 10,
  "success": true,
  "errorMessage": null,
  "validationErrors": []
}
```

---

## On Failure

* 401 — run refresh-token then retry
* 404 — key not found
