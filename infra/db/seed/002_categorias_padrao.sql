-- Categorias padrão do sistema (docs/especificacao.md
-- #categorias-e-subcategorias-de-tarefas). familia_id NULL = visível a
-- todas as famílias; cada família pode adicionar subcategorias/categorias
-- próprias por cima destas.
BEGIN;

INSERT INTO categorias (familia_id, parent_id, nome, sistema, ativo)
VALUES
    (NULL, NULL, 'Casa', true, true),
    (NULL, NULL, 'Escola', true, true),
    (NULL, NULL, 'Esporte', true, true),
    (NULL, NULL, 'Cursos', true, true),
    (NULL, NULL, 'Trabalho', true, true),
    (NULL, NULL, 'Voluntariado', true, true),
    (NULL, NULL, 'Apoio Familiar', true, true)
ON CONFLICT (nome) WHERE familia_id IS NULL AND parent_id IS NULL DO NOTHING;

COMMIT;
