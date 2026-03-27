# Communication — Frontend Guide (Blazor)

This file extends `core/frontend-blazor.md` with communication-specific patterns for the communication skill.
Always read `core/frontend-blazor.md` first, then apply the overrides and additions here.

---

## Stack

| Layer | Technology |
|-------|-----------|
| Framework | .NET 10 Blazor WebAssembly (standalone) |
| Component library | MudBlazor |
| State management | Scoped services + event pattern |
| HTTP | `HttpClient` + `DelegatingHandler` |
| Validation | FluentValidation |
| i18n | `ILocalizationService` (remote JSON resources) |
| Icons | MudBlazor built-in icons (Material Design) |

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
│   ├── TemplatesPage.razor             ← list all templates with search
│   ├── TemplateEditorPage.razor        ← create/edit template with HTML preview
│   └── TemplateCloneDialog.razor       ← clone template dialog
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
@implements IDisposable

<MudBadge Content="@BadgeText"
          Visible="@(_unreadCount > 0)"
          Color="Color.Error"
          Overlap="true">
    <MudIconButton Icon="@BellIcon"
                   Color="@(_unreadCount > 0 ? Color.Primary : Color.Default)"
                   aria-label="@Localizer["communication.notifications.bell"]"
                   OnClick="OnBellClicked" />
</MudBadge>

@code {
    [Parameter] public string SubscriptionFilter { get; set; } = string.Empty;
    [Parameter] public EventCallback OnClick { get; set; }

    private int _unreadCount;
    private Timer? _pollTimer;

    private string BellIcon => _unreadCount > 0
        ? Icons.Material.Filled.NotificationsActive
        : Icons.Material.Filled.Notifications;

    private string BadgeText => _unreadCount > 99 ? "99+" : _unreadCount.ToString();

    protected override async Task OnInitializedAsync()
    {
        await FetchUnreadCountAsync();
        _pollTimer = new Timer(async _ => await PollAsync(), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
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

Opens as a popover panel. Lists notifications with read/unread state. Marks individual items or all as read on interaction.

```razor
@* Modules/Communication/Components/NotificationList.razor *@
@inject ICommunicationService CommunicationService
@inject AppSettings AppSettings
@inject ILocalizationService Localizer

<MudPopover Open="@IsOpen" AnchorOrigin="Origin.BottomRight" TransformOrigin="Origin.TopRight"
            Class="pa-0" Style="width: 360px;">
    <div>
        <div class="d-flex align-center justify-space-between pa-3"
             style="border-bottom: 1px solid var(--mud-palette-divider);">
            <MudText Typo="Typo.subtitle1">@Localizer["communication.notifications.title"]</MudText>
            <MudButton Variant="Variant.Text"
                       Size="Size.Small"
                       Color="Color.Primary"
                       Disabled="@_markingAll"
                       OnClick="HandleMarkAllRead">
                @Localizer["communication.notifications.markAllRead"]
            </MudButton>
        </div>

        <div style="max-height: 400px; overflow-y: auto;">
            @if (_loading)
            {
                @for (int i = 0; i < 5; i++)
                {
                    <div class="pa-3" style="border-bottom: 1px solid var(--mud-palette-divider);">
                        <MudSkeleton Width="75%" Height="20px" Class="mb-2" />
                        <MudSkeleton Width="50%" Height="16px" />
                    </div>
                }
            }
            else if (_notifications.Count == 0)
            {
                <div class="pa-6 d-flex flex-column align-center">
                    <MudIcon Icon="@Icons.Material.Filled.NotificationsNone"
                             Size="Size.Large" Color="Color.Default" Class="mb-2" />
                    <MudText Typo="Typo.body2" Color="Color.Secondary">
                        @Localizer["communication.notifications.empty"]
                    </MudText>
                </div>
            }
            else
            {
                <MudList T="Notification" Dense="true" DisablePadding="true">
                    @foreach (var notification in _notifications)
                    {
                        <NotificationItem Notification="@notification"
                                          OnItemClicked="HandleItemClicked" />
                    }
                </MudList>
            }
        </div>
    </div>
</MudPopover>

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

<MudListItem OnClick="@(() => OnItemClicked.InvokeAsync(Notification))"
             Class="@(_isUnread ? "mud-theme-primary" : "")"
             Style="@(_isUnread ? "background-color: var(--mud-palette-primary-lighten);" : "")">
    <div class="d-flex flex-column">
        <MudText Typo="Typo.body2"
                 Style="@(_isUnread ? "font-weight: 600;" : "")">
            @Notification.DenormalizedPayload
        </MudText>
        <MudText Typo="Typo.caption" Color="Color.Secondary">
            @Notification.CreatedTime.ToString("g")
        </MudText>
    </div>
</MudListItem>

@code {
    [Parameter] public Notification Notification { get; set; } = default!;
    [Parameter] public EventCallback<Notification> OnItemClicked { get; set; }

    private bool _isUnread => !Notification.IsRead;
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
@inject ISnackbar Snackbar
@inject ILocalizationService Localizer

<MudText Typo="Typo.h5" Class="mb-4">@Localizer["communication.mail.compose.title"]</MudText>

<MudToggleGroup T="string" @bind-Value="_mode" Color="Color.Primary" Class="mb-4">
    <MudToggleItem Value="@("any")" Text="@Localizer["communication.mail.compose.sendToAny"]" />
    <MudToggleItem Value="@("template")" Text="@Localizer["communication.mail.compose.useTemplate"]" />
</MudToggleGroup>

@if (_mode == "any")
{
    <MudForm @ref="_anyForm" Model="_anyPayload" Validation="_anyValidator.ValidateValue">
        <MudTextField @bind-Value="_recipientInput"
                      Label="@Localizer["communication.mail.compose.recipients"]"
                      Placeholder="@Localizer["communication.mail.compose.recipientsPlaceholder"]"
                      Adornment="Adornment.End"
                      AdornmentIcon="@Icons.Material.Filled.Add"
                      OnAdornmentClick="AddRecipient"
                      OnKeyDown="OnRecipientKeyDown" />

        <MudChipSet T="string" Class="mb-2">
            @foreach (var recipient in _anyPayload.To)
            {
                <MudChip T="string" Color="Color.Primary" OnClose="@(() => RemoveRecipient(recipient))">
                    @recipient
                </MudChip>
            }
        </MudChipSet>

        <MudTextField @bind-Value="_anyPayload.Subject"
                      Label="@Localizer["communication.mail.compose.subject"]"
                      For="@(() => _anyPayload.Subject)"
                      Class="mb-3" />

        <MudTextField @bind-Value="_anyPayload.Body"
                      Label="@Localizer["communication.mail.compose.body"]"
                      For="@(() => _anyPayload.Body)"
                      Lines="8"
                      Class="mb-3" />

        <MudButton Color="Color.Primary"
                   Variant="Variant.Filled"
                   Disabled="@_sending"
                   OnClick="SendToAny">
            @if (_sending)
            {
                <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
            }
            @Localizer["communication.mail.compose.send"]
        </MudButton>
    </MudForm>
}
else
{
    <MudForm @ref="_templateForm" Model="_templatePayload" Validation="_templateValidator.ValidateValue">
        <MudTextField @bind-Value="_templatePayload.UserId"
                      Label="@Localizer["communication.mail.compose.userId"]"
                      For="@(() => _templatePayload.UserId)"
                      Class="mb-3" />

        <MudTextField @bind-Value="_templatePayload.Purpose"
                      Label="@Localizer["communication.mail.compose.purpose"]"
                      For="@(() => _templatePayload.Purpose)"
                      Class="mb-3" />

        <MudText Typo="Typo.subtitle2" Class="mb-2">
            @Localizer["communication.mail.compose.bodyDataContext"]
        </MudText>

        @foreach (var kvp in _bodyDataContext)
        {
            <MudGrid Class="mb-2">
                <MudItem xs="5">
                    <MudTextField Value="@kvp.Key" Label="@Localizer["common.key"]"
                                  ValueChanged="@(v => UpdateContextKey(kvp.Key, v))" />
                </MudItem>
                <MudItem xs="5">
                    <MudTextField @bind-Value="_bodyDataContext[kvp.Key]"
                                  Label="@Localizer["common.value"]" />
                </MudItem>
                <MudItem xs="2" Class="d-flex align-center">
                    <MudIconButton Icon="@Icons.Material.Filled.Delete"
                                   Color="Color.Error"
                                   OnClick="@(() => RemoveContextEntry(kvp.Key))" />
                </MudItem>
            </MudGrid>
        }

        <MudButton Variant="Variant.Text" Color="Color.Primary"
                   StartIcon="@Icons.Material.Filled.Add"
                   OnClick="AddContextEntry" Class="mb-3">
            @Localizer["communication.mail.compose.addVariable"]
        </MudButton>

        <MudButton Color="Color.Primary"
                   Variant="Variant.Filled"
                   Disabled="@_sending"
                   OnClick="SendWithTemplate">
            @if (_sending)
            {
                <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
            }
            @Localizer["communication.mail.compose.send"]
        </MudButton>
    </MudForm>
}

@if (!string.IsNullOrEmpty(_errorMessage))
{
    <MudAlert Severity="Severity.Error" Class="mt-3">@_errorMessage</MudAlert>
}

@code {
    private string _mode = "any";
    private bool _sending;
    private string? _errorMessage;
    private string _recipientInput = string.Empty;

    private MudForm _anyForm = default!;
    private MudForm _templateForm = default!;

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
        await _anyForm.Validate();
        if (!_anyForm.IsValid) return;

        _sending = true;
        _errorMessage = null;
        try
        {
            var result = await CommunicationService.SendToAnyAsync(_anyPayload);
            if (result.IsSuccess)
            {
                Snackbar.Add(Localizer["communication.mail.compose.sendSuccess"], Severity.Success);
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
        await _templateForm.Validate();
        if (!_templateForm.IsValid) return;

        _sending = true;
        _errorMessage = null;
        _templatePayload.BodyDataContext = _bodyDataContext.Count > 0 ? _bodyDataContext : null;
        try
        {
            var result = await CommunicationService.SendWithTemplateAsync(_templatePayload);
            if (result.IsSuccess)
            {
                Snackbar.Add(Localizer["communication.mail.compose.sendSuccess"], Severity.Success);
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

Paginated list of sent/received emails. Click a row to view full email body in a dialog.

```razor
@* Modules/Communication/Pages/MailboxPage.razor *@
@page "/communication/mailbox"
@attribute [Authorize]
@inject ICommunicationService CommunicationService
@inject AppSettings AppSettings
@inject IDialogService DialogService
@inject NavigationManager Navigation
@inject ILocalizationService Localizer

<MudText Typo="Typo.h5" Class="mb-4">@Localizer["communication.mailbox.title"]</MudText>

@if (_loading)
{
    <MudTable Items="@(Enumerable.Range(0, 5))" Hover="true" Dense="true">
        <HeaderContent>
            <MudTh>@Localizer["communication.mailbox.from"]</MudTh>
            <MudTh>@Localizer["communication.mailbox.subject"]</MudTh>
            <MudTh>@Localizer["communication.mailbox.date"]</MudTh>
            <MudTh>@Localizer["communication.mailbox.status"]</MudTh>
        </HeaderContent>
        <RowTemplate>
            <MudTd><MudSkeleton Width="120px" /></MudTd>
            <MudTd><MudSkeleton Width="200px" /></MudTd>
            <MudTd><MudSkeleton Width="100px" /></MudTd>
            <MudTd><MudSkeleton Width="60px" /></MudTd>
        </RowTemplate>
    </MudTable>
}
else if (_mails.Count == 0)
{
    <div class="d-flex flex-column align-center pa-8">
        <MudIcon Icon="@Icons.Material.Filled.MailOutline" Size="Size.Large" Color="Color.Default" Class="mb-2" />
        <MudText Typo="Typo.body1" Color="Color.Secondary" Class="mb-3">
            @Localizer["communication.mailbox.empty"]
        </MudText>
        <MudButton Variant="Variant.Filled" Color="Color.Primary"
                   StartIcon="@Icons.Material.Filled.Edit"
                   Href="/communication/compose">
            @Localizer["communication.mailbox.compose"]
        </MudButton>
    </div>
}
else
{
    <MudTable Items="@_mails" Hover="true" Dense="true"
              CurrentPage="@(_page - 1)" RowsPerPage="@_pageSize"
              OnRowClick="@(e => ShowMailDetail(e.Item))"
              ServerData="@(new Func<TableState, Task<TableData<Mail>>>(LoadMailsServerSide))">
        <HeaderContent>
            <MudTh>@Localizer["communication.mailbox.from"]</MudTh>
            <MudTh>@Localizer["communication.mailbox.subject"]</MudTh>
            <MudTh>@Localizer["communication.mailbox.date"]</MudTh>
            <MudTh>@Localizer["communication.mailbox.status"]</MudTh>
        </HeaderContent>
        <RowTemplate>
            <MudTd>@context.From</MudTd>
            <MudTd>@context.Subject</MudTd>
            <MudTd>@context.SentTime.ToString("g")</MudTd>
            <MudTd>
                <MudChip T="string" Size="Size.Small"
                         Color="@(context.IsRead ? Color.Default : Color.Primary)">
                    @(context.IsRead ? Localizer["communication.mailbox.read"] : Localizer["communication.mailbox.unread"])
                </MudChip>
            </MudTd>
        </RowTemplate>
        <PagerContent>
            <MudTablePager />
        </PagerContent>
    </MudTable>
}

@if (!string.IsNullOrEmpty(_errorMessage))
{
    <MudAlert Severity="Severity.Error" Class="mt-3">@_errorMessage</MudAlert>
}

@code {
    private List<Mail> _mails = new();
    private int _totalCount;
    private int _page = 1;
    private int _pageSize = 10;
    private bool _loading = true;
    private string? _errorMessage;

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

    private async Task<TableData<Mail>> LoadMailsServerSide(TableState state)
    {
        _page = state.Page + 1;
        _pageSize = state.PageSize;
        await LoadMailsAsync();
        return new TableData<Mail> { Items = _mails, TotalItems = _totalCount };
    }

    private async Task ShowMailDetail(Mail mail)
    {
        var parameters = new DialogParameters<MailDetailDialog>
        {
            { x => x.Mail, mail }
        };
        await DialogService.ShowAsync<MailDetailDialog>(
            Localizer["communication.mailbox.detail.title"], parameters,
            new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true });
    }
}
```

**MailDetailDialog** (embedded in same module):

```razor
@* Modules/Communication/Components/MailDetailDialog.razor *@
@inject IJSRuntime JS
@inject ILocalizationService Localizer

<MudDialog>
    <DialogContent>
        <MudText Typo="Typo.subtitle1" Class="mb-2">@Mail.Subject</MudText>
        <MudText Typo="Typo.caption" Class="mb-1">
            @Localizer["communication.mailbox.detail.from"]: @Mail.From
        </MudText>
        <MudText Typo="Typo.caption" Class="mb-3">
            @Localizer["communication.mailbox.detail.date"]: @Mail.SentTime.ToString("f")
        </MudText>
        <MudDivider Class="mb-3" />
        <iframe @ref="_iframeRef" sandbox="allow-same-origin"
                style="width: 100%; min-height: 300px; border: 1px solid var(--mud-palette-divider); border-radius: 4px;"
                title="@Localizer["communication.mailbox.detail.bodyPreview"]"></iframe>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Close">@Localizer["common.close"]</MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public Mail Mail { get; set; } = default!;

    private ElementReference _iframeRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("updateIframeSrcDoc", _iframeRef, Mail.Body);
        }
    }

    private void Close() => MudDialog.Close();
}
```

---

## Page: TemplatesPage

List all email templates with search and sort. Provides actions per row: Edit, Clone, Delete.

```razor
@* Modules/Communication/Pages/TemplatesPage.razor *@
@page "/communication/templates"
@attribute [Authorize]
@inject ICommunicationService CommunicationService
@inject AppSettings AppSettings
@inject IDialogService DialogService
@inject NavigationManager Navigation
@inject ILocalizationService Localizer

<div class="d-flex align-center justify-space-between mb-4">
    <MudText Typo="Typo.h5">@Localizer["communication.templates.title"]</MudText>
    <MudButton Variant="Variant.Filled" Color="Color.Primary"
               StartIcon="@Icons.Material.Filled.Add"
               Href="/communication/templates/new">
        @Localizer["communication.templates.create"]
    </MudButton>
</div>

<MudTextField @bind-Value="_searchText"
              Placeholder="@Localizer["communication.templates.searchPlaceholder"]"
              Adornment="Adornment.Start"
              AdornmentIcon="@Icons.Material.Filled.Search"
              Immediate="true"
              DebounceInterval="300"
              TextChanged="OnSearchChanged"
              Class="mb-4" />

@if (_loading)
{
    <MudTable Items="@(Enumerable.Range(0, 5))" Hover="true" Dense="true">
        <HeaderContent>
            <MudTh>@Localizer["communication.templates.name"]</MudTh>
            <MudTh>@Localizer["communication.templates.purpose"]</MudTh>
            <MudTh>@Localizer["communication.templates.language"]</MudTh>
            <MudTh>@Localizer["communication.templates.lastUpdated"]</MudTh>
            <MudTh>@Localizer["communication.templates.actions"]</MudTh>
        </HeaderContent>
        <RowTemplate>
            <MudTd><MudSkeleton Width="150px" /></MudTd>
            <MudTd><MudSkeleton Width="100px" /></MudTd>
            <MudTd><MudSkeleton Width="60px" /></MudTd>
            <MudTd><MudSkeleton Width="100px" /></MudTd>
            <MudTd><MudSkeleton Width="120px" /></MudTd>
        </RowTemplate>
    </MudTable>
}
else if (_templates.Count == 0)
{
    <div class="d-flex flex-column align-center pa-8">
        <MudIcon Icon="@Icons.Material.Filled.Description" Size="Size.Large" Color="Color.Default" Class="mb-2" />
        <MudText Typo="Typo.body1" Color="Color.Secondary" Class="mb-3">
            @Localizer["communication.templates.empty"]
        </MudText>
        <MudButton Variant="Variant.Filled" Color="Color.Primary"
                   StartIcon="@Icons.Material.Filled.Add"
                   Href="/communication/templates/new">
            @Localizer["communication.templates.create"]
        </MudButton>
    </div>
}
else
{
    <MudTable Items="@_templates" Hover="true" Dense="true">
        <HeaderContent>
            <MudTh>@Localizer["communication.templates.name"]</MudTh>
            <MudTh>@Localizer["communication.templates.purpose"]</MudTh>
            <MudTh>@Localizer["communication.templates.language"]</MudTh>
            <MudTh>@Localizer["communication.templates.lastUpdated"]</MudTh>
            <MudTh>@Localizer["communication.templates.actions"]</MudTh>
        </HeaderContent>
        <RowTemplate>
            <MudTd>@context.Name</MudTd>
            <MudTd><MudChip T="string" Size="Size.Small">@context.Purpose</MudChip></MudTd>
            <MudTd>@context.Language</MudTd>
            <MudTd>@context.LastUpdatedDate.ToString("g")</MudTd>
            <MudTd>
                <MudIconButton Icon="@Icons.Material.Filled.Edit" Size="Size.Small"
                               OnClick="@(() => EditTemplate(context.ItemId))"
                               aria-label="@Localizer["communication.templates.edit"]" />
                <MudIconButton Icon="@Icons.Material.Filled.ContentCopy" Size="Size.Small"
                               OnClick="@(() => OpenCloneDialog(context))"
                               aria-label="@Localizer["communication.templates.clone"]" />
                <MudIconButton Icon="@Icons.Material.Filled.Delete" Size="Size.Small" Color="Color.Error"
                               OnClick="@(() => ConfirmDelete(context))"
                               aria-label="@Localizer["communication.templates.delete"]" />
            </MudTd>
        </RowTemplate>
    </MudTable>
}

@if (!string.IsNullOrEmpty(_errorMessage))
{
    <MudAlert Severity="Severity.Error" Class="mt-3">@_errorMessage</MudAlert>
}

@code {
    private List<EmailTemplate> _templates = new();
    private string _searchText = string.Empty;
    private bool _loading = true;
    private string? _errorMessage;

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

    private async Task OnSearchChanged(string value)
    {
        _searchText = value;
        await LoadTemplatesAsync();
    }

    private void EditTemplate(string itemId)
    {
        Navigation.NavigateTo($"/communication/templates/{itemId}");
    }

    private async Task OpenCloneDialog(EmailTemplate template)
    {
        var parameters = new DialogParameters<TemplateCloneDialog>
        {
            { x => x.SourceTemplate, template }
        };
        var dialog = await DialogService.ShowAsync<TemplateCloneDialog>(
            Localizer["communication.templates.cloneDialog.title"], parameters);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            await LoadTemplatesAsync();
        }
    }

    private async Task ConfirmDelete(EmailTemplate template)
    {
        var parameters = new DialogParameters<ConfirmationModal>
        {
            { x => x.ContentText, Localizer["communication.templates.deleteConfirmation", template.Name] },
            { x => x.ButtonText, Localizer["common.delete"] },
            { x => x.Color, Color.Error }
        };
        var dialog = await DialogService.ShowAsync<ConfirmationModal>(
            Localizer["communication.templates.deleteDialog.title"], parameters);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            try
            {
                await CommunicationService.DeleteTemplateAsync(template.ItemId, AppSettings.ProjectSlug);
                await LoadTemplatesAsync();
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
        }
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
@inject ISnackbar Snackbar
@inject NavigationManager Navigation
@inject IJSRuntime JS
@inject ILocalizationService Localizer

<MudText Typo="Typo.h5" Class="mb-4">
    @(_isNew ? Localizer["communication.templates.editor.createTitle"] : Localizer["communication.templates.editor.editTitle"])
</MudText>

@if (_loading)
{
    <MudGrid>
        <MudItem xs="6">
            <MudSkeleton Height="40px" Class="mb-3" />
            <MudSkeleton Height="40px" Class="mb-3" />
            <MudSkeleton Height="40px" Class="mb-3" />
            <MudSkeleton Height="40px" Class="mb-3" />
            <MudSkeleton Height="200px" />
        </MudItem>
        <MudItem xs="6">
            <MudSkeleton Height="400px" />
        </MudItem>
    </MudGrid>
}
else
{
    <MudGrid>
        @* Left panel — form *@
        <MudItem xs="6">
            <MudForm @ref="_form" Model="_payload" Validation="_validator.ValidateValue">
                <MudTextField @bind-Value="_payload.Name"
                              Label="@Localizer["communication.templates.editor.name"]"
                              For="@(() => _payload.Name)"
                              Class="mb-3" />

                <MudTextField @bind-Value="_payload.Purpose"
                              Label="@Localizer["communication.templates.editor.purpose"]"
                              HelperText="@Localizer["communication.templates.editor.purposeHelper"]"
                              For="@(() => _payload.Purpose)"
                              Class="mb-3" />

                <MudTextField @bind-Value="_payload.Language"
                              Label="@Localizer["communication.templates.editor.language"]"
                              For="@(() => _payload.Language)"
                              Class="mb-3" />

                <MudTextField @bind-Value="_payload.TemplateSubject"
                              Label="@Localizer["communication.templates.editor.subject"]"
                              For="@(() => _payload.TemplateSubject)"
                              Class="mb-3" />

                <MudTextField @bind-Value="_payload.TemplateBody"
                              Label="@Localizer["communication.templates.editor.body"]"
                              For="@(() => _payload.TemplateBody)"
                              Lines="12"
                              Immediate="true"
                              DebounceInterval="300"
                              TextChanged="OnBodyChanged"
                              Class="mb-3" />

                <MudButton Color="Color.Primary"
                           Variant="Variant.Filled"
                           Disabled="@_saving"
                           OnClick="SaveTemplate">
                    @if (_saving)
                    {
                        <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
                    }
                    @Localizer["common.save"]
                </MudButton>
            </MudForm>

            @if (!string.IsNullOrEmpty(_errorMessage))
            {
                <MudAlert Severity="Severity.Error" Class="mt-3">@_errorMessage</MudAlert>
            }
        </MudItem>

        @* Right panel — live HTML preview *@
        <MudItem xs="6">
            <MudText Typo="Typo.subtitle2" Class="mb-2">
                @Localizer["communication.templates.editor.preview"]
            </MudText>
            <iframe @ref="_iframeRef"
                    sandbox="allow-same-origin"
                    style="width: 100%; height: 500px; border: 1px solid var(--mud-palette-divider); border-radius: 4px;"
                    title="@Localizer["communication.templates.editor.previewTitle"]">
            </iframe>
        </MudItem>
    </MudGrid>
}

@code {
    [Parameter] public string ItemId { get; set; } = string.Empty;

    private MudForm _form = default!;
    private SaveTemplatePayload _payload = new();
    private SaveTemplatePayloadValidator _validator = default!;
    private ElementReference _iframeRef;
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

    private async Task OnBodyChanged(string value)
    {
        _payload.TemplateBody = value;
        await UpdatePreviewAsync(value);
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
        await _form.Validate();
        if (!_form.IsValid) return;

        _saving = true;
        _errorMessage = null;
        try
        {
            var result = await CommunicationService.SaveTemplateAsync(_payload);
            if (result.IsSuccess)
            {
                Snackbar.Add(Localizer["communication.templates.editor.saveSuccess"], Severity.Success);
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

## Dialog: TemplateCloneDialog

```razor
@* Modules/Communication/Pages/TemplateCloneDialog.razor *@
@inject ICommunicationService CommunicationService
@inject AppSettings AppSettings
@inject ILocalizationService Localizer

<MudDialog>
    <DialogContent>
        <MudText Typo="Typo.body2" Class="mb-3">
            @Localizer["communication.templates.cloneDialog.description", SourceTemplate.Name]
        </MudText>
        <MudForm @ref="_form" Model="_payload" Validation="_validator.ValidateValue">
            <MudTextField @bind-Value="_payload.NewName"
                          Label="@Localizer["communication.templates.cloneDialog.newName"]"
                          For="@(() => _payload.NewName)" />
        </MudForm>

        @if (!string.IsNullOrEmpty(_errorMessage))
        {
            <MudAlert Severity="Severity.Error" Class="mt-3">@_errorMessage</MudAlert>
        }
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">@Localizer["common.cancel"]</MudButton>
        <MudButton Color="Color.Primary" Variant="Variant.Filled"
                   Disabled="@_cloning" OnClick="CloneAsync">
            @if (_cloning)
            {
                <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
            }
            @Localizer["communication.templates.clone"]
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public EmailTemplate SourceTemplate { get; set; } = default!;

    private MudForm _form = default!;
    private CloneTemplatePayload _payload = new();
    private CloneTemplatePayloadValidator _validator = default!;
    private bool _cloning;
    private string? _errorMessage;

    protected override void OnInitialized()
    {
        _validator = new CloneTemplatePayloadValidator(Localizer);
        _payload.ItemId = SourceTemplate.ItemId;
        _payload.ProjectKey = AppSettings.ProjectSlug;
    }

    private async Task CloneAsync()
    {
        await _form.Validate();
        if (!_form.IsValid) return;

        _cloning = true;
        _errorMessage = null;
        try
        {
            var result = await CommunicationService.CloneTemplateAsync(_payload);
            if (result.IsSuccess)
            {
                MudDialog.Close(DialogResult.Ok(true));
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

    private void Cancel() => MudDialog.Cancel();
}
```

---

## Real-time Notification Pattern

Use timer-based polling as the default strategy. SignalR can be layered in later without changing the component API.

```
Poll interval:     30 seconds (System.Threading.Timer in NotificationBell)
Trigger:           App mount + window focus (via JS interop visibilitychange listener)
On badge click:    Open NotificationList popover
On item click:     Mark as read -> update badge count via EventCallback
On "Mark all":     Mark all read -> refetch count via EventCallback
```

For window focus detection, add a JS interop listener:

```csharp
// In NotificationBell.razor OnAfterRenderAsync
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
```

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

## Rules

- Never hardcode `projectKey` -- always inject from `AppSettings.ProjectSlug`
- The `ConfiguratoinName` property in `NotifyPayload` keeps the API typo -- do not rename in C#
- Template body is always HTML -- render previews in a sandboxed `<iframe>` via JS interop, never use `MarkupString` in production UI
- All pages must handle loading (`<MudSkeleton />`), error (`<MudAlert Severity="Severity.Error">`), and empty states
- All user-visible strings must use `Localizer["key.name"]` -- no hardcoded strings, ever
- Use MudBlazor theme tokens -- never hardcode colours
- Use FluentValidation for all form validation with localized error messages
- One service class per concern (`CommunicationService`) -- components call service methods, not `HttpClient` directly
- Search inputs must be debounced at 300ms using `DebounceInterval`
- Purpose field on templates must be validated with regex `^[a-z0-9-]+$`
