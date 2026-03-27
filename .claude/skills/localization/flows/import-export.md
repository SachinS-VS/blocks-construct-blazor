# Flow: import-export

## Trigger

User wants to import an existing JSON translation file into the system, or export/download compiled translation files.

> "import my existing translation JSON"
> "upload en.json to the auth module"
> "export all translations as a zip"
> "download the compiled French translation file"

---

## Pre-flight Questions

**For Import:**
1. Which module should the file be imported into?
2. Which language does the file contain?
3. Do you have the JSON file ready? (must be flat key-value format)

**For Export/Download:**
1. Which modules do you want to export?
2. Do you need one specific language file (use `get-uilm-file`) or all languages for selected modules (use `export-uilm`)?

---

## Flow Steps — Import

### Step 1 — Confirm Module and Language

Call `get-modules` and `get-languages` to confirm the target module and language exist.

```
Action: get-modules
Action: get-languages
```

If the module or language doesn't exist, run the `language-setup` flow first.

---

### Step 2 — Upload File

Call `import-uilm` with the JSON file as multipart form data.

```
Action: import-uilm
Input:
  file         = <.json file>
  projectKey   = $VITE_X_BLOCKS_KEY
  moduleId     = <chosen moduleId>
  languageCode = <language code, e.g. "en">
```

The file must be a flat JSON object: `{ "key.name": "translated value" }`.

---

### Step 3 — Verify Import

Call `get-keys` to confirm keys were imported successfully.

```
Action: get-keys
Input:
  projectKey = $VITE_X_BLOCKS_KEY
  moduleId   = <chosen moduleId>
  pageNumber = 1
  pageSize   = 20
```

---

## Flow Steps — Download Single Language File

### Step 1 — Regenerate File

Call `generate-uilm-file` to rebuild the compiled JSON for the requested language and module.

```
Action: generate-uilm-file
Input:
  projectKey   = $VITE_X_BLOCKS_KEY
  moduleId     = <chosen moduleId>
  languageCode = <language code, e.g. "fr">
```

---

### Step 2 — Download File

Call `get-uilm-file` to retrieve the compiled JSON.

```
Action: get-uilm-file
Input:
  language   = <language code, e.g. "fr">
  moduleId   = <chosen moduleId>
  projectKey = $VITE_X_BLOCKS_KEY
```

The response is the raw flat JSON — trigger a browser download using `URL.createObjectURL`.

---

## Flow Steps — Export All Modules

### Step 1 — Trigger Export

Call `export-uilm` with the module IDs to export.

```
Action: export-uilm
Input:
  projectKey = $VITE_X_BLOCKS_KEY
  moduleIds  = ["<MODULE_ID_1>", "<MODULE_ID_2>"]
```

---

### Step 2 — Get Download Link

Call `get-exported-files` to retrieve the download URL for the generated export.

```
Action: get-exported-files
Input:
  projectKey = $VITE_X_BLOCKS_KEY
  pageNumber = 1
  pageSize   = 10
```

Present the most recent entry's `downloadUrl` to the user.

---

## Error Handling

| Step | Error | Cause | Action |
|------|-------|-------|--------|
| Import Step 2 | 400 | Invalid file format | Inform user the file must be flat key-value JSON |
| Import Step 2 | 400 | Module or language not found | Run `language-setup` flow first |
| Download Step 1 | 400 | Invalid moduleId or languageCode | Check that module and language exist via `get-modules` / `get-languages` |
| Download Step 2 | 404 | No compiled file | Ensure `generate-uilm-file` succeeded before calling `get-uilm-file` |
| Export Step 1 | 400 | Empty moduleIds array | Ask user to select at least one module |
| Any | 401 | Token expired | Run refresh-token then retry the failed step |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/localization/components/import-export/import-panel.tsx` | File upload panel with module/language selectors and drag-and-drop |
| `modules/localization/components/import-export/export-panel.tsx` | Module multi-select and export trigger with download history |
| `modules/localization/pages/keys/keys-page.tsx` | Hosts the import/export panels alongside the key list |
| `modules/localization/hooks/use-localization.tsx` | `useImportUilm`, `useExportUilm`, `useGenerateUilmFile`, `useGetUilmFile`, `useGetExportedFiles` hooks |
| `modules/localization/services/localization.service.ts` | `importUilm()`, `exportUilm()`, `generateUilmFile()`, `getUilmFile()`, `getExportedFiles()` |
| `modules/localization/types/localization.type.ts` | `ExportUilmPayload`, `ExportedFile`, `GenerationHistory` types |
