# App Layout

Covers the authenticated app shell — the layout that wraps every protected page — and shared core components used throughout the app.

Build this after `app-scaffold.md` and before generating any feature pages. Pages need a shell to mount inside.

---

## When to Run

Trigger phrases:
> "set up the app layout"
> "create the sidebar"
> "build the app shell"
> "add a navigation menu"
> "set up the main layout"

Also generate this automatically when building the first feature that results in a protected page.

---

## File Map

| File | Purpose |
|------|---------|
| `src/layout/app-layout.tsx` | Authenticated shell: i18n provider + sidebar + header + content area |
| `src/layout/app-header.tsx` | Top bar with LanguageSwitcher and ProfileMenu |
| `src/components/core/app-sidebar/app-sidebar.tsx` | Left navigation with links to feature modules |
| `src/components/core/profile-menu/profile-menu.tsx` | Avatar dropdown: user info + logout |
| `src/components/core/loading-overlay/loading-overlay.tsx` | Full-screen loading spinner |
| `src/components/core/error-alert/error-alert.tsx` | Inline error message box |
| `src/components/core/confirmation-modal/confirmation-modal.tsx` | Reusable delete/confirm dialog |
| `src/hooks/use-permissions.tsx` | Hook to check current user's permissions |
| `src/pages/oidc-callback/oidc-callback-page.tsx` | Handles `/oidc` authorization code exchange |

---

## Protected Route Guard

```tsx
// src/components/core/protected-route/protected-route.tsx
import { Navigate } from 'react-router-dom'
import { useAuthStore } from '@/state/store/auth'

interface ProtectedRouteProps {
  children: React.ReactNode
}

export const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
  const { isAuthenticated } = useAuthStore()
  if (!isAuthenticated) return <Navigate to="/login" replace />
  return <>{children}</>
}
```

---

## App Layout (`src/layout/app-layout.tsx`)

The root layout for all authenticated pages. Wraps children with the i18n provider, loads translations before rendering, and composes the sidebar + header + content.

```tsx
// src/layout/app-layout.tsx
import { Outlet } from 'react-router-dom'
import { ProtectedRoute } from '@/components/core/protected-route/protected-route'
import { AppSidebar } from '@/components/core/app-sidebar/app-sidebar'
import { AppHeader } from './app-header'
import { LoadingOverlay } from '@/components/core/loading-overlay/loading-overlay'
import { useGetUilmFile } from '@/modules/localization/hooks/use-localization'
import { useLanguageStore } from '@/state/store/language'

const I18nProvider = ({ children }: { children: React.ReactNode }) => {
  const { currentLanguage } = useLanguageStore()
  const { isLoading } = useGetUilmFile({
    projectKey: import.meta.env.VITE_PROJECT_SLUG,
    languageCode: currentLanguage,
  })

  if (isLoading) return <LoadingOverlay />
  return <>{children}</>
}

export const AppLayout = () => (
  <ProtectedRoute>
    <I18nProvider>
      <div className="flex h-screen overflow-hidden bg-background">
        <AppSidebar />
        <div className="flex flex-1 flex-col overflow-hidden">
          <AppHeader />
          <main className="flex-1 overflow-y-auto p-6">
            <Outlet />
          </main>
        </div>
      </div>
    </I18nProvider>
  </ProtectedRoute>
)
```

---

## App Header (`src/layout/app-header.tsx`)

```tsx
// src/layout/app-header.tsx
import { LanguageSwitcher } from '@/components/core/language-switcher/language-switcher'
import { ProfileMenu } from '@/components/core/profile-menu/profile-menu'

export const AppHeader = () => (
  <header className="flex h-14 items-center justify-between border-b bg-background px-6">
    <div className="flex items-center gap-2">
      {/* Breadcrumb or page title — populated per page */}
    </div>
    <div className="flex items-center gap-3">
      <LanguageSwitcher />
      <ProfileMenu />
    </div>
  </header>
)
```

---

## App Sidebar (`src/components/core/app-sidebar/app-sidebar.tsx`)

Update the `navItems` array as new feature modules are added.

```tsx
// src/components/core/app-sidebar/app-sidebar.tsx
import { NavLink } from 'react-router-dom'
import { useTranslation } from '@/hooks/use-translation'
import { cn } from '@/lib/utils'
import {
  LayoutDashboard,
  Users,
  Database,
  Globe,
  Bot,
  FileText,
  Settings,
} from 'lucide-react'

interface NavItem {
  labelKey: string
  path: string
  icon: React.ElementType
}

const navItems: NavItem[] = [
  { labelKey: 'nav.users',          path: '/users',               icon: Users },
  { labelKey: 'nav.dataManagement', path: '/data-management',     icon: Database },
  { labelKey: 'nav.localization',   path: '/localization/keys',   icon: Globe },
  { labelKey: 'nav.aiServices',     path: '/ai',                  icon: Bot },
  { labelKey: 'nav.logs',           path: '/lmt/logs',            icon: FileText },
  { labelKey: 'nav.settings',       path: '/settings',            icon: Settings },
]

export const AppSidebar = () => {
  const { t } = useTranslation()

  return (
    <aside className="flex w-56 flex-col border-r bg-background">
      <div className="flex h-14 items-center border-b px-4">
        <span className="text-lg font-semibold text-primary">Blocks App</span>
      </div>
      <nav className="flex-1 overflow-y-auto py-4">
        <ul className="space-y-1 px-2">
          {navItems.map(({ labelKey, path, icon: Icon }) => (
            <li key={path}>
              <NavLink
                to={path}
                className={({ isActive }) =>
                  cn(
                    'flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                    isActive
                      ? 'bg-primary text-primary-foreground'
                      : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
                  )
                }
              >
                <Icon className="h-4 w-4" />
                {t(labelKey)}
              </NavLink>
            </li>
          ))}
        </ul>
      </nav>
    </aside>
  )
}
```

> When a new domain is added, append its entry to `navItems`. Key names follow `nav.{module}` convention.

---

## Profile Menu (`src/components/core/profile-menu/profile-menu.tsx`)

```tsx
// src/components/core/profile-menu/profile-menu.tsx
import { useNavigate } from 'react-router-dom'
import { LogOut, User } from 'lucide-react'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui-kit/dropdown-menu'
import { Avatar, AvatarFallback } from '@/components/ui-kit/avatar'
import { useAuthStore } from '@/state/store/auth'
import { useTranslation } from '@/hooks/use-translation'
import { useLogoutMutation } from '@/modules/auth/hooks/use-auth'

export const ProfileMenu = () => {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { user, logout } = useAuthStore()
  const { mutate: logoutApi } = useLogoutMutation()

  const initials = [user?.firstName?.[0], user?.lastName?.[0]]
    .filter(Boolean)
    .join('')
    .toUpperCase() || '?'

  const handleLogout = () => {
    logoutApi(undefined, {
      onSettled: () => {
        logout()
        navigate('/login', { replace: true })
      },
    })
  }

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <button className="outline-none">
          <Avatar className="h-8 w-8 cursor-pointer">
            <AvatarFallback className="bg-primary text-primary-foreground text-xs">
              {initials}
            </AvatarFallback>
          </Avatar>
        </button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-48">
        <DropdownMenuLabel className="text-xs text-muted-foreground">
          {user?.email}
        </DropdownMenuLabel>
        <DropdownMenuSeparator />
        <DropdownMenuItem onClick={() => navigate('/profile')}>
          <User className="mr-2 h-4 w-4" />
          {t('nav.profile')}
        </DropdownMenuItem>
        <DropdownMenuSeparator />
        <DropdownMenuItem
          onClick={handleLogout}
          className="text-destructive focus:text-destructive"
        >
          <LogOut className="mr-2 h-4 w-4" />
          {t('auth.logout')}
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  )
}
```

---

## Loading Overlay (`src/components/core/loading-overlay/loading-overlay.tsx`)

```tsx
// src/components/core/loading-overlay/loading-overlay.tsx
import { Loader2 } from 'lucide-react'
import { cn } from '@/lib/utils'

interface LoadingOverlayProps {
  fullScreen?: boolean
  className?: string
}

export const LoadingOverlay = ({ fullScreen = true, className }: LoadingOverlayProps) => (
  <div
    className={cn(
      'flex items-center justify-center bg-background',
      fullScreen ? 'fixed inset-0 z-50' : 'h-full w-full',
      className
    )}
  >
    <Loader2 className="h-8 w-8 animate-spin text-primary" />
  </div>
)
```

---

## Error Alert (`src/components/core/error-alert/error-alert.tsx`)

```tsx
// src/components/core/error-alert/error-alert.tsx
import { AlertCircle } from 'lucide-react'
import { Alert, AlertDescription } from '@/components/ui-kit/alert'

interface ErrorAlertProps {
  message: string
  className?: string
}

export const ErrorAlert = ({ message, className }: ErrorAlertProps) => (
  <Alert variant="destructive" className={className}>
    <AlertCircle className="h-4 w-4" />
    <AlertDescription>{message}</AlertDescription>
  </Alert>
)
```

---

## Confirmation Modal (`src/components/core/confirmation-modal/confirmation-modal.tsx`)

```tsx
// src/components/core/confirmation-modal/confirmation-modal.tsx
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui-kit/dialog'
import { Button } from '@/components/ui-kit/button'
import { useTranslation } from '@/hooks/use-translation'

interface ConfirmationModalProps {
  open: boolean
  title: string
  description: string
  onConfirm: () => void
  onCancel: () => void
  loading?: boolean
  variant?: 'destructive' | 'default'
}

export const ConfirmationModal = ({
  open,
  title,
  description,
  onConfirm,
  onCancel,
  loading = false,
  variant = 'destructive',
}: ConfirmationModalProps) => {
  const { t } = useTranslation()

  return (
    <Dialog open={open} onOpenChange={(v) => !v && onCancel()}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>{description}</DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" onClick={onCancel} disabled={loading}>
            {t('common.cancel')}
          </Button>
          <Button variant={variant} onClick={onConfirm} disabled={loading}>
            {loading ? t('common.loading') : t('common.confirm')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
```

---

## Permissions Hook (`src/hooks/use-permissions.tsx`)

Loads the current user's permissions once and exposes a `hasPermission` check. Permissions are stored in the auth store so they survive page refresh.

```tsx
// src/hooks/use-permissions.tsx
import { useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useAuthStore } from '@/state/store/auth'
import https from '@/lib/https'

interface Permission {
  name: string
  resourceGroup: string
}

const fetchPermissions = async (projectKey: string): Promise<Permission[]> => {
  const { data } = await https.get(
    `/idp/v1/Account/GetAccountPermissions?projectKey=${projectKey}`
  )
  return data?.data ?? []
}

export const usePermissions = () => {
  const { user, isAuthenticated, setUser } = useAuthStore()
  const projectKey = import.meta.env.VITE_X_BLOCKS_KEY

  const { data: permissions } = useQuery({
    queryKey: ['permissions', projectKey],
    queryFn: () => fetchPermissions(projectKey),
    enabled: isAuthenticated && (!user?.permissions || user.permissions.length === 0),
    staleTime: 1000 * 60 * 15, // 15 minutes
  })

  useEffect(() => {
    if (permissions && user) {
      setUser({
        ...user,
        permissions: permissions.map((p) => p.name),
      })
    }
  }, [permissions])

  const hasPermission = (permissionName: string): boolean => {
    if (!user?.permissions) return false
    return user.permissions.includes(permissionName)
  }

  const hasAnyPermission = (...names: string[]): boolean =>
    names.some((n) => hasPermission(n))

  const hasAllPermissions = (...names: string[]): boolean =>
    names.every((n) => hasPermission(n))

  return {
    permissions: user?.permissions ?? [],
    hasPermission,
    hasAnyPermission,
    hasAllPermissions,
  }
}
```

Usage in components:

```tsx
const { hasPermission } = usePermissions()

{hasPermission('users.delete') && (
  <Button variant="destructive" onClick={handleDelete}>
    {t('common.delete')}
  </Button>
)}
```

---

## OIDC Callback Page (`src/pages/oidc-callback/oidc-callback-page.tsx`)

Handles the `/oidc` redirect after the user authenticates with an external OIDC provider. Reads `?code=` from the URL, exchanges it for tokens, and redirects to the dashboard.

```tsx
// src/pages/oidc-callback/oidc-callback-page.tsx
import { useEffect } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import axios from 'axios'
import { useAuthStore } from '@/state/store/auth'
import { LoadingOverlay } from '@/components/core/loading-overlay/loading-overlay'
import { ErrorAlert } from '@/components/core/error-alert/error-alert'
import { useState } from 'react'

export const OidcCallbackPage = () => {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const { login } = useAuthStore()
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const code = searchParams.get('code')
    const error = searchParams.get('error')

    if (error) {
      setError('Authentication was cancelled or denied.')
      return
    }

    if (!code) {
      navigate('/login', { replace: true })
      return
    }

    const exchangeCode = async () => {
      try {
        const params = new URLSearchParams()
        params.append('grant_type', 'authorization_code')
        params.append('code', code)
        params.append('client_id', import.meta.env.VITE_BLOCKS_OIDC_CLIENT_ID)
        params.append('redirect_uri', import.meta.env.VITE_BLOCKS_OIDC_REDIRECT_URI)

        const { data } = await axios.post(
          `${import.meta.env.VITE_API_BASE_URL}/idp/v1/Authentication/Token`,
          params,
          {
            headers: {
              'Content-Type': 'application/x-www-form-urlencoded',
              'x-blocks-key': import.meta.env.VITE_X_BLOCKS_KEY,
            },
          }
        )

        login(data.access_token, data.refresh_token)
        navigate('/dashboard', { replace: true })
      } catch {
        setError('Authentication failed. Please try logging in again.')
      }
    }

    exchangeCode()
  }, [])

  if (error) {
    return (
      <div className="flex min-h-screen items-center justify-center p-4">
        <ErrorAlert message={error} className="max-w-md" />
      </div>
    )
  }

  return <LoadingOverlay />
}
```

Add this route to `auth.route.tsx` (no layout wrapper — public):

```typescript
{ path: '/oidc', element: <OidcCallbackPage /> }
```

---

## Translation Keys Required

Before generating this layout, create these keys with `save-keys`:

```
nav.users
nav.dataManagement
nav.localization
nav.aiServices
nav.logs
nav.settings
nav.profile
auth.logout
```

Use `get-keys-by-names` to check which already exist before calling `save-keys`.

---

## Rules

- `AppLayout` is the single authenticated shell — all protected pages render inside its `<Outlet />`.
- `I18nProvider` lives inside `AppLayout` — it runs only for authenticated users, not on login pages.
- `LanguageSwitcher` and `ProfileMenu` are always present in the header from the first feature.
- When adding a new domain, append its nav item to the `navItems` array in `app-sidebar.tsx`.
- `usePermissions` must only be called inside authenticated routes — it depends on the auth store.
- The OIDC callback page must be a public route (outside `AppLayout`).
