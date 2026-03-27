# <Domain Name> — Contracts

## Request / Response Types

Define TypeScript interfaces for all API interactions in this domain.

```typescript
// Example:
export interface CreateItemRequest {
  name: string;
  // ...
}

export interface CreateItemResponse {
  id: string;
  // ...
}
```

## Notes

- Match field names exactly as the API returns them
- Use `camelCase` unless the API uses `snake_case` (e.g., ai-services)
