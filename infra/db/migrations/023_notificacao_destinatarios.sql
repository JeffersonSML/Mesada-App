-- NotificacaoDestinatario: e-mails e telefones adicionais que recebem as
-- notificações da família, além do cadastro principal do Master/Comum
-- (docs/especificacao.md#notificações). Resposta à pergunta do usuário:
-- "haverá interface para add/del de e-mail e telefone" — sim, esta tabela
-- é o dado por trás dela; a UI (tela do Master) consome os endpoints
-- GET/POST/DELETE /api/notificacoes/destinatarios.
BEGIN;

CREATE TYPE tipo_destinatario_notificacao AS ENUM ('email', 'telefone');

CREATE TABLE notificacao_destinatarios (
    id           uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    familia_id   uuid NOT NULL REFERENCES familias(id) ON DELETE CASCADE,
    tipo         tipo_destinatario_notificacao NOT NULL,
    valor        text NOT NULL,
    ativo        boolean NOT NULL DEFAULT true,
    created_at   timestamptz NOT NULL DEFAULT now(),
    updated_at   timestamptz NOT NULL DEFAULT now(),
    UNIQUE (familia_id, tipo, valor)
);

COMMENT ON COLUMN notificacao_destinatarios.valor IS 'Endereço de e-mail ou número de telefone (E.164), conforme o tipo.';

CREATE INDEX ix_notificacao_destinatarios_familia_id ON notificacao_destinatarios (familia_id);

CREATE TRIGGER trg_notificacao_destinatarios_updated_at
    BEFORE UPDATE ON notificacao_destinatarios
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

ALTER TABLE notificacao_destinatarios ENABLE ROW LEVEL SECURITY;

CREATE POLICY notificacao_destinatarios_isolamento ON notificacao_destinatarios
    USING (familia_id = current_setting('app.current_familia_id', true)::uuid)
    WITH CHECK (familia_id = current_setting('app.current_familia_id', true)::uuid);

GRANT SELECT, INSERT, UPDATE, DELETE ON notificacao_destinatarios TO mesada_app;

-- "GRANT ALL ON ALL TABLES" de 020_grants.sql só alcançou as tabelas que já
-- existiam naquele momento — uma tabela nova precisa do grant explícito,
-- senão mesada_admin (apesar de BYPASSRLS) esbarra em permission denied.
GRANT ALL PRIVILEGES ON notificacao_destinatarios TO mesada_admin;

COMMIT;
