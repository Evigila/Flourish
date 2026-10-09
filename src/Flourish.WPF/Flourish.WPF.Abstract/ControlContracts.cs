namespace ArkheideSystem.Flourish.WPF.Abstract;

public enum ButtonVariant { Primary, Secondary, Danger, Quiet, Underline, Elevated }
public enum DialogPresentation { Centered, BottomSheet }
public enum EmptyStateVariant { Standard, Watermark }
public enum PresentationTone { Canvas, Surface, Primary }
public enum NoticeKind { Information, Success, Warning, Error, Subtle }
public enum UniformGridVariant { Elevated, Filled, Outlined, Danger }
public enum UniformGridShape { Rectangle, Square }
public enum ThemeMode { System, Light, Dark }
public enum ComponentUseKind { General, Scenario, BuildingBlock }

/// <summary>Business actions remain owned by the consumer.</summary>
public sealed record MenuAction(string Text, Func<Task> OnClick, bool Disabled = false, bool Destructive = false);
public sealed record SelectOption(object? Value, string Text, bool Disabled = false);
public sealed record ReferenceItem(object Id, string Name);
public sealed record ServiceLink(string Label, string Href);
public sealed record NavigationChoiceItem(string Key, string Label, string Target, bool Disabled = false);
public sealed record MultiSelectOption(string Key, string Label, bool Selected = false, bool CanDeselect = true,
    bool CanReorder = true, bool Disabled = false);
/// <summary>A complete selection and ordering snapshot; stable keys are never positional indexes.</summary>
public sealed record MultiSelectChange(IReadOnlyList<string> OrderedKeys, IReadOnlySet<string> SelectedKeys);
public sealed record DialogView(string Key, object Content);
