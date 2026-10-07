using System;
using System.Threading.Tasks;
using System.Collections.Generic;



namespace ArkheideSystem.Flourish.Blazor.Components;

/// <summary>The unique appearance contract of a standard action.</summary>
public enum ButtonVariant { Primary, Secondary, Danger, Quiet, Underline, Elevated }

/// <summary>The geometry of a modal surface.</summary>
public enum DialogPresentation { Centered, BottomSheet }

/// <summary>Logical horizontal alignment within the available container width.</summary>
public enum HorizontalAlignment { Start, Center, End }

/// <summary>Standard empty-result feedback or a passive watermark in an unused page area.</summary>
public enum EmptyStateVariant { Standard, Watermark }

/// <summary>An action supplied by the host; the component never owns business behavior.</summary>
public sealed record MenuAction(string Text, Func<Task> OnClick, bool Disabled = false, bool Destructive = false);

/// <summary>A typed native select option.</summary>
public sealed record SelectOption<TValue>(TValue Value, string Text, bool Disabled = false);
