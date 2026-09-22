-- Planos padrão. Preços são placeholders iniciais — ajustáveis pelo
-- Administrador em produção sem deploy (docs/especificacao.md #planos-e-assinatura).
BEGIN;

INSERT INTO planos (
    codigo, nome, preco_mensal, limite_filhos, limite_masters_adicionais,
    permite_categorias_customizadas, permite_integracoes_externas,
    nivel_relatorios, nivel_suporte
) VALUES
    ('free',    'Free',    0.00,  1,    0,    false, false, 'extrato_basico',          'self_service'),
    ('basico',  'Básico',  19.90, 3,    1,    true,  false, 'completo',                 'padrao'),
    ('premium', 'Premium', 39.90, NULL, NULL, true,  true,  'completo_comparativos',    'prioritario')
ON CONFLICT (codigo) DO NOTHING;

COMMIT;
