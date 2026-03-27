---
name: <domain-name>
description: "<One-line description — be specific and outcome-oriented. Mention the key user intents this skill handles.>"
---

# <Domain Name> Skill

## Purpose

<What this skill handles — 2-3 sentences max.>

## When to Use

Example prompts that should route here:
- "<example prompt 1>"
- "<example prompt 2>"
- "<example prompt 3>"

---

## Intent Mapping

Use this table to route user requests. Check `flows/` first — if a flow covers the request, use it.

| User wants to... | Use |
|------------------|-----|
| <intent> | `flows/<flow>.md` or `actions/<action>.md` |

---

## Base Path

`$VITE_API_BASE_URL/<service>/v1`
