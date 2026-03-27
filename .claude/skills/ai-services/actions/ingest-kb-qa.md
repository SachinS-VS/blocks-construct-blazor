# Action: ingest-kb-qa

## Purpose

Ingest structured Q&A pairs into a knowledge base folder for optimized question-answering retrieval.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/kb/qa
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/kb/qa" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "pairs": [
      {
        "question": "What are your business hours?",
        "answer": "We are open Monday to Friday, 9 AM to 6 PM EST."
      },
      {
        "question": "How do I reset my password?",
        "answer": "Go to the login page and click Forgot Password. Enter your email and follow the instructions sent to you."
      }
    ],
    "kb_folder_id": "kbf_abc123",
    "project_key": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `pairs` | object[] | yes | Array of Q&A pair objects |
| `pairs[].question` | string | yes | The question |
| `pairs[].answer` | string | yes | The answer to the question |
| `kb_folder_id` | string | yes | Target KB folder ID |
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "is_success": true,
  "detail": "Q&A pairs ingested successfully",
  "item_id": "kb_qa_abc123",
  "error": {}
}
```

---

## On Failure

- `400` — Empty `pairs` array, missing `question` or `answer` in any pair, or missing required fields
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to write to this KB folder
- `404` — KB folder not found — verify `kb_folder_id` and `project_key`
