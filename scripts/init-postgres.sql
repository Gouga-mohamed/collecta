-- Script d'initialisation de la base de données CollectA
-- Exécutez avec un super-utilisateur PostgreSQL

-- Création de l'utilisateur s'il n'existe pas
DO
$$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'collecta') THEN
        CREATE ROLE collecta WITH LOGIN PASSWORD 'collecta_dev';
    END IF;
END
$$;

-- Création de la base de données si elle n'existe pas
SELECT 'CREATE DATABASE collecta OWNER collecta'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'collecta')\gexec

-- Attribution des privilèges
GRANT ALL PRIVILEGES ON DATABASE collecta TO collecta;
