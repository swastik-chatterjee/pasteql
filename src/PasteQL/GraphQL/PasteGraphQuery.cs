using PasteQL.Services;

namespace PasteQL.GraphQL;

public sealed record PasteSummary(
    string Id,
    string Title,
    string Language,
    DateTimeOffset CreatedAtUtc,
    bool IsPasswordProtected,
    bool BurnAfterRead);

public sealed record PasteDetails(
    string Id,
    string Title,
    string Content,
    string Language,
    bool IsPasswordProtected,
    bool BurnAfterRead,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    int ViewCount);

public sealed class PasteGraphQuery
{
    public async Task<PasteDetails?> Paste(string id, string? password, [Service] PasteService service)
    {
        var paste = await service.GetByIdAsync(id, password);
        if (paste is null)
        {
            return null;
        }

        return new PasteDetails(
            paste.Id,
            paste.Title,
            paste.Content,
            paste.Language,
            paste.IsPasswordProtected,
            paste.BurnAfterRead,
            paste.CreatedAtUtc,
            paste.ExpiresAtUtc,
            paste.ViewCount);
    }

    public async Task<List<PasteSummary>> RecentPastes(int limit = 10, [Service] PasteService service = null!)
    {
        return await service.GetRecentAsync(limit);
    }
}
