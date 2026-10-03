using System;
using System.Threading.Tasks;
using System.Collections.Generic;



namespace ArkheideSystem.Flourish.Blazor.Components;

/// <summary>The semantic appearance of an action.</summary>
public enum ButtonVariant { Primary, Secondary, Danger, Quiet }

/// <summary>The geometry of a modal surface.</summary>
public enum DialogPresentation { Centered, BottomSheet }

/// <summary>An action supplied by the host; the component never owns business behavior.</summary>
public sealed record MenuAction(string Text, Func<Task> OnClick, bool Disabled = false, bool Destructive = false);

/// <summary>A typed native select option.</summary>
public sealed record SelectOption<TValue>(TValue Value, string Text, bool Disabled = false);

