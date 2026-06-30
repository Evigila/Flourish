using System.Windows;

namespace AcksheedSys.Flourish.Abstract;

public interface IMaterialEffectService
{
    MaterialEffect CurrentEffect { get; }

    bool IsApplied { get; }

    void Attach(Window window, MaterialEffect effect);
}
