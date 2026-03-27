# Action: test-tool-action

## Purpose

Test a specific action on a tool by sending a sample request and verifying the response before attaching the tool to an agent.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/tools/{tool_id}/test-action/{action_id}
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/tools/tool_abc123/test-action/action_get_weather" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "parameters": {
      "city": "New York",
      "units": "metric"
    }
  }'
```

---

## Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `tool_id` | string | yes | ID of the tool (in the URL path) |
| `action_id` | string | yes | ID or name of the specific action to test (in the URL path) |

---

## Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `parameters` | object | no | Key-value map of parameter values to send with the test request |

---

## On Success (200)

```json
{
  "is_success": true,
  "detail": "Tool action test completed",
  "response": {
    "status_code": 200,
    "body": {
      "temperature": 22,
      "condition": "Partly cloudy",
      "city": "New York"
    }
  },
  "error": {}
}
```

| Field | Description |
|-------|-------------|
| `response.status_code` | HTTP status code returned by the external API |
| `response.body` | Parsed response body from the external API |

---

## On Failure

- `400` — Missing required parameters for the action
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to test tools
- `404` — Tool or action not found — verify `tool_id` and `action_id`
- `502` — External API returned an error — check the tool's `base_url` and `auth_value`
