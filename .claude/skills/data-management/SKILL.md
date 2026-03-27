---
name: data-management
description: "Use this skill for defining data schemas, querying/mutating data via GraphQL, uploading files (S3/DMS), managing access policies, setting field validations, migrating schemas, or configuring data sources on SELISE Blocks."
user-invocable: false
blocks-version: "1.0.3"
---

# Data Management Skill

## Purpose

Handles all data schema definitions, database connections, file storage, data access control, and validation for SELISE Blocks via the UDS v1 API.

Must run get-token (identity-access) before any action in a session.

---

## When to Use

Example prompts that should route here:
- "Define a schema for a products collection with name, price, and category fields"
- "Query all orders via GraphQL with pagination"
- "Upload a profile image to S3 and store the URL"
- "Set up role-based access policies on the invoices schema"
- "Add required validation to the email field"

---

## Execution Context

Before executing any action or flow from this skill, read `../core/execution-context.md` for the required supporting files, load order, and cross-domain orchestration rules.

---

## Intent Mapping

Use this table to route user requests. Check `flows/` first — if a flow covers the request, use it. For single-action requests, go directly to the action.

| User wants to... | Use |
|------------------|-----|
| Define a data schema / create a collection | `flows/define-schema-flow.md` |
| Query or mutate data in a schema / read records / insert records | `flows/query-data-flow.md` |
| Modify an existing schema / add a field / change a field type | `flows/migrate-schema-flow.md` |
| Set up database connection / connect a database | `flows/setup-data-source-flow.md` |
| Upload a file / store a document | `flows/upload-file-flow.md` |
| Manage DMS files and folders | `actions/get-dms-files.md` |
| Set up access control / security on data / restrict schema access / add RBAC to data | `flows/configure-access-policy-flow.md` |
| Add validation rules to fields / validate form inputs server-side | `actions/create-validation.md` |
| Reload GraphQL after schema changes | `actions/reload-configuration.md` |
| Get list of schemas | `actions/get-schemas.md` |
| Get schemas with access level summary | `actions/get-schemas-aggregation.md` |
| List all Entity-type schema collections | `actions/get-schema-collections.md` |
| Get schema details by collection name | `actions/get-schema-by-collection.md` |
| Get file metadata | `actions/get-files-info.md` |
| Get a specific schema | `actions/get-schema.md` |
| Delete a schema | `actions/delete-schema.md` |
| Update a schema definition | `actions/update-schema.md` |
| Add or update individual fields on a schema | `actions/save-schema-fields.md` |
| Get pending schema changes | `actions/get-unadapted-changes.md` |
| Get database connection for a project | `actions/get-data-source.md` |
| Update an existing database connection | `actions/update-data-source.md` |
| Change security type on a schema | `actions/change-security.md` |
| Create a data access policy | `actions/create-access-policy.md` |
| Update a data access policy | `actions/update-access-policy.md` |
| Delete a data access policy | `actions/delete-access-policy.md` |
| Get access policies for a schema | `actions/get-access-policies.md` |
| List all validation rules | `actions/get-validations.md` |
| Update validation rules | `actions/update-validation.md` |
| Get validation for a field | `actions/get-field-validation.md` |
| Get all validations for a schema | `actions/get-schema-validations.md` |
| Delete a validation | `actions/delete-validation.md` |
| Download a file | `actions/get-file.md` |
| Download multiple files | `actions/get-files.md` |
| Generate pre-signed upload URL (S3) | `actions/get-presigned-upload-url.md` |
| Delete a file | `actions/delete-file.md` |
| Upload file to local storage | `actions/upload-to-local-storage.md` |
| Update file metadata | `actions/update-file-info.md` |
| Upload file to DMS | `actions/upload-to-dms.md` |
| Create a DMS folder | `actions/create-folder.md` |
| Delete a DMS folder | `actions/delete-folder.md` |
| Get test / mock data | `actions/get-mock-data.md` |
| Delete mock data | `actions/delete-mock-data.md` |

---

## Flows

| Flow | File | Description |
|------|------|-------------|
| define-schema-flow | flows/define-schema-flow.md | Create a schema, add fields, set validation, configure security, reload GraphQL |
| query-data-flow | flows/query-data-flow.md | Query and mutate data via GraphQL after schema setup |
| migrate-schema-flow | flows/migrate-schema-flow.md | Add/rename/change fields on an existing schema safely |
| setup-data-source-flow | flows/setup-data-source-flow.md | Connect a MongoDB database and reload configuration |
| upload-file-flow | flows/upload-file-flow.md | Upload via S3 pre-signed URL or direct DMS upload |
| configure-access-policy-flow | flows/configure-access-policy-flow.md | Set security type and create role-based access policies |

---

## Base Path

All endpoints are prefixed with: `$VITE_API_BASE_URL/uds/v1`

---

## Action Index

### Schema

| Action | File | Description |
|--------|------|-------------|
| get-schemas | actions/get-schemas.md | List schemas with pagination and search |
| get-schema | actions/get-schema.md | Get a single schema by ID |
| delete-schema | actions/delete-schema.md | Delete a schema by ID |
| define-schema | actions/define-schema.md | Create a new schema (collection or single object) |
| update-schema | actions/update-schema.md | Update an existing schema definition |
| save-schema-info | actions/save-schema-info.md | Save field definitions for a schema |
| update-schema-info | actions/update-schema-info.md | Update field definitions for a schema |
| save-schema-fields | actions/save-schema-fields.md | Add or update a single field on a schema |
| get-unadapted-changes | actions/get-unadapted-changes.md | Get pending schema changes not yet applied |
| get-schemas-aggregation | actions/get-schemas-aggregation.md | List schemas with access level aggregation summary |
| get-schema-collections | actions/get-schema-collections.md | List all Entity-type schema collections |
| get-schema-by-collection | actions/get-schema-by-collection.md | Get schema details by collection name |

### DataSource

| Action | File | Description |
|--------|------|-------------|
| get-data-source | actions/get-data-source.md | Get the database connection for a project |
| add-data-source | actions/add-data-source.md | Add a new database connection |
| update-data-source | actions/update-data-source.md | Update an existing database connection |

### DataAccess

| Action | File | Description |
|--------|------|-------------|
| change-security | actions/change-security.md | Set schema-level security type (Public/Private/RoleBased) |
| create-access-policy | actions/create-access-policy.md | Create a data access policy for a schema |
| update-access-policy | actions/update-access-policy.md | Update an existing data access policy |
| delete-access-policy | actions/delete-access-policy.md | Remove a data access policy |
| get-access-policies | actions/get-access-policies.md | Get all access policies for a schema |

### DataValidation

| Action | File | Description |
|--------|------|-------------|
| get-validations | actions/get-validations.md | List all validation rules with pagination |
| create-validation | actions/create-validation.md | Create validation rules for a field |
| update-validation | actions/update-validation.md | Update validation rules |
| get-validation | actions/get-validation.md | Get a single validation rule by ID |
| delete-validation | actions/delete-validation.md | Delete a validation rule |
| get-schema-validations | actions/get-schema-validations.md | Get all validations for a schema |
| get-field-validation | actions/get-field-validation.md | Get validation rules for a specific field |

### Files

| Action | File | Description |
|--------|------|-------------|
| get-file | actions/get-file.md | Download a single file by ID |
| get-files | actions/get-files.md | Download multiple files |
| get-files-info | actions/get-files-info.md | Get metadata for multiple files |
| get-presigned-upload-url | actions/get-presigned-upload-url.md | Generate a pre-signed S3 URL for upload |
| delete-file | actions/delete-file.md | Delete a file |
| upload-to-local-storage | actions/upload-to-local-storage.md | Upload a file directly to local storage |
| update-file-info | actions/update-file-info.md | Update file metadata |
| get-dms-files | actions/get-dms-files.md | List files and folders in DMS |
| upload-to-dms | actions/upload-to-dms.md | Upload a file to the Document Management System |
| create-folder | actions/create-folder.md | Create a folder in DMS |
| delete-folder | actions/delete-folder.md | Delete a folder from DMS |

### Configuration

| Action | File | Description |
|--------|------|-------------|
| reload-configuration | actions/reload-configuration.md | Reload GraphQL schema after schema changes |

### DataManage

| Action | File | Description |
|--------|------|-------------|
| get-mock-data | actions/get-mock-data.md | Get test / mock data for a project |
| delete-mock-data | actions/delete-mock-data.md | Delete mock data for a project |
