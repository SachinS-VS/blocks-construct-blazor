# Action: upload-to-local-storage

## Purpose

Upload a file directly to local server storage (not S3). Use this for smaller files or when the deployment does not use cloud object storage.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uds/v1/Files/UploadFileToLocalStorage
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/Files/UploadFileToLocalStorage" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --form "File=@/path/to/document.pdf" \
  --form "Name=My Document" \
  --form "AccessModifier=Private" \
  --form "ProjectKey=$VITE_PROJECT_SLUG"
```

---

## Request — multipart/form-data

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| File | binary | yes | File binary |
| Name | string | yes | Display name for the file |
| MetaData | string | no | JSON string with arbitrary key-value metadata |
| Tags | string | no | Comma-separated tags (e.g. `"invoice,2024"`) |
| AccessModifier | string | yes | `Public` or `Private` |
| ConfigurationName | string | no | Storage configuration name (if multiple configs exist) |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

> Do not set `Content-Type` manually — the HTTP client must set it to `multipart/form-data` with the correct boundary.

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "File uploaded successfully",
  "httpStatusCode": 200,
  "data": {
    "id": "file-id-abc123",
    "name": "My Document",
    "size": 51200,
    "contentType": "application/pdf",
    "accessModifier": "Private"
  },
  "errors": {}
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Missing required fields or unsupported file type | Check form fields |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 413 | File too large | Use `get-presigned-upload-url` for large files |

---

## When to Use

| Use case | Recommended action |
|----------|--------------------|
| File < 5 MB, local storage deployment | `upload-to-local-storage` |
| File > 5 MB, or cloud/S3 deployment | `get-presigned-upload-url` → PUT to S3 |
| File going into DMS with folder structure | `upload-to-dms` |
