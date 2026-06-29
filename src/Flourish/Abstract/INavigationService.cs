using System.Windows.Controls;

namespace AcksheedSys.Flourish.Abstract;

public interface INavigationService
{
    event EventHandler<FlourishNavigatedEventArgs>? Navigated;

    bool CanGoBack { get; }

    Type? CurrentSourcePageType { get; }

    bool Navigate(Type sourcePageType, object? parameter = null, bool addToBackStack = true);

    bool Navigate<TPage>(object? parameter = null, bool addToBackStack = true)
        where TPage : Page;

    bool GoBack();

    void ClearBackStack();
}
