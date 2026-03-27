# Action: delete-access-policy

## Purpose

Remove a data access policy by its item ID. After deletion, the roles that were granted by this policy lose their access to the schema.

---

## Endpoint

```
DELETE $VITE_API_BASE_URL/uds/v1/data-access/policy/delete
```

---

## curl

```bash
curl --location --request DELETE "$VITE_API_BASE_URL/uds/v1/data-access/policy/delete?itemId=$POLICY_ITEM_ID&projectKey=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| itemId | string | yes | Policy item ID returned from `create-access-policy` or `get-access-policies` |
| projectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Access policy deleted successfully",
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
| 404 | Policy not found | Verify itemId from get-access-policies |

---

## Warning

Deleting a policy immediately removes access for all roles granted by that policy. Verify with `get-access-policies` before deleting if you are unsure which roles are affected.
