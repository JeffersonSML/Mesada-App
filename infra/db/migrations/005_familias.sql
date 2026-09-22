-- Familia é o tenant raiz do sistema multi-tenant. Todas as demais tabelas
-- tenant-scoped carregam familia_id e são isoladas via RLS (019_rls_policies.sql).
BEGIN;

CREATE TYPE ciclo_periodicidade AS ENUM ('semanal', 'quinzenal', 'mensal', 'personalizado');
CREATE TYPE status_familia       AS ENUM ('ativa', 'suspensa', 'excluida');

CREATE TABLE familias (
    id                        uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    nome                      text NOT NULL,
    ciclo_fechamento_padrao   ciclo_periodicidade NOT NULL DEFAULT 'mensal',
    status                    status_familia NOT NULL DEFAULT 'ativa',
    created_at                timestamptz NOT NULL DEFAULT now(),
    updated_at                timestamptz NOT NULL DEFAULT now()
);

CREATE TRIGGER trg_familias_updated_at
    BEFORE UPDATE ON familias
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

COMMIT;
