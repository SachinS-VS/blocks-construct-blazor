---
name: localization
description: "Use this skill for setting up languages, managing translation keys, AI-powered auto-translation, importing/exporting translation files, or configuring localization modules on SELISE Blocks."
user-invocable: false
blocks-version: "1.0.3"
---

# Localization Skill

## Purpose

Handles all UI localization management for SELISE Blocks via the UILM v1 API. Covers language setup, translation module and key management, AI-assisted translation, file import/export, and webhook configuration.

---

## When to Use

Example prompts that should route here:
- "Set up English and German as project languages"
- "Add translation keys for the login page"
- "Auto-translate all missing German translations using AI"
- "Import a JSON translation file for French"
- "Export all translations for the dashboard module"

---

## Execution Context

Before executing any action or flow from this skill, read `../core/execution-context.md` for the required supporting files, load order, and cross-domain orchestration rules.

---

## Intent Mapping

Use this table to route user requests. Check `flows/` first — if a flow covers the request, use it. For single-action requests, go directly to the action.

| User wants to... | Use |
|------------------|-----|
| Set up languages for a project | `flows/language-setup.md` |
| Add translation keys and translate them | `flows/key-management.md` |
| Import an existing JSON translation file | `flows/import-export.md` |
| Export translations / download compiled JSON | `flows/import-export.md` |
| AI-translate all untranslated keys | `actions/translate-all.md` |
| AI-translate a single key | `actions/translate-key.md` |
| Add a language | `actions/save-language.md` |
| List languages | `actions/get-languages.md` |
| Delete a language | `actions/delete-language.md` |
| Set a default language | `actions/set-default-language.md` |
| Add a module | `actions/save-module.md` |
| List modules | `actions/get-modules.md` |
| Create or update a translation key | `actions/save-key.md` |
| Batch create/update translation keys | `actions/save-keys.md` |
| Search / list translation keys | `actions/get-keys.md` |
| Get keys by name array | `actions/get-keys-by-names.md` |
| Get a single key | `actions/get-key.md` |
| Delete a key | `actions/delete-key.md` |
| Get key edit history | `actions/get-key-timeline.md` |
| Download compiled translation JSON | `actions/get-uilm-file.md` |
| Regenerate compiled translation file | `actions/generate-uilm-file.md` |
| Import a JSON translation file | `actions/import-uilm.md` |
| Export translation modules | `actions/export-uilm.md` |
| List exported translation files | `actions/get-exported-files.md` |
| View file generation history | `actions/get-generation-history.md` |
| Rollback a key to a previous version | `actions/rollback-key.md` |
| Configure a webhook | `actions/save-webhook.md` |

---

## Flows

| Flow | File | Description |
|------|------|-------------|
| language-setup | flows/language-setup.md | Add languages, set default, and create modules for a project |
| key-management | flows/key-management.md | Create translation keys, add translations, AI-translate missing values |
| import-export | flows/import-export.md | Import JSON files into UILM or export/download compiled files |

---

## Base Path

All endpoints are prefixed with: `$VITE_API_BASE_URL/uilm/v1`

---

## Action Index

### Languages
| Action | File | Description |
|--------|------|-------------|
| save-language | actions/save-language.md | Create or update a language |
| get-languages | actions/get-languages.md | List all languages for a project |
| delete-language | actions/delete-language.md | Delete a language |
| set-default-language | actions/set-default-language.md | Set the default language for a project |

### Modules
| Action | File | Description |
|--------|------|-------------|
| save-module | actions/save-module.md | Create or update a translation module |
| get-modules | actions/get-modules.md | List all modules for a project |

### Keys
| Action | File | Description |
|--------|------|-------------|
| save-key | actions/save-key.md | Create or update a single translation key |
| save-keys | actions/save-keys.md | Batch create or update translation keys |
| get-keys | actions/get-keys.md | Get keys with filtering and pagination |
| get-keys-by-names | actions/get-keys-by-names.md | Get keys by key name array |
| get-key | actions/get-key.md | Get a single key by ID |
| delete-key | actions/delete-key.md | Delete a key |
| get-key-timeline | actions/get-key-timeline.md | Get edit history for a key |
| get-uilm-file | actions/get-uilm-file.md | Download compiled translation JSON for a language/module |
| generate-uilm-file | actions/generate-uilm-file.md | Regenerate compiled translation file |
| translate-all | actions/translate-all.md | AI-translate all untranslated keys |
| translate-key | actions/translate-key.md | AI-translate a specific key |
| import-uilm | actions/import-uilm.md | Import a JSON translation file (multipart) |
| export-uilm | actions/export-uilm.md | Export translation modules |
| get-exported-files | actions/get-exported-files.md | List previously exported files |
| get-generation-history | actions/get-generation-history.md | View file generation history |
| rollback-key | actions/rollback-key.md | Rollback a key to a previous version |

### Config
| Action | File | Description |
|--------|------|-------------|
| save-webhook | actions/save-webhook.md | Configure a webhook for localization events |
