# Action: get-validations

## Purpose

List all validation rules across a project with pagination.

---

## Endpoint

```
GET $VITE_API_BASE_URL/uds/v1/data-validations
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/data-validations?page=1&pageSize=20&projectKey=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| page | integer | no | Default: 1 |
| pageSize | integer | no | Default: 20 |
| projectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Success",
  "httpStatusCode": 200,
  "data": {
    "items": [
      {
        "id": "validation-id-1",
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
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 20
  },
  "errors": {}
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 404 | Wrong projectKey or API URL | Verify `$VITE_PROJECT_SLUG` |
