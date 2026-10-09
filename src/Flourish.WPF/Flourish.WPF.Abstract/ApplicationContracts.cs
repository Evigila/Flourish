using System.Windows.Media;

namespace ArkheideSystem.Flourish.WPF.Abstract;

public sealed record NavigationEntry(string Label, string Icon, string Route, Func<object> Content,
    IReadOnlyList<NavigationEntry> Children, bool Disabled = false, TextReference? LabelReference = null);
public sealed record FrameworkOptions(string ProjectName, TextReference? ProjectNameReference, ImageSource? Logo,
    IReadOnlyList<NavigationEntry> Navigation, IReadOnlyList<Func<object>> TopRight, ITextProvider? TextProvider);
