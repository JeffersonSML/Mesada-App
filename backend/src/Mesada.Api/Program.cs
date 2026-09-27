using System.Text;
using System.Text.Json.Serialization;
using Mesada.Api.Tenancy;
using Mesada.Application.Abstractions;
using Mesada.Application.Auth;
using Mesada.Infrastructure;
using Mesada.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
// Singleton, não Scoped: HttpTenantContextAccessor não guarda estado próprio
// (só lê IHttpContextAccessor.HttpContext, que já é ambient/AsyncLocal, a
// cada acesso de propriedade) — precisa ser Singleton para que
// TenantConnectionInterceptor também possa ser Singleton (ver
// Mesada.Infrastructure.DependencyInjection), evitando o
// ManyServiceProvidersCreatedWarning do EF Core (um DbContextOptions
// "diferente" a cada requisição, por causa de um interceptor Scoped, força
// reconstruir o service provider interno do EF Core em cada chamada).
builder.Services.AddSingleton<ITenantContextAccessor, HttpTenantContextAccessor>();
builder.Services.AddMesadaInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

// Configurado via DI (IOptions<JwtOptions>), não lendo builder.Configuration
// diretamente aqui: assim como em Mesada.Infrastructure.DependencyInjection,
// isso garante resolução lazy, no primeiro uso real — necessário para que
// hosts de teste (WebApplicationFactory.ConfigureAppConfiguration) consigam
// sobrescrever a seção "Jwt" antes de qualquer leitura acontecer.
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptionsAccessor) =>
    {
        var jwt = jwtOptionsAccessor.Value;
        bearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Emissor,
            ValidateAudience = true,
            ValidAudience = jwt.Audiencia,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Chave)),
            RoleClaimType = MesadaClaimTypes.Papel,
        };
    });

builder.Services.AddAuthorization();

// Sem isso, qualquer frontend servido de outra origem (o preview do Lovable,
// ou este mesmo app rodando localmente em `vite dev`) tem toda chamada
// bloqueada pelo navegador antes mesmo de chegar à API — descoberto testando
// o preview Lovable de verdade contra a API local, não em teoria.
var origensCorsPermitidas = builder.Configuration.GetSection("Cors:OrigensPermitidas").Get<string[]>()
    ?? ["http://localhost:5173", "https://id-preview--0ffe502c-7386-4b57-9954-c12b45b20cb9.lovable.app"];
builder.Services.AddCors(options => options.AddPolicy("MesadaWeb", policy =>
    policy.WithOrigins(origensCorsPermitidas).AllowAnyHeader().AllowAnyMethod()));
// JsonStringEnumConverter: por padrão, System.Text.Json serializa enums como
// número (ex.: CicloPeriodicidade.Mensal vira 2), o que só passou
// despercebido nos testes de integração porque eles montam os requests com
// os tipos C# dos enums diretamente — qualquer cliente HTTP real (curl, o
// frontend Lovable) que envie a string "Mensal" documentada no contrato
// recebe 400. Descoberto testando de verdade contra a API via HTTP puro.
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Mesada App API", Version = "v1" });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Informe apenas o token JWT (sem o prefixo 'Bearer ').",
        Reference = new OpenApiReference { Id = JwtBearerDefaults.AuthenticationScheme, Type = ReferenceType.SecurityScheme }
    };
    options.AddSecurityDefinition(jwtScheme.Reference.Id, jwtScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtScheme, Array.Empty<string>() } });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("MesadaWeb");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Necessário para o WebApplicationFactory<Program> dos testes de integração.
public partial class Program;
