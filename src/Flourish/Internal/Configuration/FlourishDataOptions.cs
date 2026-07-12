namespace ArkheideSystem.Flourish.Internal.Configuration;

internal sealed class FlourishDataOptions
{
    public string Locale { get; set; } = "EN";

    public List<string> LocalePaths { get; } = [];
}
