using Mesada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mesada.Infrastructure.Persistence;

// Mapeamento Fluent API para o schema físico em infra/db/migrations. Tabelas
// e colunas seguem a EFCore.NamingConventions (snake_case automático a
// partir dos nomes PascalCase das entidades — registrada em
// DependencyInjection.cs); aqui só configuramos o que a convenção não
// resolve sozinha: precisão de numeric(...) e, quando necessário, o nome
// explícito de FK que não segue o padrão "PropriedadeId".

public class FamiliaConfiguration : IEntityTypeConfiguration<Familia>
{
    public void Configure(EntityTypeBuilder<Familia> builder)
    {
        builder.ToTable("familias");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.CreatedAt).ValueGeneratedOnAdd();
        builder.Property(f => f.UpdatedAt).ValueGeneratedOnAddOrUpdate();
    }
}

public class UsuarioMasterConfiguration : IEntityTypeConfiguration<UsuarioMaster>
{
    public void Configure(EntityTypeBuilder<UsuarioMaster> builder)
    {
        builder.ToTable("usuarios_master");
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => m.Email).IsUnique();
        builder.Property(m => m.CreatedAt).ValueGeneratedOnAdd();
        builder.Property(m => m.UpdatedAt).ValueGeneratedOnAddOrUpdate();

        builder.HasOne(m => m.Familia)
            .WithMany(f => f.Masters)
            .HasForeignKey(m => m.FamiliaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UsuarioComumConfiguration : IEntityTypeConfiguration<UsuarioComum>
{
    public void Configure(EntityTypeBuilder<UsuarioComum> builder)
    {
        builder.ToTable("usuarios_comuns");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.MesadaBase).HasPrecision(10, 2);
        builder.Property(c => c.ValorPonto).HasPrecision(10, 4);
        builder.Property(c => c.SaldoDevedorAcumulado).HasPrecision(10, 2);
        builder.Property(c => c.CreatedAt).ValueGeneratedOnAdd();
        builder.Property(c => c.UpdatedAt).ValueGeneratedOnAddOrUpdate();

        builder.HasOne(c => c.Familia)
            .WithMany(f => f.Filhos)
            .HasForeignKey(c => c.FamiliaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("categorias");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.CreatedAt).ValueGeneratedOnAdd();
        builder.Property(c => c.UpdatedAt).ValueGeneratedOnAddOrUpdate();

        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Subcategorias)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TarefaConfiguration : IEntityTypeConfiguration<Tarefa>
{
    public void Configure(EntityTypeBuilder<Tarefa> builder)
    {
        builder.ToTable("tarefas");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Valor).HasPrecision(10, 2);
        builder.Property(t => t.Pontos).HasPrecision(10, 2);
        builder.Property(t => t.ValorMulta).HasPrecision(10, 2);
        builder.Property(t => t.CreatedAt).ValueGeneratedOnAdd();
        builder.Property(t => t.UpdatedAt).ValueGeneratedOnAddOrUpdate();

        builder.HasOne(t => t.Categoria)
            .WithMany()
            .HasForeignKey(t => t.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TarefaUsuarioConfiguration : IEntityTypeConfiguration<TarefaUsuario>
{
    public void Configure(EntityTypeBuilder<TarefaUsuario> builder)
    {
        builder.ToTable("tarefas_usuarios");
        builder.HasKey(tu => tu.Id);
        builder.HasIndex(tu => new { tu.TarefaId, tu.UsuarioComumId }).IsUnique();
        builder.Property(tu => tu.ValorOverride).HasPrecision(10, 2);
        builder.Property(tu => tu.PontosOverride).HasPrecision(10, 2);
        builder.Property(tu => tu.CreatedAt).ValueGeneratedOnAdd();
        builder.Property(tu => tu.UpdatedAt).ValueGeneratedOnAddOrUpdate();

        builder.HasOne(tu => tu.Tarefa)
            .WithMany(t => t.Aderencias)
            .HasForeignKey(tu => tu.TarefaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tu => tu.UsuarioComum)
            .WithMany()
            .HasForeignKey(tu => tu.UsuarioComumId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ExecucaoConfiguration : IEntityTypeConfiguration<Execucao>
{
    public void Configure(EntityTypeBuilder<Execucao> builder)
    {
        builder.ToTable("execucoes");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.PercentualConclusao).HasPrecision(5, 2);
        builder.Property(e => e.ValorCalculado).HasPrecision(10, 2);
        builder.Property(e => e.PontosCalculado).HasPrecision(10, 2);
        builder.Property(e => e.CreatedAt).ValueGeneratedOnAdd();
        builder.Property(e => e.UpdatedAt).ValueGeneratedOnAddOrUpdate();

        builder.HasOne(e => e.TarefaUsuario)
            .WithMany(tu => tu.Execucoes)
            .HasForeignKey(e => e.TarefaUsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.CicloMesada)
            .WithMany(c => c.Execucoes)
            .HasForeignKey(e => e.CicloMesadaId)
            .OnDelete(DeleteBehavior.SetNull);

        // aprovado_por referencia usuarios_master mas não é navegação — evita
        // carregar o Master em toda consulta de execução, que é o caso comum.
        builder.HasOne<UsuarioMaster>()
            .WithMany()
            .HasForeignKey(e => e.AprovadoPor)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class EvidenciaConfiguration : IEntityTypeConfiguration<Evidencia>
{
    public void Configure(EntityTypeBuilder<Evidencia> builder)
    {
        builder.ToTable("evidencias");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.CreatedAt).ValueGeneratedOnAdd();
        builder.Property(e => e.IntegracaoDados).HasColumnType("jsonb");

        builder.HasOne(e => e.Execucao)
            .WithMany(x => x.Evidencias)
            .HasForeignKey(e => e.ExecucaoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CicloMesadaConfiguration : IEntityTypeConfiguration<CicloMesada>
{
    public void Configure(EntityTypeBuilder<CicloMesada> builder)
    {
        builder.ToTable("ciclos_mesada");
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => new { c.UsuarioComumId, c.DataInicio }).IsUnique();
        builder.Property(c => c.MesadaBase).HasPrecision(10, 2);
        builder.Property(c => c.SomaBonus).HasPrecision(10, 2);
        builder.Property(c => c.SomaMultas).HasPrecision(10, 2);
        builder.Property(c => c.SaldoDevedorAnterior).HasPrecision(10, 2);
        builder.Property(c => c.ValorFinal).HasPrecision(10, 2);
        builder.Property(c => c.SaldoDevedorResultante).HasPrecision(10, 2);
        builder.Property(c => c.CreatedAt).ValueGeneratedOnAdd();
        builder.Property(c => c.UpdatedAt).ValueGeneratedOnAddOrUpdate();

        builder.HasOne(c => c.UsuarioComum)
            .WithMany()
            .HasForeignKey(c => c.UsuarioComumId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PlanoConfiguration : IEntityTypeConfiguration<Plano>
{
    public void Configure(EntityTypeBuilder<Plano> builder)
    {
        builder.ToTable("planos");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Codigo).IsUnique();
        builder.Property(p => p.PrecoMensal).HasPrecision(10, 2);
        builder.Property(p => p.CreatedAt).ValueGeneratedOnAdd();
        builder.Property(p => p.UpdatedAt).ValueGeneratedOnAddOrUpdate();
    }
}

public class AssinaturaConfiguration : IEntityTypeConfiguration<Assinatura>
{
    public void Configure(EntityTypeBuilder<Assinatura> builder)
    {
        builder.ToTable("assinaturas");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.CreatedAt).ValueGeneratedOnAdd();
        builder.Property(a => a.UpdatedAt).ValueGeneratedOnAddOrUpdate();

        builder.HasOne(a => a.Plano)
            .WithMany()
            .HasForeignKey(a => a.PlanoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class HistoricoCobrancaConfiguration : IEntityTypeConfiguration<HistoricoCobranca>
{
    public void Configure(EntityTypeBuilder<HistoricoCobranca> builder)
    {
        builder.ToTable("historico_cobranca");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Valor).HasPrecision(10, 2);
        builder.Property(h => h.CreatedAt).ValueGeneratedOnAdd();

        builder.HasOne(h => h.Assinatura)
            .WithMany(a => a.HistoricoCobrancas)
            .HasForeignKey(h => h.AssinaturaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ConviteAcessoConfiguration : IEntityTypeConfiguration<ConviteAcesso>
{
    public void Configure(EntityTypeBuilder<ConviteAcesso> builder)
    {
        builder.ToTable("convites_acesso");
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.Codigo).IsUnique();
        builder.Property(c => c.CreatedAt).ValueGeneratedOnAdd();
        builder.Property(c => c.UpdatedAt).ValueGeneratedOnAddOrUpdate();

        builder.HasOne(c => c.UsuarioComum)
            .WithMany()
            .HasForeignKey(c => c.UsuarioComumId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<UsuarioMaster>()
            .WithMany()
            .HasForeignKey(c => c.CriadoPor)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class NotificacaoConfigConfiguration : IEntityTypeConfiguration<NotificacaoConfig>
{
    public void Configure(EntityTypeBuilder<NotificacaoConfig> builder)
    {
        builder.ToTable("notificacoes_config");
        builder.HasKey(n => n.Id);
        builder.HasIndex(n => new { n.FamiliaId, n.Evento, n.Canal }).IsUnique();
        builder.Property(n => n.CreatedAt).ValueGeneratedOnAdd();
        builder.Property(n => n.UpdatedAt).ValueGeneratedOnAddOrUpdate();
    }
}

public class NotificacaoDestinatarioConfiguration : IEntityTypeConfiguration<NotificacaoDestinatario>
{
    public void Configure(EntityTypeBuilder<NotificacaoDestinatario> builder)
    {
        builder.ToTable("notificacao_destinatarios");
        builder.HasKey(n => n.Id);
        builder.HasIndex(n => new { n.FamiliaId, n.Tipo, n.Valor }).IsUnique();
        builder.Property(n => n.CreatedAt).ValueGeneratedOnAdd();
        builder.Property(n => n.UpdatedAt).ValueGeneratedOnAddOrUpdate();
    }
}

public class SuporteTicketConfiguration : IEntityTypeConfiguration<SuporteTicket>
{
    public void Configure(EntityTypeBuilder<SuporteTicket> builder)
    {
        builder.ToTable("suporte_tickets");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.CreatedAt).ValueGeneratedOnAdd();
        builder.Property(s => s.UpdatedAt).ValueGeneratedOnAddOrUpdate();

        builder.HasOne<UsuarioMaster>()
            .WithMany()
            .HasForeignKey(s => s.UsuarioMasterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class AdministradorConfiguration : IEntityTypeConfiguration<Administrador>
{
    public void Configure(EntityTypeBuilder<Administrador> builder)
    {
        builder.ToTable("administradores");
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => a.Email).IsUnique();
        builder.Property(a => a.Permissoes).HasColumnType("jsonb");
        builder.Property(a => a.CreatedAt).ValueGeneratedOnAdd();
        builder.Property(a => a.UpdatedAt).ValueGeneratedOnAddOrUpdate();

        builder.HasOne(a => a.Grupo)
            .WithMany()
            .HasForeignKey(a => a.GrupoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class GrupoAdministradorConfiguration : IEntityTypeConfiguration<GrupoAdministrador>
{
    public void Configure(EntityTypeBuilder<GrupoAdministrador> builder)
    {
        builder.ToTable("grupos_administrador");
        builder.HasKey(g => g.Id);
        builder.HasIndex(g => g.Nome).IsUnique();
        builder.Property(g => g.Permissoes).HasColumnType("jsonb");
        builder.Property(g => g.CreatedAt).ValueGeneratedOnAdd();
        builder.Property(g => g.UpdatedAt).ValueGeneratedOnAddOrUpdate();
    }
}
