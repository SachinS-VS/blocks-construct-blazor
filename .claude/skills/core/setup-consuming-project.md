# Setup Consuming Project

How to configure a new project to use Blocks AI Skills remotely.

**Never clone or copy the blocks-ai-skills repository into the consuming project.**
All skills are fetched on demand via WebFetch from the raw GitHub URL.

---

## When to Run This

Run this once when setting up a new project that will use Blocks AI Skills.

Trigger phrases:
> "set up this project to use blocks-ai-skills"
> "configure claude for this project"
> "initialise blocks skills in this project"

---

## What to Create

Generate two files in the consuming project root:

1. `CLAUDE.md` — instructs Claude to always fetch skills remotely
2. `PROJECT.md` — project context file (empty template, populated as features are built)

---

## Step 1 — Generate CLAUDE.md

Create `CLAUDE.md` in the consuming project root with this exact content:

```markdown
# Blocks AI Skills

At the start of every session, fetch and follow the skills from the remote repository:

**Repository:** https://github.com/rezwanx/blocks-ai-skills

## Startup Procedure

1. Fetch `https://raw.githubusercontent.com/rezwanx/blocks-ai-skills/main/CLAUDE.md` and follow its instructions exactly.
2. Do NOT enumerate or list available skills to the user. Wait for the user to describe what they want to build.
3. When the user makes a request, fetch `https://raw.githubusercontent.com/rezwanx/blocks-ai-skills/main/skills/core/decision.md` to route the request to the correct skill file.
4. Fetch only the specific skill files needed for that request — do not fetch all skill files upfront.

## Rules

- Skills are internal routing knowledge — never present them as a menu or list to the user.
- ALWAYS use WebFetch to read skill files — never skip this step.
- Fetch skill files on demand per request, not all at startup.
- All file paths in the fetched CLAUDE.md are relative to `https://raw.githubusercontent.com/rezwanx/blocks-ai-skills/main/` — resolve them with WebFetch.
- Cache awareness: WebFetch has a 15-minute cache, so re-fetching within a session is cheap.
```

---

## Step 2 — Generate PROJECT.md

Create an empty `PROJECT.md` from the template in `skills/core/context.md`. Leave all values blank — they will be filled in as features are built.

---

## Step 3 — Confirm

Tell the developer:

> **Setup complete.**
>
> - `CLAUDE.md` created — Claude will fetch all Blocks AI Skills remotely on each session start.
> - `PROJECT.md` created — will be populated as features are built.
>
> Next step: fill in your `.env` values and run `claude` to start building.

---

## What NOT to Do

- Do not copy any files from `blocks-ai-skills` into this project
- Do not add `blocks-ai-skills` as a git submodule
- Do not clone `blocks-ai-skills` into a subfolder of this project
- Do not cache skill files locally — always fetch fresh from GitHub
