# Action: get-file

## Purpose

Download a single file by its ID. Returns the file binary content.

---

## Endpoint

```
GET $VITE_API_BASE_URL/uds/v1/Files/GetFile
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/Files/GetFile?fileId=$FILE_ID&projectKey=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --output downloaded-file
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| fileId | string | yes | File ID from upload or get-files-info |
| projectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

Returns the raw file binary with appropriate `Content-Type` header. Not a JSON response.

In frontend code, create an object URL and trigger download:

```ts
const blob = await response.blob()
const url = URL.createObjectURL(blob)
const a = document.createElement('a')
a.href = url
a.download = fileName
a.click()
URL.revokeObjectURL(url)
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | File is Private and requester lacks access | Check file access modifier and user permissions |
| 404 | File not found | Verify fileId |
