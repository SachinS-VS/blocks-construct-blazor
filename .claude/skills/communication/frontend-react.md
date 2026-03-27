# Communication Frontend

## Module Location

All communication UI lives under `src/modules/communication/`.

```
src/modules/communication/
├── components/
│   ├── notification-bell/
│   │   ├── notification-bell.tsx       ← icon button with unread badge count
│   │   └── notification-bell.test.tsx
│   └── notification-list/
│       ├── notification-list.tsx       ← dropdown/panel listing notifications
│       ├── notification-item.tsx       ← single notification row
│       └── notification-list.test.tsx
├── pages/
│   ├── mail-compose/
│   │   └── mail-compose-page.tsx       ← compose and send email form
│   ├── mailbox/
│   │   └── mailbox-page.tsx            ← paginated email list view
│   └── templates/
│       ├── templates-page.tsx          ← list all templates with search
│       ├── template-editor-page.tsx    ← create/edit template with HTML preview
│       └── template-clone-dialog.tsx   ← clone template dialog
├── hooks/
│   └── use-communication.tsx           ← all React Query hooks for this domain
├── services/
│   └── communication.service.ts        ← raw API call functions
├── types/
│   └── communication.type.ts           ← TypeScript interfaces and enums
└── index.ts                            ← public exports
```

---

## Stack

Follows `skills/core/frontend-react.md` conventions:

| Layer | Technology |
|-------|-----------|
| Framework | React 19 + TypeScript |
| Build tool | Vite |
| Styling | Tailwind CSS 3.4 |
| Component primitives | Radix UI |
| Component system | shadcn/ui style |
| Icons | Lucide React (`Bell`, `BellDot`, `Mail`, `Send`, `FileText`, `Copy`, `Trash2`) |
| Forms | React Hook Form + Zod |
| State | React Query (TanStack Query) |

---

## Types (`communication.type.ts`)

```ts
export interface Mail {
  itemId: string;
  subject: string;
  to: string[];
  from: string;
  body: string;
  purpose: string;
  language: string;
  sentTime: string;
  isRead: boolean;
}

export interface SendMailToAnyPayload {
  to: string[];
  cc?: string[];
  bcc?: string[];
  subject: string;
  body: string;
  purpose?: string;
  language?: string;
  attachments?: unknown[];
  projectKey: string;
}

export interface SendMailPayload {
  userId: string;
  purpose: string;
  language?: string;
  bodyDataContext?: Record<string, string>;
  attachments?: unknown[];
  projectKey: string;
}

export interface Notification {
  id: string;
  payload: string;
  denormalizedPayload: string;
  createdTime: string;
  isRead: boolean;
  subscriptionFilter: string;
}

export interface NotifyPayload {
  userIds?: string[];
  roles?: string[];
  subscriptionFilters?: string[];
  denormalizedPayload: string;
  configuratoinName?: string; // API typo — keep as-is
  projectKey: string;
}

export interface EmailTemplate {
  itemId: string;
  name: string;
  templateSubject: string;
  templateBody: string;
  purpose: string;
  language: string;
  createdDate: string;
  lastUpdatedDate: string;
}

export interface SaveTemplatePayload {
  itemId?: string;
  name: string;
  templateSubject: string;
  templateBody: string;
  mailConfigurationId?: string;
  language?: string;
  purpose: string;
  projectKey: string;
}

export interface CloneTemplatePayload {
  itemId: string;
  newName: string;
  projectKey: string;
}

export interface GetMailsParams {
  page: number;
  pageSize: number;
  projectKey: string;
}

export interface GetNotificationsParams {
  page: number;
  pageSize: number;
  projectKey: string;
}
```

---

## Service (`communication.service.ts`)

```ts
import { axiosInstance } from '@/lib/axios';

const BASE = '/communication/v1';

export const communicationService = {
  sendMailToAny: (payload: SendMailToAnyPayload) =>
    axiosInstance.post(`${BASE}/Mail/SendToAny`, payload),

  sendMailWithTemplate: (payload: SendMailPayload) =>
    axiosInstance.post(`${BASE}/Mail/Send`, payload),

  getMailboxMails: (params: GetMailsParams) =>
    axiosInstance.get(`${BASE}/Mail/GetMailBoxMails`, { params }),

  getMailboxMail: (itemId: string, projectKey: string) =>
    axiosInstance.get(`${BASE}/Mail/GetMailBoxMail`, { params: { itemId, projectKey } }),

  sendNotification: (payload: NotifyPayload) =>
    axiosInstance.post(`${BASE}/Notifier/Notify`, payload),

  getUnreadNotifications: (subscriptionFilter: string, projectKey: string) =>
    axiosInstance.get(`${BASE}/Notifier/GetUnreadNotificationsBySubscriptionFilter`, {
      params: { subscriptionFilter, projectKey },
    }),

  getNotifications: (params: GetNotificationsParams) =>
    axiosInstance.get(`${BASE}/Notifier/GetNotifications`, { params }),

  markNotificationRead: (payload: { notificationId: string; projectKey: string }) =>
    axiosInstance.post(`${BASE}/Notifier/MarkNotificationAsRead`, payload),

  markAllNotificationsRead: (payload: { subscriptionFilter: string; projectKey: string }) =>
    axiosInstance.post(`${BASE}/Notifier/MarkAllNotificationAsRead`, payload),

  saveTemplate: (payload: SaveTemplatePayload) =>
    axiosInstance.post(`${BASE}/Template/Save`, payload),

  getTemplate: (itemId: string, projectKey: string) =>
    axiosInstance.get(`${BASE}/Template/Get`, { params: { itemId, projectKey } }),

  getTemplates: (params: { search?: string; sort?: string; projectKey: string }) =>
    axiosInstance.get(`${BASE}/Template/Gets`, { params }),

  cloneTemplate: (payload: CloneTemplatePayload) =>
    axiosInstance.post(`${BASE}/Template/Clone`, payload),

  deleteTemplate: (itemId: string, projectKey: string) =>
    axiosInstance.delete(`${BASE}/Template/Delete`, { params: { itemId, projectKey } }),
};
```

---

## Hooks (`use-communication.tsx`)

```tsx
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { communicationService } from '../services/communication.service';

// ── Notifications ──────────────────────────────────────────────────────────

export const useGetUnreadNotifications = (subscriptionFilter: string, projectKey: string) =>
  useQuery({
    queryKey: ['notifications', 'unread', subscriptionFilter],
    queryFn: () => communicationService.getUnreadNotifications(subscriptionFilter, projectKey),
    refetchInterval: 30_000, // poll every 30 seconds
    select: (res) => res.data,
  });

export const useGetNotifications = (params: GetNotificationsParams) =>
  useQuery({
    queryKey: ['notifications', params],
    queryFn: () => communicationService.getNotifications(params),
    select: (res) => res.data,
  });

export const useMarkNotificationRead = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: communicationService.markNotificationRead,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['notifications'] }),
  });
};

export const useMarkAllNotificationsRead = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: communicationService.markAllNotificationsRead,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['notifications'] }),
  });
};

export const useSendNotification = () =>
  useMutation({ mutationFn: communicationService.sendNotification });

// ── Mail ────────────────────────────────────────────────────────────────────

export const useGetMailboxMails = (params: GetMailsParams) =>
  useQuery({
    queryKey: ['mails', params],
    queryFn: () => communicationService.getMailboxMails(params),
    select: (res) => res.data,
  });

export const useGetMailboxMail = (itemId: string, projectKey: string) =>
  useQuery({
    queryKey: ['mails', itemId],
    queryFn: () => communicationService.getMailboxMail(itemId, projectKey),
    enabled: !!itemId,
    select: (res) => res.data,
  });

export const useSendMailToAny = () =>
  useMutation({ mutationFn: communicationService.sendMailToAny });

export const useSendMailWithTemplate = () =>
  useMutation({ mutationFn: communicationService.sendMailWithTemplate });

// ── Templates ───────────────────────────────────────────────────────────────

export const useGetTemplates = (params: { search?: string; sort?: string; projectKey: string }) =>
  useQuery({
    queryKey: ['templates', params],
    queryFn: () => communicationService.getTemplates(params),
    select: (res) => res.data,
  });

export const useGetTemplate = (itemId: string, projectKey: string) =>
  useQuery({
    queryKey: ['templates', itemId],
    queryFn: () => communicationService.getTemplate(itemId, projectKey),
    enabled: !!itemId,
    select: (res) => res.data,
  });

export const useSaveTemplate = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: communicationService.saveTemplate,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['templates'] }),
  });
};

export const useCloneTemplate = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: communicationService.cloneTemplate,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['templates'] }),
  });
};

export const useDeleteTemplate = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ itemId, projectKey }: { itemId: string; projectKey: string }) =>
      communicationService.deleteTemplate(itemId, projectKey),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['templates'] }),
  });
};
```

---

## Component: NotificationBell

The notification bell sits in the app header. It polls for unread notifications every 30 seconds and shows a badge with the unread count.

```tsx
// modules/communication/components/notification-bell/notification-bell.tsx

import { Bell, BellDot } from 'lucide-react';
import { Button } from '@/components/ui-kit/button';
import { Badge } from '@/components/ui-kit/badge';
import { useGetUnreadNotifications } from '../../hooks/use-communication';

interface NotificationBellProps {
  subscriptionFilter: string;
  projectKey: string;
  onOpen: () => void;
}

export function NotificationBell({ subscriptionFilter, projectKey, onOpen }: NotificationBellProps) {
  const { data } = useGetUnreadNotifications(subscriptionFilter, projectKey);
  const unreadCount = data?.unReadNotificationsCount ?? 0;

  return (
    <Button variant="ghost" size="icon" className="relative" onClick={onOpen} aria-label="Notifications">
      {unreadCount > 0 ? (
        <>
          <BellDot className="h-5 w-5 text-primary" />
          <Badge
            className="absolute -top-1 -right-1 h-5 w-5 flex items-center justify-center p-0 text-xs bg-primary text-primary-foreground"
          >
            {unreadCount > 99 ? '99+' : unreadCount}
          </Badge>
        </>
      ) : (
        <Bell className="h-5 w-5" />
      )}
    </Button>
  );
}
```

---

## Component: NotificationList

Opens as a dropdown or slide-over panel. Lists notifications with read/unread state. Marks individual items or all as read on interaction.

```tsx
// modules/communication/components/notification-list/notification-list.tsx

import { useGetNotifications, useMarkAllNotificationsRead, useMarkNotificationRead } from '../../hooks/use-communication';
import { Skeleton } from '@/components/ui-kit/skeleton';
import { Button } from '@/components/ui-kit/button';
import { ScrollArea } from '@/components/ui-kit/scroll-area';
import { NotificationItem } from './notification-item';
import { cn } from '@/lib/utils';

interface NotificationListProps {
  projectKey: string;
  subscriptionFilter: string;
}

export function NotificationList({ projectKey, subscriptionFilter }: NotificationListProps) {
  const { data, isLoading } = useGetNotifications({ page: 1, pageSize: 20, projectKey });
  const markAll = useMarkAllNotificationsRead();

  const handleMarkAllRead = () => {
    markAll.mutate({ subscriptionFilter, projectKey });
  };

  return (
    <div className="w-80 flex flex-col">
      <div className="flex items-center justify-between px-4 py-3 border-b">
        <h3 className="font-semibold text-sm">Notifications</h3>
        <Button variant="ghost" size="sm" onClick={handleMarkAllRead} disabled={markAll.isPending}>
          Mark all read
        </Button>
      </div>

      <ScrollArea className="h-96">
        {isLoading ? (
          Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="px-4 py-3 border-b">
              <Skeleton className="h-4 w-3/4 mb-2" />
              <Skeleton className="h-3 w-1/2" />
            </div>
          ))
        ) : data?.notifications?.length === 0 ? (
          <p className="text-center text-muted-foreground text-sm py-8">No notifications</p>
        ) : (
          data?.notifications?.map((notification) => (
            <NotificationItem
              key={notification.id}
              notification={notification}
              projectKey={projectKey}
            />
          ))
        )}
      </ScrollArea>
    </div>
  );
}
```

---

## Page: MailCompose

Form for composing and sending an email. Supports both ad-hoc (send to any address) and template-based modes.

Key behaviors:
- Toggle between "Send to any" and "Use template" modes
- When using template: select purpose, fill `bodyDataContext` key/value pairs, enter `userId`
- When ad-hoc: enter recipients, subject, and compose HTML body
- Show loading state on submit with spinner inside the Send button
- Show success toast on `isSuccess: true`; display field errors from `errors` on failure

---

## Page: Mailbox

Paginated list of sent/received emails. Uses a `DataTable` component.

Key behaviors:
- Fetch on mount and on page change
- Show `<Skeleton />` rows while loading
- Click a row to view full email body in a side panel or modal
- Empty state: "No emails found" with a compose button

---

## Page: Templates

List all email templates with search and sort. Provides actions per row: Edit, Clone, Delete.

Key behaviors:
- Search input debounced at 300ms, triggers re-fetch
- Edit → navigate to `template-editor-page` with `itemId` pre-loaded
- Clone → open `template-clone-dialog` with the source `itemId`
- Delete → show confirmation dialog before calling delete action
- Empty state: "No templates yet" with a "Create template" button

---

## Page: TemplateEditor

Create or edit an email template with a live HTML preview.

Key behaviors:
- Two-panel layout: left = form fields, right = HTML preview rendered in a sandboxed `<iframe>`
- `templateBody` textarea supports full HTML input
- Preview updates on every keystroke (debounced at 300ms)
- `purpose` field is a slug-style identifier — validate with `/^[a-z0-9-]+$/`
- On save: call `save-template` action; invalidate template list cache
- Show field-level errors from `errors` dictionary under each affected input

```tsx
// Live preview pattern
<iframe
  sandbox="allow-same-origin"
  srcDoc={watchedTemplateBody}
  className="w-full h-full border rounded-md"
  title="Template preview"
/>
```

---

## Real-time Notification Pattern

Use polling as the default strategy. WebSocket can be layered in later without changing the component API.

```
Poll interval: 30 seconds (refetchInterval in useGetUnreadNotifications)
Trigger:       App mount, window focus
On badge click: Open NotificationList panel
On item click:  Mark as read → update badge count
On "Mark all":  Mark all read → refetch unread count
```

React Query handles cache invalidation automatically when mark-as-read mutations succeed.

---

## Rules

- Never hardcode `projectKey` — always pass from `import.meta.env.VITE_PROJECT_SLUG` or app config
- The `configuratoinName` field in `NotifyPayload` keeps the API typo — do not rename in TypeScript
- Template body is always HTML — render previews in a sandboxed `<iframe>`, never use `dangerouslySetInnerHTML` in production UI
- All pages must handle loading (`<Skeleton />`), error (`<ErrorAlert />`), and empty states
- Use `cn()` from `@/lib/utils` for all conditional classNames
- Use Tailwind semantic tokens — never hardcode colors
