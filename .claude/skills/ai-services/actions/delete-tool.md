# Action: delete-tool

## Purpose

Permanently delete a tool from the project.

---

## Endpoint

```
DELETE $VITE_API_BASE_URL/blocksai-api/v1/tools/{tool_id}
```

---

## curl

```bash
curl --location --request DELETE \
  "$VITE_API_BASE_URL/blocksai-api/v1/tools/tool_abc123" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `tool_id` | string | yes | ID of the tool to delete (in the URL path) |

---

## On Success (200)

```json
{
  "is_success": true,
  "detail": "Tool deleted successfully",
  "item_id": "tool_abc123",
  "error": {}
}
```

> If any agents have this tool attached, it will be automatically detached from them when deleted.

---

## On Failure

- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to delete tools in this project
- `404` — Tool not found — verify the `tool_id` in the URL path
