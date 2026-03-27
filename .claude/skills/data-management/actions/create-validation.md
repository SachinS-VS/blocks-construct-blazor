# Action: create-validation

## Purpose

Create one or more validation rules for a specific field on a schema. These rules are enforced when data is written through the UDS API.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uds/v1/data-validations
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/data-validations" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "ProjectKey": "$VITE_PROJECT_SLUG",
    "SchemaId": "$SCHEMA_ID",
    "FieldName": "email",
    "Validations": [
      {
        "Type": "Required",
        "Value": null,
        "ErrorMessage": "Email is required"
      },
      {
        "Type": "Email",
        "Value": null,
        "ErrorMessage": "Must be a valid email address"
      },
      {
        "Type": "Unique",
        "Value": null,
        "ErrorMessage": "This email is already in use"
      }
    ]
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |
| SchemaId | string | yes | Schema ID containing the field |
| FieldName | string | yes | Exact field name as defined in the schema |
| Validations | array | yes | Array of one or more validation rules |
| Validations[].Type | enum | yes | See validation types below |
| Validations[].Value | string | depends | Required for MinLength, MaxLength, Min, Max, Regex — omit for Required, Email, Unique |
| Validations[].ErrorMessage | string | yes | Message shown to user when validation fails |

### Validation Type Reference

| Type | Description | Value field |
|------|-------------|-------------|
| `Required` | Field must be present and non-empty | not needed |
| `MinLength` | Minimum string length | integer (e.g. `"8"`) |
| `MaxLength` | Maximum string length | integer (e.g. `"255"`) |
| `Regex` | Must match regex pattern | pattern (e.g. `"^[A-Z]"`) |
| `Min` | Minimum numeric value | number (e.g. `"0"`) |
| `Max` | Maximum numeric value | number (e.g. `"1000"`) |
| `Email` | Must be valid email format | not needed |
| `Unique` | Must be unique across all documents | not needed |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Validation created successfully",
  "httpStatusCode": 200,
  "data": {
    "id": "validation-id-abc123"
  },
  "errors": {}
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Invalid validation type, missing required fields, or duplicate rule | Check Validations array — ensure Type is valid |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 404 | Schema not found | Verify SchemaId from get-schemas |

---

## Common Examples

**Password field:**
```json
{
  "FieldName": "password",
  "Validations": [
    { "Type": "Required", "ErrorMessage": "Password is required" },
    { "Type": "MinLength", "Value": "8", "ErrorMessage": "Password must be at least 8 characters" },
    { "Type": "MaxLength", "Value": "128", "ErrorMessage": "Password must be under 128 characters" }
  ]
}
```

**Price field:**
```json
{
  "FieldName": "price",
  "Validations": [
    { "Type": "Required", "ErrorMessage": "Price is required" },
    { "Type": "Min", "Value": "0", "ErrorMessage": "Price cannot be negative" },
    { "Type": "Max", "Value": "999999", "ErrorMessage": "Price exceeds maximum" }
  ]
}
```
