using System.Windows.Controls;

namespace AcksheedSys.Flourish.Abstract;

public sealed class FlourishNavigatedEventArgs(Type sourcePageType, Page page, object? parameter)
    : EventArgs
{
    public Type SourcePageType { get; } = sourcePageType;

    public Page Page { get; } = page;

    public object? Parameter { get; } = parameter;
}
