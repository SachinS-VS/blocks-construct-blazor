# Communication — Frontend Guide (Blazor + Tailwind)

This file extends `core/frontend-blazor-tailwind.md` with communication-specific patterns for the communication skill.
Always read `core/frontend-blazor-tailwind.md` first, then apply the overrides and additions here.

---

## Stack

| Layer | Technology |
|-------|-----------|
| Framework | .NET 10 Blazor WebAssembly (standalone) |
| Styling | Tailwind CSS + `@tailwindcss/forms` |
| Component library | None — plain HTML + Tailwind utility classes |
| State management | Scoped services + event pattern |
| HTTP | `HttpClient` + `DelegatingHandler` |
| Validation | FluentValidation |
| i18n | `ILocalizationService` (remote JSON resources) |
| Icons | Heroicons (inline SVG) |
| Notifications | Custom `ToastService` |

---

## Module Structure

All communication UI lives inside `Modules/Communication/`:

```
Modules/Communication/
├── Components/
│   ├── NotificationBell.razor          ← icon button with unread badge count
│   ├── NotificationList.razor          ← dropdown panel listing notifications
│   └── NotificationItem.razor          ← single notification row
├── Pages/
│   ├── MailComposePage.razor           ← compose and send email form
│   ├── MailboxPage.razor               ← paginated email list view
│   ├── MailDetailModal.razor           ← mail detail modal
│   ├── TemplatesPage.razor             ← list all templates with search
│   ├── TemplateEditorPage.razor        ← create/edit template with HTML preview
│   └── TemplateCloneModal.razor        ← clone template modal
├── Services/
│   └── CommunicationService.cs         ← raw API calls (no state logic)
└── Models/
    ├── CommunicationModels.cs          ← request/response models
    └── CommunicationValidators.cs      ← FluentValidation validators
```

---

## C# Models (`CommunicationModels.cs`)

```csharp
// Modules/Communication/Models/CommunicationModels.cs

// ── Mail ──────────────────────────────────────────────────────────────────

public class Mail
{
    public string ItemId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public List<string> To { get; set; } = new();
    public string From { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public DateTime SentTime { get; set; }
    public bool IsRead { get; set; }
}

public class SendMailToAnyPayload
{
    public List<string> To { get; set; } = new();
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Purpose { get; set; }
    public string? Language { get; set; }
    public List<object>? Attachments { get; set; }
    public string ProjectKey { get; set; } = string.Empty;
}

public class SendMailPayload
{
    public string UserId { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string? Language { get; set; }
    public Dictionary<string, string>? BodyDataContext { get; set; }
    public List<object>? Attachments { get; set; }
    public string ProjectKey { get; set; } = string.Empty;
}

// ── Notifications ─────────────────────────────────────────────────────────

public class Notification
{
    public string Id { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string DenormalizedPayload { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; }
    public bool IsRead { get; set; }
    public string SubscriptionFilter { get; set; } = string.Empty;
}

public class NotifyPayload
{
    public List<string>? UserIds { get; set; }
    public List<string>? Roles { get; set; }
    public List<string>? SubscriptionFilters { get; set; }
    public string DenormalizedPayload { get; set; } = string.Empty;
    public string? ConfiguratoinName { get; set; } // API typo — keep as-is
    public string ProjectKey { get; set; } = string.Empty;
}

// ── Templates ─────────────────────────────────────────────────────────────

public class EmailTemplate
{
    public string ItemId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TemplateSubject { get; set; } = string.Empty;
    public string TemplateBody { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdatedDate { get; set; }
}

public class SaveTemplatePayload
{
    public string? ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TemplateSubject { get; set; } = string.Empty;
    public string TemplateBody { get; set; } = string.Empty;
    public string? MailConfigurationId { get; set; }
    public string? Language { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
}

public class CloneTemplatePayload
{
    public string ItemId { get; set; } = string.Empty;
    public string NewName { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
}

// ── Query Parameters ──────────────────────────────────────────────────────

public class GetMailsParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string ProjectKey { get; set; } = string.Empty;
}

public class GetNotificationsParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string ProjectKey { get; set; } = string.Empty;
}
```

---

## Validators (`CommunicationValidators.cs`)

```csharp
// Modules/Communication/Models/CommunicationValidators.cs
using FluentValidation;

public class SendMailToAnyPayloadValidator : AbstractValidator<SendMailToAnyPayload>
{
    public SendMailToAnyPayloadValidator(ILocalizationService localizer)
    {
        RuleFor(x => x.To).NotEmpty().WithMessage(localizer["communication.mail.validation.recipientsRequired"]);
        RuleFor(x => x.Subject).NotEmpty().WithMessage(localizer["communication.mail.validation.subjectRequired"]);
        RuleFor(x => x.Body).NotEmpty().WithMessage(localizer["communication.mail.validation.bodyRequired"]);
    }
}

public class SendMailPayloadValidator : AbstractValidator<SendMailPayload>
{
    public SendMailPayloadValidator(ILocalizationService localizer)
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage(localizer["communication.mail.validation.userIdRequired"]);
        RuleFor(x => x.Purpose).NotEmpty().WithMessage(localizer["communication.mail.validation.purposeRequired"]);
    }
}

public class SaveTemplatePayloadValidator : AbstractValidator<SaveTemplatePayload>
{
    public SaveTemplatePayloadValidator(ILocalizationService localizer)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(localizer["communication.template.validation.nameRequired"]);
        RuleFor(x => x.Purpose).NotEmpty()
            .WithMessage(localizer["communication.template.validation.purposeRequired"])
            .Matches("^[a-z0-9-]+$")
            .WithMessage(localizer["communication.template.validation.purposeFormat"]);
        RuleFor(x => x.TemplateSubject).NotEmpty()
            .WithMessage(localizer["communication.template.validation.subjectRequired"]);
        RuleFor(x => x.TemplateBody).NotEmpty()
            .WithMessage(localizer["communication.template.validation.bodyRequired"]);
    }
}

public class CloneTemplatePayloadValidator : AbstractValidator<CloneTemplatePayload>
{
    public CloneTemplatePayloadValidator(ILocalizationService localizer)
    {
        RuleFor(x => x.NewName).NotEmpty()
            .WithMessage(localizer["communication.template.validation.newNameRequired"]);
    }
}
```

---

## Service Layer (`CommunicationService.cs`)

Base path: `/communication/v1`

```csharp
// Modules/Communication/Services/CommunicationService.cs
using System.Net.Http.Json;

public interface ICommunicationService
{
    // Mail
    Task<BaseResponse> SendToAnyAsync(SendMailToAnyPayload payload);
    Task<BaseResponse> SendWithTemplateAsync(SendMailPayload payload);
    Task<GetMailsResponse> GetMailboxMailsAsync(GetMailsParams parameters);
    Task<GetMailResponse> GetMailboxMailAsync(string itemId, string projectKey);

    // Notifications
    Task<BaseResponse> SendNotificationAsync(NotifyPayload payload);
    Task<GetUnreadNotificationsResponse> GetUnreadNotificationsAsync(string subscriptionFilter, string projectKey);
    Task<GetNotificationsResponse> GetNotificationsAsync(GetNotificationsParams parameters);
    Task<BaseResponse> MarkNotificationReadAsync(string notificationId, string projectKey);
    Task<BaseResponse> MarkAllNotificationsReadAsync(string subscriptionFilter, string projectKey);

    // Templates
    Task<BaseResponse> SaveTemplateAsync(SaveTemplatePayload payload);
    Task<GetTemplateResponse> GetTemplateAsync(string itemId, string projectKey);
    Task<GetTemplatesResponse> GetTemplatesAsync(string? search, string? sort, string projectKey);
    Task<BaseResponse> CloneTemplateAsync(CloneTemplatePayload payload);
    Task<BaseResponse> DeleteTemplateAsync(string itemId, string projectKey);
}

public class CommunicationService : ICommunicationService
{
    private const string Base = "/communication/v1";
    private readonly HttpClient _http;
    private readonly AppSettings _settings;

    public CommunicationService(HttpClient http, AppSettings settings)
    {
        _http = http;
        _settings = settings;
    }

    // ── Mail ──────────────────────────────────────────────────────────────

    public async Task<BaseResponse> SendToAnyAsync(SendMailToAnyPayload payload)
    {
        var response = await _http.PostAsJsonAsync($"{Base}/Mail/SendToAny", payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BaseResponse>()
            ?? throw new InvalidOperationException("Invalid response");
    }

    public async Task<BaseResponse> SendWithTemplateAsync(SendMailPayload payload)
    {
        var response = await _http.PostAsJsonAsync($"{Base}/Mail/Send", payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BaseResponse>()
            ?? throw new InvalidOperationException("Invalid response");
    }

    public async Task<GetMailsResponse> GetMailboxMailsAsync(GetMailsParams parameters)
    {
        var url = $"{Base}/Mail/GetMailBoxMails?page={parameters.Page}&pageSize={parameters.PageSize}&projectKey={parameters.ProjectKey}";
        return await _http.GetFromJsonAsync<GetMailsResponse>(url)
            ?? throw new InvalidOperationException("Invalid response");
    }

    public async Task<GetMailResponse> GetMailboxMailAsync(string itemId, string projectKey)
    {
        var url = $"{Base}/Mail/GetMailBoxMail?itemId={itemId}&projectKey={projectKey}";
        return await _http.GetFromJsonAsync<GetMailResponse>(url)
            ?? throw new InvalidOperationException("Invalid response");
    }

    // ── Notifications ─────────────────────────────────────────────────────

    public async Task<BaseResponse> SendNotificationAsync(NotifyPayload payload)
    {
        var response = await _http.PostAsJsonAsync($"{Base}/Notifier/Notify", payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BaseResponse>()
            ?? throw new InvalidOperationException("Invalid response");
    }

    public async Task<GetUnreadNotificationsResponse> GetUnreadNotificationsAsync(
        string subscriptionFilter, string projectKey)
    {
        var url = $"{Base}/Notifier/GetUnreadNotificationsBySubscriptionFilter?subscriptionFilter={subscriptionFilter}&projectKey={projectKey}";
        return await _http.GetFromJsonAsync<GetUnreadNotificationsResponse>(url)
            ?? throw new InvalidOperationException("Invalid response");
    }

    public async Task<GetNotificationsResponse> GetNotificationsAsync(GetNotificationsParams parameters)
    {
        var url = $"{Base}/Notifier/GetNotifications?page={parameters.Page}&pageSize={parameters.PageSize}&projectKey={parameters.ProjectKey}";
        return await _http.GetFromJsonAsync<GetNotificationsResponse>(url)
            ?? throw new InvalidOperationException("Invalid response");
    }

    public async Task<BaseResponse> MarkNotificationReadAsync(string notificationId, string projectKey)
    {
        var response = await _http.PostAsJsonAsync($"{Base}/Notifier/MarkNotificationAsRead",
            new { notificationId, projectKey });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BaseResponse>()
            ?? throw new InvalidOperationException("Invalid response");
    }

    public async Task<BaseResponse> MarkAllNotificationsReadAsync(string subscriptionFilter, string projectKey)
    {
        var response = await _http.PostAsJsonAsync($"{Base}/Notifier/MarkAllNotificationAsRead",
            new { subscriptionFilter, projectKey });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BaseResponse>()
            ?? throw new InvalidOperationException("Invalid response");
    }

    // ── Templates ─────────────────────────────────────────────────────────

    public async Task<BaseResponse> SaveTemplateAsync(SaveTemplatePayload payload)
    {
        var response = await _http.PostAsJsonAsync($"{Base}/Template/Save", payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BaseResponse>()
            ?? throw new InvalidOperationException("Invalid response");
    }

    public async Task<GetTemplateResponse> GetTemplateAsync(string itemId, string projectKey)
    {
        var url = $"{Base}/Template/Get?itemId={itemId}&projectKey={projectKey}";
        return await _http.GetFromJsonAsync<GetTemplateResponse>(url)
            ?? throw new InvalidOperationException("Invalid response");
    }

    public async Task<GetTemplatesResponse> GetTemplatesAsync(string? search, string? sort, string projectKey)
    {
        var url = $"{Base}/Template/Gets?projectKey={projectKey}";
        if (!string.IsNullOrEmpty(search)) url += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrEmpty(sort)) url += $"&sort={Uri.EscapeDataString(sort)}";
        return await _http.GetFromJsonAsync<GetTemplatesResponse>(url)
            ?? throw new InvalidOperationException("Invalid response");
    }

    public async Task<BaseResponse> CloneTemplateAsync(CloneTemplatePayload payload)
    {
        var response = await _http.PostAsJsonAsync($"{Base}/Template/Clone", payload);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BaseResponse>()
            ?? throw new InvalidOperationException("Invalid response");
    }

    public async Task<BaseResponse> DeleteTemplateAsync(string itemId, string projectKey)
    {
        var url = $"{Base}/Template/Delete?itemId={itemId}&projectKey={projectKey}";
        var response = await _http.DeleteAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BaseResponse>()
            ?? throw new InvalidOperationException("Invalid response");
    }
}
```

**DI Registration in `Program.cs`:**

```csharp
builder.Services.AddScoped<ICommunicationService, CommunicationService>();
```

---

## Component: NotificationBell

The notification bell sits in the app header. It polls for unread notifications every 30 seconds and shows a badge with the unread count.

```razor
@* Modules/Communication/Components/NotificationBell.razor *@
@inject ICommunicationService CommunicationService
@inject AppSettings AppSettings
@inject ILocalizationService Localizer
@inject IJSRuntime JS
@implements IDisposable

<div class="relative">
    <button @onclick="OnBellClicked"
            class="relative rounded-full p-1.5 text-gray-500 hover:bg-gray-100 hover:text-gray-700 focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2"
            aria-label="@Localizer["communication.notifications.bell"]">
        @if (_unreadCount > 0)
        {
            @* Heroicon: solid/bell-alert *@
            <svg class="h-6 w-6 text-primary" fill="currentColor" viewBox="0 0 24 24">
                <path d="M5.85 3.5a.75.75 0 0 0-1.117-1 9.719 9.719 0 0 0-2.348 4.876.75.75 0 0 0 1.479.248A8.219 8.219 0 0 1 5.85 3.5ZM19.267 2.5a.75.75 0 1 0-1.118 1 8.22 8.22 0 0 1 1.987 4.124.75.75 0 0 0 1.48-.248A9.72 9.72 0 0 0 19.266 2.5Z" />
                <path fill-rule="evenodd" d="M12 2.25A6.75 6.75 0 0 0 5.25 9v.75a8.217 8.217 0 0 1-2.119 5.52.75.75 0 0 0 .298 1.206c1.544.57 3.16.99 4.831 1.243a3.75 3.75 0 1 0 7.48 0 24.583 24.583 0 0 0 4.83-1.244.75.75 0 0 0 .298-1.205 8.217 8.217 0 0 1-2.118-5.52V9A6.75 6.75 0 0 0 12 2.25ZM9.75 18c0-.034 0-.067.002-.1a25.05 25.05 0 0 0 4.496 0l.002.1a2.25 2.25 0 1 1-4.5 0Z" clip-rule="evenodd" />
            </svg>
        }
        else
        {
            @* Heroicon: outline/bell *@
            <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M14.857 17.082a23.848 23.848 0 0 0 5.454-1.31A8.967 8.967 0 0 1 18 9.75V9A6 6 0 0 0 6 9v.75a8.967 8.967 0 0 1-2.312 6.022c1.733.64 3.56 1.085 5.455 1.31m5.714 0a24.255 24.255 0 0 1-5.714 0m5.714 0a3 3 0 1 1-5.714 0" />
            </svg>
        }

        @if (_unreadCount > 0)
        {
            <span class="absolute -right-0.5 -top-0.5 flex h-5 min-w-5 items-center justify-center rounded-full bg-red-500 px-1 text-[10px] font-bold text-white">
                @BadgeText
            </span>
        }
    </button>
</div>

@code {
    [Parameter] public string SubscriptionFilter { get; set; } = string.Empty;
    [Parameter] public EventCallback OnClick { get; set; }

    private int _unreadCount;
    private Timer? _pollTimer;

    private string BadgeText => _unreadCount > 99 ? "99+" : _unreadCount.ToString();

    protected override async Task OnInitializedAsync()
    {
        await FetchUnreadCountAsync();
        _pollTimer = new Timer(async _ => await PollAsync(), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("addVisibilityChangeListener",
                DotNetObjectReference.Create(this));
        }
    }

    [JSInvokable]
    public async Task OnWindowFocused()
    {
        await FetchUnreadCountAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task PollAsync()
    {
        await FetchUnreadCountAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task FetchUnreadCountAsync()
    {
        try
        {
            var result = await CommunicationService.GetUnreadNotificationsAsync(
                SubscriptionFilter, AppSettings.ProjectSlug);
            _unreadCount = result.UnReadNotificationsCount;
        }
        catch
        {
            // Silently fail on poll — do not disrupt UI
        }
    }

    private async Task OnBellClicked()
    {
        await OnClick.InvokeAsync();
    }

    public void Dispose()
    {
        _pollTimer?.Dispose();
    }
}
```

---

## Component: NotificationList

Opens as an absolute-positioned dropdown panel. Lists notifications with read/unread state. Marks individual items or all as read on interaction.

```razor
@* Modules/Communication/Components/NotificationList.razor *@
@inject ICommunicationService CommunicationService
@inject AppSettings AppSettings
@inject ILocalizationService Localizer

@if (IsOpen)
{
    <div class="absolute right-0 z-50 mt-2 w-[360px] origin-top-right rounded-lg bg-white shadow-lg ring-1 ring-black/5">
        @* Header *@
        <div class="flex items-center justify-between border-b border-gray-200 px-4 py-3">
            <h3 class="text-sm font-semibold text-gray-900">
                @Localizer["communication.notifications.title"]
            </h3>
            <button class="text-xs font-medium text-primary hover:text-primary/80 disabled:opacity-50"
                    disabled="@_markingAll"
                    @onclick="HandleMarkAllRead">
                @Localizer["communication.notifications.markAllRead"]
            </button>
        </div>

        @* Body *@
        <div class="max-h-[400px] overflow-y-auto">
            @if (_loading)
            {
                @for (int i = 0; i < 5; i++)
                {
                    <div class="border-b border-gray-100 px-4 py-3">
                        <div class="mb-2 h-4 w-3/4 animate-pulse rounded bg-gray-200"></div>
                        <div class="h-3 w-1/2 animate-pulse rounded bg-gray-200"></div>
                    </div>
                }
            }
            else if (_notifications.Count == 0)
            {
                <div class="flex flex-col items-center px-6 py-8">
                    @* Heroicon: outline/bell-slash *@
                    <svg class="mb-2 h-10 w-10 text-gray-300" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M9.143 17.082a24.248 24.248 0 0 0 5.714 0m-5.714 0a3 3 0 1 1 5.714 0M3.124 16.715A8.966 8.966 0 0 1 6 9.75V9a6 6 0 0 1 12 0v.75a8.966 8.966 0 0 1 2.876 6.965M3.124 16.715A23.94 23.94 0 0 0 12 18.75c3.17 0 6.176-.614 8.876-1.725M3.124 16.715l.59-.42M20.876 16.715l-.59-.42M2.25 2.25l19.5 19.5" />
                    </svg>
                    <p class="text-sm text-gray-500">
                        @Localizer["communication.notifications.empty"]
                    </p>
                </div>
            }
            else
            {
                @foreach (var notification in _notifications)
                {
                    <NotificationItem Notification="@notification"
                                      OnItemClicked="HandleItemClicked" />
                }
            }
        </div>
    </div>
}

@code {
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public string SubscriptionFilter { get; set; } = string.Empty;
    [Parameter] public EventCallback OnNotificationRead { get; set; }

    private List<Notification> _notifications = new();
    private bool _loading = true;
    private bool _markingAll;

    protected override async Task OnParametersSetAsync()
    {
        if (IsOpen)
        {
            await LoadNotificationsAsync();
        }
    }

    private async Task LoadNotificationsAsync()
    {
        _loading = true;
        try
        {
            var result = await CommunicationService.GetNotificationsAsync(
                new GetNotificationsParams { Page = 1, PageSize = 20, ProjectKey = AppSettings.ProjectSlug });
            _notifications = result.Notifications;
        }
        catch
        {
            _notifications = new();
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task HandleItemClicked(Notification notification)
    {
        if (!notification.IsRead)
        {
            await CommunicationService.MarkNotificationReadAsync(notification.Id, AppSettings.ProjectSlug);
            notification.IsRead = true;
            await OnNotificationRead.InvokeAsync();
        }
    }

    private async Task HandleMarkAllRead()
    {
        _markingAll = true;
        try
        {
            await CommunicationService.MarkAllNotificationsReadAsync(SubscriptionFilter, AppSettings.ProjectSlug);
            foreach (var n in _notifications) n.IsRead = true;
            await OnNotificationRead.InvokeAsync();
        }
        finally
        {
            _markingAll = false;
        }
    }
}
```

---

## Component: NotificationItem

```razor
@* Modules/Communication/Components/NotificationItem.razor *@
@inject ILocalizationService Localizer

<button class="@ItemClass w-full border-b border-gray-100 px-4 py-3 text-left transition-colors hover:bg-gray-50"
        @onclick="() => OnItemClicked.InvokeAsync(Notification)">
    <p class="@(_isUnread ? "text-sm font-semibold text-gray-900" : "text-sm text-gray-700")">
        @Notification.DenormalizedPayload
    </p>
    <p class="mt-0.5 text-xs text-gray-400">
        @Notification.CreatedTime.ToString("g")
    </p>
</button>

@code {
    [Parameter] public Notification Notification { get; set; } = default!;
    [Parameter] public EventCallback<Notification> OnItemClicked { get; set; }

    private bool _isUnread => !Notification.IsRead;

    private string ItemClass => _isUnread
        ? "bg-primary/5"
        : "bg-white";
}
```

---

## Page: MailComposePage

Form for composing and sending an email. Supports both ad-hoc (send to any address) and template-based modes.

```razor
@* Modules/Communication/Pages/MailComposePage.razor *@
@page "/communication/compose"
@attribute [Authorize]
@inject ICommunicationService CommunicationService
@inject AppSettings AppSettings
@inject ToastService Toast
@inject ILocalizationService Localizer

<h1 class="mb-6 text-2xl font-bold text-gray-900">@Localizer["communication.mail.compose.title"]</h1>

@* Mode toggle *@
<div class="mb-6 inline-flex rounded-lg border border-gray-200 bg-gray-50 p-1">
    <button class="@(_mode == "any" ? "rounded-md bg-primary px-4 py-2 text-sm font-medium text-white shadow-sm" : "rounded-md px-4 py-2 text-sm font-medium text-gray-600 hover:text-gray-900")"
            @onclick='() => _mode = "any"'>
        @Localizer["communication.mail.compose.sendToAny"]
    </button>
    <button class="@(_mode == "template" ? "rounded-md bg-primary px-4 py-2 text-sm font-medium text-white shadow-sm" : "rounded-md px-4 py-2 text-sm font-medium text-gray-600 hover:text-gray-900")"
            @onclick='() => _mode = "template"'>
        @Localizer["communication.mail.compose.useTemplate"]
    </button>
</div>

@if (_mode == "any")
{
    <EditForm Model="_anyPayload" OnValidSubmit="SendToAny" class="space-y-4">
        @* Recipients input *@
        <div>
            <label class="label">@Localizer["communication.mail.compose.recipients"]</label>
            <div class="flex gap-2">
                <input type="email" @bind="_recipientInput" @bind:event="oninput"
                       @onkeydown="OnRecipientKeyDown"
                       placeholder="@Localizer["communication.mail.compose.recipientsPlaceholder"]"
                       class="input flex-1" />
                <button type="button" class="btn-outline" @onclick="AddRecipient">
                    @* Heroicon: outline/plus *@
                    <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
                    </svg>
                </button>
            </div>
            @if (_anyPayload.To.Any())
            {
                <div class="mt-2 flex flex-wrap gap-2">
                    @foreach (var recipient in _anyPayload.To)
                    {
                        <span class="inline-flex items-center gap-1 rounded-full bg-primary/10 px-3 py-1 text-sm font-medium text-primary">
                            @recipient
                            <button type="button" class="ml-1 text-primary/60 hover:text-primary"
                                    @onclick="() => RemoveRecipient(recipient)">
                                &times;
                            </button>
                        </span>
                    }
                </div>
            }
        </div>

        @* Subject *@
        <div>
            <label class="label">@Localizer["communication.mail.compose.subject"]</label>
            <input type="text" @bind="_anyPayload.Subject" class="input" />
        </div>

        @* Body *@
        <div>
            <label class="label">@Localizer["communication.mail.compose.body"]</label>
            <textarea @bind="_anyPayload.Body" rows="8" class="input"></textarea>
        </div>

        @* Send button *@
        <button type="submit" class="btn-primary" disabled="@_sending">
            @if (_sending)
            {
                <span class="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-white/30 border-t-white"></span>
            }
            @Localizer["communication.mail.compose.send"]
        </button>
    </EditForm>
}
else
{
    <EditForm Model="_templatePayload" OnValidSubmit="SendWithTemplate" class="space-y-4">
        @* User ID *@
        <div>
            <label class="label">@Localizer["communication.mail.compose.userId"]</label>
            <input type="text" @bind="_templatePayload.UserId" class="input" />
        </div>

        @* Purpose *@
        <div>
            <label class="label">@Localizer["communication.mail.compose.purpose"]</label>
            <input type="text" @bind="_templatePayload.Purpose" class="input" />
        </div>

        @* Body Data Context — key/value pairs *@
        <div>
            <h3 class="mb-2 text-sm font-medium text-gray-700">
                @Localizer["communication.mail.compose.bodyDataContext"]
            </h3>

            @foreach (var kvp in _bodyDataContext)
            {
                <div class="mb-2 flex items-center gap-2">
                    <input type="text" value="@kvp.Key"
                           @onchange="e => UpdateContextKey(kvp.Key, e.Value?.ToString() ?? string.Empty)"
                           placeholder="@Localizer["common.key"]"
                           class="input flex-1" />
                    <input type="text" @bind="_bodyDataContext[kvp.Key]"
                           placeholder="@Localizer["common.value"]"
                           class="input flex-1" />
                    <button type="button" class="btn-ghost text-red-500 hover:text-red-700"
                            @onclick="() => RemoveContextEntry(kvp.Key)">
                        @* Heroicon: outline/trash *@
                        <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" d="m14.74 9-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 0 1-2.244 2.077H8.084a2.25 2.25 0 0 1-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 0 0-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 0 1 3.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 0 0-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 0 0-7.5 0" />
                        </svg>
                    </button>
                </div>
            }

            <button type="button" class="btn-ghost text-sm text-primary"
                    @onclick="AddContextEntry">
                @* Heroicon: outline/plus *@
                <svg class="mr-1 inline h-4 w-4" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
                </svg>
                @Localizer["communication.mail.compose.addVariable"]
            </button>
        </div>

        @* Send button *@
        <button type="submit" class="btn-primary" disabled="@_sending">
            @if (_sending)
            {
                <span class="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-white/30 border-t-white"></span>
            }
            @Localizer["communication.mail.compose.send"]
        </button>
    </EditForm>
}

@if (!string.IsNullOrEmpty(_errorMessage))
{
    <ErrorAlert Message="@_errorMessage" Class="mt-4" />
}

@code {
    private string _mode = "any";
    private bool _sending;
    private string? _errorMessage;
    private string _recipientInput = string.Empty;

    private SendMailToAnyPayload _anyPayload = new();
    private SendMailPayload _templatePayload = new();
    private Dictionary<string, string> _bodyDataContext = new();

    private SendMailToAnyPayloadValidator _anyValidator = default!;
    private SendMailPayloadValidator _templateValidator = default!;

    protected override void OnInitialized()
    {
        _anyPayload.ProjectKey = AppSettings.ProjectSlug;
        _templatePayload.ProjectKey = AppSettings.ProjectSlug;
        _anyValidator = new SendMailToAnyPayloadValidator(Localizer);
        _templateValidator = new SendMailPayloadValidator(Localizer);
    }

    private void AddRecipient()
    {
        var email = _recipientInput.Trim();
        if (!string.IsNullOrEmpty(email) && !_anyPayload.To.Contains(email))
        {
            _anyPayload.To.Add(email);
            _recipientInput = string.Empty;
        }
    }

    private void OnRecipientKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") AddRecipient();
    }

    private void RemoveRecipient(string email) => _anyPayload.To.Remove(email);

    private void AddContextEntry() => _bodyDataContext[$"key{_bodyDataContext.Count + 1}"] = string.Empty;
    private void RemoveContextEntry(string key) => _bodyDataContext.Remove(key);

    private void UpdateContextKey(string oldKey, string newKey)
    {
        if (_bodyDataContext.ContainsKey(oldKey))
        {
            var value = _bodyDataContext[oldKey];
            _bodyDataContext.Remove(oldKey);
            _bodyDataContext[newKey] = value;
        }
    }

    private async Task SendToAny()
    {
        var validationResult = await _anyValidator.ValidateAsync(_anyPayload);
        if (!validationResult.IsValid)
        {
            _errorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return;
        }

        _sending = true;
        _errorMessage = null;
        try
        {
            var result = await CommunicationService.SendToAnyAsync(_anyPayload);
            if (result.IsSuccess)
            {
                Toast.ShowSuccess(Localizer["communication.mail.compose.sendSuccess"]);
                _anyPayload = new() { ProjectKey = AppSettings.ProjectSlug };
            }
            else
            {
                _errorMessage = string.Join(", ", result.Errors.Values);
            }
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _sending = false;
        }
    }

    private async Task SendWithTemplate()
    {
        var validationResult = await _templateValidator.ValidateAsync(_templatePayload);
        if (!validationResult.IsValid)
        {
            _errorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return;
        }

        _sending = true;
        _errorMessage = null;
        _templatePayload.BodyDataContext = _bodyDataContext.Count > 0 ? _bodyDataContext : null;
        try
        {
            var result = await CommunicationService.SendWithTemplateAsync(_templatePayload);
            if (result.IsSuccess)
            {
                Toast.ShowSuccess(Localizer["communication.mail.compose.sendSuccess"]);
                _templatePayload = new() { ProjectKey = AppSettings.ProjectSlug };
                _bodyDataContext.Clear();
            }
            else
            {
                _errorMessage = string.Join(", ", result.Errors.Values);
            }
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _sending = false;
        }
    }
}
```

---

## Page: MailboxPage

Paginated list of sent/received emails. Click a row to view full email body in a modal.

```razor
@* Modules/Communication/Pages/MailboxPage.razor *@
@page "/communication/mailbox"
@attribute [Authorize]
@inject ICommunicationService CommunicationService
@inject AppSettings AppSettings
@inject NavigationManager Navigation
@inject ILocalizationService Localizer

<h1 class="mb-6 text-2xl font-bold text-gray-900">@Localizer["communication.mailbox.title"]</h1>

<div class="card overflow-hidden">
    <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
            <tr>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["communication.mailbox.from"]
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["communication.mailbox.subject"]
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["communication.mailbox.date"]
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["communication.mailbox.status"]
                </th>
            </tr>
        </thead>
        <tbody class="divide-y divide-gray-200 bg-white">
            @if (_loading)
            {
                @for (int i = 0; i < 5; i++)
                {
                    <tr>
                        <td class="px-6 py-4"><div class="h-4 w-28 animate-pulse rounded bg-gray-200"></div></td>
                        <td class="px-6 py-4"><div class="h-4 w-48 animate-pulse rounded bg-gray-200"></div></td>
                        <td class="px-6 py-4"><div class="h-4 w-24 animate-pulse rounded bg-gray-200"></div></td>
                        <td class="px-6 py-4"><div class="h-4 w-16 animate-pulse rounded bg-gray-200"></div></td>
                    </tr>
                }
            }
            else if (_mails.Count == 0)
            {
                <tr>
                    <td colspan="4" class="px-6 py-12 text-center">
                        <div class="flex flex-col items-center">
                            @* Heroicon: outline/envelope *@
                            <svg class="mb-3 h-10 w-10 text-gray-300" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" d="M21.75 6.75v10.5a2.25 2.25 0 0 1-2.25 2.25h-15a2.25 2.25 0 0 1-2.25-2.25V6.75m19.5 0A2.25 2.25 0 0 0 19.5 4.5h-15a2.25 2.25 0 0 0-2.25 2.25m19.5 0v.243a2.25 2.25 0 0 1-1.07 1.916l-7.5 4.615a2.25 2.25 0 0 1-2.36 0L3.32 8.91a2.25 2.25 0 0 1-1.07-1.916V6.75" />
                            </svg>
                            <p class="mb-3 text-sm text-gray-500">@Localizer["communication.mailbox.empty"]</p>
                            <a href="/communication/compose" class="btn-primary">
                                @Localizer["communication.mailbox.compose"]
                            </a>
                        </div>
                    </td>
                </tr>
            }
            else
            {
                @foreach (var mail in _mails)
                {
                    <tr class="cursor-pointer hover:bg-gray-50" @onclick="() => ShowMailDetail(mail)">
                        <td class="whitespace-nowrap px-6 py-4 text-sm text-gray-900">@mail.From</td>
                        <td class="whitespace-nowrap px-6 py-4 text-sm text-gray-700">@mail.Subject</td>
                        <td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">@mail.SentTime.ToString("g")</td>
                        <td class="whitespace-nowrap px-6 py-4">
                            <span class="@(mail.IsRead ? "badge-gray" : "badge-primary")">
                                @(mail.IsRead ? Localizer["communication.mailbox.read"] : Localizer["communication.mailbox.unread"])
                            </span>
                        </td>
                    </tr>
                }
            }
        </tbody>
    </table>
    <Pagination CurrentPage="_page" PageSize="_pageSize" TotalCount="_totalCount"
                OnPageChanged="LoadPage" />
</div>

@if (!string.IsNullOrEmpty(_errorMessage))
{
    <ErrorAlert Message="@_errorMessage" Class="mt-4" />
}

@* Mail detail modal *@
<MailDetailModal IsOpen="_detailOpen" Mail="_selectedMail" OnClose="CloseDetail" />

@code {
    private List<Mail> _mails = new();
    private int _totalCount;
    private int _page = 1;
    private int _pageSize = 10;
    private bool _loading = true;
    private string? _errorMessage;
    private bool _detailOpen;
    private Mail? _selectedMail;

    protected override async Task OnInitializedAsync()
    {
        await LoadMailsAsync();
    }

    private async Task LoadMailsAsync()
    {
        _loading = true;
        _errorMessage = null;
        try
        {
            var result = await CommunicationService.GetMailboxMailsAsync(
                new GetMailsParams { Page = _page, PageSize = _pageSize, ProjectKey = AppSettings.ProjectSlug });
            _mails = result.Mails;
            _totalCount = result.TotalCount;
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task LoadPage(int page)
    {
        _page = page;
        await LoadMailsAsync();
    }

    private void ShowMailDetail(Mail mail)
    {
        _selectedMail = mail;
        _detailOpen = true;
    }

    private void CloseDetail()
    {
        _detailOpen = false;
        _selectedMail = null;
    }
}
```

**MailDetailModal** (embedded in same module):

```razor
@* Modules/Communication/Pages/MailDetailModal.razor *@
@inject IJSRuntime JS
@inject ILocalizationService Localizer

<Modal IsOpen="IsOpen" Title="@Localizer["communication.mailbox.detail.title"]" OnClose="OnClose">
    @if (Mail is not null)
    {
        <h3 class="mb-2 text-base font-semibold text-gray-900">@Mail.Subject</h3>
        <p class="mb-1 text-xs text-gray-500">
            @Localizer["communication.mailbox.detail.from"]: @Mail.From
        </p>
        <p class="mb-3 text-xs text-gray-500">
            @Localizer["communication.mailbox.detail.date"]: @Mail.SentTime.ToString("f")
        </p>
        <hr class="mb-3 border-gray-200" />
        <iframe @ref="_iframeRef" sandbox="allow-same-origin"
                class="w-full rounded border border-gray-200"
                style="min-height: 300px;"
                title="@Localizer["communication.mailbox.detail.bodyPreview"]"></iframe>

        <div class="mt-4 flex justify-end">
            <button class="btn-outline" @onclick="OnClose">
                @Localizer["common.close"]
            </button>
        </div>
    }
</Modal>

@code {
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public Mail? Mail { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private ElementReference _iframeRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsOpen && Mail is not null)
        {
            try
            {
                await JS.InvokeVoidAsync("updateIframeSrcDoc", _iframeRef, Mail.Body);
            }
            catch
            {
                // Ignore JS interop errors during render
            }
        }
    }
}
```

---

## Page: TemplatesPage

List all email templates with debounced search and sort. Provides actions per row: Edit, Clone, Delete.

```razor
@* Modules/Communication/Pages/TemplatesPage.razor *@
@page "/communication/templates"
@attribute [Authorize]
@inject ICommunicationService CommunicationService
@inject AppSettings AppSettings
@inject NavigationManager Navigation
@inject ToastService Toast
@inject ILocalizationService Localizer
@implements IDisposable

<div class="mb-6 flex items-center justify-between">
    <h1 class="text-2xl font-bold text-gray-900">@Localizer["communication.templates.title"]</h1>
    <a href="/communication/templates/new" class="btn-primary">
        @* Heroicon: outline/plus *@
        <svg class="mr-1.5 inline h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
        </svg>
        @Localizer["communication.templates.create"]
    </a>
</div>

@* Search with debounce *@
<div class="relative mb-6">
    <span class="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
        @* Heroicon: outline/magnifying-glass *@
        <svg class="h-5 w-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z" />
        </svg>
    </span>
    <input type="text" @oninput="OnSearchInput"
           placeholder="@Localizer["communication.templates.searchPlaceholder"]"
           class="input pl-10" />
</div>

<div class="card overflow-hidden">
    <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
            <tr>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["communication.templates.name"]
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["communication.templates.purpose"]
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["communication.templates.language"]
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["communication.templates.lastUpdated"]
                </th>
                <th class="relative px-6 py-3">
                    <span class="sr-only">@Localizer["communication.templates.actions"]</span>
                </th>
            </tr>
        </thead>
        <tbody class="divide-y divide-gray-200 bg-white">
            @if (_loading)
            {
                @for (int i = 0; i < 5; i++)
                {
                    <tr>
                        <td class="px-6 py-4"><div class="h-4 w-36 animate-pulse rounded bg-gray-200"></div></td>
                        <td class="px-6 py-4"><div class="h-4 w-24 animate-pulse rounded bg-gray-200"></div></td>
                        <td class="px-6 py-4"><div class="h-4 w-14 animate-pulse rounded bg-gray-200"></div></td>
                        <td class="px-6 py-4"><div class="h-4 w-24 animate-pulse rounded bg-gray-200"></div></td>
                        <td class="px-6 py-4"><div class="h-4 w-28 animate-pulse rounded bg-gray-200"></div></td>
                    </tr>
                }
            }
            else if (_templates.Count == 0)
            {
                <tr>
                    <td colspan="5" class="px-6 py-12 text-center">
                        <div class="flex flex-col items-center">
                            @* Heroicon: outline/document-text *@
                            <svg class="mb-3 h-10 w-10 text-gray-300" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" d="M19.5 14.25v-2.625a3.375 3.375 0 0 0-3.375-3.375h-1.5A1.125 1.125 0 0 1 13.5 7.125v-1.5a3.375 3.375 0 0 0-3.375-3.375H8.25m0 12.75h7.5m-7.5 3H12M10.5 2.25H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 0 0-9-9Z" />
                            </svg>
                            <p class="mb-3 text-sm text-gray-500">@Localizer["communication.templates.empty"]</p>
                            <a href="/communication/templates/new" class="btn-primary">
                                @Localizer["communication.templates.create"]
                            </a>
                        </div>
                    </td>
                </tr>
            }
            else
            {
                @foreach (var template in _templates)
                {
                    <tr class="hover:bg-gray-50">
                        <td class="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">@template.Name</td>
                        <td class="whitespace-nowrap px-6 py-4">
                            <span class="badge-gray">@template.Purpose</span>
                        </td>
                        <td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">@template.Language</td>
                        <td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">@template.LastUpdatedDate.ToString("g")</td>
                        <td class="whitespace-nowrap px-6 py-4 text-right text-sm">
                            <button class="btn-ghost btn-sm" @onclick="() => EditTemplate(template.ItemId)"
                                    aria-label="@Localizer["communication.templates.edit"]">
                                @* Heroicon: outline/pencil-square *@
                                <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" d="m16.862 4.487 1.687-1.688a1.875 1.875 0 1 1 2.652 2.652L10.582 16.07a4.5 4.5 0 0 1-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 0 1 1.13-1.897l8.932-8.931Zm0 0L19.5 7.125M18 14v4.75A2.25 2.25 0 0 1 15.75 21H5.25A2.25 2.25 0 0 1 3 18.75V8.25A2.25 2.25 0 0 1 5.25 6H10" />
                                </svg>
                            </button>
                            <button class="btn-ghost btn-sm" @onclick="() => OpenCloneModal(template)"
                                    aria-label="@Localizer["communication.templates.clone"]">
                                @* Heroicon: outline/document-duplicate *@
                                <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 17.25v3.375c0 .621-.504 1.125-1.125 1.125h-9.75a1.125 1.125 0 0 1-1.125-1.125V7.875c0-.621.504-1.125 1.125-1.125H6.75a9.06 9.06 0 0 1 1.5.124m7.5 10.376h3.375c.621 0 1.125-.504 1.125-1.125V11.25c0-4.46-3.243-8.161-7.5-8.876a9.06 9.06 0 0 0-1.5-.124H9.375c-.621 0-1.125.504-1.125 1.125v3.5m7.5 10.375H9.375a1.125 1.125 0 0 1-1.125-1.125v-9.25m12 6.625v-1.875a3.375 3.375 0 0 0-3.375-3.375h-1.5a1.125 1.125 0 0 1-1.125-1.125v-1.5a3.375 3.375 0 0 0-3.375-3.375H9.75" />
                                </svg>
                            </button>
                            <button class="btn-ghost btn-sm text-red-500 hover:text-red-700"
                                    @onclick="() => ConfirmDelete(template)"
                                    aria-label="@Localizer["communication.templates.delete"]">
                                @* Heroicon: outline/trash *@
                                <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" d="m14.74 9-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 0 1-2.244 2.077H8.084a2.25 2.25 0 0 1-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 0 0-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 0 1 3.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 0 0-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 0 0-7.5 0" />
                                </svg>
                            </button>
                        </td>
                    </tr>
                }
            }
        </tbody>
    </table>
</div>

@if (!string.IsNullOrEmpty(_errorMessage))
{
    <ErrorAlert Message="@_errorMessage" Class="mt-4" />
}

@* Clone modal *@
<TemplateCloneModal IsOpen="_cloneOpen"
                    SourceTemplate="_cloneSource"
                    OnClose="CloseCloneModal"
                    OnCloned="OnTemplateCloned" />

@* Delete confirmation modal *@
<ConfirmationModal IsOpen="_deleteOpen"
                   Title="@Localizer["communication.templates.deleteDialog.title"]"
                   Description="@_deleteDescription"
                   Loading="_deleting"
                   OnCancel="CloseDeleteModal"
                   OnConfirm="ExecuteDelete" />

@code {
    private List<EmailTemplate> _templates = new();
    private string _searchText = string.Empty;
    private bool _loading = true;
    private string? _errorMessage;
    private Timer? _debounceTimer;

    // Clone modal state
    private bool _cloneOpen;
    private EmailTemplate? _cloneSource;

    // Delete modal state
    private bool _deleteOpen;
    private bool _deleting;
    private EmailTemplate? _deleteTarget;
    private string _deleteDescription = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadTemplatesAsync();
    }

    private async Task LoadTemplatesAsync()
    {
        _loading = true;
        _errorMessage = null;
        try
        {
            var result = await CommunicationService.GetTemplatesAsync(
                _searchText, null, AppSettings.ProjectSlug);
            _templates = result.Templates;
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _loading = false;
        }
    }

    private void OnSearchInput(ChangeEventArgs e)
    {
        _searchText = e.Value?.ToString() ?? string.Empty;
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(async _ =>
        {
            await InvokeAsync(async () =>
            {
                await LoadTemplatesAsync();
                StateHasChanged();
            });
        }, null, 300, Timeout.Infinite);
    }

    private void EditTemplate(string itemId)
    {
        Navigation.NavigateTo($"/communication/templates/{itemId}");
    }

    // ── Clone ────────────────────────────────────────────────────────────

    private void OpenCloneModal(EmailTemplate template)
    {
        _cloneSource = template;
        _cloneOpen = true;
    }

    private void CloseCloneModal()
    {
        _cloneOpen = false;
        _cloneSource = null;
    }

    private async Task OnTemplateCloned()
    {
        CloseCloneModal();
        await LoadTemplatesAsync();
    }

    // ── Delete ───────────────────────────────────────────────────────────

    private void ConfirmDelete(EmailTemplate template)
    {
        _deleteTarget = template;
        _deleteDescription = Localizer["communication.templates.deleteConfirmation", template.Name];
        _deleteOpen = true;
    }

    private void CloseDeleteModal()
    {
        _deleteOpen = false;
        _deleteTarget = null;
    }

    private async Task ExecuteDelete()
    {
        if (_deleteTarget is null) return;

        _deleting = true;
        try
        {
            await CommunicationService.DeleteTemplateAsync(_deleteTarget.ItemId, AppSettings.ProjectSlug);
            CloseDeleteModal();
            await LoadTemplatesAsync();
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
            CloseDeleteModal();
        }
        finally
        {
            _deleting = false;
        }
    }

    public void Dispose()
    {
        _debounceTimer?.Dispose();
    }
}
```

---

## Page: TemplateEditorPage

Create or edit an email template with a two-panel layout: form on the left, live HTML preview on the right.

```razor
@* Modules/Communication/Pages/TemplateEditorPage.razor *@
@page "/communication/templates/{ItemId}"
@attribute [Authorize]
@inject ICommunicationService CommunicationService
@inject AppSettings AppSettings
@inject ToastService Toast
@inject NavigationManager Navigation
@inject IJSRuntime JS
@inject ILocalizationService Localizer
@implements IDisposable

<h1 class="mb-6 text-2xl font-bold text-gray-900">
    @(_isNew ? Localizer["communication.templates.editor.createTitle"] : Localizer["communication.templates.editor.editTitle"])
</h1>

@if (_loading)
{
    <div class="grid grid-cols-2 gap-6">
        <div class="space-y-3">
            <div class="h-10 animate-pulse rounded bg-gray-200"></div>
            <div class="h-10 animate-pulse rounded bg-gray-200"></div>
            <div class="h-10 animate-pulse rounded bg-gray-200"></div>
            <div class="h-10 animate-pulse rounded bg-gray-200"></div>
            <div class="h-48 animate-pulse rounded bg-gray-200"></div>
        </div>
        <div>
            <div class="h-[500px] animate-pulse rounded bg-gray-200"></div>
        </div>
    </div>
}
else
{
    <div class="grid grid-cols-2 gap-6">
        @* Left panel — form *@
        <div>
            <EditForm Model="_payload" OnValidSubmit="SaveTemplate" class="space-y-4">
                <div>
                    <label class="label">@Localizer["communication.templates.editor.name"]</label>
                    <input type="text" @bind="_payload.Name" class="input" />
                    @if (_validationErrors.ContainsKey("Name"))
                    {
                        <p class="mt-1 text-xs text-red-500">@_validationErrors["Name"]</p>
                    }
                </div>

                <div>
                    <label class="label">@Localizer["communication.templates.editor.purpose"]</label>
                    <input type="text" @bind="_payload.Purpose" class="input" />
                    <p class="mt-1 text-xs text-gray-400">@Localizer["communication.templates.editor.purposeHelper"]</p>
                    @if (_validationErrors.ContainsKey("Purpose"))
                    {
                        <p class="mt-1 text-xs text-red-500">@_validationErrors["Purpose"]</p>
                    }
                </div>

                <div>
                    <label class="label">@Localizer["communication.templates.editor.language"]</label>
                    <input type="text" @bind="_payload.Language" class="input" />
                </div>

                <div>
                    <label class="label">@Localizer["communication.templates.editor.subject"]</label>
                    <input type="text" @bind="_payload.TemplateSubject" class="input" />
                    @if (_validationErrors.ContainsKey("TemplateSubject"))
                    {
                        <p class="mt-1 text-xs text-red-500">@_validationErrors["TemplateSubject"]</p>
                    }
                </div>

                <div>
                    <label class="label">@Localizer["communication.templates.editor.body"]</label>
                    <textarea @bind="_payload.TemplateBody" @bind:event="oninput"
                              @oninput="OnBodyInput"
                              rows="12" class="input font-mono text-sm"></textarea>
                    @if (_validationErrors.ContainsKey("TemplateBody"))
                    {
                        <p class="mt-1 text-xs text-red-500">@_validationErrors["TemplateBody"]</p>
                    }
                </div>

                <button type="submit" class="btn-primary" disabled="@_saving">
                    @if (_saving)
                    {
                        <span class="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-white/30 border-t-white"></span>
                    }
                    @Localizer["common.save"]
                </button>
            </EditForm>

            @if (!string.IsNullOrEmpty(_errorMessage))
            {
                <ErrorAlert Message="@_errorMessage" Class="mt-4" />
            }
        </div>

        @* Right panel — live HTML preview *@
        <div>
            <p class="mb-2 text-sm font-medium text-gray-700">
                @Localizer["communication.templates.editor.preview"]
            </p>
            <iframe @ref="_iframeRef"
                    sandbox="allow-same-origin"
                    class="h-[500px] w-full rounded-lg border border-gray-200"
                    title="@Localizer["communication.templates.editor.previewTitle"]">
            </iframe>
        </div>
    </div>
}

@code {
    [Parameter] public string ItemId { get; set; } = string.Empty;

    private SaveTemplatePayload _payload = new();
    private SaveTemplatePayloadValidator _validator = default!;
    private Dictionary<string, string> _validationErrors = new();
    private ElementReference _iframeRef;
    private Timer? _debounceTimer;
    private bool _isNew;
    private bool _loading = true;
    private bool _saving;
    private string? _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        _validator = new SaveTemplatePayloadValidator(Localizer);
        _payload.ProjectKey = AppSettings.ProjectSlug;
        _isNew = ItemId == "new";

        if (!_isNew)
        {
            try
            {
                var result = await CommunicationService.GetTemplateAsync(ItemId, AppSettings.ProjectSlug);
                _payload.ItemId = result.Template.ItemId;
                _payload.Name = result.Template.Name;
                _payload.Purpose = result.Template.Purpose;
                _payload.Language = result.Template.Language;
                _payload.TemplateSubject = result.Template.TemplateSubject;
                _payload.TemplateBody = result.Template.TemplateBody;
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
        }

        _loading = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !string.IsNullOrEmpty(_payload.TemplateBody))
        {
            await UpdatePreviewAsync(_payload.TemplateBody);
        }
    }

    private void OnBodyInput(ChangeEventArgs e)
    {
        var value = e.Value?.ToString() ?? string.Empty;
        _payload.TemplateBody = value;

        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(async _ =>
        {
            await InvokeAsync(async () =>
            {
                await UpdatePreviewAsync(value);
            });
        }, null, 300, Timeout.Infinite);
    }

    private async Task UpdatePreviewAsync(string html)
    {
        try
        {
            await JS.InvokeVoidAsync("updateIframeSrcDoc", _iframeRef, html);
        }
        catch
        {
            // Ignore JS interop errors during render
        }
    }

    private async Task SaveTemplate()
    {
        var validationResult = await _validator.ValidateAsync(_payload);
        _validationErrors.Clear();

        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                _validationErrors[error.PropertyName] = error.ErrorMessage;
            }
            return;
        }

        _saving = true;
        _errorMessage = null;
        try
        {
            var result = await CommunicationService.SaveTemplateAsync(_payload);
            if (result.IsSuccess)
            {
                Toast.ShowSuccess(Localizer["communication.templates.editor.saveSuccess"]);
                Navigation.NavigateTo("/communication/templates");
            }
            else
            {
                _errorMessage = string.Join(", ", result.Errors.Values);
            }
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _saving = false;
        }
    }

    public void Dispose()
    {
        _debounceTimer?.Dispose();
    }
}
```

**Required JS interop** (add to `wwwroot/index.html` or a separate `.js` file):

```js
// wwwroot/js/iframe-helper.js
window.updateIframeSrcDoc = function (iframeElement, html) {
    if (iframeElement) {
        iframeElement.srcdoc = html;
    }
};
```

> Template body is always HTML. Render previews exclusively in a sandboxed `<iframe>` via JS interop. Never use `MarkupString` or `@((MarkupString)html)` in production UI to render user-supplied HTML.

---

## Modal: TemplateCloneModal

```razor
@* Modules/Communication/Pages/TemplateCloneModal.razor *@
@inject ICommunicationService CommunicationService
@inject AppSettings AppSettings
@inject ILocalizationService Localizer

<Modal IsOpen="IsOpen" Title="@Localizer["communication.templates.cloneDialog.title"]" OnClose="OnClose">
    @if (SourceTemplate is not null)
    {
        <p class="mb-4 text-sm text-gray-600">
            @Localizer["communication.templates.cloneDialog.description", SourceTemplate.Name]
        </p>

        <div class="mb-4">
            <label class="label">@Localizer["communication.templates.cloneDialog.newName"]</label>
            <input type="text" @bind="_newName" class="input" />
            @if (!string.IsNullOrEmpty(_validationError))
            {
                <p class="mt-1 text-xs text-red-500">@_validationError</p>
            }
        </div>

        @if (!string.IsNullOrEmpty(_errorMessage))
        {
            <ErrorAlert Message="@_errorMessage" Class="mb-4" />
        }

        <div class="flex justify-end gap-3">
            <button class="btn-outline" @onclick="OnClose" disabled="@_cloning">
                @Localizer["common.cancel"]
            </button>
            <button class="btn-primary" @onclick="CloneAsync" disabled="@_cloning">
                @if (_cloning)
                {
                    <span class="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-white/30 border-t-white"></span>
                }
                @Localizer["communication.templates.clone"]
            </button>
        </div>
    }
</Modal>

@code {
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EmailTemplate? SourceTemplate { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnCloned { get; set; }

    private string _newName = string.Empty;
    private bool _cloning;
    private string? _errorMessage;
    private string? _validationError;

    private CloneTemplatePayloadValidator _validator = default!;

    protected override void OnInitialized()
    {
        _validator = new CloneTemplatePayloadValidator(Localizer);
    }

    private async Task CloneAsync()
    {
        if (SourceTemplate is null) return;

        var payload = new CloneTemplatePayload
        {
            ItemId = SourceTemplate.ItemId,
            NewName = _newName,
            ProjectKey = AppSettings.ProjectSlug
        };

        var validationResult = await _validator.ValidateAsync(payload);
        if (!validationResult.IsValid)
        {
            _validationError = validationResult.Errors.First().ErrorMessage;
            return;
        }

        _cloning = true;
        _errorMessage = null;
        _validationError = null;
        try
        {
            var result = await CommunicationService.CloneTemplateAsync(payload);
            if (result.IsSuccess)
            {
                _newName = string.Empty;
                await OnCloned.InvokeAsync();
            }
            else
            {
                _errorMessage = string.Join(", ", result.Errors.Values);
            }
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _cloning = false;
        }
    }
}
```

---

## Real-time Notification Pattern

Use timer-based polling as the default strategy. SignalR can be layered in later without changing the component API.

```
Poll interval:     30 seconds (System.Threading.Timer in NotificationBell)
Trigger:           App mount + window focus (via JS interop visibilitychange listener)
On badge click:    Open NotificationList dropdown
On item click:     Mark as read -> update badge count via EventCallback
On "Mark all":     Mark all read -> refetch count via EventCallback
```

For window focus detection, add a JS interop listener:

```js
// wwwroot/js/visibility-helper.js
window.addVisibilityChangeListener = function (dotNetRef) {
    document.addEventListener("visibilitychange", function () {
        if (!document.hidden) {
            dotNetRef.invokeMethodAsync("OnWindowFocused");
        }
    });
};
```

---

## Route Definitions

```razor
@page "/communication/compose"     -> MailComposePage.razor
@page "/communication/mailbox"     -> MailboxPage.razor
@page "/communication/templates"   -> TemplatesPage.razor
@page "/communication/templates/{ItemId}" -> TemplateEditorPage.razor
```

All communication pages use `@attribute [Authorize]` and render within `MainLayout`.

---

## Error Handling

All pages and components must handle errors consistently:

### API Error Pattern

Every service call must be wrapped in try/catch. Display errors with `<ErrorAlert />` inline or `ToastService` for transient feedback:

```csharp
try
{
    var result = await CommunicationService.SomeMethodAsync(payload);
    if (result.IsSuccess)
    {
        Toast.ShowSuccess(Localizer["communication.someAction.success"]);
    }
    else
    {
        // API returned a structured error — display field-level or summary
        _errorMessage = string.Join(", ", result.Errors.Values);
    }
}
catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
{
    // Token expired — redirect to login
    Navigation.NavigateTo("/login", forceLoad: true);
}
catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
{
    _errorMessage = Localizer["common.error.forbidden"];
}
catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
{
    _errorMessage = Localizer["common.error.notFound"];
}
catch (Exception ex)
{
    _errorMessage = ex.Message;
}
finally
{
    _loading = false;
}
```

### Error States by Component Type

| Component Type | Error Display |
|----------------|---------------|
| Pages (data load failure) | `<ErrorAlert />` below the table or content area |
| Forms (submission failure) | `<ErrorAlert />` below the form + per-field validation messages |
| Background polls (NotificationBell) | Silent — catch and ignore to avoid disrupting the UI |
| Modals | `<ErrorAlert />` inside the modal body |
| Delete/Clone actions | `<ErrorAlert />` on parent page after modal closes |

### Validation Error Display

FluentValidation errors are displayed per-field using `<p class="mt-1 text-xs text-red-500">`:

```razor
@if (_validationErrors.ContainsKey("FieldName"))
{
    <p class="mt-1 text-xs text-red-500">@_validationErrors["FieldName"]</p>
}
```

### Empty + Loading States

Every table and list must handle three states:

1. **Loading** — animated pulse placeholders (`animate-pulse rounded bg-gray-200`)
2. **Empty** — centered icon + message + optional action button
3. **Data** — normal content render

---

## Rules

- Never hardcode `projectKey` — always inject from `AppSettings.ProjectSlug`
- The `ConfiguratoinName` property in `NotifyPayload` keeps the API typo — do not rename in C#
- Template body is always HTML — render previews in a sandboxed `<iframe>` via JS interop, never use `MarkupString` in production UI
- All pages must handle loading (animated pulse placeholders), error (`<ErrorAlert />`), and empty states
- All user-visible strings must use `Localizer["key.name"]` — no hardcoded strings, ever
- Use Tailwind theme color classes (`text-primary`, `bg-primary/10`, etc.) — never hardcode colours
- Use the component classes from `Styles/app.css` (`.btn-primary`, `.input`, `.card`, `.badge-*`) for consistent styling
- Use FluentValidation for all form validation with localized error messages
- One service class per concern (`CommunicationService`) — components call service methods, not `HttpClient` directly
- Search inputs must be debounced at 300ms using `System.Threading.Timer`
- Purpose field on templates must be validated with regex `^[a-z0-9-]+$`
- Use `ToastService` for success/info notifications — not browser alerts
- Use `<Modal>` from shared components for all dialogs — not browser dialogs
- Use `<Pagination>` from shared components for all paginated tables
- Use Heroicons (inline SVG) for all icons — no icon library
