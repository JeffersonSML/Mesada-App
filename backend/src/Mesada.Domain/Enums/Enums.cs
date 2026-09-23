namespace Mesada.Domain.Enums;

// Cada enum aqui espelha 1:1 um CREATE TYPE ... AS ENUM em infra/db/migrations.
// Os nomes dos membros usam PascalCase (convenção C#); a camada de
// Infraestrutura converte para o snake_case exato do Postgres — nunca
// renomear um valor aqui sem atualizar a migração correspondente.

public enum CicloPeriodicidade
{
    Semanal,
    Quinzenal,
    Mensal,
    Personalizado
}

public enum StatusFamilia
{
    Ativa,
    Suspensa,
    Excluida
}

public enum ModoCalculo
{
    ValorDireto,
    Pontos
}

public enum TipoTarefa
{
    Recorrente,
    Avulsa
}

public enum NaturezaTarefa
{
    Bonus,
    Obrigatoria
}

public enum TipoEvidencia
{
    Nenhuma,
    Foto,
    IntegracaoAutomatica
}

public enum ProvedorIntegracao
{
    Strava,
    GoogleFit,
    AppleHealth
}

public enum StatusExecucao
{
    Pendente,
    Feito,
    Parcial,
    NaoFeito
}

public enum StatusAprovacao
{
    NaoAplicavel,
    Pendente,
    Aprovado,
    Rejeitado
}

public enum StatusCiclo
{
    Aberto,
    Fechado
}

public enum StatusAssinatura
{
    Trial,
    Ativa,
    Inadimplente,
    Cancelada
}

public enum StatusCobranca
{
    Pendente,
    Pago,
    Falhou,
    Estornado
}

public enum PapelConvite
{
    Master,
    Comum
}

public enum StatusConvite
{
    Pendente,
    Utilizado,
    Expirado,
    Revogado
}

public enum EventoNotificacao
{
    TarefaPendente,
    AprovacaoNecessaria,
    TarefaAprovada,
    TarefaRejeitada,
    MesadaFechada,
    TarefaAvulsaExpirando
}

public enum CanalNotificacao
{
    Push,
    Email
}

public enum TipoDestinatarioNotificacao
{
    Email,
    Telefone
}

public enum StatusTicket
{
    Aberto,
    EmAndamento,
    Resolvido,
    Fechado
}

public enum NivelRelatorio
{
    ExtratoBasico,
    Completo,
    CompletoComparativos
}

public enum NivelSuporte
{
    SelfService,
    Padrao,
    Prioritario
}
