using Mesada.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mesada.Infrastructure.Persistence;

/// <summary>
/// Modelo compartilhado por AppDbContext (role mesada_app, sujeito a RLS) e
/// AdminDbContext (role mesada_admin, BYPASSRLS) — só a connection/role
/// difere entre os dois; o mapeamento das tabelas é o mesmo schema físico
/// (infra/db/migrations). Ver docs/adr/0002-schema-e-rls.md.
/// </summary>
public abstract class MesadaDbContextBase(DbContextOptions options) : DbContext(options)
{
    public DbSet<Familia> Familias => Set<Familia>();
    public DbSet<UsuarioMaster> UsuariosMaster => Set<UsuarioMaster>();
    public DbSet<UsuarioComum> UsuariosComuns => Set<UsuarioComum>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
    public DbSet<TarefaUsuario> TarefasUsuarios => Set<TarefaUsuario>();
    public DbSet<Execucao> Execucoes => Set<Execucao>();
    public DbSet<Evidencia> Evidencias => Set<Evidencia>();
    public DbSet<CicloMesada> CiclosMesada => Set<CicloMesada>();
    public DbSet<Plano> Planos => Set<Plano>();
    public DbSet<Assinatura> Assinaturas => Set<Assinatura>();
    public DbSet<HistoricoCobranca> HistoricoCobrancas => Set<HistoricoCobranca>();
    public DbSet<ConviteAcesso> ConvitesAcesso => Set<ConviteAcesso>();
    public DbSet<NotificacaoConfig> NotificacoesConfig => Set<NotificacaoConfig>();
    public DbSet<SuporteTicket> SuporteTickets => Set<SuporteTicket>();
    public DbSet<Administrador> Administradores => Set<Administrador>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MesadaDbContextBase).Assembly);
    }
}

/// <summary>Conecta como mesada_app — sujeito a Row Level Security por familia_id.</summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : MesadaDbContextBase(options);

/// <summary>Conecta como mesada_admin (BYPASSRLS) — uso exclusivo do módulo Administrador e dos fluxos pré-autenticação (login, resgate de convite) que precisam localizar o tenant antes de existir contexto de família.</summary>
public sealed class AdminDbContext(DbContextOptions<AdminDbContext> options) : MesadaDbContextBase(options);
