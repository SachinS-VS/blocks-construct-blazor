# App Scaffold

Covers everything needed to initialise a new React project that uses SELISE Blocks services.

Run this **once** when starting a new project — before building any feature. If a project already has `main.tsx` and `App.tsx`, skip to the specific section that is missing.

---

## When to Run

Trigger phrases:
> "set up a new project"
> "initialise the app"
> "create the app scaffold"
> "start a new blocks project"
> "what do I need to install"

---

## Step 1 — Install Dependencies

```bash
npm create vite@latest my-app -- --template react-ts
cd my-app

# Core
npm install react-router-dom @tanstack/react-query zustand

# UI
npm install @radix-ui/react-dialog @radix-ui/react-dropdown-menu @radix-ui/react-select \
  @radix-ui/react-tabs @radix-ui/react-toast @radix-ui/react-tooltip \
  class-variance-authority clsx tailwind-merge lucide-react

# Forms & Validation
npm install react-hook-form @hookform/resolvers zod

# HTTP
npm install axios

# GraphQL (for data-management schema queries)
npm install @apollo/client graphql

# shadcn/ui setup (run after Tailwind is configured)
npx shadcn@latest init
```

> Use the shadcn/ui MCP (`https://ui.shadcn.com/docs/mcp`) to get the exact install command for any additional component before adding it.

---

## Step 2 — Vite Config

```typescript
// vite.config.ts
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
})
```

```json
// tsconfig.json — add paths under compilerOptions
{
  "compilerOptions": {
    "baseUrl": ".",
    "paths": {
      "@/*": ["./src/*"]
    }
  }
}
```

---

## Step 3 — Tailwind Config

```typescript
// tailwind.config.ts
import type { Config } from 'tailwindcss'

const config: Config = {
  darkMode: ['class'],
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        border: 'hsl(var(--border))',
        input: 'hsl(var(--input))',
        ring: 'hsl(var(--ring))',
        background: 'hsl(var(--background))',
        foreground: 'hsl(var(--foreground))',
        primary: {
          DEFAULT: 'hsl(var(--primary))',
          foreground: 'hsl(var(--primary-foreground))',
        },
        secondary: {
          DEFAULT: 'hsl(var(--secondary))',
          foreground: 'hsl(var(--secondary-foreground))',
        },
        muted: {
          DEFAULT: 'hsl(var(--muted))',
          foreground: 'hsl(var(--muted-foreground))',
        },
        accent: {
          DEFAULT: 'hsl(var(--accent))',
          foreground: 'hsl(var(--accent-foreground))',
        },
        destructive: {
          DEFAULT: 'hsl(var(--destructive))',
          foreground: 'hsl(var(--destructive-foreground))',
        },
        card: {
          DEFAULT: 'hsl(var(--card))',
          foreground: 'hsl(var(--card-foreground))',
        },
      },
      borderRadius: {
        lg: 'var(--radius)',
        md: 'calc(var(--radius) - 2px)',
        sm: 'calc(var(--radius) - 4px)',
      },
      fontFamily: {
        sans: ['Nunito Sans', 'sans-serif'],
      },
    },
  },
  plugins: [require('tailwindcss-animate')],
}

export default config
```

---

## Step 4 — Global CSS + Runtime Theme

Colors from `VITE_PRIMARY_COLOR` and `VITE_SECONDARY_COLOR` are applied at runtime via a theme initialiser.

```css
/* src/styles/globals.css */
@tailwind base;
@tailwind components;
@tailwind utilities;

@import url('https://fonts.googleapis.com/css2?family=Nunito+Sans:wght@400;500;600;700&display=swap');

@layer base {
  :root {
    --background: 0 0% 100%;
    --foreground: 222.2 84% 4.9%;
    --card: 0 0% 100%;
    --card-foreground: 222.2 84% 4.9%;
    --border: 214.3 31.8% 91.4%;
    --input: 214.3 31.8% 91.4%;
    --ring: 221.2 83.2% 53.3%;
    --radius: 0.5rem;
    --muted: 210 40% 96.1%;
    --muted-foreground: 215.4 16.3% 46.9%;
    --accent: 210 40% 96.1%;
    --accent-foreground: 222.2 47.4% 11.2%;
    --destructive: 0 84.2% 60.2%;
    --destructive-foreground: 210 40% 98%;
    /* primary and secondary are set at runtime by theme-initialiser */
  }

  .dark {
    --background: 222.2 84% 4.9%;
    --foreground: 210 40% 98%;
    --card: 222.2 84% 4.9%;
    --card-foreground: 210 40% 98%;
    --border: 217.2 32.6% 17.5%;
    --input: 217.2 32.6% 17.5%;
    --muted: 217.2 32.6% 17.5%;
    --muted-foreground: 215 20.2% 65.1%;
    --accent: 217.2 32.6% 17.5%;
    --accent-foreground: 210 40% 98%;
    --destructive: 0 62.8% 30.6%;
    --destructive-foreground: 210 40% 98%;
  }
}

@layer base {
  * { @apply border-border; }
  body { @apply bg-background text-foreground font-sans; }
}
```

```typescript
// src/styles/theme/theme-initialiser.ts
// Converts a hex or hsl string to HSL custom property values

function hexToHsl(hex: string): string {
  const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex)
  if (!result) return '0 0% 0%'
  let r = parseInt(result[1], 16) / 255
  let g = parseInt(result[2], 16) / 255
  let b = parseInt(result[3], 16) / 255
  const max = Math.max(r, g, b), min = Math.min(r, g, b)
  let h = 0, s = 0
  const l = (max + min) / 2
  if (max !== min) {
    const d = max - min
    s = l > 0.5 ? d / (2 - max - min) : d / (max + min)
    switch (max) {
      case r: h = ((g - b) / d + (g < b ? 6 : 0)) / 6; break
      case g: h = ((b - r) / d + 2) / 6; break
      case b: h = ((r - g) / d + 4) / 6; break
    }
  }
  return `${Math.round(h * 360)} ${Math.round(s * 100)}% ${Math.round(l * 100)}%`
}

function toHslValue(value: string): string {
  if (value.startsWith('#')) return hexToHsl(value)
  // already hsl(...) format — extract inner values
  return value.replace(/^hsl\(/, '').replace(/\)$/, '').trim()
}

export function applyTheme(): void {
  const root = document.documentElement
  const primary = import.meta.env.VITE_PRIMARY_COLOR ?? '#15969B'
  const secondary = import.meta.env.VITE_SECONDARY_COLOR ?? '#5194B8'
  root.style.setProperty('--primary', toHslValue(primary))
  root.style.setProperty('--primary-foreground', '0 0% 100%')
  root.style.setProperty('--secondary', toHslValue(secondary))
  root.style.setProperty('--secondary-foreground', '0 0% 100%')
}
```

---

## Step 5 — HTTP Client (`src/lib/https.ts`)

The shared HTTP client used by every service layer. Handles auth headers and automatic token refresh on 401.

```typescript
// src/lib/https.ts
import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse, InternalAxiosRequestConfig } from 'axios'
import { useAuthStore } from '@/state/store/auth'

const BASE_URL = import.meta.env.VITE_API_BASE_URL
const BLOCKS_KEY = import.meta.env.VITE_X_BLOCKS_KEY

// Track in-flight refresh to avoid parallel refresh requests
let isRefreshing = false
let failedQueue: Array<{
  resolve: (value: unknown) => void
  reject: (reason?: unknown) => void
}> = []

function processQueue(error: unknown, token: string | null = null): void {
  failedQueue.forEach((p) => {
    if (error) {
      p.reject(error)
    } else {
      p.resolve(token)
    }
  })
  failedQueue = []
}

const https: AxiosInstance = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json',
    'x-blocks-key': BLOCKS_KEY,
  },
})

// Request interceptor — attach current access token
https.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const { accessToken } = useAuthStore.getState()
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`
  }
  return config
})

// Response interceptor — refresh on 401 and retry
https.interceptors.response.use(
  (response: AxiosResponse) => response,
  async (error) => {
    const original = error.config

    if (error.response?.status !== 401 || original._retry) {
      return Promise.reject(error)
    }

    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        failedQueue.push({ resolve, reject })
      })
        .then((token) => {
          original.headers.Authorization = `Bearer ${token}`
          return https(original)
        })
        .catch((err) => Promise.reject(err))
    }

    original._retry = true
    isRefreshing = true

    const { refreshToken, setAccessToken, logout } = useAuthStore.getState()

    try {
      const params = new URLSearchParams()
      params.append('grant_type', 'refresh_token')
      params.append('refresh_token', refreshToken)
      params.append('client_id', import.meta.env.VITE_BLOCKS_OIDC_CLIENT_ID)

      const { data } = await axios.post(
        `${BASE_URL}/idp/v1/Authentication/Token`,
        params,
        {
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'x-blocks-key': BLOCKS_KEY,
          },
        }
      )

      const newToken: string = data.access_token
      setAccessToken(newToken)
      processQueue(null, newToken)
      original.headers.Authorization = `Bearer ${newToken}`
      return https(original)
    } catch (refreshError) {
      processQueue(refreshError, null)
      logout()
      window.location.href = '/login'
      return Promise.reject(refreshError)
    } finally {
      isRefreshing = false
    }
  }
)

export default https
```

---

## Step 6 — Auth Store (`src/state/store/auth/index.tsx`)

```typescript
import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface User {
  id: string
  email: string
  firstName?: string
  lastName?: string
  roles?: string[]
  permissions?: string[]
}

interface AuthState {
  isAuthenticated: boolean
  accessToken: string
  refreshToken: string
  user: User | null
  login: (accessToken: string, refreshToken: string) => void
  setAccessToken: (token: string) => void
  setUser: (user: User) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      isAuthenticated: false,
      accessToken: '',
      refreshToken: '',
      user: null,

      login: (accessToken, refreshToken) =>
        set({ isAuthenticated: true, accessToken, refreshToken }),

      setAccessToken: (accessToken) => set({ accessToken }),

      setUser: (user) => set({ user }),

      logout: () =>
        set({ isAuthenticated: false, accessToken: '', refreshToken: '', user: null }),
    }),
    { name: 'auth-storage' }
  )
)
```

---

## Step 7 — Utility (`src/lib/utils.ts`)

```typescript
import { type ClassValue, clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}
```

---

## Step 8 — Apollo Client for GraphQL (`src/lib/graphql.ts`)

Required only if using data-management schemas for CRUD queries.

```typescript
// src/lib/graphql.ts
import { ApolloClient, InMemoryCache, createHttpLink, from } from '@apollo/client'
import { setContext } from '@apollo/client/link/context'
import { onError } from '@apollo/client/link/error'
import { useAuthStore } from '@/state/store/auth'

const httpLink = createHttpLink({
  uri: `${import.meta.env.VITE_API_BASE_URL}/uds/v1/${import.meta.env.VITE_PROJECT_SLUG}/graphql`,
})

const authLink = setContext((_, { headers }) => {
  const { accessToken } = useAuthStore.getState()
  return {
    headers: {
      ...headers,
      authorization: accessToken ? `Bearer ${accessToken}` : '',
      'x-blocks-key': import.meta.env.VITE_X_BLOCKS_KEY,
    },
  }
})

const errorLink = onError(({ networkError }) => {
  if (networkError && 'statusCode' in networkError && networkError.statusCode === 401) {
    useAuthStore.getState().logout()
    window.location.href = '/login'
  }
})

export const apolloClient = new ApolloClient({
  link: from([errorLink, authLink, httpLink]),
  cache: new InMemoryCache(),
})
```

---

## Step 9 — Route Assembly (`src/routes/index.tsx`)

```typescript
// src/routes/index.tsx
import { createBrowserRouter, RouteObject } from 'react-router-dom'
import { AppLayout } from '@/layout/app-layout'
import { authRoutes } from './auth.route'
import { localizationRoutes } from './localization.route'
import { dataManagementRoutes } from './data-management.route'
// import other domain routes as they are built

const protectedRoutes: RouteObject[] = [
  {
    element: <AppLayout />,
    children: [
      ...localizationRoutes,
      ...dataManagementRoutes,
      // add domain route arrays here as features are built
    ],
  },
]

export const router = createBrowserRouter([
  ...authRoutes,   // public — no layout wrapper
  ...protectedRoutes,
])
```

Add a new domain's route array here whenever a new feature module is generated.

---

## Step 10 — App Root (`src/main.tsx` and `src/App.tsx`)

```tsx
// src/main.tsx
import React from 'react'
import ReactDOM from 'react-dom/client'
import { App } from './App'
import './styles/globals.css'
import { applyTheme } from './styles/theme/theme-initialiser'

applyTheme()

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
)
```

```tsx
// src/App.tsx
import { RouterProvider } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ApolloProvider } from '@apollo/client'
import { router } from '@/routes'
import { apolloClient } from '@/lib/graphql'
import { Toaster } from '@/components/ui-kit/toaster'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      staleTime: 1000 * 60 * 5, // 5 minutes
    },
  },
})

export const App = () => (
  <QueryClientProvider client={queryClient}>
    <ApolloProvider client={apolloClient}>
      <RouterProvider router={router} />
      <Toaster />
    </ApolloProvider>
  </QueryClientProvider>
)
```

> The `<I18nProvider>` wraps protected routes inside `AppLayout`, not here — see `app-layout.md`.

---

## Output Summary

| File | Purpose |
|------|---------|
| `vite.config.ts` | Path alias `@/` |
| `tailwind.config.ts` | CSS variable based theming |
| `src/styles/globals.css` | Base CSS custom properties |
| `src/styles/theme/theme-initialiser.ts` | Apply env var colors at runtime |
| `src/lib/https.ts` | Shared HTTP client with 401 auto-refresh |
| `src/lib/utils.ts` | `cn()` class merge utility |
| `src/lib/graphql.ts` | Apollo Client for GraphQL (data-management) |
| `src/state/store/auth/index.tsx` | Persisted auth store |
| `src/routes/index.tsx` | Root router combining all domain routes |
| `src/main.tsx` | React 19 app entry point |
| `src/App.tsx` | Provider composition root |

---

## Rules

- Run this skill once at project start. Do not re-run it if the files already exist — edit them instead.
- When adding a new domain's routes, always add the route array to `src/routes/index.tsx` under `protectedRoutes`.
- Never duplicate the auth store or HTTP client — all modules import from `@/state/store/auth` and `@/lib/https`.
- `ApolloProvider` can be omitted if the project does not use data-management schemas.
