-- Privilégios de tabela. RLS (019) decide QUAIS LINHAS; GRANT decide QUAIS
-- TABELAS cada role pode tocar. mesada_app nunca recebe acesso a
-- `administradores`; só enxerga `planos` para leitura (preços/limites são
-- parametrizados apenas pelo Administrador).
BEGIN;

GRANT USAGE ON SCHEMA public TO mesada_app, mesada_admin;

GRANT SELECT ON planos TO mesada_app;

GRANT SELECT, INSERT, UPDATE, DELETE ON
    familias,
    usuarios_master,
    usuarios_comuns,
    categorias,
    tarefas,
    tarefas_usuarios,
    ciclos_mesada,
    execucoes,
    evidencias,
    assinaturas,
    historico_cobranca,
    convites_acesso,
    notificacoes_config,
    suporte_tickets
TO mesada_app;

-- mesada_admin tem BYPASSRLS e acesso irrestrito — é a role do módulo
-- Administrador (gestão de famílias/assinaturas/planos/categorias padrão).
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO mesada_admin;

COMMIT;
