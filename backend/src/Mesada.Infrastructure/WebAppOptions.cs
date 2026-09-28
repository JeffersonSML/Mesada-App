namespace Mesada.Infrastructure;

/// <summary>Configuração da própria Web (não confundir com Google/FirebaseAdmin.AppOptions, daí o nome) — hoje só a URL pública, usada para montar o link de redefinição de senha enviado por e-mail.</summary>
public sealed class WebAppOptions
{
    public const string SecaoConfiguracao = "App";

    public string UrlBaseWeb { get; set; } = "http://localhost:5173";
}
