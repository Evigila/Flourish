using System.ComponentModel.DataAnnotations;

namespace ArkheideSystem.Gallery.Flourish.Blazor.Models;

public sealed record BusinessRecord(string Id, string Code, string Name, string Category,
    string Contact, string Email, string Status, DateOnly UpdatedOn);

public sealed class RecordDraft
{
    [Required(ErrorMessage = "请输入名称。")]
    [StringLength(120, ErrorMessage = "名称不得超过 120 个字符。")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "请选择分类。")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "请输入联系人。")]
    [StringLength(80, ErrorMessage = "联系人不得超过 80 个字符。")]
    public string Contact { get; set; } = string.Empty;

    [Required(ErrorMessage = "请输入邮箱。")]
    [EmailAddress(ErrorMessage = "请输入有效的邮箱地址。")]
    public string Email { get; set; } = string.Empty;

    public bool Active { get; set; } = true;
}
