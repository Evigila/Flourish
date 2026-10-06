using System.ComponentModel.DataAnnotations;

namespace ArkheideSystem.Gallery.Flourish.Blazor.Models;

public sealed class PaletteDraft
{
    [Required(ErrorMessage = "Key.Validation_ColorRequired"), RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Key.Validation_ColorFormat")]
    public string Primary { get; set; } = "#153A32";
    [Required(ErrorMessage = "Key.Validation_ColorRequired"), RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Key.Validation_ColorFormat")]
    public string Accent { get; set; } = "#16745F";
}
