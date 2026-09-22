-- Categoria/Subcategoria: modelo híbrido. familia_id NULL = padrão do sistema
-- (visível a todas as famílias, gerenciado só pelo Administrador via
-- mesada_admin); familia_id preenchido = customizada de uma família.
-- parent_id permite subcategorias (auto-relacionamento de um nível).
BEGIN;

CREATE TABLE categorias (
    id           uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    familia_id   uuid REFERENCES familias(id) ON DELETE CASCADE,
    parent_id    uuid REFERENCES categorias(id) ON DELETE CASCADE,
    nome         text NOT NULL,
    sistema      boolean NOT NULL DEFAULT false,
    ativo        boolean NOT NULL DEFAULT true,
    created_at   timestamptz NOT NULL DEFAULT now(),
    updated_at   timestamptz NOT NULL DEFAULT now(),
    CHECK (sistema = (familia_id IS NULL))
);

CREATE INDEX ix_categorias_familia_id ON categorias (familia_id);
CREATE INDEX ix_categorias_parent_id ON categorias (parent_id);

-- Evita duplicar categorias-raiz padrão do sistema (ex.: dois "Casa").
CREATE UNIQUE INDEX ux_categorias_sistema_raiz
    ON categorias (nome)
    WHERE familia_id IS NULL AND parent_id IS NULL;

CREATE TRIGGER trg_categorias_updated_at
    BEFORE UPDATE ON categorias
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

COMMIT;
