# Action: upload-kb-file

## Purpose

Upload a file (PDF, DOCX, TXT, MD, CSV) to a knowledge base folder for indexing and retrieval.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/kb/file
```

> This endpoint uses `multipart/form-data` — do NOT set `Content-Type: application/json`.

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/kb/file" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --form "file=@/path/to/document.pdf" \
  --form "project_key=$VITE_PROJECT_SLUG" \
  --form "kb_folder_id=kbf_abc123" \
  --form "chunk_size=512"
```

---

## Request Fields (multipart/form-data)

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `file` | binary | yes | File to upload. Supported: PDF, DOCX, TXT, MD, CSV |
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |
| `kb_folder_id` | string | no | Target KB folder ID. If omitted, placed in default folder |
| `chunk_size` | integer | no | Token chunk size for indexing. Default: `512` |

---

## On Success (200)

```json
{
  "is_success": true,
  "detail": "File uploaded and indexing started",
  "item_id": "kb_doc_abc123",
  "error": {}
}
```

> Indexing is asynchronous and can take 10–30 seconds depending on file size. The `item_id` is the KB entry ID for this document. Poll `test-kb-retrieval` to verify the content is searchable.

---

## On Failure

- `400` — Unsupported file type or file exceeds size limit
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to upload to this KB folder
- `404` — KB folder not found — verify `kb_folder_id` and `project_key`
- `413` — File too large — reduce file size or split into smaller files
