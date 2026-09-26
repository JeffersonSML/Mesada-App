-- Grupos padrão do painel administrativo. "Owner" é o único grupo com
-- sistema=true (acesso total, protegido contra edição/remoção). "Tecnologia"
-- é o exemplo dado pelo dono do sistema para a equipe técnica/suporte —
-- outros grupos podem ser criados livremente pela tela de administração.
BEGIN;

INSERT INTO grupos_administrador (nome, descricao, permissoes, sistema) VALUES
    ('Owner', 'Acesso total ao sistema. Não pode ser editado ou removido.', '{}'::jsonb, true),
    ('Tecnologia', 'Equipe de desenvolvimento e suporte técnico.', '{"gerenciarAdministradores": false}'::jsonb, false)
ON CONFLICT (nome) DO NOTHING;

COMMIT;
