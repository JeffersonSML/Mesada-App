-- UsuarioMaster: pai/mãe/responsável. Uma família pode ter múltiplos Masters;
-- exatamente um deles é o Master Financeiro (is_financeiro) a cada momento —
-- reforçado pelo índice único parcial abaixo, já que a transferência de papel
-- (docs/especificacao.md #troca-de-master-financeiro) precisa ser atômica.
BEGIN;

CREATE TABLE usuarios_master (
    id             uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    familia_id     uuid NOT NULL REFERENCES familias(id) ON DELETE CASCADE,
    nome           text NOT NULL,
    email          citext NOT NULL UNIQUE,
    senha_hash     text NOT NULL,
    is_financeiro  boolean NOT NULL DEFAULT false,
    status         text NOT NULL DEFAULT 'ativo' CHECK (status IN ('ativo', 'inativo')),
    created_at     timestamptz NOT NULL DEFAULT now(),
    updated_at     timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX ix_usuarios_master_familia_id ON usuarios_master (familia_id);

-- No máximo um Master Financeiro ativo por família ao mesmo tempo.
CREATE UNIQUE INDEX ux_usuarios_master_financeiro_por_familia
    ON usuarios_master (familia_id)
    WHERE is_financeiro AND status = 'ativo';

CREATE TRIGGER trg_usuarios_master_updated_at
    BEFORE UPDATE ON usuarios_master
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

COMMIT;
