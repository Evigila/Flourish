using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class FlourishProfileBuilder(FlourishProfileOptions options)
    : IFlourishProfileBuilder
{
    public IFlourishProfileBuilder SetProfilePage<TPage>()
        where TPage : Page
    {
        options.PageType = typeof(TPage);
        return this;
    }
}
