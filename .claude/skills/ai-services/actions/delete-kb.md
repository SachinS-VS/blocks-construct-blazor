# Action: delete-kb

## Purpose

Delete a knowledge base entry (document, text block, Q&A set, or link) by its ID.

---

## Endpoint

```
DELETE $VITE_API_BASE_URL/blocksai-api/v1/kb/delete/{kb_id}
```

---

## curl

```bash
curl --location --request DELETE \
  "$VITE_API_BASE_URL/blocksai-api/v1/kb/delete/kb_doc_abc123" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `kb_id` | string | yes | ID of the knowledge base entry to delete (in the URL path) |

> The `kb_id` is the `item_id` returned by `upload-kb-file`, `ingest-kb-text`, `ingest-kb-qa`, or `ingest-kb-link`.

---

## On Success (200)

```json
{
  "is_success": true,
  "detail": "Knowledge base entry deleted successfully",
  "item_id": "kb_doc_abc123",
  "error": {}
}
```

> Deleting a KB entry removes its vectors from the index. Any agent that had this KB attached will no longer retrieve this content in future conversations.

---

## On Failure

- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to delete KB entries in this project
- `404` — KB entry not found — verify the `kb_id` in the URL path
