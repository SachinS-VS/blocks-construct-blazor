# Frontend Skill

## Stack

| Layer | Technology |
|-------|-----------|
| Framework | React 19 + TypeScript |
| Build tool | Vite |
| Styling | Tailwind CSS 3.4 |
| Component primitives | Radix UI |
| Component system | shadcn/ui style |
| Icons | Lucide React |
| Forms | React Hook Form + Zod |
| Font | Nunito Sans |

---

## Reference Implementation

**Always check the reference repo before generating any frontend code:**

```
https://github.com/SELISEdigitalplatforms/blocks-construct-react
```

Use it to verify:
- Exact component names and import paths used in production
- Auth flow shape (Zustand store, HTTP interceptor, token handling)
- Module folder structure and naming conventions
- How existing patterns are composed (form layouts, table pages, modal dialogs)

When in doubt about a UI pattern, read the reference first — do not invent structure.

---

## shadcn/ui MCP

A Model Context Protocol server is available for shadcn/ui component lookups:

```
https://ui.shadcn.com/docs/mcp
```

Use it to:
- Fetch the exact installation command for any shadcn/ui component
- Look up component API (props, variants, composition patterns)
- Check which Radix UI primitive a component wraps

**When to use it:** Any time you generate code that uses a shadcn/ui component (`Button`, `Dialog`, `Table`, `Form`, `Select`, etc.) — use the MCP to confirm the correct import path and props before writing the component.

---

## Project Structure

```
src/
├── assets/           ← static images and icons
├── components/
│   ├── ui-kit/       ← base reusable components (shadcn style)
│   └── core/         ← feature-specific composite components
├── constant/         ← app-wide constants
├── hooks/            ← custom React hooks
├── i18n/             ← translations
├── layout/           ← page layout components
├── lib/              ← utilities (cn, etc.)
├── models/           ← TypeScript interfaces and types
├── modules/          ← feature modules (one folder per domain)
├── routes/           ← route definitions
├── state/            ← global state management
├── styles/           ← global CSS and theme
└── types/            ← shared TypeScript types
```

---

## Component Layers

### ui-kit/ — Base Components
Unstyled Radix UI primitives wrapped with Tailwind. Never contain business logic.

Examples: `button`, `input`, `dialog`, `table`, `card`, `badge`, `avatar`, `tabs`, `toast`, `select`, `dropdown-menu`, `form`, `skeleton`

### core/ — Feature Components
Composite components built from ui-kit. May contain domain logic.

Examples: `data-table`, `confirmation-modal`, `otp-input`, `captcha`, `app-sidebar`, `profile-menu`, `error-alert`, `loading-overlay`

**Rule:** Always build from ui-kit components. Never use raw HTML elements where a ui-kit component exists.

---

## Theming

Colors are driven by two env variables:

```
VITE_PRIMARY_COLOR=#15969B
VITE_SECONDARY_COLOR=#5194B8
```

These are resolved to HSL CSS custom properties at runtime via `src/styles/theme/`:

```css
:root {
  --primary: <hsl from VITE_PRIMARY_COLOR>;
  --secondary: <hsl from VITE_SECONDARY_COLOR>;
}
```

Use Tailwind semantic tokens in all components — never hardcode hex values:

```tsx
// ✅ correct
<Button className="bg-primary text-primary-foreground" />

// ❌ wrong
<Button className="bg-[#15969B]" />
```

Supports light / dark / system modes via `theme-provider.tsx`.

---

## Utility Function

Always use `cn()` from `src/lib/` for conditional classnames:

```tsx
import { cn } from '@/lib/utils'

<div className={cn('base-class', condition && 'conditional-class')} />
```

---

## Forms

All forms use React Hook Form + Zod:

```tsx
const schema = z.object({
  email: z.string().email(),
  password: z.string().min(8),
})

const form = useForm<z.infer<typeof schema>>({
  resolver: zodResolver(schema),
})
```

Always wrap with shadcn `<Form>`, `<FormField>`, `<FormItem>`, `<FormLabel>`, `<FormControl>`, `<FormMessage>`.

---

## Feature Module Structure

Each domain feature follows this structure inside `src/modules/`:

```
modules/
└── feature-name/
    ├── components/     ← feature-specific UI components
    ├── hooks/          ← feature-specific hooks
    ├── pages/          ← routed page components
    └── index.ts        ← public exports
```

---

## Localization — Mandatory for All Frontend Code

**Every user-visible string must use a translation key. No hardcoded strings anywhere.**

This is not optional. It applies to labels, placeholders, button text, error messages, tooltips, headings, empty state messages, and toast notifications.

### Translation Hook

All components use `useTranslation` from `src/hooks/use-translation.tsx`:

```tsx
import { useTranslation } from '@/hooks/use-translation'

const { t } = useTranslation()

// ✅ correct
<Button>{t('common.submit')}</Button>
<FormLabel>{t('auth.login.emailLabel')}</FormLabel>
<p>{t('users.table.emptyState')}</p>

// ❌ wrong — hardcoded string
<Button>Submit</Button>
<FormLabel>Email address</FormLabel>
```

### Key Naming Convention

```
{module}.{context}.{element}

auth.login.title
auth.login.emailLabel
auth.login.passwordLabel
auth.login.submitButton
auth.login.forgotPassword
common.submit
common.cancel
common.save
common.delete
common.loading
common.error
users.table.emptyState
users.form.createTitle
```

### Key Lookup Before Creation — Required Workflow

Before writing any component, Claude must:

1. **List all user-visible strings** in the planned component
2. **Call `get-keys-by-names`** with the candidate key names to check which already exist
3. **Reuse existing keys** — do not create duplicates
4. **Call `save-keys`** (batch) to create only the missing keys
5. **Then generate the component** using the confirmed key names

```
// ✅ correct workflow
1. Component needs: "Submit", "Cancel", "Email address", "Password"
2. Check: common.submit ✅ exists | common.cancel ✅ exists | auth.login.emailLabel ❓ | auth.login.passwordLabel ❓
3. Create missing: save-keys([{ keyName: 'auth.login.emailLabel', ... }, { keyName: 'auth.login.passwordLabel', ... }])
4. Generate component using all four keys

// ❌ wrong — generating component first, adding keys later
```

### i18n Setup — `src/hooks/use-translation.tsx`

```tsx
import { useLanguageStore } from '@/state/store/language'
import { useGetUilmFile } from '@/modules/localization/hooks/use-localization'

export const useTranslation = () => {
  const { currentLanguage } = useLanguageStore()
  const { data: translations, isLoading } = useGetUilmFile({
    projectKey: import.meta.env.VITE_PROJECT_SLUG,
    languageCode: currentLanguage,
  })

  const t = (keyName: string, fallback?: string): string => {
    if (!translations) return fallback ?? keyName
    return translations[keyName] ?? fallback ?? keyName
  }

  return { t, currentLanguage, isLoading }
}
```

### Language Store — `src/state/store/language/index.tsx`

```tsx
import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface LanguageState {
  currentLanguage: string
  setLanguage: (code: string) => void
}

export const useLanguageStore = create<LanguageState>()(
  persist(
    (set) => ({
      currentLanguage: 'en',
      setLanguage: (code) => set({ currentLanguage: code }),
    }),
    { name: 'language-storage' }
  )
)
```

### Language Switcher — Required in Every App

The language switcher must be added to the app header/navbar from the very first feature. It is not added later — it is part of the base layout.

Location: `src/components/core/language-switcher/language-switcher.tsx`

```tsx
import { useLanguageStore } from '@/state/store/language'
import { useGetLanguages } from '@/modules/localization/hooks/use-localization'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui-kit/select'
import { Globe } from 'lucide-react'

export const LanguageSwitcher = () => {
  const { currentLanguage, setLanguage } = useLanguageStore()
  const { data: languages } = useGetLanguages({
    projectKey: import.meta.env.VITE_PROJECT_SLUG,
  })

  if (!languages?.length) return null

  return (
    <Select value={currentLanguage} onValueChange={setLanguage}>
      <SelectTrigger className="w-36 gap-2">
        <Globe className="h-4 w-4" />
        <SelectValue />
      </SelectTrigger>
      <SelectContent>
        {languages.map((lang) => (
          <SelectItem key={lang.code} value={lang.code}>
            {lang.name}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  )
}
```

Mount in `src/layout/app-header.tsx` or equivalent top-level layout component.

### Zod Validation Error Messages

Zod schema error messages must also use translation keys:

```tsx
// ❌ hardcoded
const schema = z.object({
  email: z.string().email('Invalid email address'),
})

// ✅ localized
const { t } = useTranslation()
const schema = z.object({
  email: z.string().email(t('validation.email.invalid')),
})
```

---

## Rules

* Use TypeScript for all components — no plain `.jsx` files
* Use `shadcn/ui` patterns for all new ui-kit components
* Use Zod schemas for all form validation
* Use Tailwind semantic color tokens — never hardcode colors
* Handle loading state with `<Skeleton />` components
* Handle error state with `<ErrorAlert />` or inline error messages
* All pages must handle loading, error, and empty states
* **Every user-visible string must use `t('key.name')` — no hardcoded strings, ever**
* **Look up existing keys with `get-keys-by-names` before creating new ones**
* **Language switcher must be in the app layout from the first feature**
