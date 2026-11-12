# Contributing to PasteQL

Thanks for your interest in contributing to PasteQL.

## How to contribute

- Open an issue before making a large change.
- Keep pull requests focused and easy to review.
- Add or update tests when changing behavior.
- Prefer small, clearly described commits.

## Local development

```bash
dotnet restore
dotnet build src/PasteQL/PasteQL.csproj
dotnet run --project src/PasteQL/PasteQL.csproj
```

## Coding conventions

- Follow the existing C# conventions already used in the project.
- Prefer clear names over clever abstractions.
- Keep security-sensitive code reviewed carefully.

## Pull request checklist

- The app builds successfully.
- The change is documented when needed.
- The scope matches the issue or feature request.
- Sensitive behavior, particularly encryption and auth, is covered with careful review.
