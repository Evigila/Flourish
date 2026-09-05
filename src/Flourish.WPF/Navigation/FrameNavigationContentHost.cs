using System;

using ArkheideSystem.Flourish.Abstract;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Navigation;

internal sealed class FrameNavigationContentHost(Frame frame) : INavigationContentHost
{
    private readonly Frame frame = frame ?? throw new ArgumentNullException(nameof(frame));

    public bool Navigate(Page page)
    {
        return frame.Navigate(page);
    }
}
