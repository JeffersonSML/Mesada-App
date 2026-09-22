-- NotificacaoConfig: por família, quais eventos geram notificação em cada
-- canal (docs/especificacao.md #notificações). Ausência de linha para um
-- (evento, canal) equivale a "ativo" por padrão de fábrica — a aplicação só
-- precisa gravar aqui as exceções que o Master desativar.
BEGIN;

CREATE TYPE evento_notificacao AS ENUM (
    'tarefa_pendente',
    'aprovacao_necessaria',
    'tarefa_aprovada',
    'tarefa_rejeitada',
    'mesada_fechada',
    'tarefa_avulsa_expirando'
);
CREATE TYPE canal_notificacao AS ENUM ('push', 'email');

CREATE TABLE notificacoes_config (
    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    familia_id    uuid NOT NULL REFERENCES familias(id) ON DELETE CASCADE,
    evento        evento_notificacao NOT NULL,
    canal         canal_notificacao NOT NULL,
    ativo         boolean NOT NULL DEFAULT true,
    created_at    timestamptz NOT NULL DEFAULT now(),
    updated_at    timestamptz NOT NULL DEFAULT now(),
    UNIQUE (familia_id, evento, canal)
);

CREATE INDEX ix_notificacoes_config_familia_id ON notificacoes_config (familia_id);

CREATE TRIGGER trg_notificacoes_config_updated_at
    BEFORE UPDATE ON notificacoes_config
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

COMMIT;
