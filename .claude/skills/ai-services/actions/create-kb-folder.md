# Action: create-kb-folder

## Purpose

Create a knowledge base folder with an embedding model configuration to organize and index content.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/kb/folder/create
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/kb/folder/create" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "name": "Product Documentation",
    "embedding_model": "text-embedding-3-small",
    "project_key": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | yes | Display name for the KB folder |
| `embedding_model` | string | yes | Embedding model to use for vectorizing content (e.g., `text-embedding-3-small`, `text-embedding-ada-002`) |
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "is_success": true,
  "detail": "Knowledge base folder created successfully",
  "item_id": "kbf_abc123",
  "error": {}
}
```

The `item_id` is the `kb_folder_id`. Use it in all subsequent KB ingestion calls (`upload-kb-file`, `ingest-kb-text`, `ingest-kb-qa`, `ingest-kb-link`).

> The embedding model cannot be changed after the folder is created. All content in the folder will use the same embedding model for consistency.

---

## On Failure

- `400` — Missing required fields or unrecognized `embedding_model`
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to create KB folders in this project
- `404` — Project not found — verify `VITE_PROJECT_SLUG`
- `409` — A KB folder with the same name already exists in this project
