-- ConviteAcesso: mesmo mecanismo para vincular um filho (Comum) ou adicionar
-- um novo Master à família (docs/especificacao.md #fluxo-de-convite).
BEGIN;

CREATE TYPE papel_convite   AS ENUM ('master', 'comum');
CREATE TYPE status_convite  AS ENUM ('pendente', 'utilizado', 'expirado', 'revogado');

CREATE TABLE convites_acesso (
    id                       uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    familia_id               uuid NOT NULL REFERENCES familias(id) ON DELETE CASCADE,
    codigo                   text NOT NULL UNIQUE,
    papel_alvo               papel_convite NOT NULL,
    nome_convidado           text,
    status                   status_convite NOT NULL DEFAULT 'pendente',
    criado_por               uuid NOT NULL REFERENCES usuarios_master(id),
    utilizado_em             timestamptz,
    dispositivo_vinculado    text,
    expira_em                timestamptz NOT NULL,
    created_at               timestamptz NOT NULL DEFAULT now(),
    updated_at               timestamptz NOT NULL DEFAULT now(),

    CHECK (status <> 'utilizado' OR utilizado_em IS NOT NULL)
);

CREATE INDEX ix_convites_acesso_familia_id ON convites_acesso (familia_id);
CREATE INDEX ix_convites_acesso_status ON convites_acesso (status) WHERE status = 'pendente';

CREATE TRIGGER trg_convites_acesso_updated_at
    BEFORE UPDATE ON convites_acesso
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

COMMIT;
