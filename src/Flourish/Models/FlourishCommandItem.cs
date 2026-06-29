using System.Windows.Input;

namespace Flourish.Models;

public sealed record FlourishCommandItem(string Label, string IconGlyph, ICommand? Command = null);
