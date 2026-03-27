# Design System

## Color Tokens

Colors are configured via environment variables and can be customized per project.

| Token | Default | Env Variable |
|-------|---------|-------------|
| Primary | `#15969B` | `VITE_PRIMARY_COLOR` |
| Secondary | `#5194B8` | `VITE_SECONDARY_COLOR` |

Use CSS custom properties in components:
```css
--color-primary: var(--primary);
--color-secondary: var(--secondary);
```

## Typography

- **Font Family:** Nunito Sans (loaded via Google Fonts)
- **Headings:** `font-semibold` or `font-bold`
- **Body:** `font-normal`, `text-sm` or `text-base`
- **Mono:** System monospace for code blocks

## Component Rules

### shadcn/ui Components
- Always use shadcn/ui components as the base — never build custom equivalents
- Import from `@/components/ui/` (the project's shadcn installation)
- Use the shadcn/ui MCP server for real-time component API lookups: `https://ui.shadcn.com/docs/mcp`
- Customize with Tailwind utility classes, not inline styles

### Forms
- React Hook Form + Zod for all forms
- Field-level validation with descriptive error messages
- Use `FormField`, `FormItem`, `FormLabel`, `FormControl`, `FormMessage` from shadcn/ui form

### Data Tables
- Use TanStack Table (via shadcn/ui DataTable) for all tabular data
- Include sorting, filtering, and pagination by default
- Server-side pagination for large datasets

### Modals & Dialogs
- Use shadcn/ui `Dialog` for confirmations and forms
- Use `Sheet` for side panels
- Use `AlertDialog` for destructive action confirmations

### Loading States
- Use `Skeleton` components for initial page loads
- Use `Spinner` or button loading state for form submissions
- Always show loading feedback — never leave the user without indication

## Accessibility

- All interactive elements must be keyboard navigable
- Use semantic HTML (`<nav>`, `<main>`, `<section>`, `<article>`)
- Labels on all form inputs (no placeholder-only inputs)
- Color contrast must meet WCAG 2.1 AA (4.5:1 for text)
- Use `aria-label` for icon-only buttons
- Focus rings on all focusable elements (shadcn/ui handles this by default)

## Responsive Design

- Mobile-first approach with Tailwind breakpoints
- Sidebar collapses to hamburger menu on mobile
- Tables become card layouts on small screens
- Minimum touch target: 44x44px
