-- Administradores do sistema (não pertencem a nenhuma família). Acesso
-- restrito à role mesada_admin — nunca concedido a mesada_app (ver 020_grants.sql).
BEGIN;

CREATE TABLE administradores (
    id           uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    nome         text NOT NULL,
    email        citext NOT NULL UNIQUE,
    senha_hash   text NOT NULL,
    permissoes   jsonb NOT NULL DEFAULT '{}'::jsonb,
    status       text NOT NULL DEFAULT 'ativo' CHECK (status IN ('ativo', 'inativo')),
    created_at   timestamptz NOT NULL DEFAULT now(),
    updated_at   timestamptz NOT NULL DEFAULT now()
);

CREATE TRIGGER trg_administradores_updated_at
    BEFORE UPDATE ON administradores
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

COMMIT;
