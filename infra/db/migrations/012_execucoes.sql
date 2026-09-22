-- Execucao: instância de uma Tarefa (via aderência TarefaUsuario) em um
-- ciclo — status de conclusão, percentual, aprovação e valor/pontos
-- resultantes do motor de cálculo (docs/especificacao.md #lógica-de-cálculo
-- e #fluxo-de-aprovação-de-tarefas). ciclo_mesada_id é preenchido no
-- fechamento do ciclo que consolida essa execução.
BEGIN;

CREATE TYPE status_execucao   AS ENUM ('pendente', 'feito', 'parcial', 'nao_feito');
CREATE TYPE status_aprovacao  AS ENUM ('nao_aplicavel', 'pendente', 'aprovado', 'rejeitado');

CREATE TABLE execucoes (
    id                     uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    familia_id             uuid NOT NULL REFERENCES familias(id) ON DELETE CASCADE,
    tarefa_usuario_id      uuid NOT NULL REFERENCES tarefas_usuarios(id) ON DELETE CASCADE,
    ciclo_mesada_id        uuid REFERENCES ciclos_mesada(id) ON DELETE SET NULL,
    data_execucao          timestamptz NOT NULL DEFAULT now(),
    status                 status_execucao NOT NULL DEFAULT 'pendente',
    percentual_conclusao   numeric(5,2) NOT NULL DEFAULT 100 CHECK (percentual_conclusao BETWEEN 0 AND 100),
    status_aprovacao       status_aprovacao NOT NULL DEFAULT 'nao_aplicavel',
    aprovado_por           uuid REFERENCES usuarios_master(id),
    aprovado_em            timestamptz,
    valor_calculado        numeric(10,2),
    pontos_calculado       numeric(10,2),
    created_at             timestamptz NOT NULL DEFAULT now(),
    updated_at             timestamptz NOT NULL DEFAULT now(),

    CHECK (status <> 'parcial' OR percentual_conclusao < 100),
    CHECK (status_aprovacao NOT IN ('aprovado', 'rejeitado') OR aprovado_em IS NOT NULL)
);

CREATE INDEX ix_execucoes_familia_id ON execucoes (familia_id);
CREATE INDEX ix_execucoes_tarefa_usuario_id ON execucoes (tarefa_usuario_id);
CREATE INDEX ix_execucoes_ciclo_mesada_id ON execucoes (ciclo_mesada_id);
CREATE INDEX ix_execucoes_status_aprovacao ON execucoes (status_aprovacao) WHERE status_aprovacao = 'pendente';

CREATE TRIGGER trg_execucoes_updated_at
    BEFORE UPDATE ON execucoes
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

COMMIT;
