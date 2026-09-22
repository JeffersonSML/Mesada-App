using Mesada.Domain.Enums;
using Npgsql;
using Npgsql.NameTranslation;

namespace Mesada.Infrastructure.Persistence;

/// <summary>
/// Registra cada enum do domínio contra o tipo nativo equivalente no
/// Postgres (infra/db/migrations/*.sql). Precisa ser chamado ao montar o
/// NpgsqlDataSource usado pelo DbContext — sem isso, Npgsql não sabe
/// converter entre o enum C# e o tipo enumerado do banco.
/// </summary>
public static class NpgsqlEnumMappings
{
    public static NpgsqlDataSourceBuilder MapMesadaEnums(this NpgsqlDataSourceBuilder builder)
    {
        var translator = new NpgsqlSnakeCaseNameTranslator();

        builder.MapEnum<CicloPeriodicidade>("ciclo_periodicidade", translator);
        builder.MapEnum<StatusFamilia>("status_familia", translator);
        builder.MapEnum<ModoCalculo>("modo_calculo", translator);
        builder.MapEnum<TipoTarefa>("tipo_tarefa", translator);
        builder.MapEnum<NaturezaTarefa>("natureza_tarefa", translator);
        builder.MapEnum<TipoEvidencia>("tipo_evidencia", translator);
        builder.MapEnum<ProvedorIntegracao>("provedor_integracao", translator);
        builder.MapEnum<StatusExecucao>("status_execucao", translator);
        builder.MapEnum<StatusAprovacao>("status_aprovacao", translator);
        builder.MapEnum<StatusCiclo>("status_ciclo", translator);
        builder.MapEnum<StatusAssinatura>("status_assinatura", translator);
        builder.MapEnum<StatusCobranca>("status_cobranca", translator);
        builder.MapEnum<PapelConvite>("papel_convite", translator);
        builder.MapEnum<StatusConvite>("status_convite", translator);
        builder.MapEnum<EventoNotificacao>("evento_notificacao", translator);
        builder.MapEnum<CanalNotificacao>("canal_notificacao", translator);
        builder.MapEnum<StatusTicket>("status_ticket", translator);
        builder.MapEnum<NivelRelatorio>("nivel_relatorio", translator);
        builder.MapEnum<NivelSuporte>("nivel_suporte", translator);

        return builder;
    }
}
