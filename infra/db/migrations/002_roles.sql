-- Roles de aplicação. Senhas NÃO são definidas aqui (nunca comitar segredo em
-- migração versionada) — são atribuídas pelo script scripts/provisionar.sh a
-- partir de variáveis de ambiente.
--
-- mesada_app:   usado pelo backend nas operações do dia a dia de uma família.
--               Sujeito a Row Level Security por familia_id em toda tabela
--               tenant-scoped (ver 019_rls_policies.sql).
-- mesada_admin: usado exclusivamente pelo módulo Administrador do backend.
--               BYPASSRLS porque o Administrador precisa enxergar todas as
--               famílias (gestão, suporte, métricas) — nunca deve ser
--               exposto a requisições de uma família comum.
BEGIN;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'mesada_app') THEN
        CREATE ROLE mesada_app LOGIN;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'mesada_admin') THEN
        CREATE ROLE mesada_admin LOGIN BYPASSRLS;
    END IF;
END
$$;

COMMIT;
