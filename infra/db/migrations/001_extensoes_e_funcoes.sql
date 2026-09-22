-- Extensões e funções utilitárias compartilhadas pelo restante do schema.
BEGIN;

-- citext permite comparação/unicidade de e-mail case-insensitive sem lower() manual.
CREATE EXTENSION IF NOT EXISTS citext;

-- gen_random_uuid() já é nativo a partir do PostgreSQL 13 (sem necessidade de pgcrypto/uuid-ossp).

CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    NEW.updated_at = now();
    RETURN NEW;
END;
$$;

COMMENT ON FUNCTION set_updated_at() IS 'Trigger genérica: mantém a coluna updated_at atualizada em qualquer UPDATE.';

COMMIT;
