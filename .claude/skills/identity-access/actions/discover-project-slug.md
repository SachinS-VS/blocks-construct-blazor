# Action: discover-project-slug

## Purpose

Automatically discover and set `VITE_PROJECT_SLUG` after authentication, so the user never needs to provide it manually.

---

## When to Run

Immediately after `get-token` succeeds — before any other API calls.

---

## Strategy

Try two methods in order. Stop as soon as one succeeds.

### Method 1 — Decode the JWT access token

The `access_token` returned by `get-token` is a JWT. Decode its payload (base64) and look for a claim that contains the project slug.

```bash
# Extract the payload (second segment) from the JWT
echo "$ACCESS_TOKEN" | cut -d'.' -f2 | base64 -d 2>/dev/null | jq .
```

Look for any of these claim keys (case-insensitive):
- `tenant`
- `tenantId`
- `tenant_id`
- `project`
- `projectSlug`
- `project_slug`
- `TenantSlug`

If a matching claim is found → use its value as `VITE_PROJECT_SLUG`.

### Method 2 — Call GetUserInfo

If the JWT does not contain a recognisable project/tenant claim, call the user-info endpoint:

```bash
curl --location "$VITE_API_BASE_URL/idp/v1/Authentication/GetUserInfo" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

Inspect the response for any field containing the project slug (e.g. `tenant`, `project`, `slug`, or nested under `claims`).

### Fallback — Ask the user

If neither method yields a slug, ask:

> "I couldn't auto-detect your project slug. What is it? (Cloud Portal → Project settings)"

---

## On Success

1. Write the discovered value into `.env` as `VITE_PROJECT_SLUG=<value>`
2. Confirm to the user: _"Auto-detected project slug: `<value>`"_

---

## Error Guidance

| Situation | Action |
|-----------|--------|
| JWT decode fails | Proceed to Method 2 |
| GetUserInfo returns 401 | Token expired — re-run `get-token` first |
| Neither method returns a slug | Fall back to asking the user |
