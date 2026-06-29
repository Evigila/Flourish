using System.Windows.Controls;

namespace Flourish.Services;

public sealed class PageNavigatedEventArgs : EventArgs
{
    public PageNavigatedEventArgs(Type sourcePageType, Page page, object? parameter)
    {
        SourcePageType = sourcePageType;
        Page = page;
        Parameter = parameter;
    }

    public Type SourcePageType { get; }

    public Page Page { get; }

    public object? Parameter { get; }
}
