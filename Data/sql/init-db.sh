#!/bin/bash
set -e

# Espera a que el contenedor de postgres esté listo
until pg_isready -h "$POSTGRES_HOST" -U "$POSTGRES_USER" -d "$POSTGRES_DB"; do
  echo "Esperando a que PostgreSQL esté disponible..."
  sleep 2
done

# Ejecuta el script de esquema y seed
psql -h "$POSTGRES_HOST" -U "$POSTGRES_USER" -d "$POSTGRES_DB" -f /docker-entrypoint-initdb.d/shema_postgres.sql
