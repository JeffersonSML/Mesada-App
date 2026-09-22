-- UsuarioComum: o filho. Sem e-mail/documento (LGPD art. 14, ver
-- docs/especificacao.md #requisitos-não-funcionais) — acesso via convite,
-- vinculado a um dispositivo.
BEGIN;

CREATE TABLE usuarios_comuns (
    id                        uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    familia_id                uuid NOT NULL REFERENCES familias(id) ON DELETE CASCADE,
    nome                      text NOT NULL,
    apelido                   text,
    ciclo_fechamento          ciclo_periodicidade NOT NULL DEFAULT 'mensal',
    saldo_devedor_acumulado   numeric(10,2) NOT NULL DEFAULT 0,
    dispositivo_vinculado     text,
    status                    text NOT NULL DEFAULT 'ativo' CHECK (status IN ('ativo', 'inativo')),
    created_at                timestamptz NOT NULL DEFAULT now(),
    updated_at                timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX ix_usuarios_comuns_familia_id ON usuarios_comuns (familia_id);

CREATE TRIGGER trg_usuarios_comuns_updated_at
    BEFORE UPDATE ON usuarios_comuns
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

COMMIT;
