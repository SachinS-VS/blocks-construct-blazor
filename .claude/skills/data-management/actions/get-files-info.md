# Action: get-files-info

## Purpose

Get metadata (name, size, content type, tags, access modifier) for multiple files without downloading their content. Use this to display file listings or verify file details before download.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uds/v1/Files/GetFilesInfo
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/Files/GetFilesInfo" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "FileIds": ["file-id-1", "file-id-2"],
    "ProjectKey": "$VITE_PROJECT_SLUG"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| FileIds | array | yes | Array of file IDs to retrieve metadata for |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Success",
  "httpStatusCode": 200,
  "data": [
    {
      "id": "file-id-1",
      "name": "product-image.png",
      "size": 204800,
      "contentType": "image/png",
      "accessModifier": "Public",
      "tags": ["product", "image"],
      "metaData": "{\"altText\": \"Product front view\"}",
      "parentDirectoryId": null,
      "createdAt": "2024-01-01T10:00:00Z"
    },
    {
      "id": "file-id-2",
      "name": "invoice-001.pdf",
      "size": 51200,
      "contentType": "application/pdf",
      "accessModifier": "Private",
      "tags": ["invoice"],
      "metaData": null,
      "parentDirectoryId": "folder-id-abc",
      "createdAt": "2024-01-02T09:00:00Z"
    }
  ],
  "errors": {}
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Empty FileIds array | Provide at least one file ID |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 404 | One or more file IDs not found | Verify all FileIds |
