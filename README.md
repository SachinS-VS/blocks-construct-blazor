# blocks-construct-blazor

A SELISE Blocks Blazor WASM application with Interactive Auto rendering. Built with .NET 10 (Blazor WASM + Server), Tailwind CSS v4, GraphQL, and OIDC authentication.

## Stack

- **Frontend**: Blazor WASM (.NET 10), Tailwind CSS v4
- **Backend**: ASP.NET Core 10, GraphQL, Swagger
- **Auth**: OIDC via SELISE Blocks identity

## Run the Project

```bash
cd src/Server
dotnet watch
```

The app will be available at `https://localhost:5001` (or the port shown in the terminal).

## Available Interfaces

| Interface | URL |
|-----------|-----|
| Login | `https://localhost:5001/login` |
| Dashboard | `https://localhost:5001/dashboard` |
| Swagger | `https://localhost:5001/swagger` *(Development only)* |
