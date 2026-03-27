# Identity & Access — Frontend Guide (Blazor)

This file extends `core/frontend-blazor.md` with auth-specific patterns for the identity-access skill.
Always read `core/frontend-blazor.md` first, then apply the overrides and additions here.

---

## Module Structure

All auth UI lives inside `Modules/Auth/`:

```
Modules/Auth/
├── Components/
│   ├── SigninEmail.razor          ← email + password form
│   ├── SigninSso.razor            ← social login buttons
│   ├── SigninOidc.razor           ← OIDC redirect button
│   ├── SignupForm.razor           ← email-only registration form
│   ├── SetPasswordForm.razor      ← password input with strength indicator
│   ├── ForgotPasswordForm.razor   ← email + captcha form
│   ├── ResetPasswordForm.razor    ← new password form
│   ├── OtpInput.razor             ← digit-by-digit OTP component
│   └── PasswordStrength.razor     ← password strength indicator
├── Pages/
│   ├── SigninPage.razor           ← main login page (conditionally renders method)
│   ├── SignupPage.razor           ← registration page
│   ├── ActivatePage.razor         ← account activation (reads code from URL)
│   ├── VerifyMfaPage.razor        ← MFA OTP entry page
│   ├── ForgotPasswordPage.razor   ← forgot password page
│   ├── ResetPasswordPage.razor    ← reset password page (reads code from URL)
│   ├── SentEmailPage.razor        ← confirmation after forgot-password
│   ├── SuccessPage.razor          ← confirmation after activation
│   └── ActivateFailedPage.razor   ← error for invalid/expired codes
├── Services/
│   └── AuthService.cs             ← raw API calls (no state logic)
└── Models/
    ├── AuthModels.cs              ← request/response models
    └── AuthValidators.cs          ← FluentValidation validators
```

---

## State Management

Use the shared `AuthState` service defined in `core/app-scaffold-blazor.md`. Token storage uses `Blazored.LocalStorage`.

```csharp
// State/AuthState.cs — defined in app-scaffold-blazor.md, referenced here for clarity
public class AuthState
{
    public bool IsAuthenticated { get; private set; }
    public string AccessToken { get; private set; } = string.Empty;
    public string RefreshToken { get; private set; } = string.Empty;
    public UserInfo? User { get; private set; }

    public event Action? OnAuthStateChanged;

    public async Task LoginAsync(string accessToken, string refreshToken) { /* ... */ }
    public async Task SetAccessTokenAsync(string accessToken) { /* ... */ }
    public async Task LogoutAsync() { /* ... */ }
    public async Task InitializeAsync() { /* ... */ }
}
```

Rules:
- `AccessToken` and `RefreshToken` are stored via `ILocalStorageService` — survives page refresh
- Never store tokens in component state or cascading values directly
- `IsAuthenticated` is derived from `!string.IsNullOrEmpty(AccessToken)`
- On logout: call `LogoutAsync()` which clears all auth state and removes from storage
- Components subscribe to `OnAuthStateChanged` if they need to react to login/logout

---

## HTTP Client

All API calls use the shared `HttpClient` from DI (see `core/app-scaffold-blazor.md`). The `TokenDelegatingHandler` automatically attaches auth headers and refreshes the access token on 401.

```csharp
// Usage in service layer — no manual token handling needed
public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly AppSettings _settings;

    public AuthService(HttpClient http, AppSettings settings)
    {
        _http = http;
        _settings = settings;
    }

    public async Task<SigninResponse> SigninAsync(SigninPayload payload)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = payload.Username,
            ["password"] = payload.Password,
            ["client_id"] = _settings.OidcClientId,
        });
        var response = await _http.PostAsync("/idp/v1/Authentication/Token", content);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return SigninResponse.Parse(json);
    }
}
```

---

## OTP Input Component

The OTP input is a Razor component that renders individual `MudTextField` inputs per digit.

```razor
@* Modules/Auth/Components/OtpInput.razor *@
@inject ILocalizationService Localizer

<div class="d-flex gap-2 justify-center">
    @for (int i = 0; i < Length; i++)
    {
        var index = i;
        <MudTextField @ref="_inputs[index]"
                      T="string"
                      Value="@_values[index]"
                      ValueChanged="@(v => OnDigitChangedAsync(index, v))"
                      Variant="Variant.Outlined"
                      MaxLength="1"
                      InputType="InputType.Number"
                      Style="width: 48px; text-align: center;"
                      Class="otp-digit"
                      Disabled="@IsDisabled"
                      @onkeydown="@(e => OnKeyDown(e, index))"
                      Placeholder="@Localizer["auth.otp.digitPlaceholder"]" />
    }
</div>

@if (_hasError)
{
    <MudText Typo="Typo.caption" Color="Color.Error" Align="Align.Center" Class="mt-2">
        @Localizer["auth.otp.invalidCode"]
    </MudText>
}

@code {
    /// <summary>Number of digits. Email OTP = 5, TOTP = 6.</summary>
    [Parameter] public int Length { get; set; } = 6;

    /// <summary>Fired when all digits are entered.</summary>
    [Parameter] public EventCallback<string> OnComplete { get; set; }

    [Parameter] public bool IsDisabled { get; set; }
    [Parameter] public bool HasError { get => _hasError; set => _hasError = value; }

    private MudTextField<string>[] _inputs = default!;
    private string[] _values = default!;
    private bool _hasError;

    protected override void OnInitialized()
    {
        _inputs = new MudTextField<string>[Length];
        _values = new string[Length];
    }

    private async Task OnDigitChangedAsync(int index, string value)
    {
        // Accept only single digit
        if (!string.IsNullOrEmpty(value) && !char.IsDigit(value[0]))
            return;

        _values[index] = value;
        _hasError = false;

        // Auto-focus next input
        if (!string.IsNullOrEmpty(value) && index < Length - 1)
        {
            await _inputs[index + 1].FocusAsync();
        }

        // Auto-submit when all digits filled
        var otp = string.Join("", _values);
        if (otp.Length == Length && _values.All(v => !string.IsNullOrEmpty(v)))
        {
            await OnComplete.InvokeAsync(otp);
        }
    }

    private async Task OnKeyDown(KeyboardEventArgs e, int index)
    {
        // Backspace moves to previous input
        if (e.Key == "Backspace" && string.IsNullOrEmpty(_values[index]) && index > 0)
        {
            _values[index - 1] = string.Empty;
            await _inputs[index - 1].FocusAsync();
        }
    }

    public void Reset()
    {
        for (int i = 0; i < Length; i++)
            _values[i] = string.Empty;
        _hasError = false;
        StateHasChanged();
    }
}
```

Rules:
- Auto-focus next input on each character entry
- Auto-submit the form when all digits are filled
- Backspace on an empty field moves focus to the previous input
- Email OTP: **5 digits** / TOTP (authenticator app): **6 digits**
- Use `Length` parameter: `<OtpInput Length="@(mfaType == "email" ? 5 : 6)" OnComplete="HandleVerify" />`

---

## Password Strength Indicator

Show below the password input on all set-password and reset-password forms.

```razor
@* Modules/Auth/Components/PasswordStrength.razor *@
@inject ILocalizationService Localizer

<MudProgressLinear Color="@GetColor()" Value="@_strengthPercent" Class="mt-2" />
<MudText Typo="Typo.caption" Color="@GetColor()">@GetLabel()</MudText>

<MudList T="string" Dense="true" Class="mt-1">
    <MudListItem Icon="@CheckIcon(_hasMinLength)" IconColor="@CheckColor(_hasMinLength)">
        @Localizer["auth.password.minLength"]
    </MudListItem>
    <MudListItem Icon="@CheckIcon(_hasUpper)" IconColor="@CheckColor(_hasUpper)">
        @Localizer["auth.password.uppercase"]
    </MudListItem>
    <MudListItem Icon="@CheckIcon(_hasLower)" IconColor="@CheckColor(_hasLower)">
        @Localizer["auth.password.lowercase"]
    </MudListItem>
    <MudListItem Icon="@CheckIcon(_hasDigit)" IconColor="@CheckColor(_hasDigit)">
        @Localizer["auth.password.digit"]
    </MudListItem>
    <MudListItem Icon="@CheckIcon(_hasSpecial)" IconColor="@CheckColor(_hasSpecial)">
        @Localizer["auth.password.special"]
    </MudListItem>
</MudList>

@code {
    [Parameter] public string Password { get; set; } = string.Empty;

    /// <summary>True when password meets minimum strength (Strong or above).</summary>
    [Parameter] public EventCallback<bool> IsStrongChanged { get; set; }

    private bool _hasMinLength, _hasUpper, _hasLower, _hasDigit, _hasSpecial;
    private int _strengthPercent;

    protected override async Task OnParametersSetAsync()
    {
        _hasMinLength = Password.Length >= 8;
        _hasUpper = Password.Any(char.IsUpper);
        _hasLower = Password.Any(char.IsLower);
        _hasDigit = Password.Any(char.IsDigit);
        _hasSpecial = Password.Any(c => !char.IsLetterOrDigit(c));
        _strengthPercent = new[] { _hasMinLength, _hasUpper, _hasLower, _hasDigit, _hasSpecial }
            .Count(b => b) * 20;

        var isStrong = _strengthPercent >= 80;
        await IsStrongChanged.InvokeAsync(isStrong);
    }

    private Color GetColor() => _strengthPercent switch
    {
        >= 100 => Color.Success,
        >= 80 => Color.Info,
        >= 60 => Color.Warning,
        _ => Color.Error
    };

    private string GetLabel() => _strengthPercent switch
    {
        >= 100 => Localizer["auth.password.veryStrong"],
        >= 80 => Localizer["auth.password.strong"],
        >= 60 => Localizer["auth.password.fair"],
        _ => Localizer["auth.password.weak"]
    };

    private string CheckIcon(bool met) => met ? Icons.Material.Filled.Check : Icons.Material.Filled.Close;
    private Color CheckColor(bool met) => met ? Color.Success : Color.Error;
}
```

Strength levels:
| Level | Requirements Met | Colour |
|-------|-----------------|--------|
| Weak | 0-2 of 5 | Error (red) |
| Fair | 3 of 5 | Warning (orange) |
| Strong | 4 of 5 | Info (blue) |
| Very Strong | 5 of 5 | Success (green) |

Minimum required: **Strong** (at least 4 of: 8+ chars, uppercase, lowercase, digit, special char). Disable the submit button until `IsStrongChanged` returns `true`.

---

## CAPTCHA Integration

CAPTCHA is optional per project. Check `AppSettings.CaptchaSiteKey` before rendering.

```csharp
var captchaEnabled = !string.IsNullOrEmpty(AppSettings.CaptchaSiteKey);
```

Supported providers (via `AppSettings.CaptchaType`):
- `reCaptcha` — Google reCAPTCHA v2
- `hCaptcha` — hCaptcha

CAPTCHA requires JS interop. Create a minimal wrapper component:

```razor
@* Components/Shared/CaptchaWidget.razor *@
@inject IJSRuntime JS
@inject AppSettings AppSettings
@inject ILocalizationService Localizer

@if (_captchaEnabled)
{
    <div id="captcha-container" class="mt-2 mb-2"></div>
}

@code {
    [Parameter] public EventCallback<string> OnTokenResolved { get; set; }
    [Parameter] public EventCallback OnTokenExpired { get; set; }

    private bool _captchaEnabled;

    protected override void OnInitialized()
    {
        _captchaEnabled = !string.IsNullOrEmpty(AppSettings.CaptchaSiteKey);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _captchaEnabled)
        {
            await JS.InvokeVoidAsync("captchaInterop.render",
                "captcha-container",
                AppSettings.CaptchaSiteKey,
                AppSettings.CaptchaType,
                DotNetObjectReference.Create(this));
        }
    }

    [JSInvokable]
    public async Task OnCaptchaResolved(string token)
    {
        await OnTokenResolved.InvokeAsync(token);
    }

    [JSInvokable]
    public async Task OnCaptchaExpired()
    {
        await OnTokenExpired.InvokeAsync();
    }

    public async Task ResetAsync()
    {
        await JS.InvokeVoidAsync("captchaInterop.reset", "captcha-container");
    }
}
```

JS interop file (`wwwroot/js/captcha-interop.js`):

```javascript
window.captchaInterop = {
    render: function (elementId, siteKey, captchaType, dotNetRef) {
        if (captchaType === 'reCaptcha') {
            grecaptcha.render(elementId, {
                sitekey: siteKey,
                callback: (token) => dotNetRef.invokeMethodAsync('OnCaptchaResolved', token),
                'expired-callback': () => dotNetRef.invokeMethodAsync('OnCaptchaExpired')
            });
        } else if (captchaType === 'hCaptcha') {
            hcaptcha.render(elementId, {
                sitekey: siteKey,
                callback: (token) => dotNetRef.invokeMethodAsync('OnCaptchaResolved', token),
                'expired-callback': () => dotNetRef.invokeMethodAsync('OnCaptchaExpired')
            });
        }
    },
    reset: function (elementId) {
        var el = document.getElementById(elementId);
        if (el) el.innerHTML = '';
    }
};
```

On pages that use CAPTCHA (forgot-password, activate):
- Mount the widget when the page loads
- On forgot-password: show the widget after the user starts typing in the email field
- Pass the resolved token to the API call as `captchaCode`
- If the CAPTCHA token expires before form submission, reset and require re-completion

---

## Protected Routes

Use Blazor's built-in `AuthorizeRouteView` and `CascadingAuthenticationState` from `App.razor` (defined in `core/app-scaffold-blazor.md`).

Protected pages use the `[Authorize]` attribute and `MainLayout`:

```razor
@page "/dashboard"
@attribute [Authorize]

<MudText Typo="Typo.h4">@Localizer["dashboard.title"]</MudText>
```

Public pages use `@layout EmptyLayout` and do **not** have `[Authorize]`:

```razor
@page "/login"
@layout EmptyLayout

@* No [Authorize] attribute *@
<MudText Typo="Typo.h4">@Localizer["auth.login.title"]</MudText>
```

Public routes (no auth required): `/login`, `/signup`, `/activate`, `/activate-failed`, `/success`, `/forgot-password`, `/sent-email`, `/resetpassword`, `/verify-mfa`, `/oidc`

The `App.razor` router handles unauthorized access by redirecting to `/login` via the `<RedirectToLogin />` component in the `<NotAuthorized>` template.

---

## Error Handling

Map API error codes to user-friendly messages using the localizer:

```csharp
// Modules/Auth/Models/AuthErrorMap.cs
public static class AuthErrorMap
{
    private static readonly Dictionary<string, string> ErrorKeys = new()
    {
        ["INVALID_CREDENTIALS"] = "auth.error.invalidCredentials",
        ["EMAIL_PASSWORD_NOT_VALID"] = "auth.error.invalidCredentials",
        ["invalid_request"] = "auth.error.genericError",
        ["ACCOUNT_LOCKED"] = "auth.error.accountLocked",
        ["ACCOUNT_NOT_ACTIVATED"] = "auth.error.accountNotActivated",
        ["INVALID_ACTIVATION_CODE"] = "auth.error.invalidActivationCode",
        ["ACTIVATION_CODE_EXPIRED"] = "auth.error.activationCodeExpired",
        ["INVALID_RESET_CODE"] = "auth.error.invalidResetCode",
        ["RESET_CODE_EXPIRED"] = "auth.error.resetCodeExpired",
        ["EMAIL_NOT_FOUND"] = "auth.error.emailNotFound",
        ["INVALID_OTP"] = "auth.error.invalidOtp",
        ["OTP_EXPIRED"] = "auth.error.otpExpired",
    };

    public static string GetMessage(string errorCode, ILocalizationService localizer)
    {
        var key = ErrorKeys.GetValueOrDefault(errorCode, "auth.error.genericError");
        return localizer[key];
    }

    public static string GetMessage(Dictionary<string, string> errors, ILocalizationService localizer)
    {
        if (errors.Count == 0) return localizer["auth.error.genericError"];
        var firstError = errors.Values.First();
        return ErrorKeys.ContainsKey(firstError)
            ? localizer[ErrorKeys[firstError]]
            : firstError;
    }
}
```

Usage in components:

```razor
@inject ILocalizationService Localizer

@if (!string.IsNullOrEmpty(_errorMessage))
{
    <MudAlert Severity="Severity.Error" Class="mb-4" Dense="true">
        @_errorMessage
    </MudAlert>
}

@code {
    private string? _errorMessage;

    private async Task HandleSubmit()
    {
        try
        {
            _errorMessage = null;
            await _authService.SigninAsync(payload);
        }
        catch (ApiException ex)
        {
            _errorMessage = AuthErrorMap.GetMessage(ex.ErrorCode, Localizer);
        }
    }
}
```

Rules:
- Show field-level errors inline (under the relevant `MudTextField` via `For` / validation) for validation errors
- Show form-level errors in `<MudAlert Severity="Severity.Error">` for API errors
- Never show raw error codes or stack traces to the user
- On 401 during login (not token refresh): show "Invalid credentials" — never "Unauthorized"

---

## Route Definitions

All auth pages use `EmptyLayout` — no sidebar or header.

```razor
@* Modules/Auth/Pages/SigninPage.razor *@
@page "/login"
@layout EmptyLayout

@* Modules/Auth/Pages/SignupPage.razor *@
@page "/signup"
@layout EmptyLayout

@* Modules/Auth/Pages/ActivatePage.razor *@
@page "/activate"
@layout EmptyLayout

@* Modules/Auth/Pages/ActivateFailedPage.razor *@
@page "/activate-failed"
@layout EmptyLayout

@* Modules/Auth/Pages/SuccessPage.razor *@
@page "/success"
@layout EmptyLayout

@* Modules/Auth/Pages/ForgotPasswordPage.razor *@
@page "/forgot-password"
@layout EmptyLayout

@* Modules/Auth/Pages/SentEmailPage.razor *@
@page "/sent-email"
@layout EmptyLayout

@* Modules/Auth/Pages/ResetPasswordPage.razor *@
@page "/resetpassword"
@layout EmptyLayout

@* Modules/Auth/Pages/VerifyMfaPage.razor *@
@page "/verify-mfa"
@layout EmptyLayout

@* Modules/Auth/Pages/OidcCallback.razor *@
@page "/oidc"
@layout EmptyLayout
```

---

## Service Layer

`AuthService.cs` contains all raw API calls. Components call service methods — never `HttpClient` directly.

```csharp
// Modules/Auth/Services/AuthService.cs
using System.Net.Http.Json;
using System.Text.Json;

public interface IAuthService
{
    Task<SigninResponse> SigninAsync(SigninPayload payload);
    Task<SigninResponse> SigninWithMfaAsync(MfaSigninPayload payload);
    Task<SigninResponse> SigninWithAuthorizationCodeAsync(AuthorizationCodePayload payload);
    Task<BaseResponse> SignupAsync(SignupPayload payload);
    Task<ValidateActivationCodeResponse> ValidateActivationCodeAsync(ValidateActivationCodePayload payload);
    Task<BaseResponse> ActivateAsync(ActivateUserPayload payload);
    Task<BaseResponse> ForgotPasswordAsync(ForgotPasswordPayload payload);
    Task<BaseResponse> ResetPasswordAsync(ResetPasswordPayload payload);
    Task<SigninResponse> VerifyMfaAsync(MfaSigninPayload payload);
    Task<LoginOptionsResponse> GetLoginOptionsAsync();
    Task<BaseResponse> ResendOtpAsync(ResendOtpPayload payload);
    Task LogoutAsync(LogoutPayload payload);
}

public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly AppSettings _settings;

    public AuthService(HttpClient http, AppSettings settings)
    {
        _http = http;
        _settings = settings;
    }

    public async Task<SigninResponse> SigninAsync(SigninPayload payload)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = payload.Username,
            ["password"] = payload.Password,
            ["client_id"] = _settings.OidcClientId,
        });
        var response = await _http.PostAsync("/idp/v1/Authentication/Token", content);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return SigninResponse.Parse(json);
    }

    public async Task<SigninResponse> SigninWithMfaAsync(MfaSigninPayload payload)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "mfa_code",
            ["client_id"] = _settings.OidcClientId,
            ["mfa_id"] = payload.MfaId,
            ["mfa_type"] = payload.MfaType,
            ["otp"] = payload.Otp,
        });
        var response = await _http.PostAsync("/idp/v1/Authentication/Token", content);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return SigninResponse.Parse(json);
    }

    public async Task<SigninResponse> SigninWithAuthorizationCodeAsync(AuthorizationCodePayload payload)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = _settings.OidcClientId,
            ["code"] = payload.Code,
            ["redirect_uri"] = _settings.OidcRedirectUri,
        });
        var response = await _http.PostAsync("/idp/v1/Authentication/Token", content);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return SigninResponse.Parse(json);
    }

    public async Task<BaseResponse> SignupAsync(SignupPayload payload)
    {
        var response = await _http.PostAsJsonAsync("/idp/v1/User/Create", payload);
        return await response.Content.ReadFromJsonAsync<BaseResponse>()
            ?? throw new InvalidOperationException("Invalid response");
    }

    public async Task<ValidateActivationCodeResponse> ValidateActivationCodeAsync(
        ValidateActivationCodePayload payload)
    {
        var response = await _http.PostAsJsonAsync("/idp/v1/User/ValidateActivationCode", payload);
        return await response.Content.ReadFromJsonAsync<ValidateActivationCodeResponse>()
            ?? throw new InvalidOperationException("Invalid response");
    }

    public async Task<BaseResponse> ActivateAsync(ActivateUserPayload payload)
    {
        var response = await _http.PostAsJsonAsync("/idp/v1/User/Activate", payload);
        return await response.Content.ReadFromJsonAsync<BaseResponse>()
            ?? throw new InvalidOperationException("Invalid response");
    }

    public async Task<BaseResponse> ForgotPasswordAsync(ForgotPasswordPayload payload)
    {
        var response = await _http.PostAsJsonAsync("/idp/v1/User/ForgotPassword", payload);
        return await response.Content.ReadFromJsonAsync<BaseResponse>()
            ?? throw new InvalidOperationException("Invalid response");
    }

    public async Task<BaseResponse> ResetPasswordAsync(ResetPasswordPayload payload)
    {
        var response = await _http.PostAsJsonAsync("/idp/v1/User/ResetPassword", payload);
        return await response.Content.ReadFromJsonAsync<BaseResponse>()
            ?? throw new InvalidOperationException("Invalid response");
    }

    public async Task<SigninResponse> VerifyMfaAsync(MfaSigninPayload payload)
    {
        return await SigninWithMfaAsync(payload);
    }

    public async Task<LoginOptionsResponse> GetLoginOptionsAsync()
    {
        var response = await _http.PostAsJsonAsync(
            "/idp/v1/Authentication/GetLoginOptions",
            new { projectKey = _settings.ProjectSlug });
        return await response.Content.ReadFromJsonAsync<LoginOptionsResponse>()
            ?? throw new InvalidOperationException("Invalid response");
    }

    public async Task<BaseResponse> ResendOtpAsync(ResendOtpPayload payload)
    {
        var response = await _http.PostAsJsonAsync("/idp/v1/Mfa/ResendOtp", payload);
        return await response.Content.ReadFromJsonAsync<BaseResponse>()
            ?? throw new InvalidOperationException("Invalid response");
    }

    public async Task LogoutAsync(LogoutPayload payload)
    {
        await _http.PostAsJsonAsync("/idp/v1/Authentication/Logout", payload);
    }
}
```

Register in `Program.cs`:

```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
```

---

## C# Models

```csharp
// Modules/Auth/Models/AuthModels.cs
using System.Text.Json;
using System.Text.Json.Serialization;

// --- Base ---

public class BaseResponse
{
    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; set; }

    [JsonPropertyName("errors")]
    public Dictionary<string, string> Errors { get; set; } = new();
}

// --- Payloads (sent to API) ---

public record SigninPayload(string Username, string Password);

public record MfaSigninPayload(string MfaId, string MfaType, string Otp);

public record AuthorizationCodePayload(string Code);

public record SignupPayload(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("projectKey")] string ProjectKey,
    [property: JsonPropertyName("mailPurpose")] string MailPurpose = "activation"
);

public record ValidateActivationCodePayload(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("projectKey")] string ProjectKey
);

public record ActivateUserPayload(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("projectKey")] string ProjectKey,
    [property: JsonPropertyName("captchaCode")] string? CaptchaCode = null,
    [property: JsonPropertyName("mailPurpose")] string? MailPurpose = null,
    [property: JsonPropertyName("preventPostEvent")] bool PreventPostEvent = false
);

public record ForgotPasswordPayload(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("projectKey")] string ProjectKey,
    [property: JsonPropertyName("captchaCode")] string? CaptchaCode = null
);

public record ResetPasswordPayload(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("projectKey")] string ProjectKey
);

public record ResendOtpPayload(
    [property: JsonPropertyName("mfaId")] string MfaId,
    [property: JsonPropertyName("projectKey")] string ProjectKey
);

public record LogoutPayload(
    [property: JsonPropertyName("refreshToken")] string RefreshToken
);

/// <summary>
/// Notification configuration name. The field name preserves the API typo — do not correct.
/// </summary>
public record NotificationPayload(
    [property: JsonPropertyName("configuratoinName")] string? ConfiguratoinName = null
);

// --- Responses (from API) ---

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("id_token")]
    public string? IdToken { get; set; }
}

public class MfaResponse
{
    [JsonPropertyName("enable_mfa")]
    public bool EnableMfa { get; set; }

    [JsonPropertyName("mfaType")]
    public string MfaType { get; set; } = string.Empty;

    [JsonPropertyName("mfaId")]
    public string MfaId { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Discriminated union for signin responses. Use <see cref="IsMfa"/> to check the type.
/// </summary>
public class SigninResponse
{
    public bool IsMfa { get; private set; }
    public TokenResponse? Token { get; private set; }
    public MfaResponse? Mfa { get; private set; }

    public static SigninResponse Parse(JsonElement json)
    {
        if (json.TryGetProperty("enable_mfa", out var mfaProp) && mfaProp.GetBoolean())
        {
            return new SigninResponse
            {
                IsMfa = true,
                Mfa = JsonSerializer.Deserialize<MfaResponse>(json.GetRawText())
            };
        }

        return new SigninResponse
        {
            IsMfa = false,
            Token = JsonSerializer.Deserialize<TokenResponse>(json.GetRawText())
        };
    }
}

public class ValidateActivationCodeResponse : BaseResponse
{
    [JsonPropertyName("isValid")]
    public bool IsValid { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}

public class LoginOptionsResponse : BaseResponse
{
    [JsonPropertyName("loginOptions")]
    public List<LoginOption> LoginOptions { get; set; } = new();
}

public class LoginOption
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("providers")]
    public List<string> Providers { get; set; } = new();
}
```

---

## FluentValidation Validators

```csharp
// Modules/Auth/Models/AuthValidators.cs
using FluentValidation;

public class SigninValidator : AbstractValidator<SigninFormModel>
{
    public SigninValidator(ILocalizationService localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(localizer["validation.email.required"])
            .EmailAddress().WithMessage(localizer["validation.email.invalid"]);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(localizer["validation.password.required"]);
    }
}

public class SignupValidator : AbstractValidator<SignupFormModel>
{
    public SignupValidator(ILocalizationService localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(localizer["validation.email.required"])
            .EmailAddress().WithMessage(localizer["validation.email.invalid"]);
    }
}

public class SetPasswordValidator : AbstractValidator<SetPasswordFormModel>
{
    public SetPasswordValidator(ILocalizationService localizer)
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(localizer["validation.password.required"])
            .MinimumLength(8).WithMessage(localizer["validation.password.minLength"])
            .Matches("[A-Z]").WithMessage(localizer["auth.password.uppercase"])
            .Matches("[a-z]").WithMessage(localizer["auth.password.lowercase"])
            .Matches("[0-9]").WithMessage(localizer["auth.password.digit"])
            .Matches("[^a-zA-Z0-9]").WithMessage(localizer["auth.password.special"]);

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage(localizer["validation.confirmPassword.required"])
            .Equal(x => x.Password).WithMessage(localizer["validation.confirmPassword.mismatch"]);
    }
}

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordFormModel>
{
    public ForgotPasswordValidator(ILocalizationService localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(localizer["validation.email.required"])
            .EmailAddress().WithMessage(localizer["validation.email.invalid"]);
    }
}

// --- Form Models (used by validators, not sent to API directly) ---

public class SigninFormModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class SignupFormModel
{
    public string Email { get; set; } = string.Empty;
}

public class SetPasswordFormModel
{
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ForgotPasswordFormModel
{
    public string Email { get; set; } = string.Empty;
}
```

---

## Localization

Every user-visible string must use `Localizer["key.name"]`. No hardcoded strings anywhere.

### Usage Pattern

```razor
@inject ILocalizationService Localizer

@* Correct *@
<MudText Typo="Typo.h4">@Localizer["auth.login.title"]</MudText>
<MudTextField Label="@Localizer["auth.login.emailLabel"]" />
<MudButton Color="Color.Primary">@Localizer["common.submit"]</MudButton>

@* Wrong — hardcoded string *@
<MudText Typo="Typo.h4">Sign In</MudText>
```

### Key Naming Convention

```
{module}.{context}.{element}

auth.login.title
auth.login.emailLabel
auth.login.passwordLabel
auth.login.forgotPassword
auth.signup.title
auth.signup.emailLabel
auth.activate.title
auth.activate.settingPassword
auth.mfa.title
auth.mfa.enterCode
auth.mfa.resend
auth.password.minLength
auth.password.uppercase
auth.password.lowercase
auth.password.digit
auth.password.special
auth.password.weak
auth.password.fair
auth.password.strong
auth.password.veryStrong
auth.error.invalidCredentials
auth.error.genericError
auth.error.accountLocked
common.submit
common.cancel
common.loading
```

### Key Lookup Before Creation

Before writing any component, Claude must:

1. **List all user-visible strings** in the planned component
2. **Call `get-keys-by-names`** with the candidate key names to check which already exist
3. **Reuse existing keys** — do not create duplicates
4. **Call `save-keys`** (batch) to create only the missing keys
5. **Then generate the component** using the confirmed key names

### Validation Error Messages

Validation messages must also use the localizer:

```csharp
// Wrong
RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required");

// Correct
RuleFor(x => x.Email).NotEmpty().WithMessage(localizer["validation.email.required"]);
```

---

## Example Pages

### SigninPage

```razor
@* Modules/Auth/Pages/SigninPage.razor *@
@page "/login"
@layout EmptyLayout
@inject IAuthService AuthService
@inject AuthState AuthState
@inject NavigationManager Navigation
@inject ILocalizationService Localizer
@inject ISnackbar Snackbar
@inject AppSettings AppSettings

<MudContainer MaxWidth="MaxWidth.Small" Class="d-flex align-center" Style="min-height: 100vh;">
    <MudPaper Elevation="3" Class="pa-8 rounded-lg" Style="width: 100%;">
        <MudText Typo="Typo.h4" Align="Align.Center" Class="mb-6">
            @Localizer["auth.login.title"]
        </MudText>

        @if (!string.IsNullOrEmpty(_errorMessage))
        {
            <MudAlert Severity="Severity.Error" Class="mb-4" Dense="true">
                @_errorMessage
            </MudAlert>
        }

        @if (_isLoadingOptions)
        {
            <MudSkeleton SkeletonType="SkeletonType.Rectangle" Height="200px" />
        }
        else
        {
            @if (_loginOptions.Any(o => o.Type == "Email"))
            {
                <SigninEmail OnSignin="HandleSignin" IsLoading="_isLoading" />
            }

            @if (_loginOptions.Any(o => o.Type == "SocialLogin"))
            {
                <MudDivider Class="my-4" />
                <SigninSso Providers="@_loginOptions.First(o => o.Type == "SocialLogin").Providers" />
            }

            @if (_loginOptions.Any(o => o.Type == "SSO"))
            {
                <MudDivider Class="my-4" />
                <SigninOidc />
            }
        }

        <MudLink Href="/signup" Typo="Typo.body2" Class="mt-4 d-block text-center">
            @Localizer["auth.login.noAccount"]
        </MudLink>
    </MudPaper>
</MudContainer>

@code {
    private List<LoginOption> _loginOptions = new();
    private bool _isLoadingOptions = true;
    private bool _isLoading;
    private string? _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        if (AuthState.IsAuthenticated)
        {
            Navigation.NavigateTo("/", forceLoad: false);
            return;
        }

        try
        {
            var response = await AuthService.GetLoginOptionsAsync();
            _loginOptions = response.LoginOptions;
        }
        catch
        {
            _errorMessage = Localizer["auth.error.genericError"];
        }
        finally
        {
            _isLoadingOptions = false;
        }
    }

    private async Task HandleSignin(SigninPayload payload)
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;
            var result = await AuthService.SigninAsync(payload);

            if (result.IsMfa)
            {
                Navigation.NavigateTo(
                    $"/verify-mfa?mfaId={result.Mfa!.MfaId}&mfaType={result.Mfa.MfaType}");
            }
            else
            {
                await AuthState.LoginAsync(result.Token!.AccessToken, result.Token.RefreshToken);
                Navigation.NavigateTo("/", forceLoad: false);
            }
        }
        catch (HttpRequestException ex)
        {
            _errorMessage = AuthErrorMap.GetMessage("INVALID_CREDENTIALS", Localizer);
        }
        finally
        {
            _isLoading = false;
        }
    }
}
```

### SigninEmail Component

```razor
@* Modules/Auth/Components/SigninEmail.razor *@
@inject ILocalizationService Localizer

<MudForm @ref="_form" Model="_model" Validation="_validator.ValidateValue">
    <MudTextField @bind-Value="_model.Email"
                  Label="@Localizer["auth.login.emailLabel"]"
                  For="@(() => _model.Email)"
                  Variant="Variant.Outlined"
                  InputType="InputType.Email"
                  Class="mb-4"
                  Immediate="true" />

    <MudTextField @bind-Value="_model.Password"
                  Label="@Localizer["auth.login.passwordLabel"]"
                  For="@(() => _model.Password)"
                  Variant="Variant.Outlined"
                  InputType="@(_showPassword ? InputType.Text : InputType.Password)"
                  Adornment="Adornment.End"
                  AdornmentIcon="@(_showPassword ? Icons.Material.Filled.Visibility : Icons.Material.Filled.VisibilityOff)"
                  OnAdornmentClick="() => _showPassword = !_showPassword"
                  Class="mb-2" />

    <MudLink Href="/forgot-password" Typo="Typo.body2" Class="d-block text-right mb-4">
        @Localizer["auth.login.forgotPassword"]
    </MudLink>

    <MudButton Color="Color.Primary"
               Variant="Variant.Filled"
               FullWidth="true"
               OnClick="HandleSubmit"
               Disabled="@IsLoading">
        @if (IsLoading)
        {
            <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
        }
        @Localizer["auth.login.signInButton"]
    </MudButton>
</MudForm>

@code {
    [Parameter] public EventCallback<SigninPayload> OnSignin { get; set; }
    [Parameter] public bool IsLoading { get; set; }

    private MudForm _form = default!;
    private SigninFormModel _model = new();
    private SigninValidator _validator = default!;
    private bool _showPassword;

    [Inject] private ILocalizationService _localizer { get; set; } = default!;

    protected override void OnInitialized()
    {
        _validator = new SigninValidator(_localizer);
    }

    private async Task HandleSubmit()
    {
        await _form.Validate();
        if (_form.IsValid)
        {
            await OnSignin.InvokeAsync(new SigninPayload(_model.Email, _model.Password));
        }
    }
}
```

### VerifyMfaPage

```razor
@* Modules/Auth/Pages/VerifyMfaPage.razor *@
@page "/verify-mfa"
@layout EmptyLayout
@inject IAuthService AuthService
@inject AuthState AuthState
@inject NavigationManager Navigation
@inject ILocalizationService Localizer

<MudContainer MaxWidth="MaxWidth.ExtraSmall" Class="d-flex align-center" Style="min-height: 100vh;">
    <MudPaper Elevation="3" Class="pa-8 rounded-lg" Style="width: 100%;">
        <MudText Typo="Typo.h5" Align="Align.Center" Class="mb-2">
            @Localizer["auth.mfa.title"]
        </MudText>
        <MudText Typo="Typo.body2" Align="Align.Center" Color="Color.Secondary" Class="mb-6">
            @Localizer["auth.mfa.enterCode"]
        </MudText>

        @if (!string.IsNullOrEmpty(_errorMessage))
        {
            <MudAlert Severity="Severity.Error" Class="mb-4" Dense="true">
                @_errorMessage
            </MudAlert>
        }

        <OtpInput @ref="_otpInput"
                  Length="@(_mfaType == "email" ? 5 : 6)"
                  OnComplete="HandleVerify"
                  IsDisabled="_isLoading" />

        @if (_isLoading)
        {
            <MudProgressLinear Indeterminate="true" Color="Color.Primary" Class="mt-4" />
        }

        <MudButton Variant="Variant.Text"
                   Color="Color.Primary"
                   FullWidth="true"
                   Class="mt-4"
                   OnClick="HandleResendOtp"
                   Disabled="_isResending || _mfaType == "authenticator"">
            @Localizer["auth.mfa.resend"]
        </MudButton>
    </MudPaper>
</MudContainer>

@code {
    [SupplyParameterFromQuery] public string? MfaId { get; set; }
    [SupplyParameterFromQuery] public string? MfaType { get; set; }

    private OtpInput _otpInput = default!;
    private string _mfaType = "email";
    private bool _isLoading;
    private bool _isResending;
    private string? _errorMessage;

    [Inject] private AppSettings AppSettings { get; set; } = default!;

    protected override void OnInitialized()
    {
        if (string.IsNullOrEmpty(MfaId))
        {
            Navigation.NavigateTo("/login");
            return;
        }
        _mfaType = MfaType ?? "email";
    }

    private async Task HandleVerify(string otp)
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;
            var result = await AuthService.VerifyMfaAsync(
                new MfaSigninPayload(MfaId!, _mfaType, otp));

            if (result.Token is not null)
            {
                await AuthState.LoginAsync(result.Token.AccessToken, result.Token.RefreshToken);
                Navigation.NavigateTo("/", forceLoad: false);
            }
        }
        catch
        {
            _errorMessage = Localizer["auth.error.invalidOtp"];
            _otpInput.Reset();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task HandleResendOtp()
    {
        try
        {
            _isResending = true;
            await AuthService.ResendOtpAsync(
                new ResendOtpPayload(MfaId!, AppSettings.ProjectSlug));
        }
        finally
        {
            _isResending = false;
        }
    }
}
```

### SetPasswordForm (used in Activate and ResetPassword)

```razor
@* Modules/Auth/Components/SetPasswordForm.razor *@
@inject ILocalizationService Localizer

<MudForm @ref="_form" Model="_model" Validation="_validator.ValidateValue">
    <MudTextField @bind-Value="_model.Password"
                  Label="@Localizer["auth.setPassword.newPasswordLabel"]"
                  For="@(() => _model.Password)"
                  Variant="Variant.Outlined"
                  InputType="@(_showPassword ? InputType.Text : InputType.Password)"
                  Adornment="Adornment.End"
                  AdornmentIcon="@(_showPassword ? Icons.Material.Filled.Visibility : Icons.Material.Filled.VisibilityOff)"
                  OnAdornmentClick="() => _showPassword = !_showPassword"
                  Immediate="true"
                  Class="mb-2" />

    <PasswordStrength Password="@_model.Password" IsStrongChanged="@(val => _isPasswordStrong = val)" />

    <MudTextField @bind-Value="_model.ConfirmPassword"
                  Label="@Localizer["auth.setPassword.confirmPasswordLabel"]"
                  For="@(() => _model.ConfirmPassword)"
                  Variant="Variant.Outlined"
                  InputType="InputType.Password"
                  Class="mt-4 mb-4" />

    <MudButton Color="Color.Primary"
               Variant="Variant.Filled"
               FullWidth="true"
               OnClick="HandleSubmit"
               Disabled="@(!_isPasswordStrong || IsLoading)">
        @if (IsLoading)
        {
            <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
        }
        @Localizer["auth.setPassword.submitButton"]
    </MudButton>
</MudForm>

@code {
    [Parameter] public EventCallback<string> OnSubmit { get; set; }
    [Parameter] public bool IsLoading { get; set; }

    private MudForm _form = default!;
    private SetPasswordFormModel _model = new();
    private SetPasswordValidator _validator = default!;
    private bool _showPassword;
    private bool _isPasswordStrong;

    [Inject] private ILocalizationService _localizer { get; set; } = default!;

    protected override void OnInitialized()
    {
        _validator = new SetPasswordValidator(_localizer);
    }

    private async Task HandleSubmit()
    {
        await _form.Validate();
        if (_form.IsValid && _isPasswordStrong)
        {
            await OnSubmit.InvokeAsync(_model.Password);
        }
    }
}
```

---

## Rules Specific to Auth Module

- Use MudBlazor components — never raw HTML where MudBlazor has an equivalent
- Use FluentValidation for all form validation — validators receive `ILocalizationService` via constructor
- Use `ILocalizationService` for every user-visible string — no hardcoded strings, ever
- Look up existing localization keys with `get-keys-by-names` before creating new ones
- Handle loading, error, and empty states on all pages
- One service class per concern (`AuthService`) — components call service methods, not `HttpClient` directly
- Never store tokens in component `@code` blocks — always use `AuthState`
- CAPTCHA requires JS interop — keep the interop minimal and isolated in a single component
- All auth pages use `EmptyLayout`, not `MainLayout`
- MFA type determines OTP length: `email` = 5 digits, `authenticator` = 6 digits
- The `configuratoinName` field preserves the API typo — do not correct the spelling in C# models
- Disable submit buttons during loading — show `MudProgressCircular` inside the button
- Password submit must be disabled until the password strength indicator reports Strong or above
