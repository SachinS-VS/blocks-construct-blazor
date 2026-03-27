# Action: test-kb-retrieval

## Purpose

Test retrieval quality for an agent's knowledge base by running a query and seeing the top matching results with relevance scores.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/kb/retrieval-test/{agent_id}
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/kb/retrieval-test/agt_abc123" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "query": "What is the return policy?",
    "top_k": 5,
    "project_key": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `agent_id` | string | yes | ID of the agent whose attached KBs to test against |

---

## Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `query` | string | yes | The test query to run against the knowledge base |
| `top_k` | integer | no | Number of top results to return. Default: `5` |
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "results": [
    {
      "content": "Our return policy allows returns within 30 days of purchase. Items must be in original condition with receipt.",
      "score": 0.94,
      "source": "Return Policy",
      "kb_id": "kb_txt_abc123"
    },
    {
      "content": "Refunds are processed within 5-7 business days after we receive the returned item.",
      "score": 0.81,
      "source": "return_policy.pdf",
      "kb_id": "kb_doc_xyz456"
    }
  ],
  "is_success": true
}
```

| Field | Description |
|-------|-------------|
| `content` | The matched text chunk |
| `score` | Relevance score from 0.0 to 1.0 (higher is more relevant) |
| `source` | Source name (file name or text title) |
| `kb_id` | ID of the KB entry this chunk came from |

---

## On Failure

- `400` — Empty `query` or invalid `top_k` value
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to test this agent's KB
- `404` — Agent not found or agent has no KB attached — attach a KB first via `update-agent-ai-config`
