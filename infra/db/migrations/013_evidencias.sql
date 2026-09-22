-- Evidencia: comprovação de uma Execucao — foto (storage compatível com S3)
-- ou dado bruto de integração automática (Strava, Google Fit/Apple HealthKit).
BEGIN;

CREATE TABLE evidencias (
    id                     uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    familia_id             uuid NOT NULL REFERENCES familias(id) ON DELETE CASCADE,
    execucao_id            uuid NOT NULL REFERENCES execucoes(id) ON DELETE CASCADE,
    tipo                   tipo_evidencia NOT NULL,
    storage_key            text,
    integracao_provider    provedor_integracao,
    integracao_dados       jsonb,
    created_at             timestamptz NOT NULL DEFAULT now(),

    CHECK (tipo <> 'foto' OR storage_key IS NOT NULL),
    CHECK (tipo <> 'integracao_automatica' OR integracao_provider IS NOT NULL)
);

CREATE INDEX ix_evidencias_familia_id ON evidencias (familia_id);
CREATE INDEX ix_evidencias_execucao_id ON evidencias (execucao_id);

COMMIT;
