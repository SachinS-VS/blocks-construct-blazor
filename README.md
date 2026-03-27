# blocks-construct-blazor

A SELISE Blocks Blazor WASM application with Interactive Auto rendering. Built with .NET 10 (Blazor WASM + Server), Tailwind CSS v4, GraphQL, and OIDC authentication.

## Stack

- **Frontend**: Blazor WASM (.NET 10), Tailwind CSS v4
- **Backend**: ASP.NET Core 10 (single host serving UI + API), REST API (ApiController), Swagger/OpenAPI

## Folder Structure

```
src/
├── Client/                          # Blazor WASM — pages and components
│   ├── Components/
│   │   ├── Shared/                  # Reusable UI components
│   │   └── Forms/                   # Form-specific components
│   └── Pages/
│       ├── Auth/LoginPage.razor
│       ├── Dashboard/DashboardPage.razor
│       └── Home/HomePage.razor
├── Server/                          # Single host for UI and API
│   ├── Components/Layout/           # App.razor, MainLayout, Routes, etc.
│   ├── Controllers/                 # [ApiController] REST endpoints
│   └── Extensions/                  # DI registration (AddApplicationServices)
├── Services/                        # Shared business logic — feature-based
│   └── SalesOrders/
│       ├── ISalesOrderService.cs
│       ├── SalesOrderService.cs
│       └── SalesOrder.cs
├── Test/                            # xUnit tests
│   └── Services/                    # Unit tests per feature
└── Worker/                          # Background service
    └── Jobs/                        # One class per background job
```

## Run the Project

```bash
dotnet watch --project src/Server
```

The app runs on `https://localhost:7075` (or the URL shown in terminal).

## Available Interfaces

| Interface | URL |
|-----------|-----|
| Home | `https://localhost:7075/` |
| Login | `https://localhost:7075/login` |
| Dashboard | `https://localhost:7075/dashboard` |

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/sales-orders` | List all sales orders |
| `GET` | `/api/sales-orders/{id}` | Get a single sales order |
| `GET` | `/api/sales-orders/by-status/{status}` | Filter by status |
