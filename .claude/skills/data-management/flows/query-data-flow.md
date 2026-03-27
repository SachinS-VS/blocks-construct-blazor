# Flow: query-data-flow

## Trigger

User wants to read or write data from a schema that has already been defined.

> "query data from my Product schema"
> "insert a record into my collection"
> "fetch all orders from the database"
> "update a record in the User collection"
> "delete a document from my schema"
> "how do I read data after setting up my schema"

---

## Pre-flight Questions

1. Has a schema been defined and `reload-configuration` called? (Data is only accessible after reload.)
2. Which schema / collection are you querying? (e.g. `Product`, `Order`)
3. Is this a read (query) or a write (mutation)?
4. Is the schema security type `Public`, `Private`, or `RoleBased`? (determines auth requirements)

---

## How UDS Data Access Works

After `reload-configuration`, SELISE Blocks exposes a **GraphQL endpoint** that reflects your defined schemas. All CRUD operations on your data go through this endpoint — NOT through any REST action in this skill.

```
POST $VITE_API_BASE_URL/uds/v1/$VITE_PROJECT_SLUG/gateway
```

Headers required:
```
Authorization: Bearer $ACCESS_TOKEN
x-blocks-key: $VITE_X_BLOCKS_KEY
Content-Type: application/json
```

> **IMPORTANT:** The project slug is part of the URL **path** — e.g. `/uds/v1/$VITE_PROJECT_SLUG/gateway`. Do NOT pass it as a query parameter (`?projectKey=...`). The `x-blocks-key` header is also required on every request.

The GraphQL schema is auto-generated from your schema definitions. Each collection gets:
- A query to list records (with filter, sort, pagination)
- A query to get a single record by ID
- A mutation to create a record
- A mutation to update a record
- A mutation to delete a record

---

## Flow Steps

### Step 1 — Confirm Schema Is Live

Check that the schema exists and configuration has been reloaded.

```
Action: get-schema
Input:  id = schemaId, ProjectKey = $VITE_PROJECT_SLUG
```

If the schema exists → proceed. If not → run `define-schema-flow` first.

If you have pending unadapted changes:
```
Action: reload-configuration
Input:  (no parameters — project identified from auth context)
```

---

### Step 2 — Query Data (Read)

Use the GraphQL endpoint directly. The query name follows the pattern `get{SchemaName}s` (plural) for lists and `get{SchemaName}` for single records.

#### List records

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/$VITE_PROJECT_SLUG/gateway" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "query": "query { getProducts(page: 1, pageSize: 20) { items { _id name price createdAt } totalCount } }"
  }'
```

#### Get single record by ID

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/$VITE_PROJECT_SLUG/gateway" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "query": "query { getProduct(id: \"RECORD_ID\") { _id name price description } }"
  }'
```

#### Filter records

```bash
--data '{
  "query": "query { getProducts(filter: { name: \"Widget\" }, sort: { field: \"createdAt\", order: \"DESC\" }) { items { _id name price } totalCount } }"
}'
```

---

### Step 3 — Create Data (Mutation)

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/$VITE_PROJECT_SLUG/gateway" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "query": "mutation { createProduct(input: { name: \"Widget\", price: 9.99, description: \"A great widget\" }) { _id name price } }"
  }'
```

---

### Step 4 — Update Data (Mutation)

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/$VITE_PROJECT_SLUG/gateway" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "query": "mutation { updateProduct(id: \"RECORD_ID\", input: { price: 12.99 }) { _id name price } }"
  }'
```

---

### Step 5 — Delete Data (Mutation)

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/$VITE_PROJECT_SLUG/gateway" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "query": "mutation { deleteProduct(id: \"RECORD_ID\") { success } }"
  }'
```

---

## GraphQL Naming Conventions

The auto-generated GraphQL API follows these naming patterns based on your `SchemaName`:

| Operation | GraphQL name | Example (SchemaName = `Product`) |
|-----------|-------------|----------------------------------|
| List records | `get{Name}s` | `getProducts` |
| Get by ID | `get{Name}` | `getProduct(id: "...")` |
| Create | `create{Name}` | `createProduct(input: {...})` |
| Update | `update{Name}` | `updateProduct(id: "...", input: {...})` |
| Delete | `delete{Name}` | `deleteProduct(id: "...")` |

Every record has an auto-generated `_id` field (MongoDB ObjectId).

---

## Frontend Pattern

Use a GraphQL client (Apollo Client or TanStack Query with fetch) to query the UDS endpoint:

```typescript
// src/lib/gateway.ts
import { ApolloClient, InMemoryCache, createHttpLink } from '@apollo/client'
import { setContext } from '@apollo/client/link/context'

// Project slug is part of the URL path — x-blocks-key header is also required
const httpLink = createHttpLink({
  uri: `${import.meta.env.VITE_API_BASE_URL}/uds/v1/${import.meta.env.VITE_PROJECT_SLUG}/gateway`,
})

const authLink = setContext((_, { headers }) => {
  const accessToken = useAuthStore.getState().accessToken
  return {
    headers: {
      ...headers,
      authorization: accessToken ? `Bearer ${accessToken}` : '',
      'x-blocks-key': import.meta.env.VITE_X_BLOCKS_KEY,
    },
  }
})

export const apolloClient = new ApolloClient({
  link: authLink.concat(httpLink),
  cache: new InMemoryCache(),
})
```

```typescript
// Example query hook
const GET_PRODUCTS = gql`
  query GetProducts($page: Int, $pageSize: Int) {
    getProducts(page: $page, pageSize: $pageSize) {
      items { _id name price }
      totalCount
    }
  }
`
export const useGetProducts = (page = 1) =>
  useQuery(GET_PRODUCTS, { variables: { page, pageSize: 20 } })
```

---

## Security Behaviour at Query Time

| Schema security | Who can query/mutate |
|----------------|----------------------|
| `Public` | Anyone — no token required for reads |
| `Private` | Any authenticated user (valid Bearer token) |
| `RoleBased` | Only roles listed in access policies for the requested operation |

For `RoleBased`: if the authenticated user's role does not have a policy that allows the operation, the GraphQL query will return a permission error.

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| `Cannot query field 'getProducts'` | reload-configuration not called after schema change | Run `reload-configuration` |
| `Null response / empty schema` | Schema not yet defined | Run `define-schema-flow` first |
| `401 Unauthorized` | Token expired | Refresh token and retry |
| `403 Forbidden` | Role not in access policy | Add role to policy via `create-access-policy` |
| GraphQL `errors` array in response | Field doesn't exist or type mismatch | Check schema field names with `get-schema` |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `src/lib/gateway.ts` | Apollo Client setup with auth link |
| `modules/data-management/hooks/use-{schema-name}.tsx` | Generated CRUD hooks per schema |
| `modules/data-management/pages/{schema-name}/list-page.tsx` | Paginated list with filter |
| `modules/data-management/pages/{schema-name}/detail-page.tsx` | Single record view/edit |
| `modules/data-management/components/{schema-name}-form/` | Create/update form with Zod validation |
