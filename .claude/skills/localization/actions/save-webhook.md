# Action: save-webhook

## Purpose

Configure a webhook URL for a project to receive notifications on localization events (e.g. key changes, file generation).

---

## Endpoint

```
POST $VITE_API_BASE_URL/uilm/v1/Config/SaveWebHook
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uilm/v1/Config/SaveWebHook" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "url": "https://your-app.com/webhooks/localization",
    "projectKey": "'$VITE_X_BLOCKS_KEY'",
    "events": ["KeySaved", "FileGenerated"]
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| url | string | yes | HTTPS URL to receive webhook POST requests |
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |
| events | array of strings | yes | Events to subscribe to |

### Supported Events

| Event | Trigger |
|-------|---------|
| `KeySaved` | A translation key is created or updated |
| `KeyDeleted` | A translation key is deleted |
| `FileGenerated` | A compiled translation file is regenerated |
| `LanguageSaved` | A language is added or updated |
| `TranslationCompleted` | AI translation finishes |

---

## On Success (200)

```json
{
  "success": true,
  "errorMessage": null,
  "validationErrors": []
}
```

---

## On Failure

* 400 — invalid URL or unsupported event names
* 401 — run refresh-token then retry
