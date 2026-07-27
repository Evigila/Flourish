using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class FlourishProfileBuilder(FlourishProfileOptions options)
    : FlourishBuilderMutationGuard,
        IFlourishProfileBuilder
{
    public IFlourishProfileBuilder InitProfilePage<TPage>()
        where TPage : Page
    {
        ThrowIfFrozen();
        options.PageType = typeof(TPage);
        return this;
    }
}
