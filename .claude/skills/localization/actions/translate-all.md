# Action: translate-all

## Purpose

AI-translate all untranslated keys in a module. The system detects keys missing translations for any configured language and fills them in automatically.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uilm/v1/Key/TranslateAll
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uilm/v1/Key/TranslateAll" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "projectKey": "'$VITE_X_BLOCKS_KEY'",
    "moduleId": "<MODULE_ID>"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |
| moduleId | string | yes | ID of the module to translate |
| targetLanguages | string[] | no | Culture codes to translate into (e.g. `["de", "fr", "es"]`). Omit to translate into ALL configured languages. |

**Example with explicit target languages:**
```bash
--data '{
  "projectKey": "'$VITE_X_BLOCKS_KEY'",
  "moduleId": "<MODULE_ID>",
  "targetLanguages": ["de", "fr", "es"]
}'
```

---

## Behaviour

- Only translates keys that are **missing a translation** for the target language. Keys that already have a value are left untouched.
- Uses the **default language** as the source. Ensure the default language has values set before calling this.
- Translation is **asynchronous** — the endpoint returns immediately but translation may take:
  - < 20 keys: 5–15 seconds
  - 20–100 keys: 15–60 seconds
  - 100+ keys: 1–3 minutes
- After the response, poll `get-keys` until `isPartiallyTranslated` is `false` on all keys, or wait the expected duration then call `generate-uilm-file` to rebuild the localization file.

---

## On Success (200)

```json
{
  "success": true,
  "errorMessage": null,
  "validationErrors": []
}
```

After success, wait for translation to complete then call `generate-uilm-file` to rebuild the JSON localization file.

---

## On Failure

* 400 — invalid `moduleId`, no untranslated keys found, or `targetLanguages` contains an unrecognised culture code
* 401 — run refresh-token then retry
* 500 — AI translation service timeout; retry after 30 seconds
