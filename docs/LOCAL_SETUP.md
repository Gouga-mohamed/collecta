# CollectA — Configuration locale

Ce document explique comment configurer PostgreSQL localement pour faire tourner CollectA.

## Prérequis

- PostgreSQL 14+ installé et en cours d'exécution sur le port `5432`.
- .NET 8 SDK.
- Node.js 22+.

## 1. Créer l'utilisateur et la base de données

### Option A — Script automatisé

Depuis la racine du projet, exécutez :

```bash
psql -U postgres -f scripts/init-postgres.sql
```

Si PostgreSQL vous demande un mot de passe, entrez celui de l'utilisateur `postgres` défini lors de l'installation.

### Option B — Commandes manuelles

Connectez-vous à PostgreSQL en tant que super-utilisateur (`postgres`) :

```bash
psql -U postgres
```

Puis exécutez :

```sql
CREATE USER collecta WITH PASSWORD 'collecta_dev';
CREATE DATABASE collecta OWNER collecta;
GRANT ALL PRIVILEGES ON DATABASE collecta TO collecta;
```

## 2. Lancer le backend

```bash
cd src/CollectA.Api
dotnet run
```

L'API est disponible sur `http://localhost:5000`.

Swagger : `http://localhost:5000/swagger`

## 3. Lancer le frontend

Dans un autre terminal :

```bash
cd frontend
npm install
npm run dev
```

Le frontend est disponible sur `http://localhost:3000`.

## Utiliser un autre utilisateur PostgreSQL

Si vous ne voulez pas créer l'utilisateur `collecta`, vous pouvez surcharger la connection string via une variable d'environnement.

### Windows (PowerShell)

```powershell
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=collecta;Username=postgres;Password=VOTRE_MOT_DE_PASSE"
cd src/CollectA.Api
dotnet run
```

### Windows (Git Bash) / Linux / macOS

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=collecta;Username=postgres;Password=VOTRE_MOT_DE_PASSE"
cd src/CollectA.Api
dotnet run
```

## Utiliser Docker Compose

Si vous préférez utiliser Docker :

```bash
docker-compose up -d postgres redis
```

Puis lancez le backend et le frontend normalement.

## Compte de démo

Après le premier démarrage, les données de démo sont créées automatiquement :

- **Tenant** : Atlas Distribution
- **Email** : `admin@atlas-distribution.dz`
- **Mot de passe** : `Admin123!`
