#!/usr/bin/env bash
# Cria (ou reseta a senha e o grupo de) o Administrador Owner — a única
# conta com acesso total ao painel administrativo interno. Idempotente:
# rodar de novo com o mesmo e-mail atualiza a senha/grupo em vez de duplicar.
#
# NUNCA commitar a senha em texto claro em lugar nenhum — ela só existe
# nesta chamada (variável de ambiente) e no hash bcrypt gravado no banco.
#
# Uso:
#   export ADMIN_OWNER_NOME="Jefferson"
#   export ADMIN_OWNER_EMAIL="jeffersonlucio@yahoo.com.br"
#   export ADMIN_OWNER_SENHA="<senha forte>"
#   ./infra/db/scripts/criar_administrador_owner.sh
set -euo pipefail

: "${PGHOST:=localhost}"
: "${PGPORT:=5432}"
: "${PGDATABASE:=mesada}"
: "${PGSUPERUSER:=postgres}"
: "${PGSUPERUSER_PASSWORD:?defina PGSUPERUSER_PASSWORD}"
: "${ADMIN_OWNER_NOME:?defina ADMIN_OWNER_NOME}"
: "${ADMIN_OWNER_EMAIL:?defina ADMIN_OWNER_EMAIL}"
: "${ADMIN_OWNER_SENHA:?defina ADMIN_OWNER_SENHA}"

export PGPASSWORD="$PGSUPERUSER_PASSWORD"
CONN=(psql -v ON_ERROR_STOP=1 -q -h "$PGHOST" -p "$PGPORT" -U "$PGSUPERUSER" -d "$PGDATABASE")

echo "==> Garantindo extensão pgcrypto (só usada aqui, para gerar o hash bcrypt da senha inicial)"
"${CONN[@]}" -c "CREATE EXTENSION IF NOT EXISTS pgcrypto;"

echo "==> Criando/atualizando o Administrador Owner ($ADMIN_OWNER_EMAIL)"
"${CONN[@]}" -v nome="$ADMIN_OWNER_NOME" -v email="$ADMIN_OWNER_EMAIL" -v senha="$ADMIN_OWNER_SENHA" <<'SQL'
INSERT INTO administradores (nome, email, senha_hash, grupo_id, deve_trocar_senha, status)
SELECT :'nome', :'email', crypt(:'senha', gen_salt('bf', 12)), g.id, true, 'ativo'
FROM grupos_administrador g
WHERE g.nome = 'Owner'
ON CONFLICT (email) DO UPDATE SET
    nome              = EXCLUDED.nome,
    senha_hash        = EXCLUDED.senha_hash,
    grupo_id          = EXCLUDED.grupo_id,
    deve_trocar_senha = true,
    status            = 'ativo';
SQL

echo "==> Concluído. Faça login em POST /api/admin/auth/login com o e-mail e a senha informados — a API vai exigir troca de senha no primeiro acesso (deve_trocar_senha=true)."
