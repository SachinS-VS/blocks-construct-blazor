# Communication Contracts

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
  "isSuccess": true,
  "errors": {
    "fieldName": "error message"
  }
}
```

> `errors` is a **dictionary** (key = field name, value = error message), not an array.
> When `isSuccess` is `false`, inspect `errors` to identify which field caused the failure.

---

## Mail

### SendMailToAnyRequest

Used for ad-hoc emails where you supply the full subject and body directly — no template required.

```json
{
  "to": ["user@example.com"],
  "cc": ["cc@example.com"],
  "bcc": [],
  "subject": "string",
  "body": "string",
  "purpose": "string",
  "language": "en",
  "attachments": [],
  "projectKey": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| to | string[] | yes | List of recipient email addresses |
| cc | string[] | no | Carbon copy recipients |
| bcc | string[] | no | Blind carbon copy recipients |
| subject | string | yes | Email subject line |
| body | string | yes | Full email body — supports HTML |
| purpose | string | no | Identifies the email category (e.g. `"welcome"`, `"invoice"`) |
| language | string | no | BCP 47 language code, defaults to `"en"` |
| attachments | array | no | File attachment list — leave empty if unused |
| projectKey | string | yes | Project identifier from `$VITE_PROJECT_SLUG` |

---

### SendMailRequest (template-based)

Used when sending to a registered user using a pre-configured email template. The template subject and body are resolved server-side using `bodyDataContext`.

```json
{
  "userId": "string",
  "purpose": "string",
  "language": "en",
  "bodyDataContext": {
    "key": "value"
  },
  "attachments": [],
  "projectKey": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| userId | string | yes | ID of the registered user to send to |
| purpose | string | yes | Must match the `purpose` field on the saved template |
| language | string | no | BCP 47 code — used to select the correct template language variant |
| bodyDataContext | object | no | Key/value pairs injected into template `{{variableName}}` placeholders |
| attachments | array | no | File attachment list |
| projectKey | string | yes | Project identifier |

---

### GetMailBoxMailsResponse

Returned by `GET /Mail/GetMailBoxMails`.

```json
{
  "mails": [
    {
      "itemId": "string",
      "subject": "string",
      "to": ["user@example.com"],
      "from": "sender@example.com",
      "body": "string",
      "purpose": "string",
      "language": "en",
      "sentTime": "2024-01-01T00:00:00Z",
      "isRead": false
    }
  ],
  "totalCount": 100,
  "isSuccess": true,
  "errors": {}
}
```

**Query parameters for `GetMailBoxMails`:**

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| page | integer | yes | 1-based page number |
| pageSize | integer | yes | Records per page |
| projectKey | string | yes | Project identifier |

---

### GetMailBoxMailResponse

Returned by `GET /Mail/GetMailBoxMail`.

```json
{
  "mail": {
    "itemId": "string",
    "subject": "string",
    "to": ["user@example.com"],
    "from": "sender@example.com",
    "body": "string",
    "purpose": "string",
    "language": "en",
    "sentTime": "2024-01-01T00:00:00Z",
    "isRead": true
  },
  "isSuccess": true,
  "errors": {}
}
```

**Query parameters for `GetMailBoxMail`:**

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| itemId | string | yes | ID of the email to retrieve |
| projectKey | string | yes | Project identifier |

---

## Notifier

### NotifyRequest

Used to push an in-app notification to one or more recipients. Recipients can be targeted by user ID, role, or subscription filter — use whichever is appropriate; all three fields can be combined.

```json
{
  "userIds": ["string"],
  "roles": ["string"],
  "subscriptionFilters": ["string"],
  "denormalizedPayload": "string",
  "configuratoinName": "string",
  "projectKey": "string"
}
```

> **API Typo:** `configuratoinName` is intentionally misspelled in the API — this matches the server contract exactly. Do not correct the spelling.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| userIds | string[] | no | Target specific users by their user ID |
| roles | string[] | no | Target all users with the specified roles |
| subscriptionFilters | string[] | no | Target users subscribed to these filter keys |
| denormalizedPayload | string | yes | Notification content — typically a JSON string or plain text message |
| configuratoinName | string | no | References a server-side notification configuration (note API typo) |
| projectKey | string | yes | Project identifier |

---

### GetUnreadNotificationsResponse

Returned by `GET /Notifier/GetUnreadNotificationsBySubscriptionFilter`.

```json
{
  "notifications": [
    {
      "id": "string",
      "payload": "string",
      "denormalizedPayload": "string",
      "createdTime": "2024-01-01T00:00:00Z",
      "isRead": false,
      "subscriptionFilter": "string"
    }
  ],
  "unReadNotificationsCount": 5,
  "isSuccess": true,
  "errors": {}
}
```

**Query parameters:**

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| subscriptionFilter | string | yes | Filter key to scope unread results |
| projectKey | string | yes | Project identifier |

---

### GetNotificationsResponse

Returned by `GET /Notifier/GetNotifications`.

```json
{
  "notifications": [
    {
      "id": "string",
      "payload": "string",
      "denormalizedPayload": "string",
      "createdTime": "2024-01-01T00:00:00Z",
      "isRead": false,
      "subscriptionFilter": "string"
    }
  ],
  "unReadNotificationsCount": 5,
  "totalNotificationsCount": 100,
  "isSuccess": true,
  "errors": {}
}
```

**Query parameters:**

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| page | integer | yes | 1-based page number |
| pageSize | integer | yes | Records per page |
| projectKey | string | yes | Project identifier |

---

### MarkNotificationAsReadRequest

```json
{
  "notificationId": "string",
  "projectKey": "string"
}
```

---

### MarkAllNotificationsAsReadRequest

```json
{
  "subscriptionFilter": "string",
  "projectKey": "string"
}
```

---

## Template

### SaveTemplateRequest

Omit `itemId` to create a new template. Include `itemId` to update an existing one.

```json
{
  "itemId": "string",
  "name": "string",
  "templateSubject": "string",
  "templateBody": "string",
  "mailConfigurationId": "string",
  "language": "en",
  "purpose": "string",
  "projectKey": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| itemId | string | no (create) / yes (update) | Omit for create; include to update |
| name | string | yes | Human-readable template name |
| templateSubject | string | yes | Email subject line — supports `{{variableName}}` placeholders |
| templateBody | string | yes | Full HTML email body — supports `{{variableName}}` and `{{.FieldName}}` placeholders |
| mailConfigurationId | string | no | Reference to a mail server configuration |
| language | string | no | BCP 47 code, defaults to `"en"` |
| purpose | string | yes | Identifier used when sending via `SendMailRequest` |
| projectKey | string | yes | Project identifier |

> **Template variable syntax:** Use `{{variableName}}` for simple keys or `{{.FieldName}}` for object field access. These are resolved at send time using the `bodyDataContext` object in `SendMailRequest`.

---

### GetTemplateResponse

Returned by `GET /Template/Get`.

```json
{
  "template": {
    "itemId": "string",
    "name": "string",
    "templateSubject": "string",
    "templateBody": "string",
    "purpose": "string",
    "language": "en",
    "createdDate": "2024-01-01T00:00:00Z",
    "lastUpdatedDate": "2024-01-01T00:00:00Z"
  },
  "isSuccess": true,
  "errors": {}
}
```

**Query parameters:**

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| itemId | string | yes | ID of the template to retrieve |
| projectKey | string | yes | Project identifier |

---

### GetAllTemplatesResponse

Returned by `GET /Template/Gets`.

```json
{
  "templates": [
    {
      "itemId": "string",
      "name": "string",
      "templateSubject": "string",
      "templateBody": "string",
      "purpose": "string",
      "language": "en",
      "createdDate": "2024-01-01T00:00:00Z",
      "lastUpdatedDate": "2024-01-01T00:00:00Z"
    }
  ],
  "totalCount": 10,
  "isSuccess": true,
  "errors": {}
}
```

**Query parameters:**

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| search | string | no | Search by template name |
| sort | string | no | Field to sort by |
| projectKey | string | yes | Project identifier |

---

### CloneTemplateRequest

```json
{
  "itemId": "string",
  "newName": "string",
  "projectKey": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| itemId | string | yes | ID of the source template to clone |
| newName | string | yes | Name to assign to the cloned template |
| projectKey | string | yes | Project identifier |

---

### DeleteTemplate (query parameters)

`DELETE /Template/Delete` uses query parameters, not a request body.

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| itemId | string | yes | ID of the template to delete |
| projectKey | string | yes | Project identifier |
