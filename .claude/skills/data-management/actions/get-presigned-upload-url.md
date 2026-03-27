# Action: get-presigned-upload-url

## Purpose

Generate a pre-signed S3 URL for uploading a file directly to object storage. This is the preferred upload method for large files (>5 MB) as it bypasses the API server and uploads directly to S3.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uds/v1/Files/GetPreSignedUrlForUpload
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/Files/GetPreSignedUrlForUpload" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "FileName": "product-image.png",
    "ContentType": "image/png",
    "FolderPath": "products/images",
    "ProjectKey": "$VITE_PROJECT_SLUG"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| FileName | string | yes | Original file name including extension |
| ContentType | string | yes | MIME type (e.g. `image/png`, `application/pdf`, `video/mp4`) |
| FolderPath | string | no | Target folder path in S3 (e.g. `products/images`) |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Pre-signed URL generated",
  "httpStatusCode": 200,
  "data": {
    "url": "https://s3.amazonaws.com/bucket/products/images/product-image.png?X-Amz-Signature=...",
    "fileId": "file-id-abc123"
  },
  "errors": {}
}
```

Store `data.fileId` — you will need it to call `update-file-info` after upload.

---

## Step 2 — Upload to S3

After receiving the pre-signed URL, PUT the file binary directly to the URL:

```bash
curl --location --request PUT "$PRESIGNED_URL" \
  --header "Content-Type: image/png" \
  --data-binary "@/path/to/product-image.png"
```

In TypeScript:

```ts
await fetch(data.url, {
  method: 'PUT',
  headers: { 'Content-Type': file.type },
  body: file,
})
```

> The pre-signed URL already includes auth — do NOT add `Authorization` or `x-blocks-key` headers to this request.

---

## Step 3 — Update File Metadata

After upload completes, call `update-file-info` with the `fileId` to save name, tags, and access modifier.

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Missing FileName or ContentType | Check request body |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |

---

## Pre-signed URL Expiry

Pre-signed URLs expire (typically within 15 minutes). Start the upload immediately after receiving the URL. Do not store the URL for later use.
