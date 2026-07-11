using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class FlourishStatusService(FlourishShellOptions options)
{
    public string StatusText => options.StatusText;

    public IReadOnlyList<FlourishStatusItem> StatusItems => options.StatusItems;
}
