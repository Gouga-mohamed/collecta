# CollectA — Plan d'action et suivi d'avancement

> **Nom :** CollectA  
> **Tagline :** Transformez vos créances en cash.  
> **Positionnement :** B2B Accounts Receivable & Collections Management Platform

---

## Vue d'ensemble

Ce document est le plan d'action vivant du projet. Il découpe la construction du SaaS en phases concrètes, chacune avec une **Definition of Done** claire.

Légende :
- ✅ Terminé
- 🚧 En cours
- ⏳ En attente
- ❌ Bloqué / à revoir

---

## Phase 0 — Architecture & Conception

**Objectif :** Poser les fondations produit et technique avant d'écrire du code métier.

### Livrables

| # | Tâche | Statut | Notes |
|---|-------|--------|-------|
| 0.1 | Architecture produit | ✅ | Positionnement, personas, workflow valeur |
| 0.2 | Architecture technique | ✅ | Stack, Modular Monolith, folder structure |
| 0.3 | Database ERD | ✅ | Entités, relations, multi-tenancy |
| 0.4 | API contract | ✅ | Endpoints MVP avec DTOs |
| 0.5 | Backend folder structure | ✅ | Clean Architecture pragmatique |
| 0.6 | Frontend folder structure | ✅ | Next.js App Router + shadcn/ui |
| 0.7 | MVP implementation plan | ✅ | Phases 1 à 4 priorisées |

**Definition of Done :**
- `ARCHITECTURE.md` rédigé et validé.
- Décisions techniques justifiées (JWT, RBAC claims, tenant isolation explicite).
- Roadmap alignée sur les priorités MVP.

---

## Phase 1 — Foundation

**Objectif :** Avoir une base technique sécurisée, multi-tenant, authentifiée et déployable.

### Backend

| # | Tâche | Statut | Fichiers / Modules |
|---|-------|--------|--------------------|
| 1.1 | Scaffold solution .NET 8 | ✅ | `CollectA.sln`, 4 projets `src/`, 2 projets `tests/` |
| 1.2 | NuGet packages & références | ✅ | EF Core, Identity, JWT, Hangfire, Redis, MediatR, FluentValidation |
| 1.3 | Domain entities | ✅ | `Domain/Entities/`, `Domain/Enums/`, `Domain/Common/` |
| 1.4 | EF Core DbContext | ✅ | `ApplicationDbContext` + configurations |
| 1.5 | Multi-tenancy | ✅ | `TenantId` sur toutes les entités, `TenantContext` depuis JWT |
| 1.6 | JWT authentication | ✅ | `JwtService`, access + refresh tokens |
| 1.7 | RBAC (roles + permissions) | ✅ | `Owner` rôle seedé avec 16 permissions |
| 1.8 | Audit logging | ✅ | `AuditService`, `AuditLog` entity |
| 1.9 | Initial EF migration | ✅ | `InitialCreate` migration |
| 1.10 | Seed data | ✅ | `SeedData.cs` : tenant Atlas Distribution + clients + factures |
| 1.11 | Auth API | ✅ | `AuthController` : register, login, refresh, me, logout |
| 1.12 | Customers API CRUD | ✅ | `CustomersController` avec isolation tenant |
| 1.13 | Health checks | ✅ | `/health`, `/health/db` |
| 1.14 | CORS | ✅ | Policy `AllowFrontend` |

### Frontend

| # | Tâche | Statut | Fichiers / Modules |
|---|-------|--------|--------------------|
| 1.15 | Scaffold Next.js + shadcn/ui | ✅ | `frontend/` avec Tailwind v4 |
| 1.16 | Auth state management | ✅ | `lib/auth.tsx`, `lib/api.ts` |
| 1.17 | TanStack Query provider | ✅ | `lib/providers.tsx` |
| 1.18 | Login page | ✅ | `app/(auth)/login/page.tsx` |
| 1.19 | Register page | ✅ | `app/(auth)/register/page.tsx` |
| 1.20 | Dashboard shell | ✅ | `components/layout/sidebar.tsx`, `header.tsx` |
| 1.21 | Dashboard home | ✅ | `app/(dashboard)/dashboard/page.tsx` |

### Infra & DevOps

| # | Tâche | Statut | Fichiers / Modules |
|---|-------|--------|--------------------|
| 1.22 | Docker Compose | ✅ | `docker-compose.yml` (postgres, redis, backend, frontend) |
| 1.23 | Backend Dockerfile | ✅ | `src/CollectA.Api/Dockerfile` |
| 1.24 | Frontend Dockerfile | ✅ | `frontend/Dockerfile` |
| 1.25 | Tests unitaires | ✅ | `InvoiceTests.cs` (4 tests passés) |
| 1.26 | Build global OK | ✅ | `dotnet build` + `npm run build` |

### Definition of Done Phase 1

- [x] Backend buildable sans warning.
- [x] Frontend buildable sans erreur TypeScript.
- [x] Migration EF Core générée.
- [x] Seed exécutable au démarrage.
- [x] Auth fonctionnelle (register/login/refresh).
- [x] Tenant isolation opérationnelle sur Customers.
- [x] Tests unitaires passent.
- [x] Docker Compose prêt.

**Compte démo généré :**
- Email : `admin@atlas-distribution.dz`
- Mot de passe : `Admin123!`

---

## Phase 2 — Core Receivables

**Objectif :** Implémenter le cœur métier : clients, factures, paiements, créances, aging, Customer 360 et import CSV.

### Backend

| # | Tâche | Statut | Notes |
|---|-------|--------|-------|
| 2.1 | Customers full CRUD + search | ⏳ | DTOs, validation FluentValidation |
| 2.2 | Customer 360 endpoint | ⏳ | `/api/customers/{id}/summary` |
| 2.3 | Invoices CRUD | ⏳ | Statuts auto, remaining amount, days overdue |
| 2.4 | Payments CRUD | ⏳ | Mise à jour automatique de `PaidAmount` sur invoice |
| 2.5 | Cheques module (MVP) | ⏳ | Statuts de suivi |
| 2.6 | Receivables summary | ⏳ | Total, overdue, current, overdue % |
| 2.7 | Aging buckets | ⏳ | Current / 1-30 / 31-60 / 61-90 / 91-120 / 121-180 / >180 |
| 2.8 | Customer risk indicators basiques | ⏳ | Total due, overdue, DSO approximatif |
| 2.9 | CSV import wizard | ⏳ | Upload, mapping, validation, preview, import |
| 2.10 | DTOs & validation | ⏳ | Ne jamais exposer les entités EF |

### Frontend

| # | Tâche | Statut | Notes |
|---|-------|--------|-------|
| 2.11 | Customers list page | ⏳ | TanStack Table, filtres, pagination |
| 2.12 | Customer 360 page | ⏳ | Onglets overview/invoices/payments/timeline |
| 2.13 | Invoices list page | ⏳ | Table avec statuts, montants, overdue |
| 2.14 | Invoice detail page | ⏳ | Timeline, actions, paiements |
| 2.15 | Payments page | ⏳ | Enregistrement manuel |
| 2.16 | Receivables dashboard | ⏳ | KPIs + aging chart |
| 2.17 | CSV import wizard UI | ⏳ | 5 étapes avec preview d'erreurs |

### Definition of Done Phase 2

- [ ] CRUD complet clients, factures, paiements.
- [ ] Calculs métier validés : remaining, overdue, aging.
- [ ] Customer 360 navigable.
- [ ] Import CSV fonctionnel.
- [ ] Tests unitaires sur calculs financiers.
- [ ] Frontend responsive, loading/error/empty states.

---

## Phase 3 — Collections

**Objectif :** Activer le workflow de recouvrement : tâches, actions, promesses, litiges.

### Backend

| # | Tâche | Statut | Notes |
|---|-------|--------|-------|
| 3.1 | Collection tasks CRUD | ⏳ | Assignation, due date, priorité, statut |
| 3.2 | Collection actions CRUD | ⏳ | Phone, email, WhatsApp, SMS, meeting, reminder |
| 3.3 | Promise to Pay module | ⏳ | Statuts pending/fulfilled/partially/broken |
| 3.4 | Disputes workflow | ⏳ | Open → Investigating → Waiting → Resolved → Closed |
| 3.5 | Agent dashboard data | ⏳ | My tasks, today's calls, promises due |
| 3.6 | Email reminders templates | ⏳ | Friendly, due, overdue, final notice |
| 3.7 | Collection workflow engine (base) | ⏳ | Règles overdue → actions |

### Frontend

| # | Tâche | Statut | Notes |
|---|-------|--------|-------|
| 3.8 | Collections tasks page | ⏳ | Vue agent + vue manager |
| 3.9 | Collection action modal | ⏳ | Création d'action avec outcome |
| 3.10 | Promises page | ⏳ | Liste, filtres par statut |
| 3.11 | Disputes page | ⏳ | Workflow visuel |
| 3.12 | Agent dashboard "Today" | ⏳ | Vue quotidienne des actions |

### Definition of Done Phase 3

- [ ] Un agent peut voir ses tâches du jour.
- [ ] Un manager peut assigner des actions.
- [ ] Promesses et litiges traçables.
- [ ] Templates de relance configurables.

---

## Phase 4 — Intelligence & Analytics

**Objectif :** Donner au CFO et aux agents la visibilité complète : dashboard, DSO, cash forecast, risk score.

### Backend

| # | Tâche | Statut | Notes |
|---|-------|--------|-------|
| 4.1 | Dashboard CFO endpoint | ⏳ | KPIs consolidés par tenant |
| 4.2 | DSO calculation | ⏳ | Période glissante |
| 4.3 | Collection rate / recovery rate | ⏳ | Taux de recouvrement |
| 4.4 | Cash forecast | ⏳ | Contractual + expected selon comportement client |
| 4.5 | Customer risk score | ⏳ | 0-100 avec facteurs explicites |
| 4.6 | Analytics endpoints | ⏳ | Receivables, customers, collections, cash |
| 4.7 | Notifications | ⏳ | Centre de notifications, types métier |

### Frontend

| # | Tâche | Statut | Notes |
|---|-------|--------|-------|
| 4.8 | Dashboard CFO complet | ⏳ | KPI cards, charts, top debtors |
| 4.9 | Aging chart interactif | ⏳ | Clic vers liste filtrée |
| 4.10 | Cash forecast chart | ⏳ | Scénarios contractual vs expected |
| 4.11 | Customer risk distribution | ⏳ | Badges Low/Medium/High/Critical |
| 4.12 | Analytics pages | ⏳ | Receivables, collections, cash |
| 4.13 | Notification center | ⏳ | Bell + dropdown |

### Definition of Done Phase 4

- [ ] Dashboard CFO riche et performant.
- [ ] DSO, collection rate, cash forecast calculés.
- [ ] Risk score transparent.
- [ ] Notifications temps réel.

---

## Phase 5 — Integrations & Polish

**Objectif :** Connecter le produit au monde extérieur et le rendre production-ready.

| # | Tâche | Statut | Notes |
|---|-------|--------|-------|
| 5.1 | Integration architecture | ⏳ | `Integration` entity, providers, logs |
| 5.2 | CSV/Excel import robuste | ⏳ | Mapping avancé, erreurs détaillées |
| 5.3 | Email provider abstraction | ⏳ | `IMessageProvider` |
| 5.4 | WhatsApp provider placeholder | ⏳ | Abstraction sans fausse intégration |
| 5.5 | Payment provider abstraction | ⏳ | `IPaymentProvider` |
| 5.6 | Global search (Ctrl+K) | ⏳ | Customers, invoices, payments, etc. |
| 5.7 | Onboarding 5 étapes | ⏳ | Company → currency → customers → invoices → team |
| 5.8 | Settings pages | ⏳ | Tenant, users, roles, branding |
| 5.9 | i18n fr/en/ar | ⏳ | RTL arabe |
| 5.10 | Multi-currency DZD/EUR/USD | ⏳ | Pas de conversion silencieuse |
| 5.11 | Monitoring & logging | ⏳ | Structured logs, health checks |
| 5.12 | Rate limiting & sécurité | ⏳ | Input validation, secure uploads |
| 5.13 | Documentation API | ⏳ | Swagger complet |
| 5.14 | Tests E2E critiques | ⏳ | Auth, tenant isolation, collections |

### Definition of Done Phase 5

- [ ] Application commercialisable.
- [ ] Onboarding fluide.
- [ ] Intégrations extensibles.
- [ ] Sécurité et monitoring en place.

---

## Backlog V2 (post-MVP)

| # | Tâche | Statut | Notes |
|---|-------|--------|-------|
| V2.1 | WhatsApp Business API | ⏳ | Intégration réelle |
| V2.2 | ERP integrations | ⏳ | API + CSV |
| V2.3 | Banking / PSP | ⏳ | Virement, carte, CIB |
| V2.4 | AI assistant "Ask Collecta" | ⏳ | Requêtes NL, respect RBAC |
| V2.5 | Advanced forecasting | ⏳ | ML/heuristique |
| V2.6 | Mobile application | ⏳ | React Native / PWA |
| V2.7 | Advanced workflows | ⏳ | Moteur de règles visuel |

---

## Métriques actuelles

- **Build backend :** ✅ 0 erreur, 0 warning
- **Build frontend :** ✅ TypeScript OK
- **Tests :** ✅ 4/4 passés
- **Migrations :** ✅ `InitialCreate` générée
- **Docker Compose :** ✅ Fichiers prêts (non exécuté ici faute de daemon)
- **Documentation :** ✅ `ARCHITECTURE.md`, `README.md`, `docs/PLAN.md`

---

## Décisions techniques clés

1. **Clean Architecture pragmatique** : domain pur, application pour les use cases, infrastructure pour EF/Identity, API pour controllers.
2. **Multi-tenancy par `TenantId`** : isolation explicite dans les requêtes, pas de filtre global magique.
3. **JWT stateless + refresh tokens stockés** : révocation possible, scalable.
4. **RBAC via claims `permission`** : granularité fine sans surcharger Identity.
5. **Identity custom** : `ApplicationUser`/`ApplicationRole` avec `TenantId` pour garder l'isolation.
6. **Modular Monolith** : modules découplés, future extraction en services possible.

---

## Prochaine action

**Phase 2 — Core Receivables :** commencer par le module Invoices (CRUD + calculs métier) puis Payments, puis Aging.
