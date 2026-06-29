using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Models;

namespace AcksheedSys.Flourish.Services;

internal sealed class FlourishStatusService(FlourishShellOptions options) : IFlourishStatusService
{
    public string StatusText => options.StatusText;

    public IReadOnlyList<FlourishStatusItem> StatusItems => options.StatusItems;
}
