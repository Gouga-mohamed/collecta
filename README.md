# CollectA

**B2B Accounts Receivable & Collections Management Platform**

> Transformez vos créances en cash.

## Stack

- **Backend** : .NET 8 Web API, Entity Framework Core, PostgreSQL
- **Frontend** : Next.js 14+ (App Router), TypeScript, Tailwind CSS, shadcn/ui
- **Cache / Jobs** : Redis, Hangfire (préparé)
- **Auth** : JWT access + refresh tokens, RBAC claims
- **Infra** : Docker + Docker Compose

## Structure

```
CollectA/
├── src/
│   ├── CollectA.Api                 # Controllers, middleware, config
│   ├── CollectA.Application         # Use cases, DTOs, interfaces
│   ├── CollectA.Domain              # Entities, enums, domain logic
│   └── CollectA.Infrastructure      # EF Core, Identity, services
├── tests/                           # xUnit + FluentAssertions
├── frontend/                        # Next.js app
├── docker-compose.yml
└── ARCHITECTURE.md
```

## Démarrage local

### Prérequis

- Docker & Docker Compose
- .NET 8 SDK
- Node.js 22+

### Lancer l'infrastructure

```bash
docker-compose up -d postgres redis
```

### Lancer le backend

```bash
cd src/CollectA.Api
dotnet run
```

L'API est disponible sur `http://localhost:5000`.
Swagger : `http://localhost:5000/swagger`

### Lancer le frontend

```bash
cd frontend
npm install
npm run dev
```

Le frontend est disponible sur `http://localhost:3000`.

### Compte de démo

Après le premier démarrage, les données de démo sont créées :

- **Tenant** : Atlas Distribution
- **Email** : `admin@atlas-distribution.dz`
- **Mot de passe** : `Admin123!`

## Tests

```bash
dotnet test
```

## Suivi d'avancement

Voir [`docs/PLAN.md`](docs/PLAN.md) pour le plan d'action complet, les phases et le statut détaillé de chaque tâche.

## MVP V1 — Statut

- [x] Architecture documentation
- [x] Backend foundation (.NET 8 + EF Core + PostgreSQL)
- [x] Multi-tenancy (TenantId isolation)
- [x] JWT authentication + refresh tokens
- [x] RBAC (roles + permissions claims)
- [x] Audit logging
- [x] Domain entities (Customer, Invoice, Payment, Collection, etc.)
- [x] Initial migration
- [x] Seed data
- [x] Auth API (register / login / refresh / me)
- [x] Customers API CRUD with tenant isolation
- [x] Docker Compose
- [x] Next.js frontend with shadcn/ui
- [x] Login / Register UI
- [x] Dashboard shell (sidebar + header)

## Prochaines phases

- Phase 2 : Core Receivables (invoices, payments, aging, customer 360)
- Phase 3 : Collections (tasks, actions, promises, disputes)
- Phase 4 : Dashboard CFO + analytics + cash forecast
- Phase 5 : Import CSV/Excel wizard
