-- Recuperação de senha do Master (pai/mãe/responsável) — lacuna encontrada
-- numa auditoria funcional: diferente do Administrador (que já tinha
-- trocar-senha/trocar-email), o Master não tinha nenhuma forma de trocar
-- ou recuperar a própria senha. Fluxo sempre pré-tenant (o usuário ainda
-- não fez login), só mesada_admin acessa esta tabela — mesmo raciocínio de
-- usuarios_master no login (ver AdminUsuarioMasterRepository).
BEGIN;

CREATE TABLE redefinicoes_senha_master (
    id                 uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    usuario_master_id  uuid NOT NULL REFERENCES usuarios_master(id) ON DELETE CASCADE,
    token_hash         text NOT NULL UNIQUE,
    expira_em          timestamptz NOT NULL,
    utilizado_em       timestamptz,
    created_at         timestamptz NOT NULL DEFAULT now()
);

COMMENT ON COLUMN redefinicoes_senha_master.token_hash IS 'SHA-256 do token enviado por e-mail — o token em si (alta entropia, gerado aleatoriamente) nunca é persistido; um vazamento do banco não deve permitir redefinir a senha de ninguém.';

CREATE INDEX ix_redefinicoes_senha_master_usuario_master_id ON redefinicoes_senha_master (usuario_master_id);

-- Só mesada_admin: nunca há contexto de família neste fluxo (o usuário
-- ainda não está autenticado quando pede/usa o link de redefinição).
GRANT ALL PRIVILEGES ON redefinicoes_senha_master TO mesada_admin;

COMMIT;
