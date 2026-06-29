namespace AcksheedSys.Flourish.Abstract;

public interface IFlourishToolbarService
{
    IReadOnlyList<FlourishToolbarItem> GetToolbarItems(Type? pageType = null);
}
