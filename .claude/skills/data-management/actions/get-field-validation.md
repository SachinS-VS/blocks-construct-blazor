# Action: get-field-validation

## Purpose

Get the validation rules for a specific field on a specific schema. Use this to check what validation is configured before updating or to display existing rules in a UI.

---

## Endpoint

```
GET $VITE_API_BASE_URL/uds/v1/data-validations/by-schema-and-field
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/data-validations/by-schema-and-field?schemaId=$SCHEMA_ID&fieldName=$FIELD_NAME&projectKey=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| schemaId | string | yes | Schema ID from define-schema or get-schemas |
| fieldName | string | yes | Exact field name as defined in the schema (case-sensitive) |
| projectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Success",
  "httpStatusCode": 200,
  "data": {
    "id": "validation-id-abc123",
    "schemaId": "schema-id-1",
    "fieldName": "email",
    "validations": [
      {
        "type": "Required",
        "value": null,
        "errorMessage": "Email is required"
      },
      {
        "type": "Email",
        "value": null,
        "errorMessage": "Must be a valid email address"
      }
    ],
    "projectKey": "my-project"
  },
  "errors": {}
}
```

Returns `data: null` if no validation has been configured for this field yet.

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 404 | Schema or field not found | Verify schemaId and fieldName |
