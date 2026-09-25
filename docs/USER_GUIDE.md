# Guide utilisateur CollectA

> CollectA vous aide à transformer vos créances en cash. Ce guide est écrit pour les responsables financiers et les agents de recouvrement d'une PME algérienne.

---

## Introduction

CollectA est une application de gestion des créances clients et de recouvrement. Elle centralise vos factures, vos paiements, vos clients et vos indicateurs de recouvrement en un seul tableau de bord.

Avec CollectA vous pouvez :

- Voir en temps réel combien vos clients vous doivent.
- Suivre les factures en retard et prioriser les relances.
- Enregistrer les paiements, y compris les chèques, et suivre leur statut.
- Consulter la fiche complète d'un client (Customer 360).
- Importer des clients et des factures par fichier CSV.
- Anticiper votre trésorerie grâce aux analytics.

Tous les montants sont affichés en **DZD** (dinar algérien) au format `fr-DZ`.

---

## Connexion

1. Ouvrez votre navigateur à l'adresse `http://localhost:3000`.
2. Cliquez sur **Se connecter**.
3. Saisissez les identifiants de démo :
   - **Email :** `admin@atlas-distribution.dz`
   - **Mot de passe :** `Admin123!`
4. Cliquez sur **Connexion**.

Résultat attendu : vous arrivez sur le tableau de bord principal avec les KPIs de créances.

---

## Navigation

La barre latérale (sidebar) se trouve à gauche de l'écran. Elle contient les menus suivants :

| Menu | Usage |
|------|-------|
| **Tableau de bord** | Vue CFO avec les KPIs et l'aging |
| **Clients** | Liste des clients et fiches 360 |
| **Factures** | Liste et détail des factures |
| **Paiements** | Historique des paiements |
| **Créances** | Suivi des créances, aging, overdue |
| **Recouvrement** | Tâches et actions de recouvrement |
| **Promesses** | Promesses de paiement des clients |
| **Litiges** | Contestations et workflow de résolution |
| **Import CSV** | Assistant d'import clients + factures |
| **Analytics / Paramètres** | Placeholders — fonctionnalités à venir |

Cliquez sur un menu pour ouvrir la page correspondante. Le menu actif est mis en évidence.

---

## Tutoriel 1 — Consulter le tableau de bord et comprendre les KPIs

1. Connectez-vous. La page `/dashboard` s'affiche par défaut.
2. En haut de page, lisez les cartes de KPIs :
   - **Créances totales** : montant total restant à recevoir (hors Paid / Cancelled / WrittenOff).
   - **En retard** : montant des factures dépassant leur échéance.
   - **À échéance** : montant des factures dont la date d'échéance n'est pas encore passée.
   - **Taux de retard** : pourcentage des créances totales qui sont en retard.
   - **DSO** : nombre moyen de jours de ventes représenté par les créances en cours.
   - **Taux de recouvrement** : part des factures du mois en cours déjà payée.
3. Juste en dessous, consultez le graphique **Aging** : il répartit les créances restantes en 7 buckets (Current, 1-30, 31-60, 61-90, 91-120, 121-180, >180 jours).
4. Scrollez vers le bas pour voir :
   - **Top débiteurs** : clients avec le plus de dettes.
   - **Cash 12 mois** : comparaison facturé / encaissé mois par mois.
   - **Activité récente** : derniers paiements, actions et promesses.

Résultat attendu : vous avez une vue d'ensemble de la santé financière de votre portefeuille clients.

---

## Tutoriel 2 — Gérer les clients

### Créer un client

1. Cliquez sur **Clients** dans la sidebar.
2. Cliquez sur **Nouveau client**.
3. Remplissez au minimum :
   - **Code client** (ex. `CUST-012`) — unique au sein de votre tenant.
   - **Nom** (raison sociale affichée).
   - **Email** et **Téléphone**.
   - **Ville** et **Adresse**.
   - **Délai de paiement** en jours (ex. `30`).
   - **Plafond de crédit** en DZD.
4. Cliquez sur **Enregistrer**.

Résultat attendu : le client apparaît dans la liste. Vous pouvez le rechercher par nom ou par code.

### Ouvrir la fiche 360 d'un client

1. Dans la liste des clients, cliquez sur le nom d'un client.
2. La page `/customers/[id]` s'ouvre avec trois zones :
   - **Vue d'ensemble** : coordonnées, contacts, soldes (total facturé, payé, dû, en retard), DSO approximatif.
   - **Onglet Factures** : toutes les factures du client avec statut et montant restant.
   - **Onglet Paiements** : historique des paiements reçus.
3. Pour ajouter un contact, cliquez sur **Ajouter un contact** dans la section contacts, puis renseignez nom, fonction, email et téléphone.

Résultat attendu : vous disposez d'une vision complète du client et de son historique financier.

---

## Tutoriel 3 — Gérer les factures

### Créer une facture

1. Allez dans **Factures**, puis cliquez sur **Nouvelle facture**.
2. Ou, depuis la fiche 360 d'un client, cliquez sur **Nouvelle facture**.
3. Renseignez :
   - **Numéro de facture** (ex. `FA-2025-0100`) — unique.
   - **Client** (sélectionné automatiquement si vous partez de sa fiche).
   - **Date de facture** et **Date d'échéance**.
   - **Montant TTC** en DZD.
   - **Description**.
4. Cliquez sur **Enregistrer**.

Résultat attendu : la facture est créée avec le statut **Open** (à payer). Si la date d'échéance est déjà passée, elle apparaît en **Overdue**.

### Comprendre les statuts et couleurs

| Statut | Signification | Couleur habituelle |
|--------|---------------|-------------------|
| **Draft** | Brouillon, non encore envoyé | Gris |
| **Open** | À payer, dans les délais | Bleu |
| **PartiallyPaid** | Partiellement payée | Orange |
| **Paid** | Entièrement payée | Vert |
| **Overdue** | Échéance dépassée | Rouge |
| **Disputed** | En litige | Violet |
| **Cancelled** | Annulée | Gris foncé |
| **WrittenOff** | Perdue / irrécouvrable | Noir |

Le statut est calculé automatiquement en fonction du montant payé, de la date d'échéance et du statut litige.

### Filtrer les factures

1. Dans la liste **Factures**, utilisez les filtres en haut de tableau :
   - **Statut** : Open, Overdue, Paid, etc.
   - **En retard** : oui / non.
   - **Client** : sélectionnez un client.
   - **Période** : date de facture entre deux dates.
2. Cliquez sur le titre d'une colonne pour trier (croissant / décroissant).
3. Utilisez la barre de recherche pour chercher par numéro de facture ou nom de client.

Résultat attendu : le tableau affiche uniquement les factures correspondant à vos critères.

---

## Tutoriel 4 — Enregistrer un paiement

### Paiement par virement ou espèces

1. Allez dans **Factures** et cliquez sur une facture en statut **Open** ou **Overdue**.
2. Cliquez sur **Enregistrer un paiement**.
3. Renseignez :
   - **Montant** en DZD.
   - **Date de paiement**.
   - **Méthode** : *Virement bancaire* ou *Espèces*.
   - **Référence** (numéro de virement, bordereau d'encaissement, etc.).
   - **Banque** (optionnel pour un virement).
4. Cliquez sur **Enregistrer**.

Résultat attendu : le paiement est créé. Le montant payé de la facture augmente et son statut passe automatiquement à **PartiallyPaid** ou **Paid**.

### Paiement par chèque

1. Depuis la fiche facture, cliquez sur **Enregistrer un paiement**.
2. Choisissez **Méthode : Chèque**.
3. Renseignez :
   - **Montant**.
   - **Date de paiement**.
   - **Numéro de chèque**.
   - **Banque** émettrice.
   - **Tiré** (nom de la personne ou société signataire).
   - **Date d'échéance du chèque** (si différente de la date de paiement).
4. Cliquez sur **Enregistrer**.

Résultat attendu : le chèque est créé avec le statut **Received** (reçu). Vous pouvez ensuite suivre son évolution dans la liste des paiements.

### Suivi d'un chèque rejeté ou retourné

1. Allez dans **Paiements** et repérez le paiement par chèque.
2. Le statut du chèque s'affiche dans la colonne **Statut chèque** :
   - **Received** : chèque reçu, pas encore déposé.
   - **PendingDeposit** : en attente de dépôt.
   - **Deposited** : déposé en banque, en cours de compensation.
   - **Cleared** : chèque compensé, les fonds sont disponibles.
   - **Rejected** : chèque rejeté (provision insuffisante, signature non conforme, etc.).
   - **Returned** : chèque retourné à l'encaissement.
   - **Cancelled** : chèque annulé.
3. Si un chèque est rejeté ou retourné, la facture associée redevient en retard du montant correspondant. Relancez le client et enregistrez une nouvelle action de recouvrement si nécessaire.

Résultat attendu : vous maîtrisez la trésorerie réelle en distinguant les paiements certains (Cleared) des paiements incertains (Received / Deposited / Rejected / Returned).

---

## Tutoriel 5 — Suivre les créances et l'aging

1. Cliquez sur **Créances** dans la sidebar.
2. En haut, lisez les KPIs (créances totales, en retard, à échéance, taux de retard).
3. Juste en dessous, le graphique **Aging** montre la répartition par bucket :
   - **Current** : non échues.
   - **1-30 jours** : retard de 1 à 30 jours.
   - **31-60 jours**, **61-90 jours**, **91-120 jours**, **121-180 jours**, **>180 jours**.
4. Cliquez sur un bucket du graphique.
5. La liste des factures correspondantes s'affiche avec le client, le montant restant et le nombre de jours de retard.

Résultat attendu : vous identifiez rapidement les créances les plus anciennes et les clients à relancer en priorité.

---

## Tutoriel 6 — Importer clients et factures via CSV

### Préparer le fichier

1. Téléchargez le fichier d'exemple : `frontend/public/exemple-import-clients-factures.csv`.
2. Ouvrez-le dans Excel ou un éditeur de texte.
3. Respectez la première ligne d'en-tête :
   ```
   CustomerCode,CustomerName,CustomerEmail,CustomerPhone,CustomerCity,CustomerCountry,PaymentTermsDays,InvoiceNumber,InvoiceDate,DueDate,Amount,Currency,Description
   ```
4. Remplissez une ligne par client ET par facture. Si un client a plusieurs factures, répétez son `CustomerCode` sur plusieurs lignes.

Colonnes requises et optionnelles :

| Colonne | Requis | Description |
|---------|--------|-------------|
| `CustomerCode` | Oui | Code unique du client (idempotence par ce champ) |
| `CustomerName` | Oui | Raison sociale |
| `CustomerEmail` | Non | Email du client |
| `CustomerPhone` | Non | Téléphone |
| `CustomerCity` | Non | Ville |
| `CustomerCountry` | Non | Pays (défaut : Algeria) |
| `PaymentTermsDays` | Non | Délai de paiement (défaut : 30) |
| `InvoiceNumber` | Non | Numéro de facture (idempotence par ce champ) |
| `InvoiceDate` | Si facture | Date de facture |
| `DueDate` | Si facture | Date d'échéance |
| `Amount` | Si facture | Montant TTC (> 0) |
| `Currency` | Non | Devise (défaut : DZD) |
| `Description` | Non | Libellé de la facture |

### Lancer l'import

1. Allez dans **Import CSV**.
2. Glissez-déposez ou sélectionnez votre fichier CSV.
3. L'assistant affiche l'**étape 1 — Aperçu** : en-têtes détectés, mapping automatique des colonnes, premières lignes.
4. Vérifiez le mapping. Corrigez si une colonne n'a pas été reconnue.
5. Passez à l'**étape 2 — Validation**. Les erreurs par ligne s'affichent (montant invalide, date manquante, doublon de numéro de facture, etc.).
6. Corrigez votre fichier si des erreurs sont présentes, puis rechargez-le.
7. Passez à l'**étape 3 — Confirmation**. Cliquez sur **Importer**.

Résultat attendu : CollectA crée ou met à jour les clients par `CustomerCode` et crée les factures par `InvoiceNumber`. Les lignes en erreur sont listées avec leur numéro et le motif. La session d'import reste disponible 2 heures.

---

## Tutoriel 7 — Gérer le recouvrement

### Créer une tâche de recouvrement

1. Cliquez sur **Recouvrement** dans la sidebar.
2. L'onglet **Tâches** s'affiche par défaut.
3. Cliquez sur **Nouvelle tâche**.
4. Renseignez :
   - **Titre** de la tâche (ex. "Relance téléphonique Pharmacie El Yasmine").
   - **Client** concerné.
   - **Facture** associée (optionnel).
   - **Assigné à** : sélectionnez l'agent chargé du suivi.
   - **Date d'échéance** de la tâche.
   - **Priorité** : Low, Medium, High, Critical.
   - **Description** (optionnel).
5. Cliquez sur **Enregistrer**.

Résultat attendu : la tâche apparaît dans la liste avec le statut **Pending**. L'agent assigné la verra dans son dashboard et dans sa liste "Mes tâches".

### Marquer une tâche comme terminée

1. Dans la liste des tâches, cliquez sur **Terminer** (icône check) sur la ligne concernée.
2. Ajoutez éventuellement des **notes de clôture**.
3. Cliquez sur **Terminer**.

Résultat attendu : le statut passe à **Completed** et la date de clôture est enregistrée.

### Enregistrer une action de recouvrement

1. Dans **Recouvrement**, passez à l'onglet **Actions**.
2. Cliquez sur **Nouvelle action**.
3. Renseignez :
   - **Client** et éventuellement **Facture**.
   - **Type** : PhoneCall, Email, WhatsApp, Sms, Meeting, Reminder, etc.
   - **Date de l'action** (date réelle du contact).
   - **Date d'échéance** (si l'action est planifiée).
   - **Assigné à**.
   - **Priorité**.
   - **Notes** : résumé de l'échange.
4. Cliquez sur **Enregistrer**.

Résultat attendu : l'action est créée. Vous pouvez ensuite la clôturer ou enregistrer son **outcome** (pas de réponse, promesse, paiement reçu, litige, etc.).

### Clôturer une action

1. Dans la liste des actions, cliquez sur **Clôturer**.
2. L'action est marquée comme clôturée (`IsClosed = true`).

Résultat attendu : l'action n'apparaît plus dans les actions actives à traiter.

## Tutoriel 8 — Gérer les promesses de paiement

### Créer une promesse

1. Cliquez sur **Promesses** dans la sidebar.
2. Cliquez sur **Nouvelle promesse**.
3. Renseignez :
   - **Client** et éventuellement **Facture**.
   - **Montant promis** en DZD.
   - **Date promise** (date d'échéance de l'engagement).
   - **Responsable** (agent en charge du suivi).
   - **Notes**.
4. Cliquez sur **Enregistrer**.

Résultat attendu : la promesse est créée avec le statut **Pending**.

### Enregistrer un encaissement sur une promesse

1. Dans la liste des promesses, cliquez sur **Encaisser** sur la ligne concernée.
2. Saisissez le **montant effectivement reçu**.
3. Ajoutez éventuellement une note.
4. Cliquez sur **Confirmer**.

Résultat attendu :
- Si le montant reçu est supérieur ou égal au montant promis → statut **Fulfilled**.
- Si le montant reçu est partiel → statut **PartiallyFulfilled**.
- Si aucun montant n'est reçu → statut **Broken**.

## Tutoriel 9 — Gérer les litiges

### Créer un litige

1. Cliquez sur **Litiges** dans la sidebar.
2. Cliquez sur **Nouveau litige**.
3. Renseignez :
   - **Titre** (ex. "Contestations quantités livrées").
   - **Description** détaillée.
   - **Client** et **Facture** concernés.
   - **Type** : PricingIssue, DeliveryIssue, QualityIssue, MissingDocument, IncorrectInvoice, ContractIssue, Other.
   - **Montant contesté** (optionnel).
   - **Responsable** et **Département**.
   - **Date d'échéance** de résolution souhaitée.
   - **Notes**.
4. Cliquez sur **Enregistrer**.

Résultat attendu : le litige est créé avec le statut **Open**. La facture liée passe automatiquement en statut **Disputed**.

### Faire avancer le workflow d'un litige

1. Dans la liste des litiges, cliquez sur **Changer le statut**.
2. Sélectionnez le nouveau statut :
   - **Investigating** : en cours d'analyse interne.
   - **WaitingCustomer** : en attente d'informations du client.
   - **WaitingInternal** : en attente d'une décision interne.
   - **Resolved** : litige résolu.
   - **Closed** : clôturé sans suite.
3. Ajoutez des **notes de résolution**.
4. Cliquez sur **Confirmer**.

Résultat attendu : le statut est mis à jour. Si le litige passe à **Resolved** ou **Closed**, la date de résolution est enregistrée et la facture liée perd son statut **Disputed**.

## Tutoriel 10 — Comprendre les analytics

1. Allez dans **Analytics** (placeholder visuel actuel ; les données sont déjà accessibles via le dashboard et les endpoints API).
2. Les indicateurs disponibles sont :
   - **DSO** : jours de ventes en créances. Plus il est bas, mieux c'est.
   - **Cash forecast** : prévision de trésorerie sur 6 semaines glissantes. Deux courbes :
     - **Contractual** : ce qui devrait rentrer selon les échéances de factures.
     - **Expected** : ce qui est réaliste selon l'historique de recouvrement et les promesses.
   - **Collection rate** : part des factures d'une période glissante qui a été payée.
   - **Collections trend** : évolution facturé vs encaissé sur 12 mois.
   - **Receivables analytics** : créances totales, retard moyen, top débiteurs.

Résultat attendu : vous anticipez votre trésorerie et mesurez l'efficacité de votre recouvrement.

---

## FAQ

**J'ai oublié mon mot de passe.**
> Contactez l'administrateur de votre tenant. Il peut réinitialiser votre accès. Il n'y a pas de self-service de réinitialisation dans cette version.

**Les montants sont-ils toujours en DZD ?**
> Oui. Le tenant de démo et l'application V1 utilisent le dinar algérien (DZD) pour tous les montants.

**Mes données sont-elles isolées des autres entreprises ?**
> Oui. CollectA est multi-tenant. Chaque entreprise possède son propre `TenantId`. Vous ne voyez que vos clients, vos factures et vos paiements.

**Puis-je annuler un import ?**
> L'import n'est pas annulable une fois confirmé, mais il est idempotent : importer à nouveau le même `CustomerCode` mettra à jour le client existant sans doublon. Les numéros de facture déjà existants sont rejetés.

**Comment régénérer les données de démo ?**
> Supprimez la base PostgreSQL et redémarrez le backend (`dotnet run`). Le seed s'exécute automatiquement si aucun tenant n'existe.

---

*Besoin d'aide supplémentaire ? Ouvrez un ticket ou consultez la documentation fonctionnelle (`docs/FUNCTIONAL.md`) et le plan d'action (`docs/PLAN.md`).*
