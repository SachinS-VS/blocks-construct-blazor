# Action: import-uilm

## Purpose

Import a JSON translation file into a module for a specific language. The file must be a flat key-value JSON object.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uilm/v1/Key/UilmImport
```

Content-Type: `multipart/form-data`

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uilm/v1/Key/UilmImport" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --form "file=@/path/to/en.json" \
  --form "projectKey=$VITE_X_BLOCKS_KEY" \
  --form "moduleId=<MODULE_ID>" \
  --form "languageCode=en"
```

---

## Form Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| file | binary | yes | Flat key-value JSON file |
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |
| moduleId | string | yes | Target module ID |
| languageCode | string | yes | Language the file is for (e.g. "en") |

### Expected File Format

```json
{
  "login.title": "Welcome Back",
  "login.button.submit": "Sign In",
  "login.placeholder.email": "Enter your email"
}
```

---

## On Success (200)

```json
{
  "success": true,
  "errorMessage": null,
  "validationErrors": []
}
```

After success, call `get-keys` to verify the imported translations.

---

## On Failure

* 400 — invalid file format, invalid moduleId, or languageCode not configured
* 401 — run refresh-token then retry
