using System.Windows.Controls;

namespace Flourish.Services;

public interface INavigationService
{
    event EventHandler<PageNavigatedEventArgs>? Navigated;

    bool CanGoBack { get; }

    Type? CurrentSourcePageType { get; }

    void Initialize(Frame contentFrame);

    bool Navigate(Type sourcePageType, object? parameter = null, bool addToBackStack = true);

    bool Navigate<TPage>(object? parameter = null, bool addToBackStack = true)
        where TPage : Page;

    bool GoBack();

    void ClearBackStack();
}
