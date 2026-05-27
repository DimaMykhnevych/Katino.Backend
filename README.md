# Katino.Backend

Backend monorepo for **Katino Clo©** — a Ukrainian women's clothing brand. Contains two independent API systems that share a common domain and infrastructure layer:

- **Katino CRM** — internal CRM system for managing orders, products, inventory, logistics, and finances
- **Katino Store** — public-facing backend for the brand's website

Both systems are built with .NET 10 and follow a Clean/Layered Architecture.

---

## Solution Structure

```
Katino.Web/
├── Katino.Web/                  # CRM — ASP.NET Core REST API
├── Katino.Application/          # CRM — CQRS handlers, DTOs, mappers
├── Katino.Domain/               # Shared — entities, interfaces, enums
├── Katino.Infrastructure/       # Shared — EF Core, repositories, services
├── Katino.Functions/            # Azure Functions worker (background jobs)
├── Katino.Migrator/             # Data/image migration utility
└── site/
    ├── Katino.Store.Web/        # Store — ASP.NET Core REST API
    └── Katino.Store.Application/ # Store — CQRS handlers, DTOs, mappers
```

**Katino.Domain** and **Katino.Infrastructure** are referenced by both the CRM and Store systems.

---

## Architecture

The solution uses a layered Clean Architecture with CQRS:

```
HTTP Request
    ↓
Controllers  (Katino.Web / Katino.Store.Web)
    ↓
Commands / Queries  (MediatR)
    ↓
Handlers  (Katino.Application / Katino.Store.Application)
    ↓
Repository Interfaces / Service Interfaces  (Katino.Domain)
    ↓
Implementations  (Katino.Infrastructure)
    ↓
MySQL Database  (EF Core + Pomelo)
```

**Patterns used:**
- CQRS via MediatR
- Generic Repository pattern
- Strategy pattern (delivery handlers)
- Builder pattern (product query builder)
- Installer pattern (auto-discovered DI installers via reflection)

---

## Tech Stack

| Category | Technology |
|---|---|
| Framework | .NET 10 / ASP.NET Core |
| Database | MySQL (Pomelo EF Core 9.x) |
| ORM | Entity Framework Core 9 |
| CQRS | MediatR 13 |
| Mapping | AutoMapper 15 |
| Auth | ASP.NET Identity + JWT Bearer |
| Cloud Storage | Azure Blob Storage |
| Background Jobs | Azure Functions (.NET Worker) |
| Delivery Integration | NovaPost API (custom HTTP clients) |
| Notifications | Telegram Bot API |
| Image Processing | SixLabors.ImageSharp (WebP) |
| Logging | log4net (rolling file) |
| Rate Limiting | ASP.NET Core built-in |
| API Docs | Swagger / Swashbuckle |

---

## Key Features

### CRM System
- **Product management** — multi-variant products (color × size), pricing tiers (cost / wholesale / drop / retail), photo management, auto-generated SKUs
- **Order processing** — full order lifecycle with status tracking, order items, sewing/production queue, recipient and address management
- **NovaPost integration** — creating internet documents, real-time delivery status sync (via Azure Function timer), warehouse and city lookup
- **Finance module** — expense tracking by category, P&L reports, Excel report storage in Azure Blob
- **Telegram notifications** — configurable per-user notification types for order events (new order, rejection, delivery updates, etc.)
- **Statistics & dashboard** — order metrics and aggregate reports
- **Role-based access** — Admin and Owner roles with fine-grained endpoint authorization

### Store (Site Backend)
- Customer-facing product catalog API
- Shares the same domain entities and database with the CRM
- Separate auth and application layer

---

## Projects Overview

### `Katino.Domain`
Core of the system. Contains:
- **Entities** (30+): `Product`, `ProductVariant`, `Order`, `OrderItem`, `NpWarehouse`, `FinanceEntry`, `AppUser`, `TelegramSettings`, etc.
- **Enums** (15+): `OrderStatus`, `ProductStatus`, `TelegramNotificationType`, `FinanceCategoryType`, etc.
- **Repository & Service interfaces** — all contracts used by the application layer
- **Options / Constants / Exceptions**

### `Katino.Infrastructure`
All side-effectful implementations:
- **EF Core DbContext** (`KatinoDbContext`) — extends `IdentityDbContext<AppUser, UserRole, Guid>`
- **Repositories** (25+) — generic base + entity-specific implementations
- **Services**: Azure Blob Storage, NovaPost (3 HTTP clients), Telegram Bot, Order/Product/Finance business logic
- **EF Core Migrations** and **DbSeeder** (initial data on startup)

### `Katino.Application`
CRM-specific application layer:
- **Commands** (50+): Add/Update/Delete for all entities, finance entries, auth
- **Queries** (20+): Product/Order listings, statistics, NovaPost lookups, app logs
- **DTOs** (30+) and **AutoMapper profiles**

### `Katino.Functions`
Azure Functions Worker:
- `NpStatusSync` — timer-triggered function that polls NovaPost and syncs delivery statuses into the DB

### `Katino.Migrator`
One-off CLI utility:
- Reads `ProductPhoto` records from DB
- Converts images to WebP using ImageSharp
- Uploads results to Azure Blob Storage
- Supports dry-run mode and quality tuning

---

## Configuration

The application uses `appsettings.json` + User Secrets (Development). Key configuration sections:

| Section | Purpose |
|---|---|
| `MySqlConfig` | MySQL connection string |
| `SecretKey` | JWT signing key |
| `AzureBlobStorage` | Azure storage connection |
| `NovaPost` | NovaPost API credentials |
| `Telegram` | Bot token and chat configuration |
| `EmailService` | SMTP settings |

Logs are written to the path specified by the `KATINO_LOG_DIR` environment variable (default: `%LocalAppData%/Katino/logs`), with daily rolling rotation via log4net.

---

## Getting Started

### Prerequisites
- .NET 10 SDK
- MySQL 8+
- Azure Storage account (or Azurite emulator for local dev)

### Setup

1. Clone the repo and navigate to the solution:
   ```bash
   cd Katino.Web
   ```

2. Configure user secrets (Development):
   ```bash
   dotnet user-secrets set "MySqlConfig:ConnectionString" "server=...;database=katino;..."
   dotnet user-secrets set "SecretKey:Value" "<your-jwt-secret>"
   ```

3. Apply database migrations:
   ```bash
   dotnet ef database update --project Katino.Infrastructure --startup-project Katino.Web
   ```

4. Run the CRM API:
   ```bash
   dotnet run --project Katino.Web
   ```

5. Open Swagger UI at `https://localhost:{port}/swagger`

---

## API Overview

The CRM API exposes 20+ controllers covering:

| Controller Group | Endpoints |
|---|---|
| Auth | Sign-in, current user |
| Products | CRUD, variants, photos |
| Orders | CRUD, items, tags, NovaPost options |
| NovaPost | Warehouses, cities, contact persons, document sync |
| Finance | Expenses, categories, P&L reports |
| Statistics | Dashboard metrics, order analytics |
| CRM Settings | User preferences, Telegram config |

All protected endpoints require a JWT Bearer token and Admin or Owner role.

**Rate limiting:**
- Global: 100 requests / 60 s per IP
- Auth endpoints: 5 attempts / 60 s per IP (sliding window)
