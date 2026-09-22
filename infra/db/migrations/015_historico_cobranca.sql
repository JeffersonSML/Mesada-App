-- Histórico de cobrança de uma Assinatura — base do "histórico de billing"
-- exibido em Detalhe da Família (Admin) e em Assinatura (Master Financeiro).
BEGIN;

CREATE TYPE status_cobranca AS ENUM ('pendente', 'pago', 'falhou', 'estornado');

CREATE TABLE historico_cobranca (
    id                   uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    familia_id           uuid NOT NULL REFERENCES familias(id) ON DELETE CASCADE,
    assinatura_id        uuid NOT NULL REFERENCES assinaturas(id) ON DELETE CASCADE,
    valor                numeric(10,2) NOT NULL CHECK (valor >= 0),
    status               status_cobranca NOT NULL DEFAULT 'pendente',
    provider_charge_id   text,
    data_cobranca        timestamptz NOT NULL DEFAULT now(),
    created_at           timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX ix_historico_cobranca_familia_id ON historico_cobranca (familia_id);
CREATE INDEX ix_historico_cobranca_assinatura_id ON historico_cobranca (assinatura_id);

COMMIT;
