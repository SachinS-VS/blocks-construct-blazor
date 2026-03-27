# Identity & Access — Frontend Guide (Blazor + Tailwind)

This file extends `core/frontend-blazor-tailwind.md` with auth-specific patterns for the identity-access skill.
Always read `core/frontend-blazor-tailwind.md` first, then apply the overrides and additions here.

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

Use the shared `AuthState` service defined in `core/app-scaffold-blazor-tailwind.md`. Token storage uses `Blazored.LocalStorage`.

```csharp
// State/AuthState.cs — defined in app-scaffold, referenced here for clarity
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

All API calls use the shared `HttpClient` from DI (see `core/app-scaffold-blazor-tailwind.md`). The `TokenDelegatingHandler` automatically attaches auth headers and refreshes the access token on 401.

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

The OTP input renders individual `<input>` elements per digit with Tailwind styling and JS interop for focus management.

```razor
@* Modules/Auth/Components/OtpInput.razor *@
@inject ILocalizationService Localizer
@inject IJSRuntime JS

<div class="flex items-center justify-center gap-2">
    @for (int i = 0; i < Length; i++)
    {
        var index = i;
        <input @ref="_inputRefs[index]"
               type="text"
               inputmode="numeric"
               maxlength="1"
               value="@_values[index]"
               @oninput="@(e => OnDigitChangedAsync(index, e.Value?.ToString()))"
               @onkeydown="@(e => OnKeyDown(e, index))"
               disabled="@IsDisabled"
               placeholder="@Localizer["auth.otp.digitPlaceholder"]"
               class="h-12 w-12 rounded-md border border-gray-300 text-center text-lg font-semibold
                      focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary
                      disabled:bg-gray-100 disabled:text-gray-400" />
    }
</div>

@if (_hasError)
{
    <p class="mt-2 text-center text-xs text-red-500">
        @Localizer["auth.otp.invalidCode"]
    </p>
}

@code {
    /// <summary>Number of digits. Email OTP = 5, TOTP = 6.</summary>
    [Parameter] public int Length { get; set; } = 6;

    /// <summary>Fired when all digits are entered.</summary>
    [Parameter] public EventCallback<string> OnComplete { get; set; }

    [Parameter] public bool IsDisabled { get; set; }
    [Parameter] public bool HasError { get => _hasError; set => _hasError = value; }

    private ElementReference[] _inputRefs = default!;
    private string[] _values = default!;
    private bool _hasError;

    protected override void OnInitialized()
    {
        _inputRefs = new ElementReference[Length];
        _values = new string[Length];
    }

    private async Task OnDigitChangedAsync(int index, string? value)
    {
        // Accept only single digit
        if (!string.IsNullOrEmpty(value) && !char.IsDigit(value[0]))
            return;

        _values[index] = value ?? string.Empty;
        _hasError = false;

        // Auto-focus next input
        if (!string.IsNullOrEmpty(value) && index < Length - 1)
        {
            await _inputRefs[index + 1].FocusAsync();
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
            await _inputRefs[index - 1].FocusAsync();
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
- Use `Length` parameter: `<OtpInput Length="@(_mfaType == "email" ? 5 : 6)" OnComplete="HandleVerify" />`

---

## Password Strength Indicator

Show below the password input on all set-password and reset-password forms.

```razor
@* Modules/Auth/Components/PasswordStrength.razor *@
@inject ILocalizationService Localizer

<div class="mt-2">
    @* Progress bar *@
    <div class="h-2 w-full rounded-full bg-gray-200">
        <div class="h-2 rounded-full transition-all duration-300 @GetBarColor()"
             style="width: @(_strengthPercent)%"></div>
    </div>
    <p class="mt-1 text-xs font-medium @GetTextColor()">@GetLabel()</p>
</div>

<ul class="mt-2 space-y-1">
    <li class="flex items-center gap-2 text-xs @(_hasMinLength ? "text-green-600" : "text-red-500")">
        @if (_hasMinLength)
        {
            <svg class="h-4 w-4 shrink-0" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="m4.5 12.75 6 6 9-13.5" />
            </svg>
        }
        else
        {
            <svg class="h-4 w-4 shrink-0" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M6 18 18 6M6 6l12 12" />
            </svg>
        }
        @Localizer["auth.password.minLength"]
    </li>
    <li class="flex items-center gap-2 text-xs @(_hasUpper ? "text-green-600" : "text-red-500")">
        @if (_hasUpper)
        {
            <svg class="h-4 w-4 shrink-0" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="m4.5 12.75 6 6 9-13.5" />
            </svg>
        }
        else
        {
            <svg class="h-4 w-4 shrink-0" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M6 18 18 6M6 6l12 12" />
            </svg>
        }
        @Localizer["auth.password.uppercase"]
    </li>
    <li class="flex items-center gap-2 text-xs @(_hasLower ? "text-green-600" : "text-red-500")">
        @if (_hasLower)
        {
            <svg class="h-4 w-4 shrink-0" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="m4.5 12.75 6 6 9-13.5" />
            </svg>
        }
        else
        {
            <svg class="h-4 w-4 shrink-0" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M6 18 18 6M6 6l12 12" />
            </svg>
        }
        @Localizer["auth.password.lowercase"]
    </li>
    <li class="flex items-center gap-2 text-xs @(_hasDigit ? "text-green-600" : "text-red-500")">
        @if (_hasDigit)
        {
            <svg class="h-4 w-4 shrink-0" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="m4.5 12.75 6 6 9-13.5" />
            </svg>
        }
        else
        {
            <svg class="h-4 w-4 shrink-0" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M6 18 18 6M6 6l12 12" />
            </svg>
        }
        @Localizer["auth.password.digit"]
    </li>
    <li class="flex items-center gap-2 text-xs @(_hasSpecial ? "text-green-600" : "text-red-500")">
        @if (_hasSpecial)
        {
            <svg class="h-4 w-4 shrink-0" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="m4.5 12.75 6 6 9-13.5" />
            </svg>
        }
        else
        {
            <svg class="h-4 w-4 shrink-0" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M6 18 18 6M6 6l12 12" />
            </svg>
        }
        @Localizer["auth.password.special"]
    </li>
</ul>

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

    private string GetBarColor() => _strengthPercent switch
    {
        >= 100 => "bg-green-500",
        >= 80  => "bg-blue-500",
        >= 60  => "bg-yellow-500",
        _      => "bg-red-500"
    };

    private string GetTextColor() => _strengthPercent switch
    {
        >= 100 => "text-green-600",
        >= 80  => "text-blue-600",
        >= 60  => "text-yellow-600",
        _      => "text-red-500"
    };

    private string GetLabel() => _strengthPercent switch
    {
        >= 100 => Localizer["auth.password.veryStrong"],
        >= 80  => Localizer["auth.password.strong"],
        >= 60  => Localizer["auth.password.fair"],
        _      => Localizer["auth.password.weak"]
    };
}
```

Strength levels:
| Level | Requirements Met | Colour |
|-------|-----------------|--------|
| Weak | 0-2 of 5 | Red (`bg-red-500`) |
| Fair | 3 of 5 | Yellow (`bg-yellow-500`) |
| Strong | 4 of 5 | Blue (`bg-blue-500`) |
| Very Strong | 5 of 5 | Green (`bg-green-500`) |

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

Use Blazor's built-in `AuthorizeRouteView` and `CascadingAuthenticationState` from `App.razor` (defined in `core/app-scaffold-blazor-tailwind.md`).

Protected pages use the `[Authorize]` attribute and `MainLayout`:

```razor
@page "/dashboard"
@attribute [Authorize]

<h1 class="text-2xl font-bold text-gray-900">@Localizer["dashboard.title"]</h1>
```

Public pages use `@layout EmptyLayout` and do **not** have `[Authorize]`:

```razor
@page "/login"
@layout EmptyLayout

@* No [Authorize] attribute *@
<h1 class="text-2xl font-bold text-gray-900">@Localizer["auth.login.title"]</h1>
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
    <ErrorAlert Message="@_errorMessage" Class="mb-4" />
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
- Show field-level errors inline (under the relevant `<InputText>` via `<ValidationMessage>`) for validation errors
- Show form-level errors in `<ErrorAlert />` for API errors
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
<h1 class="text-2xl font-bold text-gray-900">@Localizer["auth.login.title"]</h1>
<label class="label">@Localizer["auth.login.emailLabel"]</label>
<button class="btn-primary">@Localizer["common.submit"]</button>

@* Wrong — hardcoded string *@
<h1 class="text-2xl font-bold text-gray-900">Sign In</h1>
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
@inject ToastService Toast
@inject AppSettings AppSettings

<div class="flex min-h-screen items-center justify-center bg-gray-50 px-4">
    <div class="w-full max-w-md rounded-lg bg-white p-8 shadow-lg">
        <h1 class="mb-6 text-center text-2xl font-bold text-gray-900">
            @Localizer["auth.login.title"]
        </h1>

        @if (!string.IsNullOrEmpty(_errorMessage))
        {
            <ErrorAlert Message="@_errorMessage" Class="mb-4" />
        }

        @if (_isLoadingOptions)
        {
            <div class="flex justify-center py-12">
                <span class="inline-block h-8 w-8 animate-spin rounded-full border-4 border-gray-200 border-t-primary"></span>
            </div>
        }
        else
        {
            @if (_loginOptions.Any(o => o.Type == "Email"))
            {
                <SigninEmail OnSignin="HandleSignin" IsLoading="_isLoading" />
            }

            @if (_loginOptions.Any(o => o.Type == "SocialLogin"))
            {
                <div class="my-4 border-t border-gray-200"></div>
                <SigninSso Providers="@_loginOptions.First(o => o.Type == "SocialLogin").Providers" />
            }

            @if (_loginOptions.Any(o => o.Type == "SSO"))
            {
                <div class="my-4 border-t border-gray-200"></div>
                <SigninOidc />
            }
        }

        <a href="/signup" class="mt-4 block text-center text-sm text-primary hover:underline">
            @Localizer["auth.login.noAccount"]
        </a>
    </div>
</div>

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

<EditForm Model="_model" OnValidSubmit="HandleSubmit" class="space-y-4">
    <FluentValidationValidator Validator="_validator" />

    <div>
        <label class="label">@Localizer["auth.login.emailLabel"]</label>
        <InputText @bind-Value="_model.Email" type="email" class="input" />
        <ValidationMessage For="@(() => _model.Email)" class="text-red-500 text-xs mt-1" />
    </div>

    <div>
        <label class="label">@Localizer["auth.login.passwordLabel"]</label>
        <div class="relative">
            <InputText @bind-Value="_model.Password"
                       type="@(_showPassword ? "text" : "password")"
                       class="input pr-10" />
            <button type="button"
                    class="absolute inset-y-0 right-0 flex items-center pr-3 text-gray-400 hover:text-gray-600"
                    @onclick="() => _showPassword = !_showPassword">
                @if (_showPassword)
                {
                    @* Heroicon: outline/eye *@
                    <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M2.036 12.322a1.012 1.012 0 0 1 0-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178Z" />
                        <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z" />
                    </svg>
                }
                else
                {
                    @* Heroicon: outline/eye-slash *@
                    <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M3.98 8.223A10.477 10.477 0 0 0 1.934 12c1.292 4.338 5.31 7.5 10.066 7.5.993 0 1.953-.138 2.863-.395M6.228 6.228A10.451 10.451 0 0 1 12 4.5c4.756 0 8.773 3.162 10.065 7.498a10.522 10.522 0 0 1-4.293 5.774M6.228 6.228 3 3m3.228 3.228 3.65 3.65m7.894 7.894L21 21m-3.228-3.228-3.65-3.65m0 0a3 3 0 1 0-4.243-4.243m4.242 4.242L9.88 9.88" />
                    </svg>
                }
            </button>
        </div>
        <ValidationMessage For="@(() => _model.Password)" class="text-red-500 text-xs mt-1" />
    </div>

    <div class="text-right">
        <a href="/forgot-password" class="text-sm text-primary hover:underline">
            @Localizer["auth.login.forgotPassword"]
        </a>
    </div>

    <button type="submit" class="btn-primary w-full" disabled="@IsLoading">
        @if (IsLoading)
        {
            <span class="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-white/30 border-t-white"></span>
        }
        @Localizer["auth.login.signInButton"]
    </button>
</EditForm>

@code {
    [Parameter] public EventCallback<SigninPayload> OnSignin { get; set; }
    [Parameter] public bool IsLoading { get; set; }

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
        await OnSignin.InvokeAsync(new SigninPayload(_model.Email, _model.Password));
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

<div class="flex min-h-screen items-center justify-center bg-gray-50 px-4">
    <div class="w-full max-w-sm rounded-lg bg-white p-8 shadow-lg">
        <h1 class="mb-2 text-center text-xl font-bold text-gray-900">
            @Localizer["auth.mfa.title"]
        </h1>
        <p class="mb-6 text-center text-sm text-gray-500">
            @Localizer["auth.mfa.enterCode"]
        </p>

        @if (!string.IsNullOrEmpty(_errorMessage))
        {
            <ErrorAlert Message="@_errorMessage" Class="mb-4" />
        }

        <OtpInput @ref="_otpInput"
                  Length="@(_mfaType == "email" ? 5 : 6)"
                  OnComplete="HandleVerify"
                  IsDisabled="_isLoading" />

        @if (_isLoading)
        {
            <div class="mt-4 h-1 w-full overflow-hidden rounded-full bg-gray-200">
                <div class="h-1 animate-pulse rounded-full bg-primary" style="width: 100%"></div>
            </div>
        }

        <button class="mt-4 w-full text-center text-sm text-primary hover:underline disabled:text-gray-400 disabled:no-underline"
                @onclick="HandleResendOtp"
                disabled="@(_isResending || _mfaType == "authenticator")">
            @Localizer["auth.mfa.resend"]
        </button>
    </div>
</div>

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

<EditForm Model="_model" OnValidSubmit="HandleSubmit" class="space-y-4">
    <FluentValidationValidator Validator="_validator" />

    <div>
        <label class="label">@Localizer["auth.setPassword.newPasswordLabel"]</label>
        <div class="relative">
            <InputText @bind-Value="_model.Password"
                       type="@(_showPassword ? "text" : "password")"
                       class="input pr-10" />
            <button type="button"
                    class="absolute inset-y-0 right-0 flex items-center pr-3 text-gray-400 hover:text-gray-600"
                    @onclick="() => _showPassword = !_showPassword">
                @if (_showPassword)
                {
                    <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M2.036 12.322a1.012 1.012 0 0 1 0-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178Z" />
                        <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z" />
                    </svg>
                }
                else
                {
                    <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M3.98 8.223A10.477 10.477 0 0 0 1.934 12c1.292 4.338 5.31 7.5 10.066 7.5.993 0 1.953-.138 2.863-.395M6.228 6.228A10.451 10.451 0 0 1 12 4.5c4.756 0 8.773 3.162 10.065 7.498a10.522 10.522 0 0 1-4.293 5.774M6.228 6.228 3 3m3.228 3.228 3.65 3.65m7.894 7.894L21 21m-3.228-3.228-3.65-3.65m0 0a3 3 0 1 0-4.243-4.243m4.242 4.242L9.88 9.88" />
                    </svg>
                }
            </button>
        </div>
        <ValidationMessage For="@(() => _model.Password)" class="text-red-500 text-xs mt-1" />
    </div>

    <PasswordStrength Password="@_model.Password" IsStrongChanged="@(val => _isPasswordStrong = val)" />

    <div>
        <label class="label">@Localizer["auth.setPassword.confirmPasswordLabel"]</label>
        <InputText @bind-Value="_model.ConfirmPassword" type="password" class="input" />
        <ValidationMessage For="@(() => _model.ConfirmPassword)" class="text-red-500 text-xs mt-1" />
    </div>

    <button type="submit" class="btn-primary w-full" disabled="@(!_isPasswordStrong || IsLoading)">
        @if (IsLoading)
        {
            <span class="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-white/30 border-t-white"></span>
        }
        @Localizer["auth.setPassword.submitButton"]
    </button>
</EditForm>

@code {
    [Parameter] public EventCallback<string> OnSubmit { get; set; }
    [Parameter] public bool IsLoading { get; set; }

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
        if (_isPasswordStrong)
        {
            await OnSubmit.InvokeAsync(_model.Password);
        }
    }
}
```

---

## Rules Specific to Auth Module

- Use plain HTML + Tailwind utility classes for all UI — never add a component library
- Use `EditForm` + FluentValidation for all form validation — validators receive `ILocalizationService` via constructor
- Use `ILocalizationService` for every user-visible string — no hardcoded strings, ever
- Look up existing localization keys with `get-keys-by-names` before creating new ones
- Handle loading, error, and empty states on all pages
- One service class per concern (`AuthService`) — components call service methods, not `HttpClient` directly
- Never store tokens in component `@code` blocks — always use `AuthState`
- CAPTCHA requires JS interop — keep the interop minimal and isolated in a single component
- All auth pages use `EmptyLayout`, not `MainLayout`
- MFA type determines OTP length: `email` = 5 digits, `authenticator` = 6 digits
- The `configuratoinName` field preserves the API typo — do not correct the spelling in C# models
- Disable submit buttons during loading — show an inline `animate-spin` spinner inside the button
- Password submit must be disabled until the password strength indicator reports Strong or above
- Use `<ErrorAlert />` for form-level API errors, `<ValidationMessage>` for field-level validation errors
- Use `ToastService` for success/error/info notifications — not browser alerts
- Use Heroicons (inline SVG) for all icons — eye/eye-slash for password toggle
- Use the component classes from `Styles/app.css` (`.btn-primary`, `.input`, `.label`, `.card`) for consistent styling
