namespace Mesada.Application.Abstractions;

/// <summary>Abstrai a URL pública da Web — hoje só usada para montar o link enviado no e-mail de redefinição de senha.</summary>
public interface IAppUrlProvider
{
    string UrlBaseWeb { get; }
}
