using System.Collections.Generic;

namespace ArkheideSystem.Flourish.Navigation;

internal sealed class FlourishServiceCollectionState
{
    public List<NavigablePageRegistration> NavigablePages { get; } = [];
}
