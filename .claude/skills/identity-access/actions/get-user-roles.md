# Action: get-user-roles

## Purpose

Get all roles assigned to a specific user.

---

## Endpoint

```
GET $VITE_API_BASE_URL/idp/v1/Iam/GetUserRoles
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/idp/v1/Iam/GetUserRoles?userId=USER_ID" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Param | Type | Required |
|-------|------|----------|
| userId | string | yes |

---

## On Success (200)

Returns list of roles assigned to the user.

---

## On Failure

* 401 — run refresh-token then retry
