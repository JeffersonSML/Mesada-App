using Amazon;
using Amazon.S3;
using Mesada.Application.Abstractions;
using Mesada.Application.Auth;
using Mesada.Application.Familias;
using Mesada.Application.Notificacoes;
using Mesada.Application.Repositories;
using Mesada.Infrastructure.Notifications;
using Mesada.Infrastructure.Payments;
using Mesada.Infrastructure.Persistence;
using Mesada.Infrastructure.Repositories;
using Mesada.Infrastructure.Security;
using Mesada.Infrastructure.Storage;
using Mesada.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using PagarMe;
using Resend;

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

        // Consultas e repositórios tenant-scoped (AppDbContext / mesada_app / RLS).
        services.AddScoped<ITenantUnitOfWork, AppUnitOfWork>();
        services.AddScoped<IFilhosDaFamiliaQuery, FilhosDaFamiliaQuery>();
        services.AddScoped<IDestinatariosNotificacaoRepository, DestinatariosNotificacaoRepository>();

        services.AddScoped<AutenticarMasterUseCase>();
        services.AddScoped<ResgatarConviteComumUseCase>();
        services.AddScoped<ListarFilhosUseCase>();
        services.AddScoped<ListarDestinatariosNotificacaoUseCase>();
        services.AddScoped<AdicionarDestinatarioNotificacaoUseCase>();
        services.AddScoped<RemoverDestinatarioNotificacaoUseCase>();

        services.AddInfraServicesExternas(configuration);

        return services;
    }

    /// <summary>
    /// Pagamento (Stone/Pagar.me), e-mail (Resend), push (FCM) e storage de
    /// evidências (S3-compatível) — Etapa 6. Cada registro constrói o
    /// cliente do SDK dentro de uma fábrica (AddSingleton(sp => ...)),
    /// nunca inline no corpo deste método: fábricas só executam no primeiro
    /// uso real, depois que toda configuração (inclusive overrides de
    /// teste) já foi aplicada — mesmo cuidado do NpgsqlDataSource acima.
    /// </summary>
    private static IServiceCollection AddInfraServicesExternas(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PagarMeOptions>(configuration.GetSection(PagarMeOptions.SecaoConfiguracao));
        services.AddSingleton<IPagarMeApiClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PagarMeOptions>>().Value;
            return new PagarMeApiClient(options.SecretKey, null, null, null, null, null, null, null, false);
        });
        services.AddScoped<IPaymentProvider, PagarMePaymentProvider>();

        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SecaoConfiguracao));
        services.AddResend(_ => { });
        services.AddOptions<ResendClientOptions>().Configure<IConfiguration>((resendOptions, config) =>
        {
            resendOptions.ApiToken = config["Email:ResendApiKey"]
                ?? throw new InvalidOperationException("Email:ResendApiKey não configurada.");

            // Só usado para apontar a um servidor local/mock em testes — em
            // produção, deixar em branco e o SDK usa a API real do Resend.
            var apiUrlOverride = config["Email:ResendApiUrl"];
            if (!string.IsNullOrWhiteSpace(apiUrlOverride))
                resendOptions.ApiUrl = apiUrlOverride;
        });
        services.AddScoped<IEmailSender, ResendEmailSender>();

        services.Configure<FcmOptions>(configuration.GetSection(FcmOptions.SecaoConfiguracao));
        services.AddSingleton<IPushNotificationSender, FcmPushNotificationSender>();

        services.Configure<S3Options>(configuration.GetSection(S3Options.SecaoConfiguracao));
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<S3Options>>().Value;
            var credenciais = new Amazon.Runtime.BasicAWSCredentials(options.AccessKey, options.SecretKey);
            // SignatureVersion = "4": afeta a assinatura das chamadas
            // autenticadas normais (PutObject etc.). Testando localmente
            // (ver S3EvidenceStorageServiceTests) descobri que isso NÃO
            // muda o esquema de GetPreSignedURLAsync quando ServiceUrl é
            // customizado sem RegionEndpoint — a URL pré-assinada continua
            // saindo em V2 (AWSAccessKeyId/Signature) nesta versão do SDK.
            // Se o provedor S3-compatível escolhido exigir V4 também nas
            // URLs pré-assinadas, revisitar com a documentação dele em mãos.
            var config = string.IsNullOrWhiteSpace(options.ServiceUrl)
                ? new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region), SignatureVersion = "4" }
                : new AmazonS3Config { ServiceURL = options.ServiceUrl, ForcePathStyle = true, SignatureVersion = "4" };
            return new AmazonS3Client(credenciais, config);
        });
        services.AddScoped<IEvidenceStorageService, S3EvidenceStorageService>();

        return services;
    }
}
