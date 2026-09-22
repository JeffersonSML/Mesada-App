-- Tarefa: cadastro exclusivo da Web (docs/especificacao.md
-- #cadastro-e-parametrização-de-tarefas e #lógica-de-cálculo).
BEGIN;

CREATE TYPE modo_calculo        AS ENUM ('valor_direto', 'pontos');
CREATE TYPE tipo_tarefa         AS ENUM ('recorrente', 'avulsa');
CREATE TYPE natureza_tarefa     AS ENUM ('bonus', 'obrigatoria');
CREATE TYPE tipo_evidencia      AS ENUM ('nenhuma', 'foto', 'integracao_automatica');
CREATE TYPE provedor_integracao AS ENUM ('strava', 'google_fit', 'apple_health');

CREATE TABLE tarefas (
    id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    familia_id            uuid NOT NULL REFERENCES familias(id) ON DELETE CASCADE,
    categoria_id          uuid NOT NULL REFERENCES categorias(id),
    nome                  text NOT NULL,
    descricao             text,
    modo_calculo          modo_calculo NOT NULL,
    valor                 numeric(10,2) CHECK (valor IS NULL OR valor >= 0),
    pontos                numeric(10,2) CHECK (pontos IS NULL OR pontos >= 0),
    permite_parcial       boolean NOT NULL DEFAULT false,
    tipo                  tipo_tarefa NOT NULL DEFAULT 'recorrente',
    validade_avulsa       timestamptz,
    natureza              natureza_tarefa NOT NULL DEFAULT 'bonus',
    valor_multa           numeric(10,2) CHECK (valor_multa IS NULL OR valor_multa >= 0),
    requer_aprovacao      boolean NOT NULL DEFAULT true,
    tipo_evidencia        tipo_evidencia NOT NULL DEFAULT 'foto',
    provedor_integracao   provedor_integracao,
    ativo                 boolean NOT NULL DEFAULT true,
    created_at            timestamptz NOT NULL DEFAULT now(),
    updated_at            timestamptz NOT NULL DEFAULT now(),

    CHECK (modo_calculo <> 'valor_direto' OR valor IS NOT NULL),
    CHECK (modo_calculo <> 'pontos' OR pontos IS NOT NULL),
    CHECK (tipo <> 'avulsa' OR validade_avulsa IS NOT NULL),
    CHECK (natureza = 'obrigatoria' OR valor_multa IS NULL),
    CHECK (tipo_evidencia = 'integracao_automatica' OR provedor_integracao IS NULL)
);

COMMENT ON COLUMN tarefas.valor IS 'Valor cadastrado em R$, usado quando modo_calculo = valor_direto.';
COMMENT ON COLUMN tarefas.pontos IS 'Pontuação cadastrada, usado quando modo_calculo = pontos (convertido via valor do ponto da Familia/UsuarioComum).';
COMMENT ON COLUMN tarefas.valor_multa IS 'Multa aplicada apenas quando a tarefa é Obrigatória e não é cumprida; independente do valor que a tarefa geraria se concluída.';

CREATE INDEX ix_tarefas_familia_id ON tarefas (familia_id);
CREATE INDEX ix_tarefas_categoria_id ON tarefas (categoria_id);

CREATE TRIGGER trg_tarefas_updated_at
    BEFORE UPDATE ON tarefas
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

COMMIT;
