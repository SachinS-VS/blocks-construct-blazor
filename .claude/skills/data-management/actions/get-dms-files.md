# Action: get-dms-files

## Purpose

List files and folders in the Document Management System (DMS) for a given parent directory. Use this to build a folder/file browser UI. Omit `ParentDirectoryId` to list root-level items.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uds/v1/Files/GetDmsFileAndFolder
```

---

## curl

List root level:

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/Files/GetDmsFileAndFolder" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "ParentDirectoryId": null,
    "ProjectKey": "$VITE_PROJECT_SLUG",
    "Page": 1,
    "PageSize": 20
  }'
```

List items in a folder:

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/Files/GetDmsFileAndFolder" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "ParentDirectoryId": "folder-id-abc123",
    "ProjectKey": "$VITE_PROJECT_SLUG",
    "Page": 1,
    "PageSize": 20
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| ParentDirectoryId | string | no | Parent folder ID — set `null` for root level |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |
| Page | integer | no | Default: 1 |
| PageSize | integer | no | Default: 20 |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Success",
  "httpStatusCode": 200,
  "data": {
    "items": [
      {
        "id": "folder-id-abc123",
        "name": "Product Images",
        "isFolder": true,
        "parentDirectoryId": null,
        "createdAt": "2024-01-01T00:00:00Z"
      },
      {
        "id": "file-id-def456",
        "name": "hero-image.png",
        "isFolder": false,
        "parentDirectoryId": null,
        "size": 204800,
        "contentType": "image/png",
        "accessModifier": "Public",
        "tags": ["hero"],
        "createdAt": "2024-01-02T00:00:00Z"
      }
    ],
    "total": 2,
    "page": 1,
    "pageSize": 20
  },
  "errors": {}
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Invalid ParentDirectoryId | Verify folder ID exists |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 404 | Parent folder not found | Verify ParentDirectoryId or set to null for root |
