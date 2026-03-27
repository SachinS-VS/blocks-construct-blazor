# Flow: define-schema-flow

## Trigger

User wants to create a new data schema, define a collection, or set up a data model.

> "create a data schema"
> "define a collection"
> "set up a data model"
> "I need a Product schema with fields"
> "create a users table / collection"

---

## Pre-flight Questions

Before starting, confirm:

1. What is the schema name? (e.g. `Product`, `Order`, `User`)
2. What is the MongoDB collection name? (lowercase, no spaces — e.g. `products`, `orders`, `users`)
3. Is this a `Collection` (multiple documents) or `SingleObject` (one config document)?
4. What fields does the schema need? For each: name, type (`String`, `Number`, `Boolean`, `Date`, `ObjectId`, `Object`, `Array`), required?, is array?
5. Does a database connection already exist for this project? (If unsure, check with `get-data-source`)
6. What security model? `Public` (open), `Private` (authenticated), or `RoleBased` (policy-driven)?
7. If `RoleBased`: which roles need access, and what operations (Read/Create/Update/Delete)?
8. Are there any validation rules? (e.g. required fields, min/max length, email format, unique)

---

## Flow Steps

### Step 1 — Verify Data Source

Check whether a database connection exists.

```
Action: get-data-source
Input:  (no parameters — project identified from auth context)
```

**Branch:**
- If 200 with data → data source exists → skip to Step 2
- If 404 → no data source registered → continue to Step 1a

#### Step 1a — Add Data Source (only if not already set up)

```
Action: add-data-source
Input:
  ItemId           = "<project-slug>-db"
  ConnectionString = "<mongodb+srv connection string>"
  DatabaseName     = "<database name>"
  ProjectKey       = $VITE_PROJECT_SLUG
```

On success → continue to Step 1b.

#### Step 1b — Reload Configuration (after adding data source)

```
Action: reload-configuration
Input:  (no parameters — project identified from auth context)
```

On success → continue to Step 2.

---

### Step 2 — Define Schema

```
Action: define-schema
Input:
  CollectionName = "<collection-name>"
  SchemaName     = "<SchemaName>"
  ProjectKey     = $VITE_PROJECT_SLUG
  SchemaType     = "Collection" | "SingleObject"
  Description    = "<optional description>"
```

On success → store `data.id` as `$SCHEMA_ID`. Continue to Step 3.

---

### Step 3 — Save Schema Fields

> **DESTRUCTIVE WARNING:** `save-schema-info` **replaces all fields** on the schema entirely. Only use it here during initial creation. To add or update a single field on an existing schema later, use `save-schema-fields` instead — it modifies one field without touching the rest.

```
Action: save-schema-info
Input:
  SchemaId   = $SCHEMA_ID
  ProjectKey = $VITE_PROJECT_SLUG
  Fields     = [
    { Name, Type, IsArray, IsRequired, Description, DefaultValue },
    ...
  ]
```

On success → continue to Step 4.

---

### Step 4 — Add Validation Rules (optional)

If the user specified validation rules, call `create-validation` for each field that needs validation.

```
Action: create-validation
Input:
  ProjectKey  = $VITE_PROJECT_SLUG
  SchemaId    = $SCHEMA_ID
  FieldName   = "<field-name>"
  Validations = [
    { Type, Value, ErrorMessage },
    ...
  ]
```

Repeat for each field requiring validation. Then continue to Step 5.

If no validation rules needed → skip to Step 5.

---

### Step 5 — Configure Security

```
Action: change-security
Input:
  SchemaName   = "<SchemaName>"
  SecurityType = "Public" | "Private" | "RoleBased"
  ProjectKey   = $VITE_PROJECT_SLUG
```

**Branch:**
- If SecurityType is `Public` or `Private` → skip to Step 7
- If SecurityType is `RoleBased` → continue to Step 6

---

### Step 6 — Create Access Policies (only if RoleBased)

For each role/operation combination specified, call `create-access-policy`:

```
Action: create-access-policy
Input:
  SchemaName   = "<SchemaName>"
  PolicyName   = "<policy-name>"
  AllowedRoles = ["<role-slug>", ...]
  Operations   = ["Read", "Create", "Update", "Delete"]
  ProjectKey   = $VITE_PROJECT_SLUG
```

Repeat for each distinct policy (e.g. admin full access, viewer read-only).

---

### Step 7 — Reload Configuration

Apply all schema changes to the live GraphQL API.

```
Action: reload-configuration
Input:  (no parameters — project identified from auth context)
```

On success → schema is live and accessible through the GraphQL API.

---

## Error Handling

| Step | Error | Cause | Action |
|------|-------|-------|--------|
| Step 1 | 500 on reload | DB connection failed | Check ConnectionString in add-data-source |
| Step 2 | 400 | Duplicate CollectionName | Choose a different collection name |
| Step 2 | 400 | Missing required fields | Check all four required fields in request |
| Step 3 | 400 | Invalid field Type | Use only: String, Number, Boolean, Date, ObjectId, Object, Array |
| Step 4 | 400 | Invalid validation Type | Check the Validation Type enum in contracts.md |
| Step 5 | 400 | SchemaName not found | Verify SchemaName matches the name set in Step 2 |
| Step 6 | 400 | Duplicate PolicyName | Use unique policy names per schema |
| Step 7 | 500 | MongoDB unreachable | Verify connection string and database name |
| Any | 401 | Expired token | Run get-token to refresh |
| Any | 403 | Missing cloudadmin role | Add cloudadmin role in Cloud Portal → People |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/data-management/pages/schemas/schemas-page.tsx` | Schema list page with "New Schema" button |
| `modules/data-management/components/schema-builder/schema-builder.tsx` | Visual field definition form |
| `modules/data-management/components/schema-builder/field-row.tsx` | Individual field row (name, type, required, array toggles) |
| `modules/data-management/components/schema-builder/field-type-select.tsx` | Field type dropdown |
| `modules/data-management/pages/schema-detail/schema-detail-page.tsx` | Schema detail with tabs: Fields, Validations, Access Control |
| `modules/data-management/hooks/use-data-management.tsx` | `useDefineSchema`, `useSaveSchemaInfo`, `useChangeSecurity`, `useCreateAccessPolicy` hooks |
| `modules/data-management/services/data-management.service.ts` | API calls for schema operations |
| `modules/data-management/types/data-management.type.ts` | `DefineSchemaPayload`, `SchemaField`, `FieldType` types |
| `routes/data-management.route.tsx` | `/data-management/schemas`, `/data-management/schemas/:id` routes |
