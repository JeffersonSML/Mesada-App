using Mesada.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace Mesada.Infrastructure;

public sealed class AppUrlProvider(IOptions<WebAppOptions> options) : IAppUrlProvider
{
    public string UrlBaseWeb => options.Value.UrlBaseWeb;
}
