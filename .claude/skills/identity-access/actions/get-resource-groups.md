# Action: get-resource-groups

## Purpose

List all resource groups available in the project.

---

## Endpoint

```
GET $VITE_API_BASE_URL/idp/v1/Iam/GetResourceGroups
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/idp/v1/Iam/GetResourceGroups" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Request Body

None.

---

## On Success (200)

Returns array of resource group objects. Use these when creating permissions.

---

## On Failure

* 401 — run refresh-token then retry
