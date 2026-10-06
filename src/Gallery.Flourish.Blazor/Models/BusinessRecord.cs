using System.ComponentModel.DataAnnotations;

namespace ArkheideSystem.Gallery.Flourish.Blazor.Models;

public sealed record BusinessRecord(string Id, string Code, string Name, string Category,
    string Contact, string Email, string Status, DateOnly UpdatedOn);

public sealed class RecordDraft
{
    [Required(ErrorMessage = "Key.Validation_NameRequired")]
    [StringLength(120, ErrorMessage = "Key.Validation_NameLength")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Key.Validation_CategoryRequired")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Key.Validation_ContactRequired")]
    [StringLength(80, ErrorMessage = "Key.Validation_ContactLength")]
    public string Contact { get; set; } = string.Empty;

    [Required(ErrorMessage = "Key.Validation_EmailRequired")]
    [EmailAddress(ErrorMessage = "Key.Validation_EmailInvalid")]
    public string Email { get; set; } = string.Empty;

    public bool Active { get; set; } = true;
}
