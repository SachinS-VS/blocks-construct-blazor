# Action: delete-conversation

## Purpose

Permanently delete a chat session and its message history.

---

## Endpoint

```
DELETE $VITE_API_BASE_URL/blocksai-api/v1/conversation/llm-sessions/{session_id}
```

---

## curl

```bash
curl --location --request DELETE \
  "$VITE_API_BASE_URL/blocksai-api/v1/conversation/llm-sessions/sess_xyz789" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `session_id` | string | yes | ID of the session to delete (in the URL path) |

---

## On Success (200)

```json
{
  "is_success": true,
  "detail": "Conversation session deleted successfully",
  "item_id": "sess_xyz789",
  "error": {}
}
```

> This operation is irreversible. All message history for this session is permanently removed.

---

## On Failure

- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to delete this conversation session
- `404` — Session not found — verify the `session_id` in the URL path
