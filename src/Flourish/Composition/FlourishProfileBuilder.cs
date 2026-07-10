using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Composition;

internal sealed class FlourishProfileBuilder(FlourishProfileOptions options)
    : IFlourishProfileBuilder
{
    public IFlourishProfileBuilder SetProfilePage<TPage>()
        where TPage : Page
    {
        return SetProfilePage(typeof(TPage));
    }

    public IFlourishProfileBuilder SetProfilePage(Type pageType)
    {
        ArgumentNullException.ThrowIfNull(pageType);
        if (!typeof(Page).IsAssignableFrom(pageType))
        {
            throw new ArgumentException(
                $"Profile page type {pageType.FullName} must derive from {typeof(Page).FullName}.",
                nameof(pageType)
            );
        }

        options.PageType = pageType;
        return this;
    }
}
