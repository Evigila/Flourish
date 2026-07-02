namespace AcksheedSys.Flourish.Abstract;

public interface IFlourishMotionBuilder
{
    IFlourishMotionBuilder SetEnabled(bool enabled = true);

    IFlourishMotionBuilder SetDuration(TimeSpan duration);

    IFlourishMotionBuilder SetPageTransition(
        FlourishPageTransition transition = FlourishPageTransition.EntranceFromBottom
    );

    IFlourishMotionBuilder SetNavigationPanelTransition(
        FlourishNavigationPanelTransition transition = FlourishNavigationPanelTransition.Resize
    );

    IFlourishMotionBuilder RespectSystemReducedMotion(bool enabled = true);
}
