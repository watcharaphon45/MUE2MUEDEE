# MUE2MUEDEE Coding Standards & Architecture Rules

This rule defines the core system patterns and conventions for MUE2MUEDEE project that MUST be strictly followed across all features.

## 1. Frontend Rules (Angular 22+)
- **File Separation**: Always separate `.ts`, `.html`, and `.css` for every component. Never use inline template or inline styles.
- **Form Architecture**: Use Angular Reactive Forms (`FormGroup`, `FormControl`, `ControlValueAccessor`).
- **Feature Structure**: Group feature services and models at the feature folder level (e.g. `inventory.service.ts`, `inventory.models.ts`). Do not create bloated per-component services.
- **Shared Components**: Located in `src/app/shared/components/`:
  - `<app-textbox>`: Text input with label, icon, validation error.
  - `<app-number>`:
    - `<app-number>` (no currency attribute): Digits only / integer input (e.g., shutter count, quantity).
    - `<app-number currency>`: Financial format (THB ฿, comma separators, 2 decimal places).
  - `<app-textarea>`: Multiline text for notes, descriptions.
  - `<app-select>`: Dropdown select.
  - `<app-checkbox>`: Boolean toggle.
  - `<app-status-badge>`: State badges for status enums.
- **CLI Commands on Windows**: Always run `npm.cmd` or `npx.cmd` instead of `npm` / `npx` to prevent PowerShell execution policy blocks.

## 2. Backend Rules (ASP.NET Core 10 & Clean Architecture)
- **Response Format**: Every endpoint MUST return `ApiResponse<T>`:
  - Format: `{ success, statusCode, message, data, errors, timestamp }`
  - Base class: Controllers inherit from `ApiControllerBase` and return `Success(data, message)` or `CreatedSuccess(...)`.
- **Dynamic Exceptions**:
  - Do NOT hardcode error messages centrally.
  - Throw `new AppException("Specific message here", statusCode)` at the point of failure.
  - Handled by `ExceptionHandlingMiddleware` which automatically populates `ApiResponse.Fail(...)`.
- **Database Architecture**:
  - Strict pattern: **Entity -> Configuration -> DbContext**.
  - `Entity`: Defined in `Domain/Entities/` (inheriting `AuditableEntity<TId>`, `BaseEntity<TId>`, implementing `ITenantEntity`, `ISoftDelete`).
  - `Configuration`: Defined in `Infrastructure/Persistence/Configurations/` implementing `IEntityTypeConfiguration<T>`. Never place fluent configs directly inside `DbContext.OnModelCreating`.
  - `DbContext`: Register `DbSet<T>`. `modelBuilder.ApplyConfigurationsFromAssembly(...)` handles registration automatically.
- **Naming Conventions**:
  - C# code: `PascalCase` (e.g. `UserProduct`, `InventoryItem`, `SerialNumber`).
  - PostgreSQL DB: `snake_case` (e.g. `user_product`, `inventory_item`, `serial_number`).
  - Configured via EF Core `UseSnakeCaseNamingConvention()`.
- **Multi-Tenancy**:
  - Tenant isolation is strictly enforced via `StoreId` in JWT claims and `CurrentStore`.
  - Never trust `StoreId` in request bodies for tenant operations.
  - Automatically filtered in `ApplicationDbContext` global query filters.
