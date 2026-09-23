using Mesada.Application.Exceptions;
using Mesada.Application.Repositories;
using Mesada.Domain.Calculo;
using Mesada.Domain.Enums;

namespace Mesada.Application.Tarefas;

public sealed record SugestaoValorTarefa(decimal Valor, ModoCalculo ModoCalculo);

/// <summary>
/// Sugestão automática de valor/pontos por tarefa (docs/especificacao.md
/// #sugestão-automática-de-valor): divide a mesada base do filho pela
/// quantidade de tarefas às quais ele já é aderente, somada a 1 para
/// representar a tarefa que está prestes a ser vinculada — o Master pode
/// sempre sobrescrever via *_override em TarefaUsuario.
/// </summary>
public sealed class SugerirValorTarefaUseCase(
    IFilhosRepository filhos,
    IAderenciasRepository aderencias)
{
    public async Task<SugestaoValorTarefa> ExecutarAsync(Guid usuarioComumId, ModoCalculo modoCalculo, CancellationToken ct = default)
    {
        var filho = await filhos.ObterPorIdAsync(usuarioComumId, ct)
            ?? throw new RecursoNaoEncontradoException("Filho não encontrado.");

        var quantidadeAderentes = await aderencias.ContarAtivasPorUsuarioComumAsync(usuarioComumId, ct) + 1;

        if (modoCalculo == ModoCalculo.ValorDireto)
        {
            var valor = MotorCalculoMesada.SugerirValorDireto(filho.MesadaBase, quantidadeAderentes);
            return new SugestaoValorTarefa(valor, ModoCalculo.ValorDireto);
        }

        if (filho.ValorPonto is null)
            throw new ValidacaoException("Este filho não tem 'valor do ponto' configurado — configure-o antes de sugerir pontos.");

        var pontos = MotorCalculoMesada.SugerirPontos(filho.MesadaBase, filho.ValorPonto.Value, quantidadeAderentes);
        return new SugestaoValorTarefa(pontos, ModoCalculo.Pontos);
    }
}
