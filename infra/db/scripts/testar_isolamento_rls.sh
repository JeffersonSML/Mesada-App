#!/usr/bin/env bash
# Prova, com dados reais, que o isolamento por familia_id funciona:
# cria duas famílias de teste, conecta como mesada_app (não superusuário)
# escopado a uma delas via app.current_familia_id, e confirma que a outra
# família é totalmente invisível — inclusive para categorias do sistema
# (que devem continuar visíveis a ambas). Remove os dados de teste ao final.
set -euo pipefail

: "${PGHOST:=localhost}"
: "${PGPORT:=5432}"
: "${PGDATABASE:=mesada}"
: "${PGSUPERUSER:=postgres}"
: "${PGSUPERUSER_PASSWORD:?defina PGSUPERUSER_PASSWORD}"
: "${MESADA_APP_DB_PASSWORD:?defina MESADA_APP_DB_PASSWORD}"

export PGPASSWORD="$PGSUPERUSER_PASSWORD"
SUPER=(psql -q -v ON_ERROR_STOP=1 -h "$PGHOST" -p "$PGPORT" -U "$PGSUPERUSER" -d "$PGDATABASE")

echo "==> Criando 2 famílias de teste (via superusuário, fora do escopo de RLS)"
FAM_A=$("${SUPER[@]}" -tAc "INSERT INTO familias (nome) VALUES ('[teste-rls] Família A') RETURNING id")
FAM_B=$("${SUPER[@]}" -tAc "INSERT INTO familias (nome) VALUES ('[teste-rls] Família B') RETURNING id")
echo "    Família A: $FAM_A"
echo "    Família B: $FAM_B"

"${SUPER[@]}" -c "INSERT INTO usuarios_comuns (familia_id, nome) VALUES ('$FAM_A', '[teste-rls] Filho A')"
"${SUPER[@]}" -c "INSERT INTO usuarios_comuns (familia_id, nome) VALUES ('$FAM_B', '[teste-rls] Filho B')"

unset PGPASSWORD
export PGPASSWORD="$MESADA_APP_DB_PASSWORD"
# PGOPTIONS injeta o GUC já no startup packet da conexão, então cada
# invocação abaixo já nasce escopada à Família A — sem precisar de um SELECT
# set_config() prévio cuja saída poluiria a captura das consultas seguintes.
export PGOPTIONS="-c app.current_familia_id=$FAM_A"
APP_AS_A=(psql -q -v ON_ERROR_STOP=1 -h "$PGHOST" -p "$PGPORT" -U mesada_app -d "$PGDATABASE")

echo "==> Consultando como mesada_app escopado à Família A"

VISIVEIS_A=$("${APP_AS_A[@]}" -tAc "SELECT count(*) FROM usuarios_comuns WHERE familia_id = '$FAM_A'")
VISIVEIS_B=$("${APP_AS_A[@]}" -tAc "SELECT count(*) FROM usuarios_comuns WHERE familia_id = '$FAM_B'")
TOTAL_VISIVEL=$("${APP_AS_A[@]}" -tAc "SELECT count(*) FROM usuarios_comuns")
CATEGORIAS_SISTEMA=$("${APP_AS_A[@]}" -tAc "SELECT count(*) FROM categorias WHERE familia_id IS NULL")

echo "    Filhos da própria família (A) visíveis: $VISIVEIS_A (esperado: 1)"
echo "    Filhos da outra família (B) visíveis:   $VISIVEIS_B (esperado: 0)"
echo "    Total de linhas em usuarios_comuns via SELECT *: $TOTAL_VISIVEL (esperado: 1)"
echo "    Categorias padrão do sistema visíveis: $CATEGORIAS_SISTEMA (esperado: 7)"

echo "==> Tentando inserir um filho na Família B estando escopado à Família A (deve falhar)"
if "${APP_AS_A[@]}" -c "INSERT INTO usuarios_comuns (familia_id, nome) VALUES ('$FAM_B', '[teste-rls] Invasão')" 2>/tmp/rls_insert_err.log; then
    echo "    FALHA DE SEGURANÇA: o INSERT cross-tenant foi aceito!"
    RESULTADO=1
else
    echo "    OK — bloqueado pela política WITH CHECK: $(tail -n1 /tmp/rls_insert_err.log)"
fi

unset PGPASSWORD PGOPTIONS
export PGPASSWORD="$PGSUPERUSER_PASSWORD"
echo "==> Limpando dados de teste"
"${SUPER[@]}" -c "DELETE FROM familias WHERE id IN ('$FAM_A', '$FAM_B')"
unset PGPASSWORD

if [ "$VISIVEIS_A" = "1" ] && [ "$VISIVEIS_B" = "0" ] && [ "$TOTAL_VISIVEL" = "1" ] && [ "$CATEGORIAS_SISTEMA" = "7" ]; then
    echo "==> RLS validado com sucesso: isolamento por familia_id funcionando."
    exit 0
else
    echo "==> FALHA: isolamento por familia_id não se comportou como esperado."
    exit 1
fi
