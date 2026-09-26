-- Grupos de acesso do painel administrativo interno (não confundir com
-- papéis de família — Master/Comum). Pedido do dono do sistema: ele tem
-- acesso total (grupo Owner) e pode dar acesso a outras pessoas da equipe
-- dentro de um grupo específico (ex.: "Tecnologia"), sem dar acesso total.
-- `sistema = true` marca o grupo Owner: não pode ser editado nem removido
-- pela API, só existe um e é criado por infra/db/scripts/criar_administrador_owner.sh.
BEGIN;

CREATE TABLE grupos_administrador (
    id           uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    nome         text NOT NULL UNIQUE,
    descricao    text,
    permissoes   jsonb NOT NULL DEFAULT '{}'::jsonb,
    sistema      boolean NOT NULL DEFAULT false,
    created_at   timestamptz NOT NULL DEFAULT now(),
    updated_at   timestamptz NOT NULL DEFAULT now()
);

COMMENT ON COLUMN grupos_administrador.permissoes IS 'Chaves booleanas livres (ex.: {"gerenciarAdministradores": true}) — checadas pela Application. Grupo sistema=true (Owner) sempre tem acesso total, independente do conteúdo aqui.';

CREATE TRIGGER trg_grupos_administrador_updated_at
    BEFORE UPDATE ON grupos_administrador
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

ALTER TABLE administradores
    ADD COLUMN grupo_id uuid REFERENCES grupos_administrador(id),
    ADD COLUMN deve_trocar_senha boolean NOT NULL DEFAULT false;

COMMENT ON COLUMN administradores.deve_trocar_senha IS 'true logo após criação/reset — força a troca de senha no próximo login antes de liberar qualquer outra ação.';

-- "GRANT ALL ON ALL TABLES" de 020_grants.sql só alcançou as tabelas que já
-- existiam naquele momento (mesmo caso de notificacao_destinatarios, ver
-- migration 023) — mesada_app nunca recebe acesso a esta tabela, só mesada_admin.
GRANT ALL PRIVILEGES ON grupos_administrador TO mesada_admin;

COMMIT;
