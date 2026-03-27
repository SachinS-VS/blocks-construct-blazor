---
name: lmt
description: "Use this skill for viewing service logs, filtering logs by date, streaming live logs, browsing distributed traces, or analyzing API performance and HTTP status distributions on SELISE Blocks."
user-invocable: false
blocks-version: "1.0.3"
---

# LMT Skill — Logging, Monitoring & Tracing

## Purpose

Provides access to service logs, distributed traces, and performance analytics for SELISE Blocks projects. Used by developers and platform admins to debug, monitor, and analyze running services.

---

## When to Use

Example prompts that should route here:
- "Show me the last 100 logs for the auth service"
- "Stream live logs while I test the login endpoint"
- "Find all traces with errors from the past hour"
- "Show API latency analytics for the data service"

---

## Execution Context

Before executing any action or flow from this skill, read `../core/execution-context.md` for the required supporting files, load order, and cross-domain orchestration rules.

---

## Base Path

All endpoints are prefixed with: `$VITE_API_BASE_URL/lmt/v1`

---

## Intent Mapping

| User wants to... | Use |
|------------------|-----|
| View service logs | `flows/view-logs-flow.md` |
| Search logs by date range | `actions/get-logs-by-date.md` |
| Watch live log output | `actions/stream-live-logs.md` |
| View distributed traces | `flows/view-traces-flow.md` |
| Get a single trace | `actions/get-trace.md` |
| Analyze API performance / latency | `actions/get-operational-analytics.md` |
| View HTTP status code distribution | `actions/get-service-analytics.md` |

---

## Flows

| Flow | File | Description |
|------|------|-------------|
| view-logs-flow | flows/view-logs-flow.md | Filter and paginate service logs |
| view-traces-flow | flows/view-traces-flow.md | Browse traces and drill into performance |

---

## Action Index

### Logs
| Action | File | Description |
|--------|------|-------------|
| get-logs | actions/get-logs.md | Get paginated logs with filtering |
| get-logs-by-date | actions/get-logs-by-date.md | Get logs filtered by date range |
| stream-live-logs | actions/stream-live-logs.md | Stream live log output for a service |

### Traces
| Action | File | Description |
|--------|------|-------------|
| get-traces | actions/get-traces.md | Get paginated traces with filtering |
| get-trace | actions/get-trace.md | Get a single trace by ID |
| get-operational-analytics | actions/get-operational-analytics.md | API-level latency and performance metrics |
| get-service-analytics | actions/get-service-analytics.md | HTTP status code distribution analytics |
