using System.ComponentModel.DataAnnotations;

namespace ArkheideSystem.Gallery.Flourish.Blazor.Models;

public sealed class PaletteDraft
{
    [Required, RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "使用 #RRGGBB 格式。")]
    public string Primary { get; set; } = "#153A32";
    [Required, RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "使用 #RRGGBB 格式。")]
    public string Accent { get; set; } = "#16745F";
}
