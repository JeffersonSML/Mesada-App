-- Lacuna descoberta ao implementar os endpoints de tarefas/ciclo (Etapa
-- "endpoints faltantes"): "A mesada tem um valor base fixo por filho"
-- (docs/especificacao.md#visão-geral) e "taxa configurável, o valor do
-- ponto" (docs/especificacao.md#lógica-de-cálculo) nunca tinham uma coluna
-- de origem — só existiam como snapshot em ciclos_mesada.mesada_base,
-- preenchido NO FECHAMENTO, sem nenhum lugar para o Master configurar o
-- valor ANTES disso. Ambos os campos são só relevantes para o filho no
-- modo Pontos ter valor_ponto preenchido; no modo Valor Direto ele fica
-- nulo (nenhuma tarefa do filho o utiliza).
BEGIN;

ALTER TABLE usuarios_comuns
    ADD COLUMN mesada_base numeric(10,2) NOT NULL DEFAULT 0,
    ADD COLUMN valor_ponto numeric(10,4);

COMMENT ON COLUMN usuarios_comuns.mesada_base IS 'Valor base da mesada deste filho, configurado pelo Master — usado no fechamento de ciclo e na sugestão automática de valor/pontos.';
COMMENT ON COLUMN usuarios_comuns.valor_ponto IS 'Taxa de conversão de pontos em R$ para tarefas deste filho em modo Pontos. NULL se o filho só usa tarefas em modo Valor Direto.';

COMMIT;
