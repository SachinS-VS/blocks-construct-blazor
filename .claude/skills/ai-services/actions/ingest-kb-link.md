# Action: ingest-kb-link

## Purpose

Crawl a URL and index its content into a knowledge base folder.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/kb/link
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/kb/link" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "url": "https://docs.example.com/api-reference",
    "kb_folder_id": "kbf_abc123",
    "project_key": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `url` | string | yes | Fully qualified URL to crawl and index (must be publicly accessible) |
| `kb_folder_id` | string | yes | Target KB folder ID |
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "is_success": true,
  "detail": "URL crawled and indexing started",
  "item_id": "kb_link_abc123",
  "error": {}
}
```

> Crawling and indexing is asynchronous. Complex pages may take 15–60 seconds. The `item_id` is the KB entry ID. Use `test-kb-retrieval` to verify the content is available.

---

## On Failure

- `400` — Invalid or malformed URL, or URL is not reachable
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to write to this KB folder, or URL is blocked
- `404` — KB folder not found — verify `kb_folder_id` and `project_key`
- `422` — URL content could not be parsed or extracted (e.g., JavaScript-only SPA with no server-rendered content)
