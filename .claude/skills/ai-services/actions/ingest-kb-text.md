# Action: ingest-kb-text

## Purpose

Ingest raw text content directly into a knowledge base folder for indexing and retrieval.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/kb/text
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/kb/text" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "content": "Our return policy allows returns within 30 days of purchase. Items must be in original condition with receipt.",
    "title": "Return Policy",
    "kb_folder_id": "kbf_abc123",
    "project_key": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `content` | string | yes | The raw text content to ingest |
| `title` | string | yes | Title or label for this content block |
| `kb_folder_id` | string | yes | Target KB folder ID |
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "is_success": true,
  "detail": "Text content ingested successfully",
  "item_id": "kb_txt_abc123",
  "error": {}
}
```

The `item_id` is the KB entry ID. Use it to delete this specific content block later with `delete-kb`.

---

## On Failure

- `400` — Empty `content` or `title`, or missing required fields
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to write to this KB folder
- `404` — KB folder not found — verify `kb_folder_id` and `project_key`
