# Action: delete-key

## Purpose

Delete a translation key and all its translations by ID.

---

## Endpoint

```
DELETE $VITE_API_BASE_URL/uilm/v1/Key/Delete?itemId=<KEY_ID>&projectKey=$VITE_X_BLOCKS_KEY
```

---

## curl

```bash
curl --location --request DELETE \
  "$VITE_API_BASE_URL/uilm/v1/Key/Delete?itemId=<KEY_ID>&projectKey=$VITE_X_BLOCKS_KEY" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| itemId | string | yes | ID of the key to delete |
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |

---

## On Success (200)

```json
{
  "success": true,
  "errorMessage": null,
  "validationErrors": []
}
```

---

## On Failure

* 401 — run refresh-token then retry
* 404 — key not found
