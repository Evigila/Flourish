namespace AcksheedSys.Flourish.Abstract;

public interface IFlourishPageHistoryService
{
    bool CanGoBack { get; }

    IReadOnlyCollection<FlourishPageStackEntry> BackStack { get; }

    void Clear();
}
