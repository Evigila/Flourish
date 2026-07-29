using ArkheideSystem.Flourish.Abstract.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class FlourishConfigurationBuilder
    : FlourishBuilderMutationGuard,
        IFlourishConfigurationBuilder
{
    private readonly List<IConfigurationSource> sources = [];

    internal IReadOnlyList<IConfigurationSource> Sources => sources;

    public IFlourishConfigurationBuilder UseConfigurationFile(
        string path,
        bool optional = true,
        bool reloadOnChange = true
    )
    {
        ThrowIfFrozen();
        var source = new JsonConfigurationSource
        {
            Path = FlourishDataBuilder.ResolveFilePath(path, nameof(path)),
            Optional = optional,
            ReloadOnChange = reloadOnChange,
            ReloadDelay = 250,
        };
        source.ResolveFileProvider();
        sources.Add(source);
        return this;
    }

    public IFlourishConfigurationBuilder AddConfigurationSource(IConfigurationSource source)
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(source);
        sources.Add(source);
        return this;
    }
}
