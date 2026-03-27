# Localization Contracts

## Common Headers (all authenticated requests)

```
Authorization: Bearer $ACCESS_TOKEN
x-blocks-key: $VITE_X_BLOCKS_KEY
Content-Type: application/json
```

---

## Common Response: BaseResponse

```json
{
  "success": true,
  "errorMessage": null,
  "validationErrors": []
}
```

> `validationErrors` is an **array**, not a dictionary. `success` (not `isSuccess`) indicates outcome.

---

## Languages

### SaveLanguageRequest

```json
{
  "id": "string",
  "name": "string",
  "code": "string",
  "projectKey": "string"
}
```

> Omit `id` to create. Include `id` to update.

### GetLanguagesResponse

```json
{
  "data": [
    {
      "id": "string",
      "name": "string",
      "code": "string",
      "isDefault": false,
      "projectKey": "string"
    }
  ],
  "success": true,
  "errorMessage": null,
  "validationErrors": []
}
```

### DeleteLanguageRequest (query params)

| Param | Type | Required |
|-------|------|----------|
| itemId | string | yes |
| projectKey | string | yes |

### SetDefaultLanguageRequest

```json
{
  "languageId": "string",
  "projectKey": "string"
}
```

---

## Modules

### SaveModuleRequest

```json
{
  "id": "string",
  "name": "string",
  "projectKey": "string"
}
```

> Omit `id` to create. Include `id` to update.

### GetModulesResponse

```json
{
  "data": [
    {
      "id": "string",
      "name": "string",
      "projectKey": "string"
    }
  ],
  "success": true,
  "errorMessage": null,
  "validationErrors": []
}
```

---

## Keys

### SaveKeyRequest

```json
{
  "id": "string",
  "keyName": "string",
  "moduleId": "string",
  "projectKey": "string",
  "translations": [
    {
      "languageCode": "string",
      "value": "string"
    }
  ]
}
```

> Omit `id` to create. Include `id` to update.

### SaveKeysRequest (batch)

```json
{
  "projectKey": "string",
  "moduleId": "string",
  "keys": [
    {
      "keyName": "string",
      "translations": [
        {
          "languageCode": "string",
          "value": "string"
        }
      ]
    }
  ]
}
```

### GetKeysRequest

```json
{
  "projectKey": "string",
  "moduleId": "string",
  "pageNumber": 1,
  "pageSize": 20,
  "filter": {
    "search": "string",
    "languageCode": "string",
    "untranslatedOnly": false
  }
}
```

### GetKeysByKeyNamesRequest

```json
{
  "projectKey": "string",
  "moduleId": "string",
  "keyNames": ["string"]
}
```

### GetKeysResponse

```json
{
  "data": [
    {
      "id": "string",
      "keyName": "string",
      "moduleId": "string",
      "projectKey": "string",
      "translations": [
        {
          "languageCode": "string",
          "value": "string"
        }
      ]
    }
  ],
  "totalCount": 0,
  "success": true,
  "errorMessage": null,
  "validationErrors": []
}
```

### GetKeyRequest (query params)

| Param | Type | Required |
|-------|------|----------|
| itemId | string | yes |
| projectKey | string | yes |

### DeleteKeyRequest (query params)

| Param | Type | Required |
|-------|------|----------|
| itemId | string | yes |
| projectKey | string | yes |

### GetKeyTimelineRequest (query params)

| Param | Type | Required |
|-------|------|----------|
| keyId | string | yes |
| pageNumber | integer | yes |
| pageSize | integer | yes |

### GetKeyTimelineResponse

```json
{
  "data": [
    {
      "id": "string",
      "keyId": "string",
      "languageCode": "string",
      "value": "string",
      "changedAt": "2024-01-01T00:00:00Z",
      "changedBy": "string"
    }
  ],
  "totalCount": 0,
  "success": true,
  "errorMessage": null,
  "validationErrors": []
}
```

### GetUilmFileRequest (query params)

| Param | Type | Required |
|-------|------|----------|
| language | string | yes |
| moduleId | string | yes |
| projectKey | string | yes |

> Returns a compiled JSON object with key-value pairs for the given language and module.

### GenerateUilmFileRequest

```json
{
  "projectKey": "string",
  "moduleId": "string",
  "languageCode": "string"
}
```

### TranslateAllRequest

```json
{
  "projectKey": "string",
  "moduleId": "string"
}
```

### TranslateKeyRequest

```json
{
  "keyId": "string",
  "projectKey": "string",
  "languageCode": "string"
}
```

### UilmImportRequest (multipart/form-data)

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| file | binary | yes | JSON translation file |
| projectKey | string | yes | |
| moduleId | string | yes | |
| languageCode | string | yes | |

### ExportUilmRequest

```json
{
  "projectKey": "string",
  "moduleIds": ["string"]
}
```

### GetExportedFilesRequest (query params)

| Param | Type | Required |
|-------|------|----------|
| projectKey | string | yes |
| pageNumber | integer | yes |
| pageSize | integer | yes |

### GetGenerationHistoryRequest (query params)

| Param | Type | Required |
|-------|------|----------|
| projectKey | string | yes |
| pageNumber | integer | yes |
| pageSize | integer | yes |

### RollbackKeyRequest

```json
{
  "keyId": "string",
  "timelineId": "string",
  "projectKey": "string"
}
```

---

## Config

### SaveWebHookRequest

```json
{
  "url": "string",
  "projectKey": "string",
  "events": ["string"]
}
```
