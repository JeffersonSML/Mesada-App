-- Row Level Security: isolamento por familia_id.
--
-- Contrato com o backend: toda conexão feita como mesada_app DEVE, no início
-- de cada transação, executar
--
--     SELECT set_config('app.current_familia_id', '<uuid-da-familia>', true);
--
-- (o terceiro parâmetro `true` = escopo LOCAL à transação; nunca usar SET
-- global de sessão em um pool de conexões compartilhado). Sem essa variável
-- definida, current_setting(..., true) retorna NULL e nenhuma linha
-- tenant-scoped é visível nem gravável — falha fechada, não aberta.
--
-- A criação de uma nova Familia (fluxo de signup) e todo o módulo
-- Administrador não têm — ou não devem ficar limitados a — um
-- app.current_familia_id: usam a role mesada_admin (BYPASSRLS, ver
-- 002_roles.sql), nunca mesada_app.
BEGIN;

-- ---- familias: aqui a própria chave primária é o identificador do tenant.
ALTER TABLE familias ENABLE ROW LEVEL SECURITY;

CREATE POLICY familias_isolamento ON familias
    USING (id = current_setting('app.current_familia_id', true)::uuid)
    WITH CHECK (id = current_setting('app.current_familia_id', true)::uuid);

-- ---- tabelas tenant-scoped "simples" (familia_id NOT NULL em todas as linhas).
DO $$
DECLARE
    tabela text;
BEGIN
    FOREACH tabela IN ARRAY ARRAY[
        'usuarios_master',
        'usuarios_comuns',
        'tarefas',
        'tarefas_usuarios',
        'ciclos_mesada',
        'execucoes',
        'evidencias',
        'assinaturas',
        'historico_cobranca',
        'convites_acesso',
        'notificacoes_config',
        'suporte_tickets'
    ]
    LOOP
        EXECUTE format('ALTER TABLE %I ENABLE ROW LEVEL SECURITY', tabela);
        EXECUTE format(
            'CREATE POLICY %I ON %I
                USING (familia_id = current_setting(''app.current_familia_id'', true)::uuid)
                WITH CHECK (familia_id = current_setting(''app.current_familia_id'', true)::uuid)',
            tabela || '_isolamento', tabela
        );
    END LOOP;
END
$$;

-- ---- categorias: híbrida (familia_id NULL = padrão do sistema, visível a todas).
-- Leitura enxerga o próprio tenant + os defaults do sistema; escrita da role
-- mesada_app fica restrita ao próprio tenant (defaults do sistema só mudam
-- via mesada_admin, que ignora RLS).
ALTER TABLE categorias ENABLE ROW LEVEL SECURITY;

CREATE POLICY categorias_select ON categorias
    FOR SELECT
    USING (familia_id IS NULL OR familia_id = current_setting('app.current_familia_id', true)::uuid);

CREATE POLICY categorias_insert ON categorias
    FOR INSERT
    WITH CHECK (familia_id = current_setting('app.current_familia_id', true)::uuid);

CREATE POLICY categorias_update ON categorias
    FOR UPDATE
    USING (familia_id = current_setting('app.current_familia_id', true)::uuid)
    WITH CHECK (familia_id = current_setting('app.current_familia_id', true)::uuid);

CREATE POLICY categorias_delete ON categorias
    FOR DELETE
    USING (familia_id = current_setting('app.current_familia_id', true)::uuid);

COMMIT;
