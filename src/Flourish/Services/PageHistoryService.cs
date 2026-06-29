namespace Flourish.Services;

public sealed class PageHistoryService
{
    private readonly Stack<PageStackEntry> backStack = new();

    public bool CanGoBack => backStack.Count > 0;

    public IReadOnlyCollection<PageStackEntry> BackStack => backStack;

    public void Push(PageStackEntry entry)
    {
        backStack.Push(entry);
    }

    public bool TryPop(out PageStackEntry entry)
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
