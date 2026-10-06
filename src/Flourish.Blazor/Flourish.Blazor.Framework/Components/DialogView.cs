using Microsoft.AspNetCore.Components;

namespace ArkheideSystem.Flourish.Blazor.Components;

/// <summary>A keyed content view within a general dialog; the dialog owner chooses its active view.</summary>
public sealed record DialogView(string Key, RenderFragment Content);
