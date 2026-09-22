-- SuporteTicket: ocorrências reportadas por Masters, tratadas no módulo
-- Administrador (docs/especificacao.md #módulo-administrador).
BEGIN;

CREATE TYPE status_ticket AS ENUM ('aberto', 'em_andamento', 'resolvido', 'fechado');

CREATE TABLE suporte_tickets (
    id                   uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    familia_id           uuid NOT NULL REFERENCES familias(id) ON DELETE CASCADE,
    usuario_master_id    uuid NOT NULL REFERENCES usuarios_master(id),
    assunto              text NOT NULL,
    descricao            text,
    status               status_ticket NOT NULL DEFAULT 'aberto',
    prioridade           text NOT NULL DEFAULT 'normal' CHECK (prioridade IN ('baixa', 'normal', 'alta', 'urgente')),
    resolvido_em         timestamptz,
    created_at           timestamptz NOT NULL DEFAULT now(),
    updated_at           timestamptz NOT NULL DEFAULT now(),

    CHECK (status NOT IN ('resolvido', 'fechado') OR resolvido_em IS NOT NULL)
);

CREATE INDEX ix_suporte_tickets_familia_id ON suporte_tickets (familia_id);
CREATE INDEX ix_suporte_tickets_status ON suporte_tickets (status) WHERE status IN ('aberto', 'em_andamento');

CREATE TRIGGER trg_suporte_tickets_updated_at
    BEFORE UPDATE ON suporte_tickets
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

COMMIT;
