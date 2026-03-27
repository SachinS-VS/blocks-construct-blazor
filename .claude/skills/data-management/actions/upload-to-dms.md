# Action: upload-to-dms

## Purpose

Upload a file to the Document Management System (DMS), optionally placing it inside a folder. Use this when you need DMS folder organization for uploaded files.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uds/v1/Files/UploadFile
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/Files/UploadFile" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --form "File=@/path/to/report.pdf" \
  --form "Name=Annual Report 2024" \
  --form "ParentDirectoryId=folder-id-abc123" \
  --form "Tags=report,2024,annual" \
  --form "AccessModifier=Private" \
  --form "ProjectKey=$VITE_PROJECT_SLUG"
```

Upload to root (no folder):

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/Files/UploadFile" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --form "File=@/path/to/banner.jpg" \
  --form "Name=Homepage Banner" \
  --form "AccessModifier=Public" \
  --form "ProjectKey=$VITE_PROJECT_SLUG"
```

---

## Request — multipart/form-data

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| File | binary | yes | File binary |
| Name | string | yes | Display name for the file |
| MetaData | string | no | JSON string with arbitrary key-value metadata |
| ParentDirectoryId | string | no | Folder ID to upload into — omit for root |
| Tags | string | no | Comma-separated tags |
| AccessModifier | string | yes | `Public` or `Private` |
| ConfigurationName | string | no | Storage configuration name |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

> Do not set `Content-Type` manually — let the HTTP client set it to `multipart/form-data` with the correct boundary.

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "File uploaded successfully",
  "httpStatusCode": 200,
  "data": {
    "id": "file-id-xyz789",
    "name": "Annual Report 2024",
    "parentDirectoryId": "folder-id-abc123",
    "size": 1048576,
    "contentType": "application/pdf",
    "accessModifier": "Private",
    "tags": ["report", "2024", "annual"]
  },
  "errors": {}
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Missing required fields or invalid ParentDirectoryId | Check form fields and folder ID |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 404 | ParentDirectoryId folder not found | Verify folder ID from get-dms-files or create with create-folder |
| 413 | File too large | Use `get-presigned-upload-url` for large files |

---

## Next Steps

After upload, call `get-dms-files` with the `ParentDirectoryId` to refresh the folder listing.
