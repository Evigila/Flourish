namespace AcksheedSys.Flourish.Abstract;

public interface IFlourishNavigationPanelBuilder
{
    IFlourishNavigationPanelBuilder SetEnabled(bool enabled = true);

    IFlourishNavigationPanelBuilder SetDirection(
        NavigationPanelDirection direction = NavigationPanelDirection.Left
    );

    IFlourishNavigationPanelBuilder SetInitiallyOpen(bool enabled = true);

    IFlourishNavigationPanelBuilder SetTitle(string title);
}
