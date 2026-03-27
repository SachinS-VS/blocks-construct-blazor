# Flow: setup-knowledge-base

## Trigger

User wants to build a knowledge base and attach it to an AI agent.

> "add documents to my agent"
> "set up a knowledge base"
> "upload files for the agent to search"
> "let the agent answer from my documentation"
> "ingest my FAQ into the agent"

---

## Pre-flight Questions

Before starting, confirm:

1. What type of content will you be ingesting? (files, plain text, Q&A pairs, URLs, or a mix)
2. How many sources do you have? (helps gauge indexing time)
3. Is there an existing KB folder to use, or should a new one be created?
4. Which embedding model should be used? (e.g., `text-embedding-3-small`, `text-embedding-ada-002`)
5. Which agent should this KB be attached to?

---

## Flow Steps

### Step 1 — Create a KB folder

Create a folder to organize the knowledge base content with a chosen embedding model.

```
Action: create-kb-folder
Input:
  name            = descriptive folder name (e.g., "Product Documentation", "Support FAQs")
  embedding_model = user-chosen model (e.g., "text-embedding-3-small")
  project_key     = $VITE_PROJECT_SLUG

Output:
  item_id → kb_folder_id (store this — needed for all ingestion calls)
```

> The embedding model cannot be changed after creation. Choose carefully.

**Branch:**
- Existing folder → skip Step 1 and use the existing `kb_folder_id`
- New folder → store `kb_folder_id` from the response

---

### Step 2 — Ingest content

Choose the ingestion method based on content type. Multiple methods can be combined.

---

#### Branch A — File upload (PDF, DOCX, TXT, MD, CSV)

```
Action: upload-kb-file
Input:
  file         = binary file
  project_key  = $VITE_PROJECT_SLUG
  kb_folder_id = kb_folder_id from Step 1
  chunk_size   = 512 (default) or user preference

Output:
  item_id → kb_id for this document
```

Repeat for each file. Each file gets its own `kb_id`.

**Chunk size guidance:**
- `256` — Short, precise content (FAQs, product specs)
- `512` — General documentation (default)
- `1024` — Long-form content (guides, articles)

---

#### Branch B — Raw text

```
Action: ingest-kb-text
Input:
  content      = text content
  title        = descriptive label
  kb_folder_id = kb_folder_id from Step 1
  project_key  = $VITE_PROJECT_SLUG

Output:
  item_id → kb_id for this text block
```

Repeat for each text block.

---

#### Branch C — Q&A pairs

```
Action: ingest-kb-qa
Input:
  pairs = [
    { question: "...", answer: "..." },
    ...
  ]
  kb_folder_id = kb_folder_id from Step 1
  project_key  = $VITE_PROJECT_SLUG

Output:
  item_id → kb_id for this Q&A batch
```

Best for help center articles, FAQ pages, and structured support content.

---

#### Branch D — URL crawl

```
Action: ingest-kb-link
Input:
  url          = fully qualified URL (must be publicly accessible)
  kb_folder_id = kb_folder_id from Step 1
  project_key  = $VITE_PROJECT_SLUG

Output:
  item_id → kb_id for this URL's content
```

Repeat for each URL. Crawling is asynchronous — allow 15–60 seconds per URL.

---

### Step 3 — Wait for indexing

Indexing is asynchronous. The ingestion endpoints return immediately, but the content is not yet searchable. Poll `test-kb-retrieval` to confirm readiness.

| Content type | Expected indexing time |
|-------------|------------------------|
| Small text/Q&A | 5–15 seconds |
| PDF/DOCX (< 10 pages) | 10–30 seconds |
| Large files (> 50 pages) | 30–120 seconds |
| URLs | 15–60 seconds per page |

**Polling pattern:**

```bash
# Poll every 10 seconds until retrieval returns results
MAX_ATTEMPTS=12
ATTEMPT=0

while [ $ATTEMPT -lt $MAX_ATTEMPTS ]; do
  RESULT=$(curl --silent --location \
    "$VITE_API_BASE_URL/blocksai-api/v1/kb/retrieval-test/$AGENT_ID" \
    --header "Authorization: Bearer $ACCESS_TOKEN" \
    --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
    --header "Content-Type: application/json" \
    --data "{\"query\": \"test\", \"top_k\": 1, \"project_key\": \"$VITE_PROJECT_SLUG\"}")

  COUNT=$(echo $RESULT | jq '.results | length')
  if [ "$COUNT" -gt "0" ]; then
    echo "KB ready — $COUNT results returned"
    break
  fi

  ATTEMPT=$((ATTEMPT + 1))
  echo "Attempt $ATTEMPT/$MAX_ATTEMPTS — not ready yet, waiting 10s..."
  sleep 10
done
```

Frontend: show a spinner with "Indexing your content…" until the first test-kb-retrieval returns results.

---

### Step 4 — Test retrieval quality

Verify the KB content is indexed and retrievable.

```
Action: test-kb-retrieval
Input:
  agent_id    = agent_id of the target agent (must have this KB attached)
  query       = representative test question
  top_k       = 5
  project_key = $VITE_PROJECT_SLUG
```

**Evaluate results:**
- Score ≥ 0.8 — Excellent retrieval
- Score 0.6–0.79 — Acceptable; review content structure
- Score < 0.6 — Poor retrieval; see remediation steps below

**Retrieval remediation:**
- Low scores on file upload → try smaller `chunk_size` (256) or split large files
- Low scores on text → ensure text is well-structured with clear headings
- No results → check indexing is complete; wait longer and retry
- Wrong results → content may be too generic; add more specific context to chunks

---

### Step 5 — Attach KB to agent

Update the agent's AI configuration to include the new KB folder.

```
Action: update-agent-ai-config
Input:
  agent_id    = target agent ID
  kb_ids      = [kb_folder_id, ...existing kb_ids...]
  project_key = $VITE_PROJECT_SLUG
  (keep all other fields the same as current configuration)
```

> Fetch the current agent config with `get-agent` first to avoid overwriting existing `model_id`, `tool_ids`, etc.

---

## Error Handling

| Step | Error | Cause | Action |
|------|-------|-------|--------|
| Step 1 | `409` | Folder name already exists | Use a different name or retrieve the existing folder |
| Step 1 | `400` | Unrecognized `embedding_model` | Check available embedding models with the provider |
| Step 2A | `413` | File too large | Split the file into smaller parts |
| Step 2A | `400` | Unsupported file type | Convert to PDF, DOCX, TXT, MD, or CSV |
| Step 2D | `422` | URL not parseable | URL may be a JavaScript SPA — try exporting content as text or PDF |
| Step 2D | `400` | URL unreachable | Ensure the URL is publicly accessible (no auth, no VPN required) |
| Step 4 | No results | Indexing not complete | Wait 30 seconds and retry |
| Step 4 | Low scores | Poor content structure | Re-ingest with better-structured, more granular content |
| Step 5 | `404` on `model_id` | Agent config has stale model ID | Fetch current agent with `get-agent` before updating |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `src/modules/ai/pages/agent-detail/agent-kb-tab.tsx` | KB management tab — list attached KBs, add/remove |
| `src/modules/ai/components/kb-upload/kb-upload.tsx` | Drag-drop file upload with progress |
| `src/modules/ai/components/kb-upload/kb-upload-progress.tsx` | Per-file upload progress bar |
| `src/modules/ai/components/kb-upload/kb-processing-status.tsx` | Indexing status indicator |
| `src/modules/ai/hooks/use-ai.tsx` | `useCreateKBFolder`, `useUploadKBFile`, `useIngestKBText`, `useIngestKBQA`, `useIngestKBLink`, `useTestKBRetrieval` |
| `src/modules/ai/services/ai.service.ts` | KB API call implementations |
| `src/modules/ai/types/ai.type.ts` | `KBFolder`, `KBTextIngestPayload`, `KBQAIngestPayload`, `KBLinkIngestPayload`, `RetrievalResult` |
