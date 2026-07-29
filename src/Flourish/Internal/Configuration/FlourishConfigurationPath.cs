namespace ArkheideSystem.Flourish.Internal.Configuration;

internal static class FlourishConfigurationPath
{
    public const string Root = "Flourish";
    public const string Prefix = $"{Root}:";

    public static bool IsOwnedDescendant(string path)
    {
        return path.Length > Prefix.Length
            && path.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase);
    }
}
