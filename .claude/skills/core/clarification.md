# Clarification Guide

Defines when to ask, what to ask, and how to ask — before starting any task and during execution.

---

## When to Ask Before Starting

Ask before generating any code or executing any API if **any** of the following are true:

| Situation | Why it blocks execution |
|-----------|------------------------|
| The request spans multiple domains | Must determine execution order and scope |
| The feature name or module is not mentioned | Needed for folder/file naming — cannot invent this |
| The request is ambiguous between a flow and a single action | Determines whether to follow a multi-step flow or call one API |
| The target page/component is unclear | New page vs addition to existing changes the output entirely |
| A required env variable is missing or empty | Cannot execute backend calls without it |
| The request says "build X" with no detail | X must be specific enough to select a flow |

**Do not ask** if:
- The request maps cleanly to a flow or action in `decision.md`
- The pre-flight questions in the matched flow already cover what you need
- The answer can be reasonably inferred from context (e.g., "add a login page" → `identity-access`, `login-flow`)

---

## How to Ask

### Ask one focused question at a time

Never dump a list of 8 questions at the user. Identify the single most important unknown that blocks execution and ask only that.

```
// ❌ wrong — overwhelming
Before I start, I need to know:
1. Feature name?
2. New page or existing?
3. Which login methods?
4. MFA required?
5. CAPTCHA enabled?
6. Self-registration or admin-created?
7. Which roles?
8. Organizations?

// ✅ correct — one question, clear reason
What should this module be called? I need this for folder and file naming before I start.
```

Once the first answer is received, ask the next if still needed.

### Explain why you need it

A bare question with no context feels like an interrogation. One sentence of context helps the developer answer correctly the first time.

```
// ❌ just a question
Is MFA required?

// ✅ question with context
Is MFA required for this login flow? If yes, I need to know whether it's email OTP, authenticator app (TOTP), or both — this determines which branch of the flow to follow.
```

### Give options when the answer set is bounded

If there are only 2–4 valid answers, list them rather than asking an open question.

```
Which login method should be supported?
- Email and password only
- Social login (Google, GitHub, etc.)
- OIDC / enterprise SSO
- Multiple (specify which)
```

---

## Mid-Flow Ambiguity

If you hit an ambiguous decision **during** execution of a flow:

1. **Stop at that step** — do not guess and proceed
2. State what you have done so far
3. Ask the specific question that blocks the next step
4. Resume from where you stopped once answered

```
// ✅ correct mid-flow pause
I've created the KB folder and uploaded the files. Before attaching to the agent I need to confirm:
Which agent should this knowledge base be attached to? I need the agent ID or name.
```

Do not restart the entire flow — resume from the blocked step.

---

## When the Request Is Too Vague to Start

If the request is so broad that no flow or action can be selected (e.g., "build a dashboard", "set up the app"), do not attempt to infer the full scope. Ask:

```
That covers a lot of ground. What's the first specific feature you want to build?

For example:
- Login page
- User management table
- AI chat interface
- Data schema for [entity name]
```

This focuses the scope to something executable without asking the developer to write a spec.

---

## When the Developer Says "Just Do It" or "Figure It Out"

If the developer explicitly tells you to proceed without answering, do the following:

1. State the assumption you are making
2. Proceed based on that assumption
3. Flag it at the end so it can be corrected

```
Proceeding with the assumption that MFA is not required. If MFA is needed, let me know and I'll add the OTP branch.
```

Never silently assume. Always surface the assumption so it can be verified.

---

## Domain-Specific Ambiguities to Always Clarify

These are non-obvious decisions that have significant impact on the generated output. Always ask these before proceeding with the relevant feature.

### Identity & Access
- Which login methods are enabled? (email/password, social, OIDC — determines login-flow branches)
- Is MFA required? Which type(s)? (determines OTP vs TOTP vs both)
- Is CAPTCHA enabled? Which provider? (reCaptcha vs hCaptcha — different component implementations)
- Self-registration or admin-created users? (determines registration flow vs onboarding flow)
- Which roles exist in this project? (needed for set-roles calls)

### Data Management
- Is this a new schema or modifying an existing one? (new → define-schema-flow; existing → migrate-schema-flow — wrong choice is destructive)
- What fields are needed? (required before any schema creation — cannot generate field definitions without this)

### AI Services
- Which embedding model should be used for the knowledge base? (cannot be changed after creation)
- Is there an existing agent to attach the KB/tools to, or should one be created?
- Should the chat use SSE streaming or standard request/response?

### Communication
- Is this email, in-app notification, or both?
- Should a template be used, or is this a one-off send?

### Localization
- Which languages are configured in this project? (needed before creating keys)
- Should auto-translation be triggered after saving keys?

---

## Reference in Execution Order

This file is checked:
- **Before Step 3** in `core/decision.md` (pre-flight questions) — use this file's rules to decide which pre-flight questions are actually needed
- **At any point during flow execution** where the next step has more than one valid branch and the correct branch cannot be determined from context
