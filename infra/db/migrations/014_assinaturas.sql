-- Assinatura: cobrança por família, gerida exclusivamente pelo Master
-- Financeiro (docs/especificacao.md #planos-e-assinatura). provider/
-- provider_customer_id/provider_subscription_id são o ponto de integração
-- com a implementação concreta de IPaymentProvider (Asaas ou Stripe) —
-- nunca modelados como colunas específicas de um gateway.
BEGIN;

CREATE TYPE status_assinatura AS ENUM ('trial', 'ativa', 'inadimplente', 'cancelada');

CREATE TABLE assinaturas (
    id                          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    familia_id                  uuid NOT NULL REFERENCES familias(id) ON DELETE CASCADE,
    plano_id                    uuid NOT NULL REFERENCES planos(id),
    status                      status_assinatura NOT NULL DEFAULT 'trial',
    provider                    text CHECK (provider IS NULL OR provider IN ('asaas', 'stripe')),
    provider_customer_id        text,
    provider_subscription_id    text,
    data_inicio                 timestamptz NOT NULL DEFAULT now(),
    data_fim                    timestamptz,
    proxima_cobranca            timestamptz,
    created_at                  timestamptz NOT NULL DEFAULT now(),
    updated_at                  timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX ix_assinaturas_familia_id ON assinaturas (familia_id);
CREATE INDEX ix_assinaturas_plano_id ON assinaturas (plano_id);

-- Uma família tem no máximo uma assinatura não cancelada por vez.
CREATE UNIQUE INDEX ux_assinaturas_ativa_por_familia
    ON assinaturas (familia_id)
    WHERE status <> 'cancelada';

CREATE TRIGGER trg_assinaturas_updated_at
    BEFORE UPDATE ON assinaturas
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

COMMIT;
