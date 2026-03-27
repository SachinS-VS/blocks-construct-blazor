# Data Management Contracts

## Common Headers (all authenticated requests)

```
Authorization: Bearer $ACCESS_TOKEN
x-blocks-key: $VITE_X_BLOCKS_KEY
Content-Type: application/json
```

> For multipart file uploads, omit `Content-Type` — let the HTTP client set it with the correct boundary.

---

## Common Response: UdsResponse

UDS uses a slightly different response wrapper from other Blocks APIs:

```json
{
  "isSuccess": true,
  "message": "string",
  "httpStatusCode": 200,
  "data": {},
  "errors": {}
}
```

> `data` contains the actual payload. `errors` is a dictionary (key = field name, value = error message).
> `message` is a human-readable status string.
> Always check `isSuccess` before reading `data`.

---

## Naming Convention

> All request body fields use **PascalCase** — this API is backed by C#/.NET.
> Do not use camelCase in request bodies.

---

## Schema

### DefineSchemaRequest

```json
{
  "CollectionName": "string",
  "SchemaName": "string",
  "ProjectKey": "string",
  "SchemaType": "Collection | SingleObject",
  "Description": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| CollectionName | string | yes | MongoDB collection name — lowercase, no spaces |
| SchemaName | string | yes | Display name for the schema |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |
| SchemaType | enum | yes | `Collection` (multi-document) or `SingleObject` (single config doc) |
| Description | string | no | Optional human-readable description |

### SaveSchemaInfoRequest

Used to set or replace all fields on a schema.

```json
{
  "SchemaId": "string",
  "Fields": [
    {
      "Name": "string",
      "Type": "String | Number | Boolean | Date | ObjectId | Object | Array",
      "IsArray": false,
      "IsRequired": false,
      "Description": "string",
      "DefaultValue": "string"
    }
  ],
  "ProjectKey": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| SchemaId | string | yes | ID returned from define-schema |
| Fields | array | yes | Full field list — replaces existing |
| Fields[].Name | string | yes | Field name — camelCase recommended |
| Fields[].Type | enum | yes | See field types below |
| Fields[].IsArray | boolean | no | If true, field holds an array of the given type |
| Fields[].IsRequired | boolean | no | Whether the field is required |
| Fields[].Description | string | no | Field description |
| Fields[].DefaultValue | string | no | Default value as a string |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

### SaveSchemaFieldsRequest

Used to add or update a single field without replacing all fields.

```json
{
  "SchemaId": "string",
  "FieldName": "string",
  "FieldType": "String | Number | Boolean | Date | ObjectId | Object | Array",
  "IsArray": false,
  "IsRequired": false,
  "ProjectKey": "string"
}
```

### Field Type Enum

| Value | Description |
|-------|-------------|
| `String` | Text value |
| `Number` | Integer or float |
| `Boolean` | true / false |
| `Date` | ISO 8601 date-time |
| `ObjectId` | MongoDB ObjectId reference |
| `Object` | Nested sub-document |
| `Array` | Array of values (combine with IsArray: true) |

### GetSchemasQueryParams

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| page | integer | no | Default: 1 |
| pageSize | integer | no | Default: 20 |
| search | string | no | Search by schema name |
| projectKey | string | yes | `$VITE_PROJECT_SLUG` |

### GetSchemasAggregationQueryParams

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |
| PageNo | integer | no | Default: 1 |
| PageSize | integer | no | Default: 20 |
| Keyword | string | no | General search keyword |
| SchemaName | string | no | Filter by schema name |
| CollectionName | string | no | Filter by collection name |
| SchemaType | enum | no | `Entity` or `Dto` |
| SortBy | string | no | Field name to sort by |
| SortDescending | boolean | no | Sort direction — `true` for descending |

### GetSchemaCollectionsQueryParams

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| projectKey | string | yes | `$VITE_PROJECT_SLUG` |

### GetSchemaByCollectionQueryParams

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| schemaName | string | yes | Collection name (e.g. `products`) |
| projectKey | string | yes | `$VITE_PROJECT_SLUG` |

### SchemaType Enum

| Value | Description |
|-------|-------------|
| `Entity` | Standard entity schema backed by a MongoDB collection |
| `Dto` | Data transfer object schema (no backing collection) |

### GetUnadaptedChangesQueryParams

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| projectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## DataSource

### AddDataSourceRequest

```json
{
  "ItemId": "string",
  "ConnectionString": "string",
  "DatabaseName": "string",
  "ProjectKey": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| ItemId | string | yes | Unique identifier — use a UUID or project slug |
| ConnectionString | string | yes | MongoDB connection string (e.g. `mongodb+srv://...`) |
| DatabaseName | string | yes | MongoDB database name |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

### UpdateDataSourceRequest

```json
{
  "ItemId": "string",
  "ConnectionString": "string",
  "DatabaseName": "string",
  "ProjectKey": "string",
  "IsActive": true
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| ItemId | string | yes | ID of existing connection |
| ConnectionString | string | yes | Updated MongoDB connection string |
| DatabaseName | string | yes | Updated database name |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |
| IsActive | boolean | yes | Set to `false` to disable the connection |

---

## DataAccess

### ChangeSecurityRequest

```json
{
  "SchemaName": "string",
  "SecurityType": "Public | Private | RoleBased",
  "ProjectKey": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| SchemaName | string | yes | The schema's `SchemaName` (not ID) |
| SecurityType | enum | yes | `Public`: open access, `Private`: authenticated only, `RoleBased`: policy-driven |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

### CreateAccessPolicyRequest

```json
{
  "SchemaName": "string",
  "PolicyName": "string",
  "AllowedRoles": ["string"],
  "Operations": ["Read", "Create", "Update", "Delete"],
  "ProjectKey": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| SchemaName | string | yes | The schema name the policy applies to |
| PolicyName | string | yes | Unique name for this policy |
| AllowedRoles | array | yes | Role slugs that this policy grants access to |
| Operations | array | yes | Permitted operations: `Read`, `Create`, `Update`, `Delete` |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

### UpdateAccessPolicyRequest

Same shape as `CreateAccessPolicyRequest`. The `PolicyName` identifies which policy to update.

### Operation Enum

| Value | Description |
|-------|-------------|
| `Read` | Can query / read documents |
| `Create` | Can insert new documents |
| `Update` | Can modify existing documents |
| `Delete` | Can remove documents |

---

## DataValidation

### CreateValidationRequest

```json
{
  "ProjectKey": "string",
  "SchemaId": "string",
  "FieldName": "string",
  "Validations": [
    {
      "Type": "Required | MinLength | MaxLength | Regex | Min | Max | Email | Unique",
      "Value": "string",
      "ErrorMessage": "string"
    }
  ]
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |
| SchemaId | string | yes | ID of the schema containing the field |
| FieldName | string | yes | Exact field name as defined in the schema |
| Validations | array | yes | One or more validation rules |
| Validations[].Type | enum | yes | See validation type enum below |
| Validations[].Value | string | no | The constraint value (e.g. `"8"` for MinLength) |
| Validations[].ErrorMessage | string | yes | Message shown when validation fails |

### Validation Type Enum

| Value | Description | Value field |
|-------|-------------|-------------|
| `Required` | Field must be present and non-empty | not needed |
| `MinLength` | Minimum string length | integer as string |
| `MaxLength` | Maximum string length | integer as string |
| `Regex` | Must match regex pattern | regex string |
| `Min` | Minimum numeric value | number as string |
| `Max` | Maximum numeric value | number as string |
| `Email` | Must be valid email format | not needed |
| `Unique` | Must be unique across collection | not needed |

### GetValidationsQueryParams

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| page | integer | no | Default: 1 |
| pageSize | integer | no | Default: 20 |
| projectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## Files

### GetPreSignedUrlRequest

```json
{
  "FileName": "string",
  "ContentType": "string",
  "FolderPath": "string",
  "ProjectKey": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| FileName | string | yes | Original file name including extension |
| ContentType | string | yes | MIME type (e.g. `image/png`, `application/pdf`) |
| FolderPath | string | no | Target folder path in S3 |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

### GetPreSignedUrlResponse

```json
{
  "isSuccess": true,
  "message": "string",
  "httpStatusCode": 200,
  "data": {
    "url": "https://s3.amazonaws.com/...",
    "fileId": "string"
  }
}
```

> Upload directly to `data.url` via HTTP PUT with the file binary. After upload, call `update-file-info` with `data.fileId`.

### UploadFileRequest (multipart/form-data)

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| File | binary | yes | File binary data |
| Name | string | yes | Display name for the file |
| MetaData | string | no | JSON string with arbitrary key-value metadata |
| ParentDirectoryId | string | no | ID of parent DMS folder (omit for root) |
| Tags | string | no | Comma-separated tags |
| AccessModifier | string | yes | `Public` or `Private` |
| ConfigurationName | string | no | Storage configuration name |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

### GetDmsFilesRequest

```json
{
  "ParentDirectoryId": "string",
  "ProjectKey": "string",
  "Page": 1,
  "PageSize": 20
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| ParentDirectoryId | string | no | Parent folder ID — omit or set `null` for root |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |
| Page | integer | no | Default: 1 |
| PageSize | integer | no | Default: 20 |

### CreateFolderRequest

```json
{
  "Name": "string",
  "ParentDirectoryId": "string",
  "ProjectKey": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Name | string | yes | Folder display name |
| ParentDirectoryId | string | no | Parent folder ID — omit for root level |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

### DeleteFileRequest

```json
{
  "FileId": "string",
  "ProjectKey": "string"
}
```

### DeleteFolderRequest

```json
{
  "folderId": "string",
  "configurationName": "string",
  "projectKey": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| folderId | string | yes | ID of the folder to delete |
| configurationName | string | no | Optional storage configuration name |
| projectKey | string | yes | `$VITE_PROJECT_SLUG` |

### UpdateFileAdditionalInfoRequest

```json
{
  "FileId": "string",
  "Name": "string",
  "MetaData": "string",
  "Tags": ["string"],
  "AccessModifier": "Public | Private",
  "ProjectKey": "string"
}
```

### GetFilesRequest

```json
{
  "FileIds": ["string"],
  "ProjectKey": "string"
}
```

### GetFilesInfoRequest

```json
{
  "FileIds": ["string"],
  "ProjectKey": "string"
}
```

---

## Configuration

### ReloadConfiguration

No request body or path parameters required. The project is identified from the authentication context (Bearer token + x-blocks-key header).

---

## DataManage

### GetMockData

No path parameters required. The project is identified from the authentication context (Bearer token + x-blocks-key header).

### DeleteMockDataRequest

```json
{
  "ProjectKey": "string",
  "SchemaName": "string"
}
```

---

## Access Modifier Enum

| Value | Description |
|-------|-------------|
| `Public` | Accessible without authentication |
| `Private` | Requires authentication |

---

## Security Type Enum

| Value | Description |
|-------|-------------|
| `Public` | Anyone can read/write the schema |
| `Private` | Only authenticated users can access |
| `RoleBased` | Access controlled by access policies |

---

## GraphQL Queries

All GraphQL requests go to: `POST $VITE_API_BASE_URL/uds/v1/$VITE_PROJECT_SLUG/graphql`

Headers are the same as all authenticated requests (Bearer token + x-blocks-key + Content-Type: application/json).

### Query (Read)

```bash
curl -X POST "$VITE_API_BASE_URL/uds/v1/$VITE_PROJECT_SLUG/graphql" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "query { products(page: 1, pageSize: 10) { data { _id name price category } totalCount } }"
  }'
```

Response:
```json
{
  "data": {
    "products": {
      "data": [
        { "_id": "abc123", "name": "Widget", "price": 29.99, "category": "tools" }
      ],
      "totalCount": 1
    }
  }
}
```

### Query with Filters

```json
{
  "query": "query { products(page: 1, pageSize: 10, filter: { category: \"tools\", price_gte: 10 }) { data { _id name price } totalCount } }"
}
```

Filter operators: `_eq`, `_ne`, `_gt`, `_gte`, `_lt`, `_lte`, `_in`, `_nin`, `_regex`

### Mutation (Insert)

```json
{
  "query": "mutation { createProducts(input: { name: \"New Widget\", price: 19.99, category: \"tools\" }) { _id name price } }"
}
```

> Mutation name pattern: `create{CollectionName}` for insert, `update{CollectionName}` for update, `delete{CollectionName}` for delete. The collection name is PascalCase and matches the schema's `CollectionName`.

### Mutation (Update)

```json
{
  "query": "mutation { updateProducts(id: \"abc123\", input: { price: 24.99 }) { _id name price } }"
}
```

### Mutation (Delete)

```json
{
  "query": "mutation { deleteProducts(id: \"abc123\") { _id } }"
}
```

### Frontend Usage (TanStack Query)

```typescript
// In generated hooks, use the graphql endpoint via the http client:
const response = await httpClient.post(
  `/uds/v1/${import.meta.env.VITE_PROJECT_SLUG}/graphql`,
  { query: `query { products(page: 1, pageSize: 10) { data { _id name price } totalCount } }` }
);
```
