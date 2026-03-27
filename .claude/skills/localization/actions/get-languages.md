# Action: get-languages

## Purpose

List all languages configured for a project.

---

## Endpoint

```
GET $VITE_API_BASE_URL/uilm/v1/Language/Gets?projectKey=$VITE_X_BLOCKS_KEY
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uilm/v1/Language/Gets?projectKey=$VITE_X_BLOCKS_KEY" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |

---

## On Success (200)

```json
{
  "data": [
    {
      "id": "string",
      "name": "English",
      "code": "en",
      "isDefault": true,
      "projectKey": "string"
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
