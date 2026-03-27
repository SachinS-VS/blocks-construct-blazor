# Action: update-validation

## Purpose

Update the validation rules for a field. Replaces the existing validation rules with the new set provided.

---

## Endpoint

```
PUT $VITE_API_BASE_URL/uds/v1/data-validations
```

---

## curl

```bash
curl --location --request PUT "$VITE_API_BASE_URL/uds/v1/data-validations" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "ProjectKey": "$VITE_PROJECT_SLUG",
    "SchemaId": "$SCHEMA_ID",
    "FieldName": "username",
    "Validations": [
      {
        "Type": "Required",
        "Value": null,
        "ErrorMessage": "Username is required"
      },
      {
        "Type": "MinLength",
        "Value": "3",
        "ErrorMessage": "Username must be at least 3 characters"
      },
      {
        "Type": "MaxLength",
        "Value": "50",
        "ErrorMessage": "Username must be under 50 characters"
      },
      {
        "Type": "Regex",
        "Value": "^[a-zA-Z0-9_]+$",
        "ErrorMessage": "Username can only contain letters, numbers, and underscores"
      }
    ]
  }'
```

---

## Request Body

Same shape as `create-validation` (POST). The SchemaId and FieldName together identify which validation to update.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |
| SchemaId | string | yes | Schema ID containing the field |
| FieldName | string | yes | Field name whose validations should be updated |
| Validations | array | yes | Complete updated set of validation rules — replaces existing |
| Validations[].Type | enum | yes | `Required`, `MinLength`, `MaxLength`, `Regex`, `Min`, `Max`, `Email`, `Unique` |
| Validations[].Value | string | depends | Required for length/range/regex types |
| Validations[].ErrorMessage | string | yes | Validation failure message |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Validation updated successfully",
  "httpStatusCode": 200,
  "data": null,
  "errors": {}
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Invalid validation type or missing required fields | Check Validations array |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 404 | Validation not found for this SchemaId + FieldName | Check that validation exists via get-field-validation |
