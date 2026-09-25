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

Dans la V1, les étapes **Invoice**, **Payment**, le suivi du **Due Date**, les **Collection Actions**, les **Tasks**, les **Promises to Pay** et les **Disputes** sont pleinement opérationnels en API et en UI. Seuls les templates d'email de relance et le moteur de workflow automatique (scénarios overdue → action) sont reportés à la Phase 5.

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

### 2.10 Tâches de recouvrement (Collection Tasks)

**Objectif :** planifier et suivre le travail des agents de recouvrement.

**Fonctionnalités livrées :**

- CRUD complet des tâches de recouvrement (`/api/collection-tasks`).
- Assignation à un utilisateur (`AssignedToId`), priorité (Low / Medium / High / Critical) et date d'échéance (`DueDate`).
- Lien optionnel à un client et à une facture.
- Endpoint de clôture `POST /api/collection-tasks/{id}/complete` avec notes de clôture.

**Règles métier :**

- Statuts possibles : `Pending`, `InProgress`, `Completed`, `Cancelled`, `Overdue`.
- Une tâche est créée en statut `Pending`.
- La clôture passe le statut à `Completed` et enregistre la date/heure UTC de clôture.
- Les tâches en retard sont identifiées par `DueDate < Today` et statut non terminal.

### 2.11 Actions de recouvrement (Collection Actions)

**Objectif :** tracer chaque contact ou tentative de contact avec un débiteur.

**Fonctionnalités livrées :**

- CRUD complet des actions (`/api/collection-actions`).
- Types d'action : `PhoneCall`, `Email`, `WhatsApp`, `Sms`, `Meeting`, `Reminder`, `InternalTask`, `Escalation`.
- Outcome : `None`, `NoAnswer`, `PromisedPayment`, `PaymentReceived`, `Dispute`, `WrongContact`, `Refused`, `CustomerUnreachable`, `Escalate`, `Other`.
- Endpoint de clôture `POST /api/collection-actions/{id}/close`.

**Règles métier :**

- L'utilisateur authentifié est enregistré comme `CreatedById`.
- Une action clôturée (`IsClosed = true`) ne peut plus être modifiée via le endpoint de clôture.
- `ActionDate` représente la date réelle de l'action ; `DueDate` permet de planifier une action future.
- Le `CustomerId` est obligatoire ; la facture est optionnelle.

### 2.12 Promesses de paiement (Promise to Pay)

**Objectif :** formaliser et suivre les engagements de règlement des clients.

**Fonctionnalités livrées :**

- CRUD complet des promesses (`/api/promises`).
- Statuts : `Pending`, `Fulfilled`, `PartiallyFulfilled`, `Broken`, `Cancelled`.
- Endpoint d'encaissement partiel ou total `POST /api/promises/{id}/fulfill`.
- Lien optionnel à un client et à une facture.

**Règles métier :**

- Une promesse est créée en statut `Pending`.
- `Fulfill` enregistre le montant effectivement perçu (`FulfilledAmount`) et la date d'encaissement.
- Le statut final est calculé par `PromiseStatusCalculator` :
  - `FulfilledAmount >= PromisedAmount` → `Fulfilled`
  - `FulfilledAmount > 0` → `PartiallyFulfilled`
  - Sinon → `Broken`
- Une promesse `Cancelled` ne peut pas être encaissée.

### 2.13 Litiges (Disputes)

**Objectif :** gérer les contestations clients de la création à la résolution.

**Fonctionnalités livrées :**

- CRUD complet des litiges (`/api/disputes`).
- Workflow de statut : `Open` → `Investigating` → `WaitingCustomer` / `WaitingInternal` → `Resolved` → `Closed`.
- Types de litige : `PricingIssue`, `DeliveryIssue`, `QualityIssue`, `MissingDocument`, `IncorrectInvoice`, `ContractIssue`, `Other`.
- Endpoint de changement de statut `PUT /api/disputes/{id}/status` avec notes de résolution.
- Marquage automatique de la facture liée en statut `Disputed`.

**Règles métier :**

- Les transitions de statut sont contrôlées par `DisputeWorkflow.CanTransitionTo`.
- Passage à `Resolved` ou à un statut inactif (`Closed`) enregistre automatiquement `ResolvedAt`.
- Lors de la résolution, la facture liée perd son flag `IsDisputed` et son statut est recalculé.
- Le montant contesté (`DisputedAmount`) est optionnel ; la devise par défaut est `DZD`.

### 2.14 Agent dashboard

**Objectif :** donner à l'agent sa vue quotidienne opérationnelle.

**Fonctionnalités livrées :**

- Endpoint consolidé `GET /api/agent-dashboard`.
- Compteurs : tâches en attente, actions du jour, promesses à échéance, tâches en retard.
- Listes détaillées : mes tâches, mes actions du jour, mes promesses à échéance.

**Règles métier :**

- Seules les tâches assignées à l'utilisateur connecté comptent dans "mes tâches".
- Les actions du jour sont filtrées sur `ActionDate == Today` (UTC).
- Les promesses à échéance sont celles dont la date d'échéance est dans les 7 prochains jours et qui ne sont pas encore `Fulfilled`/`Cancelled`.

### 2.15 Hors périmètre V1

- Workflows de recouvrement automatisés (règles overdue → action) — Phase 5.
- Templates d'email de relance configurables — Phase 5.
- Notifications temps réel — Phase 4.
- Intégrations ERP, WhatsApp, banque — Phase 5.
- Multi-devise — Phase 5.
- i18n arabe / anglais — Phase 5.
- Mobile app — V2.

---

## 3. Phase 4 — Ce qui est prévu

### Phase 4 — Intelligence & Analytics avancée

- Dashboard CFO enrichi avec filtres temporels.
- Risk score client (0-100) avec facteurs explicites.
- Centre de notifications.
- Scénarios de cash forecast plus fins.
- Pages analytics dédiées en UI.

La Phase 3 (Collections) est déjà livrée et documentée dans les sections 2.10 à 2.14.

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
- 2 factures en statut **Disputed** (CUST-003 litige sur quantités, CUST-005 litige sur prix).

### 4.4 Paiements (15)

- Virements via BNA, BDL, CPA, BEA, AGB, Al Baraka Bank.
- Espèces.
- 6 chèques avec statuts variés : `Received`, `Deposited`, `Cleared`, `Rejected`, `Returned`.

### 4.5 Tâches de recouvrement (8)

- Assignées à l'agent `agent@atlas-distribution.dz`.
- Priorités variées : Low, Medium, High, Critical.
- Dates d'échéance passées et futures.

### 4.6 Actions de recouvrement (10)

- Types : PhoneCall, Email, WhatsApp.
- Outcomes : NoAnswer, PromisedPayment, Dispute, WrongContact, Refused.
- Quelques actions clôturées, d'autres ouvertes.

### 4.7 Promesses de paiement (4)

- Montants en DZD, dates passées et futures.
- Statuts : `Fulfilled`, `PartiallyFulfilled`, `Pending`, `Broken`.

### 4.8 Litiges (2)

- CUST-003 : litige sur quantités livrées.
- CUST-005 : litige sur prix appliqué.

### 4.9 Régénérer les données de démo

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

### Tâches de recouvrement

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/collection-tasks` | Liste paginée des tâches |
| GET | `/api/collection-tasks/{id}` | Détail d'une tâche |
| POST | `/api/collection-tasks` | Créer une tâche |
| PUT | `/api/collection-tasks/{id}` | Modifier une tâche |
| DELETE | `/api/collection-tasks/{id}` | Supprimer une tâche |
| POST | `/api/collection-tasks/{id}/complete` | Marquer une tâche comme terminée |

**Query params `GET /api/collection-tasks` :** `pageNumber`, `pageSize`, `search`, `sortBy`, `sortDescending`, `customerId`, `assignedToId`, `status`, `priority`, `isOverdue`, `fromDate`, `toDate`.

### Actions de recouvrement

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/collection-actions` | Liste paginée des actions |
| GET | `/api/collection-actions/{id}` | Détail d'une action |
| POST | `/api/collection-actions` | Créer une action |
| PUT | `/api/collection-actions/{id}` | Modifier une action (outcome, assignation, clôture) |
| DELETE | `/api/collection-actions/{id}` | Supprimer une action |
| POST | `/api/collection-actions/{id}/close` | Clôturer une action |

**Query params `GET /api/collection-actions` :** `pageNumber`, `pageSize`, `search`, `sortBy`, `sortDescending`, `customerId`, `assignedToId`, `type`, `outcome`, `isClosed`, `fromDate`, `toDate`.

### Promesses de paiement

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/promises` | Liste paginée des promesses |
| GET | `/api/promises/{id}` | Détail d'une promesse |
| POST | `/api/promises` | Créer une promesse |
| PUT | `/api/promises/{id}` | Modifier une promesse |
| DELETE | `/api/promises/{id}` | Supprimer une promesse |
| POST | `/api/promises/{id}/fulfill` | Enregistrer un encaissement sur la promesse |

**Query params `GET /api/promises` :** `pageNumber`, `pageSize`, `search`, `sortBy`, `sortDescending`, `customerId`, `status`, `fromDate`, `toDate`.

### Litiges

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/disputes` | Liste paginée des litiges |
| GET | `/api/disputes/{id}` | Détail d'un litige |
| POST | `/api/disputes` | Créer un litige |
| PUT | `/api/disputes/{id}` | Modifier un litige |
| DELETE | `/api/disputes/{id}` | Supprimer un litige |
| PUT | `/api/disputes/{id}/status` | Changer le statut du litige |

**Query params `GET /api/disputes` :** `pageNumber`, `pageSize`, `search`, `sortBy`, `sortDescending`, `customerId`, `status`, `type`, `fromDate`, `toDate`.

### Agent dashboard

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/api/agent-dashboard` | Vue quotidienne consolidée de l'agent connecté |

---

*Pour le détail technique de l'architecture, voir `ARCHITECTURE.md`. Pour le suivi des phases et tâches, voir `docs/PLAN.md`.*
