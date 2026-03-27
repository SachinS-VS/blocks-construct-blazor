# Flow: upload-file-flow

## Trigger

User wants to upload a file, store a document, or manage file storage.

> "upload a file"
> "store a document"
> "file management"
> "I need to upload images for products"
> "save a PDF to the DMS"
> "upload to S3"

---

## Pre-flight Questions

Before starting, confirm:

1. What is the file type and approximate size?
   - Large files (>5 MB) → use S3 pre-signed URL path
   - Small files (≤5 MB) → direct upload path
2. Should the file be organized in a DMS folder?
   - Yes, with folders → use DMS upload path
   - No folder needed → use local storage or S3
3. What should the access modifier be? `Public` (no auth required to download) or `Private` (requires auth)?
4. Does a destination folder exist, or does it need to be created?
5. Are there tags or metadata to attach to the file?

---

## Flow Steps

### Path A — S3 Pre-signed URL Upload (recommended for files >5 MB or cloud deployments)

#### Step A1 — Generate Pre-signed Upload URL

```
Action: get-presigned-upload-url
Input:
  FileName      = "<original-filename.ext>"
  ContentType   = "<mime-type>"  (e.g. "image/png", "application/pdf")
  FolderPath    = "<optional/folder/path>"
  ProjectKey    = $VITE_PROJECT_SLUG
```

On success → store `data.url` as `$PRESIGNED_URL` and `data.fileId` as `$FILE_ID`.

---

#### Step A2 — Upload File to S3

PUT the file binary directly to the pre-signed URL. Do NOT include `Authorization` or `x-blocks-key` — the URL already contains auth.

```bash
curl --location --request PUT "$PRESIGNED_URL" \
  --header "Content-Type: <mime-type>" \
  --data-binary "@/path/to/file"
```

In TypeScript:

```ts
await fetch(presignedUrl, {
  method: 'PUT',
  headers: { 'Content-Type': file.type },
  body: file,
})
```

Track upload progress via `XMLHttpRequest` `upload.onprogress` for UI progress bar.

---

#### Step A3 — Update File Metadata

After upload completes, register metadata via the Blocks API:

```
Action: update-file-info
Input:
  FileId         = $FILE_ID
  Name           = "<display-name>"
  MetaData       = "<JSON string with custom fields>"
  Tags           = ["<tag1>", "<tag2>"]
  AccessModifier = "Public" | "Private"
  ProjectKey     = $VITE_PROJECT_SLUG
```

On success → upload complete.

---

### Path B — DMS Upload (for folder-organized file management)

#### Step B1 — Ensure Destination Folder Exists (optional)

If the user wants to upload into a specific folder, check or create it first.

```
Action: get-dms-files
Input:
  ParentDirectoryId = null   (check root for existing folders)
  ProjectKey        = $VITE_PROJECT_SLUG
  Page              = 1
  PageSize          = 50
```

**Branch:**
- If target folder exists in response → store its `id` as `$PARENT_FOLDER_ID` → skip to Step B2
- If not found → continue to Step B1a

##### Step B1a — Create Folder

```
Action: create-folder
Input:
  Name              = "<folder-name>"
  ParentDirectoryId = "<parent-id or omit for root>"
  ProjectKey        = $VITE_PROJECT_SLUG
```

Store `data.id` as `$PARENT_FOLDER_ID`. Continue to Step B2.

---

#### Step B2 — Upload File to DMS

```
Action: upload-to-dms
Request: multipart/form-data
Fields:
  File              = <file binary>
  Name              = "<display-name>"
  ParentDirectoryId = $PARENT_FOLDER_ID  (omit for root)
  Tags              = "<comma-separated-tags>"
  AccessModifier    = "Public" | "Private"
  ProjectKey        = $VITE_PROJECT_SLUG
```

Track upload progress via `XMLHttpRequest` `upload.onprogress`.

On success → upload complete. Call `get-dms-files` to refresh folder listing.

---

### Path C — Direct Local Storage Upload (for small files, non-S3 deployments)

#### Step C1 — Upload File to Local Storage

```
Action: upload-to-local-storage
Request: multipart/form-data
Fields:
  File           = <file binary>
  Name           = "<display-name>"
  Tags           = "<comma-separated-tags>"
  AccessModifier = "Public" | "Private"
  ProjectKey     = $VITE_PROJECT_SLUG
```

On success → upload complete. Store returned `data.id` if further operations needed.

---

## Path Selection Guide

| Scenario | Recommended Path |
|----------|-----------------|
| File > 5 MB, cloud/S3 deployment | Path A (pre-signed URL) |
| File needs DMS folder organization | Path B (DMS upload) |
| File < 5 MB, local storage deployment | Path C (local storage) |
| Images, videos for public CDN delivery | Path A with `AccessModifier: Public` |
| Private documents (invoices, contracts) | Path A or B with `AccessModifier: Private` |

---

## Error Handling

| Step | Error | Cause | Action |
|------|-------|-------|--------|
| Step A1 | 400 | Missing FileName or ContentType | Check request body |
| Step A2 | 403 | S3 signature mismatch | URL may have expired — regenerate from Step A1 |
| Step A2 | 403 | Wrong Content-Type header | Match Content-Type exactly to what was sent in Step A1 |
| Step A2 | timeout | File too large for time limit | Split into chunks or increase timeout |
| Step A3 | 404 | FileId not found | Check fileId from Step A1 response |
| Step B1a | 400 | Invalid ParentDirectoryId | Verify parent folder ID from get-dms-files |
| Step B2 | 413 | File too large | Use Path A (pre-signed URL) for large files |
| Step C1 | 413 | File too large | Use Path A (pre-signed URL) instead |
| Any | 401 | Expired token | Run get-token to refresh |
| Any | 403 | Missing cloudadmin role | Add cloudadmin role in Cloud Portal → People |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/data-management/pages/files/files-page.tsx` | File manager page with upload toolbar |
| `modules/data-management/components/file-upload/file-upload.tsx` | Upload component — drag-drop zone + path selection |
| `modules/data-management/components/file-upload/file-drop-zone.tsx` | Drag-and-drop target area |
| `modules/data-management/components/file-upload/upload-progress.tsx` | Progress bar with status indicator |
| `modules/data-management/components/file-browser/file-browser.tsx` | Combined folder tree + file list |
| `modules/data-management/components/file-browser/folder-tree.tsx` | Recursive folder navigation |
| `modules/data-management/components/file-browser/file-list.tsx` | File list with actions (download, delete) |
| `modules/data-management/hooks/use-data-management.tsx` | `useUploadFile`, `useGetDmsFiles`, `useCreateFolder`, `useDeleteFile` hooks |
| `modules/data-management/services/data-management.service.ts` | `getPreSignedUploadUrl()`, `uploadToS3()`, `uploadToDms()`, `getDmsFiles()` methods |
| `modules/data-management/types/data-management.type.ts` | `UploadProgress`, `DmsFile`, `AccessModifier` types |
| `routes/data-management.route.tsx` | `/data-management/files` route |
