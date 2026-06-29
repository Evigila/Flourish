namespace AcksheedSys.Flourish.Abstract;

public interface IFlourishStatusService
{
    string StatusText { get; }

    IReadOnlyList<FlourishStatusItem> StatusItems { get; }
}
