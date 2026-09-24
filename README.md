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
- [x] Invoices CRUD + statuts auto (Open / PartiallyPaid / Paid / Overdue / Disputed / Draft / Cancelled / WrittenOff)
- [x] Payments CRUD avec recalcul automatique du statut facture
- [x] Suivi des chèques (numéro, banque, tiré, statuts Received / Deposited / Cleared / Rejected / Returned)
- [x] Receivables summary + aging 7 buckets (Current / 1-30 / 31-60 / 61-90 / 91-120 / 121-180 / >180)
- [x] Customer 360 (infos, contacts, factures, paiements)
- [x] Dashboard CFO (KPIs, aging, top débiteurs, cash 12 mois, activité)
- [x] Analytics (DSO, cash-forecast, collection-rate, collections trend, receivables)
- [x] Import CSV wizard (preview + confirm, upsert clients/factures par Code / InvoiceNumber)
- [x] 31/31 tests unitaires passés

## Pages de l'application

| Route | Description |
|-------|-------------|
| `/login` | Connexion |
| `/register` | Création de compte |
| `/dashboard` | Tableau de bord CFO (KPIs, aging, top débiteurs, cash) |
| `/customers` | Liste clients (recherche, tri, nouveau client) |
| `/customers/[id]` | Fiche 360 d'un client (infos, contacts, factures, paiements) |
| `/invoices` | Liste des factures (filtres statut / retard / client / date) |
| `/invoices/[id]` | Détail facture + timeline paiements + enregistrer un paiement |
| `/payments` | Liste des paiements |
| `/receivables` | Créances (KPIs, aging cliquable, overdue) |
| `/receivables/import` | Assistant import CSV 3 étapes |
| `/collections` | Placeholder — Phase 3 (recouvrement) |
| `/promises` | Placeholder — Phase 3 (promesses) |
| `/disputes` | Placeholder — Phase 3 (litiges) |
| `/analytics` | Placeholder — Phase 4 (analytics avancé) |
| `/settings` | Placeholder — Phase 5 (paramètres) |

## Données de démo

Après le premier démarrage, un tenant de démo est créé automatiquement :

- **Tenant :** Atlas Distribution SPA (`admin@atlas-distribution.dz` / `Admin123!`)
- **11 clients algériens** répartis sur Alger, Oran, Constantine, Sétif, Annaba et Blida :
  - Pharmacie El Yasmine (Alger) — pharmacie
  - AgroDis Sud (Blida) — distribution agroalimentaire
  - Gros Œuvre Bâtiments Est (Constantine) — BTP
  - Benali Frères Électroménager (Sétif) — électroménager
  - Nord Lait Distribution (Oran) — produits laitiers
  - Pharmacie du Centre (Annaba) — pharmacie
  - BTP Horizon Travaux (Alger) — BTP / promotion immobilière
  - Electro Atlas Magasin (Alger) — électroménager / high-tech
  - Humasud Distribution (Blida) — distribution agroalimentaire
  - Grossiste El Anka (Oran) — grossiste alimentaire
  - Pharmacie Essalem (Constantine) — pharmacie
- **34 factures** `INV-2025-001` à `INV-2025-034` réparties sur tous les buckets aging (courant, 1-30, 31-60, 61-90, 91-120, 121-180, >180) plus 2 brouillons et 1 facture en litige.
- **15 paiements** : virements (BNA, BDL, CPA, BEA, AGB, Al Baraka Bank), espèces et **6 chèques** (statuts : Received, Deposited, Cleared, Rejected, Returned).

Pour régénérer les données de démo : supprimer la base PostgreSQL et redémarrer l'API (`dotnet run`) — le seed s'exécute automatiquement si aucun tenant n'existe.

## Prochaines phases

- Phase 3 : Collections (tasks, actions, promises, disputes, notifications)
- Phase 4 : Dashboard avancé + analytics + cash forecast
- Phase 5 : Integrations & polish (ERP, WhatsApp, multi-devise, onboarding, settings)
