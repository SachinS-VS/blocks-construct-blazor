---
name: communication
description: "Use this skill for sending emails (template-based or ad-hoc), managing in-app notifications (send, read, mark-read), and creating/managing email templates on SELISE Blocks."
user-invocable: false
blocks-version: "1.0.3"
---

# Communication Skill

## Purpose

Handles all email sending, in-app notifications, and email template management for SELISE Blocks via the Communication v1 API.

Covers three sub-domains: Mail (send and retrieve emails), Notifier (push in-app notifications), and Template (create and manage email templates).

---

## When to Use

Example prompts that should route here:
- "Send a welcome email to new users using a template"
- "Build a notification bell with unread count"
- "Create an HTML email template for password resets"
- "Show me all sent emails in the mailbox"

---

## Execution Context

Before executing any action or flow from this skill, read `../core/execution-context.md` for the required supporting files, load order, and cross-domain orchestration rules.

---

## Intent Mapping

Use this table to route user requests. Check `flows/` first — if a flow covers the request, use it. For single-action requests, go directly to the action.

| User wants to... | Use |
|------------------|-----|
| Send email to a user | `flows/send-email-flow.md` → `actions/send-email-with-template.md` |
| Send email to any address | `flows/send-email-flow.md` → `actions/send-email-to-any.md` |
| Send notification to users | `flows/notification-flow.md` → `actions/send-notification.md` |
| View notifications | `flows/notification-flow.md` → `actions/get-notifications.md` |
| Mark notifications as read | `flows/notification-flow.md` → `actions/mark-notification-read.md` or `actions/mark-all-notifications-read.md` |
| Create or manage email templates | `flows/manage-templates-flow.md` → `actions/save-template.md` |
| List email templates | `flows/manage-templates-flow.md` → `actions/get-templates.md` |
| Get a specific email from mailbox | `actions/get-mailbox-mail.md` |
| List emails in mailbox | `actions/get-mailbox-mails.md` |
| Get unread notification count | `actions/get-unread-notifications.md` |
| Get a specific template | `actions/get-template.md` |
| Clone a template | `actions/clone-template.md` |
| Delete a template | `actions/delete-template.md` |

---

## Flows

| Flow | File | Description |
|------|------|-------------|
| send-email-flow | flows/send-email-flow.md | Send email via template or ad-hoc to any address |
| notification-flow | flows/notification-flow.md | Send, display, and mark in-app notifications as read |
| manage-templates-flow | flows/manage-templates-flow.md | Create, update, preview, clone, and delete email templates |

---

## Base Path

All endpoints are prefixed with: `$VITE_API_BASE_URL/communication/v1`

---

## Action Index

### Mail

| Action | File | Description |
|--------|------|-------------|
| send-email-to-any | actions/send-email-to-any.md | Send email to any address (ad-hoc, no template required) |
| send-email-with-template | actions/send-email-with-template.md | Send email to a registered user using a saved template |
| get-mailbox-mails | actions/get-mailbox-mails.md | Get paginated list of sent/received emails |
| get-mailbox-mail | actions/get-mailbox-mail.md | Get a specific email by ID |

### Notifier

| Action | File | Description |
|--------|------|-------------|
| send-notification | actions/send-notification.md | Send in-app notification to users by userId, role, or subscriptionFilter |
| get-unread-notifications | actions/get-unread-notifications.md | Get unread notifications for the current user by subscriptionFilter |
| mark-all-notifications-read | actions/mark-all-notifications-read.md | Mark all notifications as read for a subscriptionFilter |
| mark-notification-read | actions/mark-notification-read.md | Mark a single notification as read |
| get-notifications | actions/get-notifications.md | Get all notifications with pagination |

### Template

| Action | File | Description |
|--------|------|-------------|
| save-template | actions/save-template.md | Create or update an email template |
| get-template | actions/get-template.md | Get a single template by ID |
| get-templates | actions/get-templates.md | List all templates with search and sort |
| clone-template | actions/clone-template.md | Clone an existing template under a new name |
| delete-template | actions/delete-template.md | Delete a template by ID |
