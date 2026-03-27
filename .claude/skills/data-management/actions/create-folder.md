# Action: create-folder

## Purpose

Create a new folder in the Document Management System (DMS). Folders can be nested inside other folders using `ParentDirectoryId`. Omit `ParentDirectoryId` to create a root-level folder.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uds/v1/Files/CreateFolder
```

---

## curl

Create root folder:

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/Files/CreateFolder" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "Name": "Product Images",
    "ProjectKey": "$VITE_PROJECT_SLUG"
  }'
```

Create nested folder:

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/Files/CreateFolder" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "Name": "Thumbnails",
    "ParentDirectoryId": "folder-id-abc123",
    "ProjectKey": "$VITE_PROJECT_SLUG"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Name | string | yes | Folder display name |
| ParentDirectoryId | string | no | Parent folder ID — omit or set `null` for root level |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Folder created successfully",
  "httpStatusCode": 200,
  "data": {
    "id": "folder-id-new123",
    "name": "Product Images",
    "parentDirectoryId": null,
    "isFolder": true,
    "createdAt": "2024-01-01T00:00:00Z"
  },
  "errors": {}
}
```

Store `data.id` to use as `ParentDirectoryId` when uploading files into this folder.

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Missing Name or invalid ParentDirectoryId | Check request body |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 404 | ParentDirectoryId folder not found | Verify parent folder ID from get-dms-files |
