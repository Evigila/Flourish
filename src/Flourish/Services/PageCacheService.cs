using System.Windows.Controls;
using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Configuration;

namespace AcksheedSys.Flourish.Services;

internal sealed class PageCacheService(IServiceProvider serviceProvider, FlourishShellOptions options)
{
    private readonly Dictionary<Type, Page> cachedPages = [];
    private readonly Dictionary<Type, FlourishPageCacheMode> cacheModesByPageType =
        CreateCacheModeMap(options);

    private static Dictionary<Type, FlourishPageCacheMode> CreateCacheModeMap(
        FlourishShellOptions options
    )
    {
        return new Dictionary<Type, FlourishPageCacheMode>(options.PageCacheModesByPageType);
    }

    public Page GetPage(Type sourcePageType)
    {
        if (
            cacheModesByPageType.TryGetValue(sourcePageType, out var cacheMode)
            && cacheMode == FlourishPageCacheMode.Enabled
        )
        {
            if (!cachedPages.TryGetValue(sourcePageType, out var cachedPage))
            {
                cachedPage = CreatePage(sourcePageType);
                cachedPages[sourcePageType] = cachedPage;
            }

            return cachedPage;
        }

        return CreatePage(sourcePageType);
    }

    private Page CreatePage(Type sourcePageType)
    {
        var page =
            serviceProvider.GetService(sourcePageType) ?? Activator.CreateInstance(sourcePageType);

        return page as Page
            ?? throw new InvalidOperationException(
                $"{sourcePageType.FullName} must derive from System.Windows.Controls.Page."
            );
    }
}
