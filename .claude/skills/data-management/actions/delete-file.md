# Action: delete-file

## Purpose

Permanently delete a file from storage (S3 or local) by its ID. This cannot be undone.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uds/v1/Files/DeleteFile
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/Files/DeleteFile" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "FileId": "$FILE_ID",
    "ProjectKey": "$VITE_PROJECT_SLUG"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| FileId | string | yes | ID of the file to delete |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "File deleted successfully",
  "httpStatusCode": 200,
  "data": null,
  "errors": {}
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Missing FileId | Check request body |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role or file belongs to another project | Check user role and ProjectKey |
| 404 | File not found | Verify FileId |

---

## Warning

Deletion is permanent. Always show a confirmation dialog before calling this action in the frontend.
