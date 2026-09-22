-- Planos de assinatura (Free/Básico/Premium). Tabela global, sem familia_id:
-- gerenciada exclusivamente pelo Administrador (ver docs/especificacao.md
-- #planos-e-assinatura). Preços e limites são parametrizáveis sem deploy.
BEGIN;

CREATE TYPE nivel_relatorio AS ENUM ('extrato_basico', 'completo', 'completo_comparativos');
CREATE TYPE nivel_suporte   AS ENUM ('self_service', 'padrao', 'prioritario');

CREATE TABLE planos (
    id                               uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    codigo                           text NOT NULL UNIQUE,
    nome                             text NOT NULL,
    preco_mensal                     numeric(10,2) NOT NULL DEFAULT 0 CHECK (preco_mensal >= 0),
    limite_filhos                    integer CHECK (limite_filhos IS NULL OR limite_filhos > 0),
    limite_masters_adicionais        integer CHECK (limite_masters_adicionais IS NULL OR limite_masters_adicionais >= 0),
    permite_categorias_customizadas  boolean NOT NULL DEFAULT false,
    permite_integracoes_externas     boolean NOT NULL DEFAULT false,
    nivel_relatorios                 nivel_relatorio NOT NULL DEFAULT 'extrato_basico',
    nivel_suporte                    nivel_suporte NOT NULL DEFAULT 'self_service',
    ativo                            boolean NOT NULL DEFAULT true,
    created_at                       timestamptz NOT NULL DEFAULT now(),
    updated_at                       timestamptz NOT NULL DEFAULT now()
);

COMMENT ON TABLE planos IS 'Planos de assinatura. limite_filhos/limite_masters_adicionais = NULL significa ilimitado.';

CREATE TRIGGER trg_planos_updated_at
    BEFORE UPDATE ON planos
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

COMMIT;
