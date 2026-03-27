# Flow: migrate-schema-flow

## Trigger

User wants to change an existing schema — rename a field, change a field type, add fields, or remove fields — without losing existing data.

> "rename a field in my schema"
> "change a field type from String to Number"
> "add a new field to an existing schema"
> "remove a field from my collection"
> "update my schema without losing data"

---

## Pre-flight Questions

1. Which schema is being changed? (name and ID)
2. What is the change? (add field / rename field / change type / remove field)
3. Is there existing data in the collection?
4. Is this change breaking? (type change from String → Number with existing string data will cause query errors)

---

## Important Warnings

> **`save-schema-info` is destructive** — it REPLACES all fields on the schema. Do NOT use it to add a single field to an existing schema. Use `save-schema-fields` instead, which adds or updates one field at a time without touching the others.

> **Type changes on fields with existing data are risky.** MongoDB is schema-flexible, but GraphQL will enforce the new type. Existing documents with the old type will fail to deserialize. Always plan for data migration before changing a field type.

---

## Flow Steps

### Step 1 — Read Current Schema State

Before making any changes, capture the current field definitions.

```
Action: get-schema
Input:  id = $SCHEMA_ID, ProjectKey = $VITE_PROJECT_SLUG
```

Store the full field list. This is your rollback reference.

---

### Step 2 — Make the Change

#### Branch A — Add a new field (safe, non-breaking)

```
Action: save-schema-fields
Input:
  SchemaId   = $SCHEMA_ID
  FieldName  = "newFieldName"
  FieldType  = "String" (or appropriate type)
  IsArray    = false
  IsRequired = false
  ProjectKey = $VITE_PROJECT_SLUG
```

Safe to call on a live schema with existing data. Existing documents will return `null` for the new field until populated.

---

#### Branch B — Update a field (rename or change constraints — not type)

```
Action: save-schema-fields
Input:
  SchemaId   = $SCHEMA_ID
  FieldName  = "existingFieldName"
  FieldType  = (keep same type)
  IsRequired = true  ← change here
  ProjectKey = $VITE_PROJECT_SLUG
```

> Do NOT change `FieldType` on a field that has existing data with a different type in MongoDB. This will break reads for any document where the stored type doesn't match.

---

#### Branch C — Remove a field

Fields cannot be deleted via API directly. Workaround:

1. Stop writing to the field in your application
2. Use `save-schema-fields` to set `IsRequired = false` (prevents validation failures)
3. Reload configuration
4. Query all documents and null out the field value via GraphQL mutations
5. Then remove the field from your application's GraphQL queries

There is no hard-delete-field API — removing a field from a MongoDB document does not remove it from the schema definition. Accept that the field stays in the schema as optional/nullable.

---

#### Branch D — Change a field type (breaking — requires data migration)

This is a two-phase operation:

**Phase 1 — Add a new field with the target type:**
```
Action: save-schema-fields
Input:
  SchemaId   = $SCHEMA_ID
  FieldName  = "priceV2"   ← new name for the migrated field
  FieldType  = "Number"
  ProjectKey = $VITE_PROJECT_SLUG
```

**Phase 2 — Migrate data via GraphQL:**
1. Call `reload-configuration`
2. Query all documents using the old field (`price` as String)
3. For each document, run an update mutation setting `priceV2 = parseFloat(price)`
4. Once all documents are migrated, update your app to use `priceV2`
5. The old field (`price`) remains in the schema but can be ignored

---

### Step 3 — Check for Unadapted Changes

After any schema modification:

```
Action: get-unadapted-changes
Input:  projectKey = $VITE_PROJECT_SLUG
```

If changes are listed → must reload before they take effect in GraphQL.

---

### Step 4 — Reload Configuration

```
Action: reload-configuration
Input:  (no parameters — project identified from auth context)
```

This regenerates the GraphQL schema. Takes 2–5 seconds. Any active GraphQL connections should be re-established after reload.

---

### Step 5 — Verify Change

Query the updated schema:

```
Action: get-schema
Input:  id = $SCHEMA_ID, ProjectKey = $VITE_PROJECT_SLUG
```

Confirm the new field appears. Then run a test GraphQL query to confirm the new field is queryable.

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| GraphQL type error after type change | Existing documents have old type stored | Run data migration for all existing documents |
| `save-schema-info` wiped all fields | Used save-schema-info instead of save-schema-fields | Restore field list from Step 1 snapshot using save-schema-info |
| 500 on reload | DB unreachable or schema conflict | Check connection string; check for naming collisions |
| Field still missing after reload | reload-configuration not called | Call reload-configuration and wait 5 seconds |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/data-management/pages/schema-detail/schema-detail-page.tsx` | Add field editor tab |
| `modules/data-management/components/schema-builder/add-field-modal.tsx` | Modal to add one field at a time (calls save-schema-fields) |
| `modules/data-management/hooks/use-data-management.tsx` | `useSaveSchemaFields`, `useGetUnadaptedChanges`, `useReloadConfiguration` hooks |
