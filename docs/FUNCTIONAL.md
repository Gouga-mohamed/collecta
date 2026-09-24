# Documentation fonctionnelle CollectA V1

> Document destiné aux équipes produit, commerciales et techniques qui souhaitent comprendre le périmètre et les règles métier de CollectA sans lire le code.

---

## 1. Vision produit

### 1.1 Positionnement

CollectA est une plateforme B2B de gestion des créances et de recouvrement, conçue pour le marché algérien. Sa promesse : **"Transformez vos créances en cash"**.

Elle aide les PME et ETI à :

- Centraliser leurs factures et leurs paiements.
- Identifier rapidement les retards de paiement.
- Prioriser les actions de recouvrement.
- Anticiper la trésorerie.
- Réduire le DSO (Days Sales Outstanding).

### 1.2 Personas

| Persona | Rôle | Besoins principaux |
|---------|------|-------------------|
| **CFO / Directeur financier** | Vision stratégique | Tableau de bord consolidé, DSO, cash forecast, taux de recouvrement, top débiteurs. |
| **Credit Manager** | Gestion du risque client | Suivi des plafonds de crédit, aging, historique de paiements, alertes retard. |
| **Agent de recouvrement** | Opérationnel au quotidien | Liste des factures en retard, fiches clients, enregistrement des actions et des paiements. |
| **Admin** | Configuration et sécurité | Gestion des utilisateurs, rôles, tenant, isolation des données. |

### 1.3 Flux de valeur

```
Invoice (facture émise)
   ↓
Due Date (date d'échéance)
   ↓
Collection Action (relance téléphone, email, visite...)
   ↓
Promise to Pay (promesse de paiement)
   ↓
Payment (virement, espèces, chèque)
   ↓
Cash (encaissement effectif)
```

Dans la V1, les étapes **Invoice**, **Payment** et le suivi du **Due Date** sont pleinement opérationnels. Les étapes **Collection Action** et **Promise to Pay** existent en modèle de données mais ne sont pas encore actives en API/UI (Phase 3).

---

## 2. Périmètre V1 — Modules livrés

### 2.1 Authentification, RBAC et multi-tenancy

**Objectif** : sécuriser l'accès, isoler les données par entreprise et contrôler les permissions.

**Fonctionnalités livrées :**

- Inscription (`register`), connexion (`login`), refresh token, logout, profil utilisateur (`me`).
- JWT access + refresh tokens. Refresh tokens stockés et révocables.
- Rôle `Owner` seedé avec 16 permissions via claims `permission`.
- Multi-tenancy explicite par `TenantId` sur toutes les entités métier.
- Audit log automatique sur les opérations de création / mise à jour / suppression.

**Règles métier :**

- Chaque requête authentifiée porte le `tenant_id` du JWT.
- Les endpoints métier filtrent systématiquement les données par `TenantId`.
- Un utilisateur ne peut accéder qu'aux données de son propre tenant.

### 2.2 Clients (Customers)

**Objectif** : référencer et suivre les débiteurs.

**Fonctionnalités livrées :**

- CRUD complet avec isolation tenant.
- Recherche, tri et pagination côté backend.
- Fiche 360 (`/api/customers/{id}/summary`) : coordonnées, contacts, factures, paiements, DSO approximatif.
- Contacts associés (gérant, comptabilité, etc.) avec indicateurs `IsPrimary` et `IsBillingContact`.

**Règles métier :**

- Le `Code` client est unique au sein d'un tenant.
- Les totaux facturés / payés / dus / en retard sont calculés à la volée depuis les factures liées.
- Le DSO approximatif du Customer 360 est calculé par la formule : `RemainingAmount / (TotalInvoiced / 365)`.

### 2.3 Factures (Invoices)

**Objectif** : représenter les créances et leur état de paiement.

**Fonctionnalités livrées :**

- CRUD complet avec DTOs dédiés (`InvoiceDto`, `InvoiceDetailDto`).
- Lignes de facture optionnelles (l'amount total est calculé comme somme des lignes si présentes, sinon pris de la commande).
- Filtres par client, statut, retard, période, recherche textuelle.
- Statut automatique recalculé après chaque paiement.

**Règles métier :**

- `RemainingAmount = Amount - PaidAmount` (calculé en lecture).
- `DaysOverdue = max(0, Today - DueDate)`.
- `IsOverdue = DaysOverdue > 0` et facture non soldée / non annulée / non irrécouvrable.
- Les statuts **Cancelled** et **WrittenOff** sont terminaux : le calculateur ne les modifie plus.
- Ordre de calcul du statut (`InvoiceStatusCalculator.Recalculate`) :
  1. Si `PaidAmount >= Amount` → **Paid**
  2. Sinon si `PaidAmount > 0` → **PartiallyPaid**
  3. Sinon si `DueDate < Today` → **Overdue**
  4. Sinon si `IsDisputed == true` → **Disputed**
  5. Sinon → **Open**
- Une facture créée avec un montant nul ou en statut brouillon est **Draft**.
- Le numéro de facture est unique au sein d'un tenant.

### 2.4 Paiements (Payments) et chèques

**Objectif** : enregistrer les encaissements et suivre les chèques jusqu'à compensation.

**Fonctionnalités livrées :**

- Enregistrement d'un paiement lié à un client et optionnellement à une facture.
- Méthodes supportées : virement bancaire, espèces, chèque, carte, autre.
- Si la méthode est **Cheque**, création automatique d'une entité `Cheque` avec numéro, banque, tiré et statut.
- Recalcul automatique du statut de la facture après chaque paiement.

**Règles métier :**

- Un paiement lié à une facture augmente `PaidAmount` de cette facture.
- Le statut de la facture est recalculé immédiatement via `InvoiceStatusCalculator.Recalculate`.
- Un chèque est créé avec le statut **Received** par défaut.
- Le paiement porte la référence du chèque (`ChequeNumber`) et la banque du chèque si elles ne sont pas fournies explicitement.
- Statuts du chèque : `Received`, `PendingDeposit`, `Deposited`, `Cleared`, `Rejected`, `Returned`, `Cancelled`.
- Si un chèque est rejeté ou retourné, la facture concernée redevient due du montant correspondant (le `PaidAmount` n'est pas automatiquement réduit dans la V1 ; l'ajustement se fait manuellement ou via une note de crédit future).

### 2.5 Créances et aging (Receivables)

**Objectif** : quantifier et segmenter les créances selon leur ancienneté.

**Fonctionnalités livrées :**

- Résumé des créances : total dû, total en retard, total à échéance, taux de retard, nombre de factures ouvertes, nombre de clients en retard.
- Aging en 7 buckets.
- Liste des factures en retard filtrable par tranche de jours et par client.

**Règles métier :**

- Seules les factures dont le statut n'est pas **Paid**, **Cancelled** ou **WrittenOff** entrent dans le calcul des créances.
- Les buckets aging sont calculés sur `DaysOverdue` et `RemainingAmount` :

| Bucket | Jours de retard | Label technique |
|--------|-----------------|-----------------|
| À échéance | ≤ 0 | Current |
| 1-30 jours | 1 – 30 | 1-30 days |
| 31-60 jours | 31 – 60 | 31-60 days |
| 61-90 jours | 61 – 90 | 61-90 days |
| 91-120 jours | 91 – 120 | 91-120 days |
| 121-180 jours | 121 – 180 | 121-180 days |
| > 180 jours | ≥ 181 | >180 days |

- Le total "à échéance" (Current) = créances totales - créances en retard.
- `OverduePercentage = TotalOverdue / TotalOutstanding * 100`.

### 2.6 Dashboard CFO

**Objectif** : donner au CFO une vue consolidée en un coup d'œil.

**Fonctionnalités livrées :**

- KPI cards : créances totales, en retard, à échéance, taux de retard, taux de recouvrement, DSO.
- Comparatif mois en cours / mois précédent pour les encaissements.
- Aging chart interactif.
- Top 10 débiteurs.
- Cash 12 mois (facturé vs encaissé).
- Promesses de paiement à venir dans les 7 jours.
- Activité récente (actions, promesses, paiements, litiges).

**Règles métier :**

- Le DSO du dashboard est calculé sur une période glissante de 90 jours : `DSO = TotalOutstanding / (TotalInvoiced90j / 90)`.
- Le taux de recouvrement du mois = `TotalPaidThisMonth / TotalInvoicedThisMonth * 100`.
- Les top débiteurs sont classés par `TotalDue` décroissant.

### 2.7 Analytics

**Objectif** : fournir des indicateurs avancés pour piloter le recouvrement.

**Fonctionnalités livrées :**

- **DSO** : paramétrable par période (défaut 90 jours).
- **Cash forecast** : prévision hebdomadaire sur 6 périodes (contractual vs expected).
- **Collection rate** : taux de recouvrement sur une période glissante (défaut 30 jours).
- **Collections trend** : tendance facturé / encaissé sur 12 mois.
- **Receivables analytics** : créances, retard moyen, top débiteurs, tendances mensuelles.

**Règles métier :**

- `DSO = TotalOutstanding / AverageDailySales` où `AverageDailySales = TotalInvoicedInPeriod / PeriodDays`.
- `CollectionRate = CollectedAmount / InvoicedAmount * 100`.
- Le cash forecast **contractual** repose sur les échéances de factures restant dues.
- Le cash forecast **expected** applique le taux historique de recouvrement sur 90 jours, tout en garantissant au minimum le montant des promesses de paiement enregistrées.

### 2.8 Import CSV wizard

**Objectif** : charger rapidement un portefeuille clients + factures existant.

**Fonctionnalités livrées :**

- Upload d'un fichier CSV encodé en UTF-8, envoyé en base64.
- Détection automatique des colonnes et mapping par défaut.
- Aperçu avec validation ligne par ligne.
- Confirmation de l'import.
- Gestion des erreurs par numéro de ligne.

**Règles métier :**

- Colonnes reconnues : `CustomerCode`, `CustomerName`, `CustomerEmail`, `CustomerPhone`, `CustomerAddress`, `CustomerCity`, `CustomerCountry`, `PaymentTermsDays`, `InvoiceNumber`, `InvoiceDate`, `DueDate`, `Amount`, `Currency`, `Description`.
- Les clients sont identifiés et mis à jour par `CustomerCode` (idempotence).
- Les factures sont identifiées par `InvoiceNumber` ; un numéro déjà existant déclenche une erreur de ligne.
- `CustomerCode` et `CustomerName` sont obligatoires.
- Si une facture est fournie, `InvoiceNumber`, `InvoiceDate`, `DueDate` et `Amount` (> 0) sont requis.
- La devise par défaut est `DZD`.
- Le pays par défaut est `Algeria`.
- Les sessions d'import sont conservées 2 heures en mémoire.

### 2.9 Audit et gestion des exceptions

**Fonctionnalités livrées :**

- Audit log sur les opérations `Create`, `Update`, `Delete` et `Import`.
- Middleware d'exception centralisé :
  - `ValidationException` → 400 Bad Request
  - `KeyNotFoundException` → 404 Not Found
  - `InvalidOperationException` → 409 Conflict

### 2.10 Hors périmètre V1

- Workflows de recouvrement automatisés (Phase 3).
- Promesses de paiement et litiges actifs en UI/API (entités Domain prêtes, non branchées).
- Notifications temps réel (Phase 4).
- Intégrations ERP, WhatsApp, banque (Phase 5).
- Multi-devise (Phase 5).
- i18n arabe / anglais (Phase 5).
- Mobile app (V2).

---

## 3. Phase 3 et Phase 4 — Ce qui est prévu

> Ces phases sont en conception. Les entités de domaine existent déjà mais ne sont pas encore exposées via API/UI.

### Phase 3 — Collections (recouvrement)

- **Tâches de recouvrement** : assignation à un agent, priorité, échéance, statut.
- **Actions de recouvrement** : appel téléphonique, email, SMS, WhatsApp, visite, rappel.
- **Promesses de paiement** : statuts `Pending`, `Fulfilled`, `PartiallyFulfilled`, `Broken`.
- **Litiges** : workflow `Open → Investigating → Waiting → Resolved → Closed`.
- **Templates de relance** : friendly, due, overdue, final notice.
- **Agent dashboard "Today"** : vue quotidienne des tâches et appels.

### Phase 4 — Intelligence & Analytics avancée

- Dashboard CFO enrichi avec filtres temporels.
- Risk score client (0-100) avec facteurs explicites.
- Centre de notifications.
- Scénarios de cash forecast plus fins.
- Pages analytics dédiées en UI.

---

## 4. Données de démo

### 4.1 Contenu du seed

Lors du premier démarrage, un tenant de démo est créé automatiquement si la base est vide.

**Tenant :** Atlas Distribution SPA
- **Devise :** DZD
- **Langue :** fr
- **Fuseau horaire :** Africa/Algiers
- **Plan :** Pro

**Utilisateur admin :**
- Email : `admin@atlas-distribution.dz`
- Mot de passe : `Admin123!`
- Rôle : `Owner`

### 4.2 Clients (11)

| Code | Nom | Secteur | Ville | Délai | Plafond crédit |
|------|-----|---------|-------|-------|----------------|
| CUST-001 | Pharmacie El Yasmine | Pharmacie | Alger | 30 j | 8 000 000 DZD |
| CUST-002 | AgroDis Sud | Distribution agroalimentaire | Blida | 45 j | 25 000 000 DZD |
| CUST-003 | Gros Œuvre Bâtiments Est | BTP | Constantine | 60 j | 60 000 000 DZD |
| CUST-004 | Benali Frères Électroménager | Électroménager | Sétif | 30 j | 15 000 000 DZD |
| CUST-005 | Nord Lait Distribution | Produits laitiers | Oran | 45 j | 30 000 000 DZD |
| CUST-006 | Pharmacie du Centre | Pharmacie | Annaba | 30 j | 6 000 000 DZD |
| CUST-007 | BTP Horizon Travaux | BTP / Promotion immobilière | Alger | 90 j | 80 000 000 DZD |
| CUST-008 | Electro Atlas Magasin | Électroménager / High-tech | Alger | 60 j | 20 000 000 DZD |
| CUST-009 | Humasud Distribution | Distribution agroalimentaire | Blida | 30 j | 12 000 000 DZD |
| CUST-010 | Grossiste El Anka | Grossiste alimentaire | Oran | 45 j | 40 000 000 DZD |
| CUST-011 | Pharmacie Essalem | Pharmacie | Constantine | 60 j | 5 000 000 DZD |

**Contacts :** 3 contacts associés à Pharmacie El Yasmine (gérant, comptabilité, assistant administratif).

### 4.3 Factures (34)

- Numérotation : `INV-2025-001` à `INV-2025-034`.
- Répartition sur tous les buckets aging.
- 2 factures en statut **Draft**.
- 1 facture en statut **Disputed** (CUST-003, litige sur quantités livrées).

### 4.4 Paiements (15)

- Virements via BNA, BDL, CPA, BEA, AGB, Al Baraka Bank.
- Espèces.
- 6 chèques avec statuts variés : `Received`, `Deposited`, `Cleared`, `Rejected`, `Returned`.

### 4.5 Régénérer les données de démo

1. Arrêtez le backend.
2. Supprimez la base PostgreSQL (`docker-compose down -v` ou `dropdb collecta`).
3. Redémarrez le backend (`dotnet run` dans `src/CollectA.Api`).
4. Le seed s'exécute automatiquement si aucun tenant n'existe.

---

## 5. Glossaire métier

| Terme | Définition |
|-------|------------|
| **Aging** | Répartition des créances selon l'ancienneté du retard de paiement. |
| **Bucket** | Tranche d'ancienneté dans l'aging (ex. 1-30 jours). |
| **Créance** | Somme que vos clients vous doivent (factures non payées). |
| **Collection rate / Taux de recouvrement** | Part des factures émises qui a été effectivement payée sur une période. |
| **DSO** | *Days Sales Outstanding* — nombre de jours de ventes immobilisés dans les créances. |
| **Due date** | Date d'échéance d'une facture. |
| **Invoice** | Facture. |
| **Litige** | Contestation d'une facture par le client (statut `Disputed`). |
| **NIF** | Numéro d'identification fiscale (15 chiffres en Algérie). |
| **Overdue** | En retard de paiement par rapport à la date d'échéance. |
| **Payment** | Paiement reçu d'un client. |
| **Promise to Pay** | Promesse de paiement formalisée par le client. |
| **RC** / **Trade Register** | Registre du commerce. |
| **Remaining amount** | Montant restant à payer sur une facture. |
| **Risk score** | Score de risque client (prévu Phase 4). |
| **Tenant** | Entreprise isolée dans CollectA. |
| **WrittenOff** | Créance considérée comme irrécouvrable. |

---

## 6. Référence API publique

Tous les endpoints métier nécessitent un JWT valide, sauf `auth/register`, `auth/login` et `auth/refresh`.

### Auth

| Méthode | Route | Description |
|---------|-------|-------------|
| POST | `/api/auth/register` | Créer un compte utilisateur |
| POST | `/api/auth/login` | Authentification, retourne access + refresh tokens |
| POST | `/api/auth/refresh` | Rafraîchir l'access token |
| POST | `/api/auth/logout` | Révoquer le refresh token |
| GET | `/api/auth/me` | Profil de l'utilisateur connecté |

### Clients

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/customers` | Liste des clients (triée par nom) |
| GET | `/api/customers/{id}` | Détail d'un client |
| POST | `/api/customers` | Créer un client |
| PUT | `/api/customers/{id}` | Mettre à jour un client |
| DELETE | `/api/customers/{id}` | Supprimer un client |
| GET | `/api/customers/{id}/summary` | Customer 360 (factures, paiements, DSO) |

### Factures

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/invoices` | Liste paginée des factures avec filtres |
| GET | `/api/invoices/{id}` | Détail d'une facture (lignes + paiements) |
| POST | `/api/invoices` | Créer une facture |
| PUT | `/api/invoices/{id}` | Modifier une facture |
| DELETE | `/api/invoices/{id}` | Supprimer une facture |

**Query params `GET /api/invoices` :** `pageNumber`, `pageSize`, `search`, `sortBy`, `sortDescending`, `customerId`, `status`, `isOverdue`, `fromDate`, `toDate`.

### Paiements

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/payments` | Liste paginée des paiements |
| GET | `/api/payments/{id}` | Détail d'un paiement |
| POST | `/api/payments` | Enregistrer un paiement (avec chèque optionnel) |

**Query params `GET /api/payments` :** `pageNumber`, `pageSize`, `search`, `sortBy`, `sortDescending`, `customerId`, `invoiceId`, `fromDate`, `toDate`.

### Créances

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/receivables/summary` | Résumé des créances et KPIs |
| GET | `/api/receivables/aging` | Aging en 7 buckets |
| GET | `/api/receivables/overdue` | Liste des factures en retard |

**Query params `GET /api/receivables/overdue` :** `minDays`, `maxDays`, `customerId`, `take`.

### Dashboard

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/dashboard` | Dashboard CFO consolidé |
| GET | `/api/dashboard/aging` | Aging pour le dashboard |

### Analytics

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/analytics/dso` | DSO (paramètre `periodDays`, défaut 90) |
| GET | `/api/analytics/cash-forecast` | Prévision de trésorerie (paramètre `periods`, défaut 6) |
| GET | `/api/analytics/collection-rate` | Taux de recouvrement (paramètre `periodDays`, défaut 30) |
| GET | `/api/analytics/collections` | Tendances de recouvrement (paramètre `trendMonths`, défaut 12) |
| GET | `/api/analytics/receivables` | Analytics des créances |

### Imports

| Méthode | Route | Description |
|---------|-------|-------------|
| POST | `/api/imports/csv` | Uploader un CSV (base64), retourne l'aperçu |
| GET | `/api/imports/{sessionId}/preview` | Prévisualiser avec mapping personnalisé |
| POST | `/api/imports/{sessionId}/confirm` | Confirmer l'import |

---

*Pour le détail technique de l'architecture, voir `ARCHITECTURE.md`. Pour le suivi des phases et tâches, voir `docs/PLAN.md`.*
