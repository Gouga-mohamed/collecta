# COLLECTA — Architecture & MVP Plan

**Tagline:** Transformez vos créances en cash.

## 1. Product Architecture

COLLECTA est une plateforme B2B de **Accounts Receivable & Collections Management**.

### Flux de valeur

```
Invoice → Due Date → Collection Action → Promise → Payment → Cash
```

### Modules MVP V1

1. Authentication / RBAC
2. Multi-tenancy
3. Customers
4. Invoices
5. Payments
6. Receivables & Aging
7. Collection Tasks & Actions
8. Promise to Pay
9. Dashboard CFO
10. CSV Import Wizard
11. Audit Log

## 2. Technical Architecture

### Stack

| Couche | Technologie |
|--------|-------------|
| Frontend | Next.js 14 (App Router), TypeScript, Tailwind CSS, shadcn/ui |
| State Server | TanStack Query |
| Tables | TanStack Table |
| Forms | React Hook Form + Zod |
| Charts | Recharts |
| Backend | .NET 8 Web API, C# |
| ORM | Entity Framework Core |
| Database | PostgreSQL 16 |
| Cache / Jobs | Redis + Hangfire |
| Auth | JWT Access + Refresh tokens, RBAC claims |
| Docs API | Swagger / OpenAPI |
| Infra | Docker + Docker Compose |

### Architecture Backend : Modular Monolith

```
CollectA/
├── src/
│   ├── CollectA.Api/                 # Controllers, middleware, config
│   ├── CollectA.Application/         # Use cases, DTOs, interfaces services
│   ├── CollectA.Domain/              # Entités, value objects, règles métier pures
│   ├── CollectA.Infrastructure/    # EF Core, identity, messaging, file storage
│   └── CollectA.Workers/             # Hangfire background jobs
├── tests/
│   ├── CollectA.Domain.Tests/
│   ├── CollectA.Application.Tests/
│   └── CollectA.Api.Tests/
└── docker-compose.yml
```

### Architecture Frontend

```
frontend/
├── app/                    # Next.js App Router
│   ├── (auth)/
│   ├── (dashboard)/
│   │   ├── dashboard/
│   │   ├── customers/
│   │   ├── invoices/
│   │   ├── receivables/
│   │   ├── collections/
│   │   ├── promises/
│   │   └── settings/
│   ├── api/
│   └── layout.tsx
├── components/
│   ├── ui/                 # shadcn components
│   ├── layout/             # sidebar, header, shell
│   ├── tables/
│   └── forms/
├── hooks/
├── lib/
│   ├── api.ts
│   ├── auth.ts
│   └── utils.ts
├── types/
└── public/
```

## 3. Database ERD

```text
Tenant (1) ───< User
Tenant (1) ───< Role
Tenant (1) ───< Customer
Tenant (1) ───< Invoice
Tenant (1) ───< Payment
Tenant (1) ───< CollectionAction
Tenant (1) ───< CollectionTask
Tenant (1) ───< PromiseToPay
Tenant (1) ───< AuditLog
Tenant (1) ───< Integration
Tenant (1) ───< ReminderTemplate
Tenant (1) ───< ReminderRule

Customer (1) ───< Invoice
Customer (1) ───< Payment
Customer (1) ───< CollectionAction
Customer (1) ───< CollectionTask
Customer (1) ───< PromiseToPay
Customer (1) ───< CustomerContact
Customer (1) ───< CustomerAssignment
Customer (1) ───< Dispute
Customer (1) ───< CreditLimit
Customer (1) ───< RiskScore

Invoice (1) ───< InvoiceLine
Invoice (1) ───< Payment
Invoice (1) ───< CollectionAction
Invoice (1) ───< PromiseToPay
Invoice (1) ───< Dispute

Payment (1) ───< Cheque (optionnel)

User (1) ───< CollectionTask (AssignedTo)
User (1) ───< CollectionAction (CreatedBy / AssignedTo)
User (1) ───< PromiseToPay (Responsible)
User (1) ───< Dispute (Responsible)

Role (1) ───< RolePermission >─── (1) Permission
```

## 4. Backend Folder Structure

```
CollectA.Api/
├── Controllers/
│   ├── AuthController.cs
│   ├── TenantsController.cs
│   ├── UsersController.cs
│   ├── CustomersController.cs
│   ├── InvoicesController.cs
│   ├── PaymentsController.cs
│   ├── ReceivablesController.cs
│   ├── CollectionsController.cs
│   ├── PromisesController.cs
│   └── DashboardController.cs
├── Middleware/
│   ├── TenantResolutionMiddleware.cs
│   ├── ExceptionHandlingMiddleware.cs
│   └── AuditMiddleware.cs
├── Program.cs
└── appsettings.json

CollectA.Application/
├── Common/
│   ├── Interfaces/
│   ├── Models/
│   └── Behaviors/
├── Features/
│   ├── Auth/
│   ├── Tenants/
│   ├── Users/
│   ├── Customers/
│   ├── Invoices/
│   ├── Payments/
│   ├── Receivables/
│   ├── Collections/
│   ├── Promises/
│   └── Dashboard/
└── DependencyInjection.cs

CollectA.Domain/
├── Entities/
├── Enums/
├── ValueObjects/
├── Exceptions/
└── Common/

CollectA.Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   ├── Migrations/
│   └── Seed/
├── Identity/
├── Services/
├── Messaging/
└── DependencyInjection.cs
```

## 5. Frontend Folder Structure

```
frontend/
├── app/
│   ├── layout.tsx
│   ├── page.tsx
│   ├── (auth)/login/page.tsx
│   ├── (auth)/register/page.tsx
│   └── (dashboard)/
│       ├── layout.tsx
│       ├── dashboard/page.tsx
│       ├── customers/page.tsx
│       ├── customers/[id]/page.tsx
│       ├── invoices/page.tsx
│       ├── receivables/aging/page.tsx
│       ├── collections/tasks/page.tsx
│       └── promises/page.tsx
├── components/
│   ├── ui/
│   ├── layout/
│   ├── kpis/
│   ├── tables/
│   └── forms/
├── hooks/
├── lib/
│   ├── api.ts
│   ├── query-client.ts
│   └── auth.tsx
├── types/
└── styles/
```

## 6. API Contract (MVP)

### Auth

```
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
GET  /api/auth/me
```

### Tenants

```
POST /api/tenants
GET  /api/tenants/current
PUT  /api/tenants/current
```

### Users

```
GET    /api/users
POST   /api/users
GET    /api/users/{id}
PUT    /api/users/{id}
DELETE /api/users/{id}
```

### Customers

```
GET    /api/customers
POST   /api/customers
GET    /api/customers/{id}
PUT    /api/customers/{id}
DELETE /api/customers/{id}
GET    /api/customers/{id}/invoices
GET    /api/customers/{id}/payments
```

### Invoices

```
GET    /api/invoices
POST   /api/invoices
GET    /api/invoices/{id}
PUT    /api/invoices/{id}
DELETE /api/invoices/{id}
POST   /api/invoices/{id}/payments
POST   /api/invoices/import
```

### Payments

```
GET  /api/payments
POST /api/payments
GET  /api/payments/{id}
```

### Receivables

```
GET /api/receivables/summary
GET /api/receivables/aging
GET /api/receivables/overdue
```

### Collections

```
GET    /api/collections/tasks
POST   /api/collections/tasks
GET    /api/collections/actions
POST   /api/collections/actions
PUT    /api/collections/actions/{id}
DELETE /api/collections/actions/{id}
```

### Promises

```
GET    /api/promises
POST   /api/promises
GET    /api/promises/{id}
PUT    /api/promises/{id}
```

### Dashboard

```
GET /api/dashboard
```

## 7. MVP Implementation Plan

### Phase 1 — Foundation

1. Scaffold backend .NET 8 solution (modular monolith)
2. Scaffold frontend Next.js + shadcn/ui
3. PostgreSQL + Redis via Docker Compose
4. Domain entities (Tenant, User, Role, Permission, Customer, Invoice, Payment, etc.)
5. EF Core DbContext + migrations + tenant isolation
6. JWT authentication + refresh tokens
7. RBAC (claims-based)
8. Audit log middleware/service
9. Seed demo data
10. Login / Register frontend
11. Dashboard shell + sidebar

### Phase 2 — Core Receivables

1. Customers CRUD + Customer 360
2. Invoices CRUD + status auto-calculation
3. Payments CRUD + remaining amount recalculation
4. Receivables summary
5. Aging buckets
6. CSV import wizard

### Phase 3 — Collections

1. Collection tasks
2. Collection actions
3. Promise to Pay
4. Disputes
5. Workflows & reminders

### Phase 4 — Intelligence

1. Dashboard CFO
2. Risk score
3. Cash forecast
4. Analytics

---

**Décisions techniques MVP**

- Clean Architecture pragmatique : domain pur, application pour les use cases, infrastructure pour EF/identity, API pour controllers.
- Multi-tenancy par `TenantId` sur chaque entité métier + filtre global EF Core.
- JWT stateless avec refresh tokens stockés en base (revocation possible).
- RBAC via claims `permissions` générés à partir des rôles utilisateur.
- Hangfire pour les jobs (rappels, workflow) mais désactivé en V1 si non critique.
- Frontend Next.js App Router côté client pour le dashboard (TanStack Query).
