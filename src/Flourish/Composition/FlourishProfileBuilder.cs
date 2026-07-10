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
        options.PageType = typeof(TPage);
        return this;
    }
}
