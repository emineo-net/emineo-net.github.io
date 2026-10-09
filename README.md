## Overview

This solution contains a Blazor WebAssembly application and two reusable Razor Class Libraries. The application uses Supabase for authentication and Paddle for checkout and customer-portal operations.

## Projects

### EmineoBlazor

`EmineoBlazor` is the Blazor WebAssembly application. It references `PaddleRcl` and `SupabaseRcl`, configures authentication services, and contains the application pages, layout, static assets, and service-worker files.

### PaddleRcl

`PaddleRcl` is a browser-supported Razor Class Library that provides a reusable Paddle checkout component and a service for retrieving a Paddle customer-portal URL through a Supabase Edge Function.

### SupabaseRcl

`SupabaseRcl` is a browser-supported Razor Class Library that provides Supabase authentication, browser session persistence, an authentication state provider, login and registration components, and an authentication dialog.

## Target Frameworks

All three projects target `.NET 10` (`net10.0`). `EmineoBlazor` uses the `Microsoft.NET.Sdk.BlazorWebAssembly` SDK. `PaddleRcl` and `SupabaseRcl` use the `Microsoft.NET.Sdk.Razor` SDK.

## Dependencies

`EmineoBlazor` references:

- `Microsoft.AspNetCore.Components.WebAssembly`
- `Microsoft.AspNetCore.Components.WebAssembly.DevServer` as a private asset
- Project references to `PaddleRcl` and `SupabaseRcl`

`PaddleRcl` references:

- `Microsoft.AspNetCore.Components.Web`
- `Microsoft.Extensions.Http`

`SupabaseRcl` references:

- `Microsoft.AspNetCore.Components.Web`
- `Supabase`
- `Microsoft.AspNetCore.Components.Authorization`

## Build/Test/Run

The project files support building the application with:

```text
dotnet build EmineoBlazor/EmineoBlazor.csproj
```

The configured development launch profiles are:

- HTTP: `http://localhost:5136`
- HTTPS: `https://localhost:7265`
- HTTPS profile HTTP fallback: `http://localhost:5136`

The application can be started with the project launch configuration using:

```text
dotnet run --project EmineoBlazor/EmineoBlazor.csproj
```

No test project or test files were identified in the three analyzed project folders.

## Folder Structure

```text
EmineoBlazor/
	Layout/
	Pages/
		Auth/
		User/
	Properties/
	wwwroot/

PaddleRcl/
	Services/
	wwwroot/

SupabaseRcl/
	Components/
	Handlers/
	Providers/
	Services/
	wwwroot/
```

## Features

- Blazor WebAssembly application with client-side routing and a default application layout.
- Supabase initialization and authentication-state integration.
- Login, registration, password-reset, profile, and password-change pages.
- Reusable login and registration dialog backed by Supabase authentication.
- Browser `localStorage` persistence for Supabase sessions.
- Paddle checkout component with sandbox/production environment support and checkout event callbacks.
- Paddle customer-portal URL retrieval through a Supabase Edge Function.
- Bootstrap and custom CSS assets.
- Web app manifest and service-worker assets.
