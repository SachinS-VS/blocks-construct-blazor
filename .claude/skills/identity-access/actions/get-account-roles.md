# Action: get-account-roles

## Purpose

Get all roles assigned to the currently authenticated account.

---

## Endpoint

```
GET $VITE_API_BASE_URL/idp/v1/Iam/GetAccountRoles
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/idp/v1/Iam/GetAccountRoles" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Request Body

None.

---

## On Success (200)

Returns list of roles assigned to the current account.

---

## On Failure

* 401 — run refresh-token then retry
