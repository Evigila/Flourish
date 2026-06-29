using AcksheedSys.Flourish.Composition;

namespace AcksheedSys.Flourish.Abstract;

public static class FlourishBuilder
{
    public static IFlourishBuilder CreateDefaultBuilder(string[] args)
    {
        return new DefaultFlourishBuilder(args);
    }
}
