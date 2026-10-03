using ArkheideSystem.Gallery.Blazor.Models;

namespace ArkheideSystem.Gallery.Blazor.Services;

/// <summary>In-memory demonstration data belonging to one interactive server circuit.</summary>
public sealed class RecordStore
{
    private readonly List<BusinessRecord> records = CreateSamples();
    private int nextCode = 34;
    public IReadOnlyList<BusinessRecord> Records => records.ToArray();
    public BusinessRecord? Find(string id) => records.FirstOrDefault(record => record.Id == id);
    public bool Remove(string id) => records.RemoveAll(record => record.Id == id) > 0;

    public BusinessRecord Save(RecordDraft draft, string? id = null)
    {
        var existing = id is null ? null : Find(id);
        var record = new BusinessRecord(existing?.Id ?? Guid.NewGuid().ToString("N"),
            existing?.Code ?? $"DEMO-{nextCode++:000}", draft.Name.Trim(), draft.Category,
            draft.Contact.Trim(), draft.Email.Trim(), draft.Active ? "启用" : "停用",
            DateOnly.FromDateTime(DateTime.UtcNow));
        if (existing is not null) records[records.IndexOf(existing)] = record;
        else records.Add(record);
        return record;
    }

    private static List<BusinessRecord> CreateSamples()
    {
        var names = new[] { "演示组织", "演示北区仓库", "演示服务中心", "演示销售团队", "演示供应单位" };
        var categories = new[] { "客户", "供应商", "内部部门" };
        return Enumerable.Range(1, 33).Select(index => new BusinessRecord(
            index == 1 ? "sample" : $"demo-{index:000}", $"DEMO-{index:000}",
            index == 1 ? names[0] : $"{names[index % names.Length]} {index:00}",
            categories[index % categories.Length], $"演示联系人 {index:00}",
            $"sample{index:00}@example.com", index % 5 == 0 ? "停用" : "启用",
            new DateOnly(2026, 10, 1).AddDays(-(index % 12)))).ToList();
    }
}
