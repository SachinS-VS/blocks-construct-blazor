# Flow: setup-data-source-flow

## Trigger

User wants to connect a database, set up a data source, or configure MongoDB for their project.

> "connect a database"
> "set up a data source"
> "configure MongoDB"
> "add a database connection"
> "my schemas are failing — no data source found"

---

## Pre-flight Questions

Before starting, confirm:

1. Do you have a MongoDB connection string? (e.g. `mongodb+srv://user:password@cluster.mongodb.net`)
2. What is the database name inside the MongoDB instance?
3. Is there an existing connection already registered? (Check `get-data-source` if unsure)
4. Should the connection be set as active? (default: yes)

---

## Flow Steps

### Step 1 — Check Existing Data Source

Check if a connection already exists to avoid creating a duplicate.

```
Action: get-data-source
Input:  (no parameters — project identified from auth context)
```

**Branch:**
- If 200 with active connection → ask user: update existing or leave as-is?
  - Update → skip to Step 3 (Update Data Source)
  - Leave as-is → skip to Step 4 (Reload Configuration)
- If 404 → no connection registered → continue to Step 2

---

### Step 2 — Add Data Source

```
Action: add-data-source
Input:
  ItemId           = "$VITE_PROJECT_SLUG-db"
  ConnectionString = "<mongodb+srv://...>"
  DatabaseName     = "<database-name>"
  ProjectKey       = $VITE_PROJECT_SLUG
```

On success → continue to Step 4.

---

### Step 3 — Update Data Source (only if updating existing)

```
Action: update-data-source
Input:
  ItemId           = "<existing ItemId from get-data-source>"
  ConnectionString = "<new or same connection string>"
  DatabaseName     = "<new or same database name>"
  ProjectKey       = $VITE_PROJECT_SLUG
  IsActive         = true
```

On success → continue to Step 4.

---

### Step 4 — Reload Configuration

Apply the new database connection by reloading the GraphQL schema.

```
Action: reload-configuration
Input:  (no parameters — project identified from auth context)
```

**Branch:**
- If 200 → data source is connected and active → flow complete
- If 500 → MongoDB connection failed → see error table below

---

## Error Handling

| Step | Error | Cause | Action |
|------|-------|-------|--------|
| Step 1 | 401 | Expired token | Run get-token to refresh |
| Step 2 | 400 duplicate | ItemId already exists | Use a unique ItemId or go to update path |
| Step 2 | 400 | Invalid connection string format | Verify the MongoDB URI format |
| Step 3 | 400 | ItemId not found | Verify ItemId from get-data-source |
| Step 4 | 500 | MongoDB authentication failed | Check username/password in ConnectionString |
| Step 4 | 500 | MongoDB host unreachable | Verify MongoDB cluster URL and network access |
| Step 4 | 500 | Database does not exist | Ensure DatabaseName matches an existing database |
| Any | 401 | Expired token | Run get-token to refresh |
| Any | 403 | Missing cloudadmin role | Add cloudadmin role in Cloud Portal → People |

---

## Connection String Format Reference

| MongoDB type | Example format |
|--------------|----------------|
| MongoDB Atlas | `mongodb+srv://user:password@cluster0.abcde.mongodb.net` |
| Self-hosted | `mongodb://user:password@host:27017` |
| Local dev | `mongodb://localhost:27017` |

---

## Security Note

The `ConnectionString` contains credentials. UDS stores it encrypted. It is never returned in GET responses. Never log or expose the connection string in frontend code.

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/data-management/pages/schemas/schemas-page.tsx` | Can surface "No data source" warning banner |
| `modules/data-management/hooks/use-data-management.tsx` | `useAddDataSource`, `useUpdateDataSource` hooks |
| `modules/data-management/services/data-management.service.ts` | `addDataSource()`, `updateDataSource()` service methods |
| `modules/data-management/types/data-management.type.ts` | `AddDataSourcePayload`, `UpdateDataSourcePayload` types |
