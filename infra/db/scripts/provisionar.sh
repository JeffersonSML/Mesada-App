#!/usr/bin/env bash
# Provisiona o banco Mesada: cria o database (se não existir), cria/atualiza
# as roles de aplicação com senha a partir do ambiente, aplica as migrações
# em ordem e roda os seeds. Idempotente — seguro rodar de novo.
#
# Uso:
#   cp infra/db/.env.example infra/db/.env   # e ajuste os valores
#   set -a && source infra/db/.env && set +a
#   ./infra/db/scripts/provisionar.sh
set -euo pipefail

: "${PGHOST:=localhost}"
: "${PGPORT:=5432}"
: "${PGDATABASE:=mesada}"
: "${PGSUPERUSER:=postgres}"
: "${PGSUPERUSER_PASSWORD:?defina PGSUPERUSER_PASSWORD}"
: "${MESADA_APP_DB_PASSWORD:?defina MESADA_APP_DB_PASSWORD}"
: "${MESADA_ADMIN_DB_PASSWORD:?defina MESADA_ADMIN_DB_PASSWORD}"

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

export PGPASSWORD="$PGSUPERUSER_PASSWORD"
SUPERUSER_CONN=(psql -v ON_ERROR_STOP=1 -h "$PGHOST" -p "$PGPORT" -U "$PGSUPERUSER")

echo "==> Garantindo que o database '$PGDATABASE' existe"
DB_EXISTS=$("${SUPERUSER_CONN[@]}" -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname = '$PGDATABASE'")
if [ "$DB_EXISTS" != "1" ]; then
    "${SUPERUSER_CONN[@]}" -d postgres -c "CREATE DATABASE \"$PGDATABASE\""
fi

APP_CONN=("${SUPERUSER_CONN[@]}" -d "$PGDATABASE")

echo "==> Garantindo tabela de controle de migrações"
"${APP_CONN[@]}" -c "CREATE TABLE IF NOT EXISTS _migracoes_aplicadas (arquivo text PRIMARY KEY, aplicada_em timestamptz NOT NULL DEFAULT now())"

echo "==> Aplicando migrações pendentes"
for arquivo in "$DIR"/migrations/*.sql; do
    nome="$(basename "$arquivo")"
    JA_APLICADA=$("${APP_CONN[@]}" -tAc "SELECT 1 FROM _migracoes_aplicadas WHERE arquivo = '$nome'")
    if [ "$JA_APLICADA" = "1" ]; then
        echo "    -> $nome (já aplicada, pulando)"
        continue
    fi
    echo "    -> $nome"
    "${APP_CONN[@]}" -f "$arquivo"
    "${APP_CONN[@]}" -c "INSERT INTO _migracoes_aplicadas (arquivo) VALUES ('$nome')"
done

echo "==> Definindo senha das roles de aplicação"
"${APP_CONN[@]}" -v senha_app="'$MESADA_APP_DB_PASSWORD'" -v senha_admin="'$MESADA_ADMIN_DB_PASSWORD'" <<'SQL'
ALTER ROLE mesada_app WITH PASSWORD :senha_app;
ALTER ROLE mesada_admin WITH PASSWORD :senha_admin;
SQL

echo "==> Aplicando seeds"
for arquivo in "$DIR"/seed/*.sql; do
    echo "    -> $(basename "$arquivo")"
    "${APP_CONN[@]}" -f "$arquivo"
done

unset PGPASSWORD
echo "==> Provisionamento concluído."
