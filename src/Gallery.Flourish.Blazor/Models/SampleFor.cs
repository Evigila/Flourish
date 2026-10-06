namespace ArkheideSystem.Gallery.Flourish.Blazor.Models;

/// <summary>Associates a compiled Gallery sample with a documented component.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SampleFor(string componentName) : Attribute
{
    public string ComponentName { get; } = componentName;
    public string? DescriptionKey { get; set; }
}
