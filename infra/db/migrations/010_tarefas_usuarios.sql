-- TarefaUsuario: aderência N:N entre Tarefa e UsuarioComum. Uma tarefa só
-- gera valor para o filho a que está vinculada (docs/especificacao.md
-- #cadastro-e-parametrização-de-tarefas). *_override permite ao Master
-- sobrescrever a sugestão automática de valor/pontos por filho
-- (docs/especificacao.md #sugestão-automática-de-valor).
BEGIN;

CREATE TABLE tarefas_usuarios (
    id                 uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    familia_id         uuid NOT NULL REFERENCES familias(id) ON DELETE CASCADE,
    tarefa_id          uuid NOT NULL REFERENCES tarefas(id) ON DELETE CASCADE,
    usuario_comum_id   uuid NOT NULL REFERENCES usuarios_comuns(id) ON DELETE CASCADE,
    valor_override     numeric(10,2) CHECK (valor_override IS NULL OR valor_override >= 0),
    pontos_override    numeric(10,2) CHECK (pontos_override IS NULL OR pontos_override >= 0),
    ativo              boolean NOT NULL DEFAULT true,
    created_at         timestamptz NOT NULL DEFAULT now(),
    updated_at         timestamptz NOT NULL DEFAULT now(),
    UNIQUE (tarefa_id, usuario_comum_id)
);

CREATE INDEX ix_tarefas_usuarios_familia_id ON tarefas_usuarios (familia_id);
CREATE INDEX ix_tarefas_usuarios_usuario_comum_id ON tarefas_usuarios (usuario_comum_id);

CREATE TRIGGER trg_tarefas_usuarios_updated_at
    BEFORE UPDATE ON tarefas_usuarios
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

COMMIT;
