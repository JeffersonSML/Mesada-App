using System.Data.Common;
using Mesada.Application.Abstractions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Mesada.Infrastructure.Tenancy;

/// <summary>
/// Ao abrir a conexão física do AppDbContext, executa
/// SELECT set_config('app.current_familia_id', '&lt;id&gt;', false) para que
/// as políticas de RLS (infra/db/migrations/019_rls_policies.sql) enxerguem
/// a família do usuário autenticado.
///
/// Simplificação intencional desta etapa: usamos set_config em escopo de
/// SESSÃO (terceiro parâmetro false), não de transação (LOCAL/true) — o
/// mais correto para um pool de conexões compartilhado seria escopo de
/// transação. Para isso funcionar com segurança contra vazamento entre
/// requisições, a connection string do mesada_app é registrada com
/// Pooling=false (ver DependencyInjection.cs): cada AppDbContext abre uma
/// conexão física própria, então não há reuso capaz de vazar o contexto de
/// uma família para outra. Trade-off aceito para fechar o ciclo
/// autenticação → RLS nesta etapa; revisitar (SET LOCAL + transação por
/// requisição, ou pooler ciente de tenant) quando performance virar
/// requisito (Etapa 6/7).
/// </summary>
public sealed class TenantConnectionInterceptor(ITenantContextAccessor tenantContext) : DbConnectionInterceptor
{
    private const string Sql = "SELECT set_config('app.current_familia_id', @familiaId, false)";

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        AplicarContexto(connection);
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await AplicarContextoAsync(connection, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private void AplicarContexto(DbConnection connection)
    {
        var familiaId = tenantContext.FamiliaId;
        if (familiaId is null) return;

        using var comando = connection.CreateCommand();
        comando.CommandText = Sql;
        AdicionarParametro(comando, familiaId.Value);
        comando.ExecuteNonQuery();
    }

    private async Task AplicarContextoAsync(DbConnection connection, CancellationToken ct)
    {
        var familiaId = tenantContext.FamiliaId;
        if (familiaId is null) return;

        await using var comando = connection.CreateCommand();
        comando.CommandText = Sql;
        AdicionarParametro(comando, familiaId.Value);
        await comando.ExecuteNonQueryAsync(ct);
    }

    private static void AdicionarParametro(DbCommand comando, Guid familiaId)
    {
        var parametro = comando.CreateParameter();
        parametro.ParameterName = "familiaId";
        parametro.Value = familiaId.ToString();
        comando.Parameters.Add(parametro);
    }
}
