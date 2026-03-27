# Action: delete-folder

## Purpose

Delete a folder from the Document Management System (DMS). Use this to remove empty or unwanted folders from the file hierarchy.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uds/v1/Files/DeleteFolder
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/Files/DeleteFolder" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "folderId": "folder-id-here",
    "projectKey": "$VITE_PROJECT_SLUG"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| folderId | string | yes | ID of the folder to delete |
| configurationName | string | no | Optional storage configuration name |
| projectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Folder deleted successfully",
  "httpStatusCode": 200
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Missing folderId or invalid request | Verify folderId from `get-dms-files` |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 404 | Folder not found | Verify folderId exists using `get-dms-files` |

---

## Warning

This is a destructive operation. Verify the folder contents using `get-dms-files` before deleting. Deleting a folder may also remove files within it.
