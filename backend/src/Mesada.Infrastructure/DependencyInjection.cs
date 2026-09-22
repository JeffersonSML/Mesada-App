using Mesada.Application.Abstractions;
using Mesada.Application.Auth;
using Mesada.Application.Familias;
using Mesada.Application.Repositories;
using Mesada.Infrastructure.Persistence;
using Mesada.Infrastructure.Repositories;
using Mesada.Infrastructure.Security;
using Mesada.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Mesada.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Requer que o chamador já tenha registrado um ITenantContextAccessor
    /// (em Mesada.Api, lendo os claims do JWT) antes de chamar este método —
    /// o TenantConnectionInterceptor do AppDbContext depende dele.
    /// </summary>
    private const string ChaveDataSourceApp = "mesada-app";
    private const string ChaveDataSourceAdmin = "mesada-admin";

    public static IServiceCollection AddMesadaInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // As NpgsqlDataSource são resolvidas lazily (primeira vez que algo
        // pede o serviço), nunca construídas aqui de forma eager: isso é o
        // que permite hosts de teste (WebApplicationFactory) sobrescreverem
        // ConnectionStrings via ConfigureAppConfiguration antes do primeiro
        // uso real — se lêssemos `configuration` diretamente neste método,
        // capturaríamos a configuração de ANTES desses overrides serem
        // aplicados.
        services.AddKeyedSingleton(ChaveDataSourceApp, (sp, _) =>
        {
            var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("MesadaApp")
                ?? throw new InvalidOperationException("ConnectionStrings:MesadaApp não configurada.");
            return new NpgsqlDataSourceBuilder(connectionString).MapMesadaEnums().Build();
        });

        services.AddKeyedSingleton(ChaveDataSourceAdmin, (sp, _) =>
        {
            var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("MesadaAdmin")
                ?? throw new InvalidOperationException("ConnectionStrings:MesadaAdmin não configurada.");
            return new NpgsqlDataSourceBuilder(connectionString).MapMesadaEnums().Build();
        });

        services.AddScoped<TenantConnectionInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) => options
            .UseNpgsql(sp.GetRequiredKeyedService<NpgsqlDataSource>(ChaveDataSourceApp))
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(sp.GetRequiredService<TenantConnectionInterceptor>()));

        services.AddDbContext<AdminDbContext>((sp, options) => options
            .UseNpgsql(sp.GetRequiredKeyedService<NpgsqlDataSource>(ChaveDataSourceAdmin))
            .UseSnakeCaseNamingConvention());

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SecaoConfiguracao));

        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Repositórios do fluxo pré-tenant (login, resgate de convite) —
        // ver AdminUsuarioMasterRepository para a justificativa de usarem
        // AdminDbContext (mesada_admin) em vez de AppDbContext.
        services.AddScoped<IUsuarioMasterRepository, AdminUsuarioMasterRepository>();
        services.AddScoped<IUsuarioComumRepository, AdminUsuarioComumRepository>();
        services.AddScoped<IConviteAcessoRepository, AdminConviteAcessoRepository>();
        services.AddScoped<IFamiliaRepository, AdminFamiliaRepository>();
        services.AddScoped<IUnitOfWork, AdminUnitOfWork>();

        // Consultas tenant-scoped (AppDbContext / mesada_app / RLS).
        services.AddScoped<IFilhosDaFamiliaQuery, FilhosDaFamiliaQuery>();

        services.AddScoped<AutenticarMasterUseCase>();
        services.AddScoped<ResgatarConviteComumUseCase>();
        services.AddScoped<ListarFilhosUseCase>();

        return services;
    }
}
