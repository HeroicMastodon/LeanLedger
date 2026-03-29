**Agent Guide**

- Purpose: landing page for autonomous agents operating in this repository; contains build/test/lint commands and repository-specific style rules.
- Where: root of repo. Use these commands from the repository root unless explicitly noted.
- When uncertain: prefer non-destructive actions, run tests locally before changing behavior, and ask one targeted question if blocked.

Build / run / test
- Build solution: `dotnet build LeanLedger.sln -c Release` (builds all projects).
- Build only backend: `dotnet build LeanLedgerServer/LeanLedgerServer.csproj -c Release`.
- Run server locally: `dotnet run --project LeanLedgerServer/LeanLedgerServer.csproj` or use the included script `LeanLedgerServer/run-local.sh`.
- Docker build (full image): `docker build -t leanledger .` (uses `Dockerfile`).
- Run tests (all): `dotnet test Tests/Tests.csproj -c Debug` or `dotnet test LeanLedger.sln`.
- Run a single test: discover the test fully-qualified name and filter by it. Examples:
  - `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName=Tests.QueryByMonthTests.DecrementTests.DecrementTest"`
  - `dotnet test Tests/Tests.csproj --filter "Name=DecrementTest"` (less strict; may match multiple tests)
  - To list tests (SDK-dependent): `dotnet test Tests/Tests.csproj --list-tests` then run the filter above.
- Run tests with coverage (coverlet): `dotnet test Tests/Tests.csproj --collect:"XPlat Code Coverage"`.

Lint / format / analyzers
- The repository provides a comprehensive `.editorconfig` that encodes the C# style and analyzer severities. Respect it.
- Apply formatting automatically: install dotnet-format and run: `dotnet tool install -g dotnet-format` then `dotnet format LeanLedger.sln`.
- Analyzer warnings appear on `dotnet build` (Roslyn analyzers + any package analyzers). Fix warnings that are meaningful; treat analyzer failures per project policy (default: warnings).
- System using ordering and grouping: `dotnet_sort_system_directives_first = true`; usings should be placed inside namespace (`csharp_using_directive_placement = inside_namespace`).

Code Style Guidelines (rules agents should follow)
- Formatting: follow `.editorconfig`. Key items:
  - UTF-8, LF for shell scripts, CRLF for Windows batch; preserve `insert_final_newline`.
  - Indent with spaces: 4 for C#/most files; JSON/YAML use 2 where present.
  - `file-scoped` namespaces are preferred (`csharp_style_namespace_declarations = file_scoped`).
  - Prefer expression-bodied members where the `.editorconfig` marks them as preferred.

- Using directives / imports:
  - Place `using` directives inside the namespace (inside_namespace).
  - Sort `System.*` directives first (`dotnet_sort_system_directives_first = true`).
  - Keep using groups minimal and organized; don't leave unused usings (clean them up).

- Types and naming conventions:
  - Types (classes, structs, enums, delegates) use PascalCase.
  - Interfaces use PascalCase and start with `I` (e.g., `IRepository`).
  - Generic type parameters use PascalCase and prefixed with `T` when appropriate (e.g., `TItem`).
  - Method names, properties, events, and public fields use PascalCase.
  - Local variables and method parameters use camelCase.
  - Constants use ALL_CAPS with underscores when configured, otherwise PascalCase for public constants per .editorconfig.

- `var` usage and typing:
  - Follow `.editorconfig` `csharp_style_var_*` settings: this project prefers `var` broadly (including built-in types and when the type is apparent). Use explicit types when it improves readability.
  - Prefer `implicit` type (`var`) when the type is obvious from the right-hand side.

- Nullability and typesafety:
  - Nullable reference types are enabled (`<Nullable>enable</Nullable>`). Respect nullability annotations and handle nullable values explicitly.
  - Prefer `ArgumentNullException.ThrowIfNull(param);` early in public APIs.
  - Use `??` / `?.` and null-coalescing / propagation where appropriate, following `.editorconfig` preferences for null propagation/coalesce.

- Error handling:
  - Do not silently swallow exceptions. Catch and handle exceptions only at appropriate boundaries (top-level request handlers, background worker boundaries).
  - Use specific exception types when possible; avoid catching `Exception` unless rethrowing or logging and propagating.
  - Log errors with context; prefer structured logging (include IDs and relevant state) at the boundary.
  - Validate inputs early and return appropriate HTTP status codes from controller endpoints (use model validation attributes and explicit checks for business rules).

- Tests and test organization:
  - Tests live in `Tests/` and are NUnit tests (see `Tests/Tests.csproj`).
  - Keep tests small, deterministic and isolated; use in-memory or a test database when necessary (the repo contains a `ledger.db` sample file used in development).
  - When adding tests, reference the backend project using ProjectReference (existing approach in `Tests.csproj`).

- Migrations / database:
  - Entity Framework Core is used (`Microsoft.EntityFrameworkCore`, `Sqlite`). Migrations live under `LeanLedgerServer/Migrations/`.
  - When modifying models, create and check in migration files following the existing timestamped naming style.

Repository-specific rules and files
- `.editorconfig` is the source of truth for formatting and many analyzer rules — read it before making style changes: `.editorconfig` (root).
- There are no repository Cursor rules in `.cursor/rules/` nor a Copilot instructions file in `.github/` in this repository. If such files are later added, agents must ingest and follow them.

Agent behavior expectations
- Run unit tests locally before proposing behavior-changing edits. For small changes, run the specific affected tests (`--filter`) rather than the whole suite.
- Keep commits focused and atomic; do not run destructive git operations. Do not force-push branches.
- If adding dependencies, prefer minimal, well-maintained packages and update `LeanLedgerServer/*.csproj` or `Tests/*.csproj` accordingly.

Quick references
- Solution file: `LeanLedger.sln`
- Backend project: `LeanLedgerServer/LeanLedgerServer.csproj`
- Tests project: `Tests/Tests.csproj`
- Run-local helper: `LeanLedgerServer/run-local.sh`
- Dockerfile: `Dockerfile` (multi-stage build that builds a Node frontend then the .NET backend).

Next steps for agents
- 1) If making code edits, run `dotnet build` and `dotnet test` (or filtered tests) and include the failing test names in any report.
- 2) Run `dotnet format` before opening a PR to ensure `.editorconfig` compliance.
- 3) Include short reasoning in PR/commit messages about why rule changes (if any) were made.

Appendix: Example commands
```
dotnet build LeanLedger.sln -c Release
dotnet run --project LeanLedgerServer/LeanLedgerServer.csproj
LeanLedgerServer/run-local.sh
dotnet test Tests/Tests.csproj --filter "Name=DecrementTest"
dotnet test Tests/Tests.csproj --collect:"XPlat Code Coverage"
dotnet tool install -g dotnet-format
dotnet format LeanLedger.sln
docker build -t leanledger .
```
