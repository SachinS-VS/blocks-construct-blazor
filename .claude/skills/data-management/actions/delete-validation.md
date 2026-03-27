# Action: delete-validation

## Purpose

Delete a validation rule set by its ID. This removes all validation rules associated with the given ID.

---

## Endpoint

```
DELETE $VITE_API_BASE_URL/uds/v1/data-validations
```

---

## curl

```bash
curl --location --request DELETE "$VITE_API_BASE_URL/uds/v1/data-validations?validationId=$VALIDATION_ID&projectKey=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| validationId | string | yes | Validation ID to delete |
| projectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Validation deleted successfully",
  "httpStatusCode": 200,
  "data": null,
  "errors": {}
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 404 | Validation not found | Verify ID from get-validations or get-schema-validations |
