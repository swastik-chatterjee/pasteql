namespace PasteQL.Data;

public class PasteRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = "Untitled Paste";
    public string Language { get; set; } = "text";
    public string ContentCipherText { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAtUtc { get; set; } = DateTimeOffset.UtcNow.AddDays(7);
    public bool BurnAfterRead { get; set; }
    public bool IsDeleted { get; set; }
    public int ViewCount { get; set; }
    public int MaxViews { get; set; }
}
