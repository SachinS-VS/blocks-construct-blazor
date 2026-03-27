# Action: update-file-info

## Purpose

Update the metadata for an existing file — change its display name, tags, access modifier, or custom metadata. Typically called after a pre-signed S3 upload to associate metadata with the uploaded file.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uds/v1/Files/updateFileAdditionalInfo
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/Files/updateFileAdditionalInfo" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "FileId": "$FILE_ID",
    "Name": "Product Hero Image",
    "MetaData": "{\"altText\": \"Hero product shot\", \"category\": \"marketing\"}",
    "Tags": ["product", "hero", "marketing"],
    "AccessModifier": "Public",
    "ProjectKey": "$VITE_PROJECT_SLUG"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| FileId | string | yes | File ID from upload or get-presigned-upload-url |
| Name | string | yes | Updated display name |
| MetaData | string | no | JSON string with custom key-value metadata |
| Tags | array | no | Array of tag strings |
| AccessModifier | string | yes | `Public` or `Private` |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "File info updated successfully",
  "httpStatusCode": 200,
  "data": null,
  "errors": {}
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Missing FileId or invalid AccessModifier | Check request body |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 404 | File not found | Verify FileId |

---

## Usage in S3 Upload Flow

This is step 3 of the pre-signed upload flow:

```
1. get-presigned-upload-url  →  receive { url, fileId }
2. PUT file binary to url
3. update-file-info          →  associate name, tags, access modifier with fileId
```
