# PasteQL

PasteQL is a self-hosted pastebin built with ASP.NET Core, EF Core, GraphQL, and server-side Markdown rendering. It supports encrypted content storage, password protection, burn-after-read behavior, automatic expiry, and a lightweight HTMX-inspired UI.

## Features

- Encrypted paste contents at rest with ASP.NET Core Data Protection
- Optional password protection for private pastes
- Optional burn-after-read and max-view limits
- Expiration timestamps for automatic cleanup
- Markdown rendering with sanitization
- Server-side syntax highlighting for common languages
- Native GraphQL endpoint for querying recent pastes and full paste payloads
- PostgreSQL support via EF Core, with an in-memory fallback for local development

## Stack

- ASP.NET Core Web App
- Entity Framework Core
- PostgreSQL via Npgsql
- HotChocolate GraphQL
- Markdig for Markdown processing
- HTMX-style Razor UI with a minimal custom stylesheet

## Running locally

1. Restore NuGet packages:

   ```bash
   dotnet restore
   ```

2. Start the app:

   ```bash
   dotnet run --project src/PasteQL/PasteQL.csproj
   ```

3. Open the app in a browser:

   - Web UI: http://localhost:5000
   - GraphQL: http://localhost:5000/graphql

## Configuring PostgreSQL

Set a connection string in `src/PasteQL/appsettings.json` or environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=pasteql;Username=postgres;Password=postgres"
  }
}
```

If no connection string is configured, the app will use an in-memory database so it can start immediately in development.

## Example GraphQL query

```graphql
query RecentPastes {
  recentPastes(limit: 5) {
    id
    title
    language
    createdAtUtc
    isPasswordProtected
    burnAfterRead
  }
}
```

## Example GraphQL fetch by id

```graphql
query PasteById($id: String!) {
  paste(id: $id, password: "secret") {
    id
    title
    content
    language
    viewCount
    expiresAtUtc
  }
}
```

## Security note

This application is intended for self-hosting in trusted environments. For production, use a strong database password, rotate Data Protection keys, enable TLS, and consider running behind a reverse proxy such as NGINX or Caddy.

## License

This project is released under the MIT License. See the `LICENSE` file for details.
