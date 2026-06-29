using System.Windows.Controls;

namespace Flourish.Models;

public sealed record FlourishNavigationItem(
    string Key,
    string Label,
    string IconGlyph,
    Type PageType
)
{
    public void Validate()
    {
        if (!typeof(Page).IsAssignableFrom(PageType))
        {
            throw new ArgumentException(
                $"{PageType.FullName} must derive from System.Windows.Controls.Page.",
                nameof(PageType)
            );
        }
    }
}
