using AcksheedSys.Flourish.Abstract;

namespace AcksheedSys.Flourish.Services;

internal sealed class PageHistoryService : IFlourishPageHistoryService
{
    private readonly Stack<FlourishPageStackEntry> backStack = new();

    public bool CanGoBack => backStack.Count > 0;

    public IReadOnlyCollection<FlourishPageStackEntry> BackStack => backStack;

    public void Push(FlourishPageStackEntry entry)
    {
        backStack.Push(entry);
    }

    public bool TryPop(out FlourishPageStackEntry entry)
    {
        if (backStack.Count == 0)
        {
            entry = default!;
            return false;
        }

        entry = backStack.Pop();
        return true;
    }

    public void Clear()
    {
        backStack.Clear();
    }
}
