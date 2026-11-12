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

Set `DATABASE_URL` to a PostgreSQL connection URI in the environment or in a `.env` file:

```text
DATABASE_URL=postgresql://postgres:postgres@localhost:5432/pasteql
```

The app requires this variable in every environment. Copy `.env.example` to `.env` and replace its example credentials with your database values.

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

This project is licensed under the GNU Affero General Public License v3.0 (AGPL-3.0). See the `LICENSE` file for the full text.
