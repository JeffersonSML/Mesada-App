-- CicloMesada: fechamento por UsuarioComum (docs/especificacao.md
-- #ciclo-de-mesada). Mesada_final = Mesada_base + Σbônus − Σmultas; se
-- negativa, zera e o débito acumula para o próximo ciclo (saldo_devedor_resultante
-- alimenta usuarios_comuns.saldo_devedor_acumulado no fechamento seguinte).
BEGIN;

CREATE TYPE status_ciclo AS ENUM ('aberto', 'fechado');

CREATE TABLE ciclos_mesada (
    id                          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    familia_id                  uuid NOT NULL REFERENCES familias(id) ON DELETE CASCADE,
    usuario_comum_id            uuid NOT NULL REFERENCES usuarios_comuns(id) ON DELETE CASCADE,
    data_inicio                 date NOT NULL,
    data_fim                    date NOT NULL,
    mesada_base                 numeric(10,2) NOT NULL,
    soma_bonus                  numeric(10,2) NOT NULL DEFAULT 0,
    soma_multas                 numeric(10,2) NOT NULL DEFAULT 0,
    saldo_devedor_anterior      numeric(10,2) NOT NULL DEFAULT 0,
    valor_final                 numeric(10,2),
    saldo_devedor_resultante    numeric(10,2) NOT NULL DEFAULT 0,
    status                      status_ciclo NOT NULL DEFAULT 'aberto',
    fechado_em                  timestamptz,
    created_at                  timestamptz NOT NULL DEFAULT now(),
    updated_at                  timestamptz NOT NULL DEFAULT now(),
    CHECK (data_fim >= data_inicio),
    UNIQUE (usuario_comum_id, data_inicio)
);

CREATE INDEX ix_ciclos_mesada_familia_id ON ciclos_mesada (familia_id);
CREATE INDEX ix_ciclos_mesada_usuario_comum_id ON ciclos_mesada (usuario_comum_id);

CREATE TRIGGER trg_ciclos_mesada_updated_at
    BEFORE UPDATE ON ciclos_mesada
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

COMMIT;
