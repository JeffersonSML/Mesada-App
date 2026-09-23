using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Domain.Enums;

namespace Mesada.Application.Tarefas;

public sealed record DadosTarefa(
    Guid CategoriaId,
    string Nome,
    string? Descricao,
    ModoCalculo ModoCalculo,
    decimal? Valor,
    decimal? Pontos,
    bool PermiteParcial,
    TipoTarefa Tipo,
    DateTimeOffset? ValidadeAvulsa,
    NaturezaTarefa Natureza,
    decimal? ValorMulta,
    bool RequerAprovacao,
    TipoEvidencia TipoEvidencia,
    ProvedorIntegracao? ProvedorIntegracao);

public sealed class ListarTarefasUseCase(ITarefasRepository tarefas)
{
    public Task<IReadOnlyList<Tarefa>> ExecutarAsync(CancellationToken ct = default) =>
        tarefas.ListarAsync(ct);
}

/// <summary>
/// Cadastro e parametrização de tarefas (docs/especificacao.md
/// #cadastro-e-parametrização-de-tarefas) — cadastro exclusivo da Web. As
/// validações espelham 1:1 as CHECK constraints de
/// infra/db/migrations/009_tarefas.sql: falhar aqui, com mensagem clara,
/// em vez de deixar a exceção crua do Postgres subir até a Api.
/// </summary>
public sealed class CriarTarefaUseCase(
    ITarefasRepository tarefas,
    ICategoriasRepository categorias,
    ITenantContextAccessor tenantContext,
    ITenantUnitOfWork unitOfWork)
{
    public async Task<Tarefa> ExecutarAsync(DadosTarefa dados, CancellationToken ct = default)
    {
        Validar(dados);

        if (await categorias.ObterPorIdAsync(dados.CategoriaId, ct) is null)
            throw new RecursoNaoEncontradoException("Categoria não encontrada.");

        var tarefa = new Tarefa
        {
            Id = Guid.NewGuid(),
            FamiliaId = tenantContext.FamiliaId ?? throw new InvalidOperationException("Requisição sem contexto de família."),
            CategoriaId = dados.CategoriaId,
            Nome = dados.Nome,
            Descricao = dados.Descricao,
            ModoCalculo = dados.ModoCalculo,
            Valor = dados.Valor,
            Pontos = dados.Pontos,
            PermiteParcial = dados.PermiteParcial,
            Tipo = dados.Tipo,
            ValidadeAvulsa = dados.ValidadeAvulsa,
            Natureza = dados.Natureza,
            ValorMulta = dados.ValorMulta,
            RequerAprovacao = dados.RequerAprovacao,
            TipoEvidencia = dados.TipoEvidencia,
            ProvedorIntegracao = dados.ProvedorIntegracao,
            Ativo = true,
        };

        tarefas.Adicionar(tarefa);
        await unitOfWork.SaveChangesAsync(ct);
        return tarefa;
    }

    internal static void Validar(DadosTarefa dados)
    {
        if (string.IsNullOrWhiteSpace(dados.Nome))
            throw new ValidacaoException("Nome da tarefa é obrigatório.");
        if (dados.ModoCalculo == ModoCalculo.ValorDireto && dados.Valor is null or < 0)
            throw new ValidacaoException("Tarefa em modo Valor Direto precisa de 'valor' >= 0.");
        if (dados.ModoCalculo == ModoCalculo.Pontos && dados.Pontos is null or < 0)
            throw new ValidacaoException("Tarefa em modo Pontos precisa de 'pontos' >= 0.");
        if (dados.Tipo == TipoTarefa.Avulsa && dados.ValidadeAvulsa is null)
            throw new ValidacaoException("Tarefa Avulsa precisa de 'validadeAvulsa'.");
        if (dados.Natureza != NaturezaTarefa.Obrigatoria && dados.ValorMulta is not null)
            throw new ValidacaoException("'valorMulta' só é aplicável a tarefas Obrigatórias.");
        if (dados.ValorMulta is < 0)
            throw new ValidacaoException("'valorMulta' não pode ser negativo.");
        if (dados.TipoEvidencia != TipoEvidencia.IntegracaoAutomatica && dados.ProvedorIntegracao is not null)
            throw new ValidacaoException("'provedorIntegracao' só é aplicável quando tipoEvidencia = IntegracaoAutomatica.");
        if (dados.TipoEvidencia == TipoEvidencia.IntegracaoAutomatica && dados.ProvedorIntegracao is null)
            throw new ValidacaoException("tipoEvidencia = IntegracaoAutomatica exige 'provedorIntegracao'.");
    }
}

public sealed class AtualizarTarefaUseCase(
    ITarefasRepository tarefas,
    ICategoriasRepository categorias,
    ITenantUnitOfWork unitOfWork)
{
    public async Task<Tarefa> ExecutarAsync(Guid id, DadosTarefa dados, CancellationToken ct = default)
    {
        CriarTarefaUseCase.Validar(dados);

        var tarefa = await tarefas.ObterPorIdAsync(id, ct)
            ?? throw new RecursoNaoEncontradoException("Tarefa não encontrada.");

        if (await categorias.ObterPorIdAsync(dados.CategoriaId, ct) is null)
            throw new RecursoNaoEncontradoException("Categoria não encontrada.");

        tarefa.CategoriaId = dados.CategoriaId;
        tarefa.Nome = dados.Nome;
        tarefa.Descricao = dados.Descricao;
        tarefa.ModoCalculo = dados.ModoCalculo;
        tarefa.Valor = dados.Valor;
        tarefa.Pontos = dados.Pontos;
        tarefa.PermiteParcial = dados.PermiteParcial;
        tarefa.Tipo = dados.Tipo;
        tarefa.ValidadeAvulsa = dados.ValidadeAvulsa;
        tarefa.Natureza = dados.Natureza;
        tarefa.ValorMulta = dados.ValorMulta;
        tarefa.RequerAprovacao = dados.RequerAprovacao;
        tarefa.TipoEvidencia = dados.TipoEvidencia;
        tarefa.ProvedorIntegracao = dados.ProvedorIntegracao;

        await unitOfWork.SaveChangesAsync(ct);
        return tarefa;
    }
}

/// <summary>Exclusão lógica — tarefas têm histórico de execuções vinculado.</summary>
public sealed class RemoverTarefaUseCase(
    ITarefasRepository tarefas,
    ITenantUnitOfWork unitOfWork)
{
    public async Task ExecutarAsync(Guid id, CancellationToken ct = default)
    {
        var tarefa = await tarefas.ObterPorIdAsync(id, ct)
            ?? throw new RecursoNaoEncontradoException("Tarefa não encontrada.");

        tarefa.Ativo = false;
        await unitOfWork.SaveChangesAsync(ct);
    }
}
