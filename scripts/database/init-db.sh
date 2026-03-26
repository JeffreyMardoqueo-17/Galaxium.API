#!/bin/bash
set -e

echo "Esperando a que PostgreSQL esté disponible..."
until pg_isready -h "$POSTGRES_HOST" -U "$POSTGRES_USER" -d "$POSTGRES_DB"; do
  echo "PostgreSQL no disponible, esperando..."
  sleep 2
done

echo "PostgreSQL está listo. Ejecutando schema.sql..."
psql -h "$POSTGRES_HOST" -U "$POSTGRES_USER" -d "$POSTGRES_DB" -f /docker-entrypoint-initdb.d/schema.sql

  # Refuerza el seed del método de pago por si el schema.sql no lo insertó correctamente
  echo "Verificando/inserción forzada del método de pago Efectivo..."
  psql -h "$POSTGRES_HOST" -U "$POSTGRES_USER" -d "$POSTGRES_DB" -c "INSERT INTO \"PaymentMethod\" (\"Id\", \"Name\", \"Description\", \"IsActive\", \"CreatedAt\") VALUES (1, 'Efectivo', 'Pago en efectivo', TRUE, NOW()) ON CONFLICT (\"Id\") DO NOTHING;"
echo "Schema y seed data aplicados correctamente"
