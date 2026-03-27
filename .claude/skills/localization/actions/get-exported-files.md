# Action: get-exported-files

## Purpose

List previously exported translation files for a project. Use this after calling `export-uilm` to retrieve download links.

---

## Endpoint

```
GET $VITE_API_BASE_URL/uilm/v1/Key/GetUilmExportedFiles?projectKey=$VITE_X_BLOCKS_KEY&pageNumber=1&pageSize=20
```

---

## curl

```bash
curl --location \
  "$VITE_API_BASE_URL/uilm/v1/Key/GetUilmExportedFiles?projectKey=$VITE_X_BLOCKS_KEY&pageNumber=1&pageSize=20" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |
| pageNumber | integer | yes | Starts at 1 |
| pageSize | integer | yes | Number of entries per page |

---

## On Success (200)

```json
{
  "data": [
    {
      "id": "string",
      "fileName": "export-2024-01-01.zip",
      "downloadUrl": "string",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "totalCount": 5,
  "success": true,
  "errorMessage": null,
  "validationErrors": []
}
```

---

## On Failure

* 401 — run refresh-token then retry
