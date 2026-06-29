using System.Windows.Controls;
using AcksheedSys.Flourish.Abstract;

namespace AcksheedSys.Flourish.Services;

internal sealed class NavigationService(
    IServiceProvider serviceProvider,
    PageHistoryService pageHistoryService
) : INavigationService, IFrameNavigationService
{
    private Frame? contentFrame;
    private object? currentParameter;

    public event EventHandler<FlourishNavigatedEventArgs>? Navigated;

    public bool CanGoBack => pageHistoryService.CanGoBack;

    public Type? CurrentSourcePageType { get; private set; }

    public void Initialize(Frame contentFrame)
    {
        this.contentFrame = contentFrame;
    }

    public bool Navigate(Type sourcePageType, object? parameter = null, bool addToBackStack = true)
    {
        return NavigateCore(sourcePageType, parameter, addToBackStack);
    }

    public bool Navigate<TPage>(object? parameter = null, bool addToBackStack = true)
        where TPage : Page
    {
        return Navigate(typeof(TPage), parameter, addToBackStack);
    }

    public bool GoBack()
    {
        if (!pageHistoryService.TryPop(out var entry))
        {
            return false;
        }

        return NavigateCore(entry.SourcePageType, entry.Parameter, false);
    }

    public void ClearBackStack()
    {
        pageHistoryService.Clear();
    }

    private bool NavigateCore(Type sourcePageType, object? parameter, bool addToBackStack)
    {
        if (contentFrame is null)
        {
            throw new InvalidOperationException(
                "NavigationService must be initialized with a frame."
            );
        }

        if (CurrentSourcePageType == sourcePageType && Equals(currentParameter, parameter))
        {
            return false;
        }

        if (addToBackStack && CurrentSourcePageType is not null)
        {
            pageHistoryService.Push(
                new FlourishPageStackEntry(CurrentSourcePageType, currentParameter)
            );
        }

        var page = CreatePage(sourcePageType);
        contentFrame.Navigate(page);
        CurrentSourcePageType = sourcePageType;
        currentParameter = parameter;

        Navigated?.Invoke(this, new FlourishNavigatedEventArgs(sourcePageType, page, parameter));
        return true;
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
