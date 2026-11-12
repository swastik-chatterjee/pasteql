using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Ganss.Xss;
using Markdig;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PasteQL.Data;
using PasteQL.GraphQL;

namespace PasteQL.Services;

public sealed record PasteCreateRequest(
    string Title,
    string Content,
    string Language,
    string? Password,
    bool BurnAfterRead = false,
    int ExpiresInMinutes = 1440,
    int MaxViews = 0);

public sealed record PasteReadResult(
    string Id,
    string Title,
    string Content,
    string RenderedHtml,
    string Language,
    bool BurnAfterRead,
    bool IsPasswordProtected,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    int ViewCount);

public class PasteService
{
    private static readonly HtmlSanitizer Sanitizer = new();
    private readonly PasteDbContext _db;
    private readonly IDataProtector _protector;
    private readonly PasswordHasher<string> _passwordHasher = new();

    public PasteService(PasteDbContext db, IDataProtectionProvider protectionProvider)
    {
        _db = db;
        _protector = protectionProvider.CreateProtector("pasteql.content");
    }

    public async Task<PasteRecord> CreateAsync(PasteCreateRequest request)
    {
        var title = string.IsNullOrWhiteSpace(request.Title) ? "Untitled Paste" : request.Title.Trim();
        var content = request.Content ?? string.Empty;
        var language = string.IsNullOrWhiteSpace(request.Language) ? "text" : request.Language.Trim();
        var expiresMinutes = request.ExpiresInMinutes <= 0 ? 1440 : request.ExpiresInMinutes;

        var record = new PasteRecord
        {
            Slug = await GenerateUniqueSlugAsync(),
            Title = title,
            Language = language,
            ContentCipherText = _protector.Protect(content),
            PasswordHash = string.IsNullOrWhiteSpace(request.Password) ? null : _passwordHasher.HashPassword("pasteql", request.Password),
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(expiresMinutes),
            BurnAfterRead = request.BurnAfterRead,
            MaxViews = request.MaxViews > 0 ? request.MaxViews : 0
        };

        _db.Pastes.Add(record);
        await _db.SaveChangesAsync();

        return record;
    }

    public async Task<PasteReadResult?> GetByIdAsync(string id, string? password)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        var record = await _db.Pastes
            .FirstOrDefaultAsync(x => x.Slug == id);

        if (record is null)
        {
            return null;
        }

        if (record.IsDeleted || record.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            record.IsDeleted = true;
            await _db.SaveChangesAsync();
            return null;
        }

        if (!string.IsNullOrWhiteSpace(record.PasswordHash))
        {
            var verification = _passwordHasher.VerifyHashedPassword("pasteql", record.PasswordHash, password ?? string.Empty);
            if (verification == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("This paste is protected by a password.");
            }
        }

        var content = _protector.Unprotect(record.ContentCipherText);
        record.ViewCount++;

        if (record.BurnAfterRead || (record.MaxViews > 0 && record.ViewCount >= record.MaxViews))
        {
            record.IsDeleted = true;
        }

        await _db.SaveChangesAsync();

        return new PasteReadResult(
            record.Slug,
            record.Title,
            content,
            RenderMarkdown(record.ContentCipherText == string.Empty ? string.Empty : content, record.Language),
            record.Language,
            record.BurnAfterRead,
            !string.IsNullOrWhiteSpace(record.PasswordHash),
            record.CreatedAtUtc,
            record.ExpiresAtUtc,
            record.ViewCount);
    }

    public async Task<List<PasteSummary>> GetRecentAsync(int limit = 10)
    {
        var items = await _db.Pastes
            .Where(x => !x.IsDeleted && x.ExpiresAtUtc > DateTimeOffset.UtcNow)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(Math.Max(1, limit))
            .Select(x => new PasteSummary(
                x.Slug,
                x.Title,
                x.Language,
                x.CreatedAtUtc,
                !string.IsNullOrWhiteSpace(x.PasswordHash),
                x.BurnAfterRead))
            .ToListAsync();

        return items;
    }

    private async Task<string> GenerateUniqueSlugAsync()
    {
        for (var attempt = 0; attempt < 25; attempt++)
        {
            var slug = RandomString(10);
            var exists = await _db.Pastes.AnyAsync(x => x.Slug == slug);
            if (!exists)
            {
                return slug;
            }
        }

        return Guid.NewGuid().ToString("N")[..12];
    }

    private static string RandomString(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var builder = new StringBuilder();
        var random = Random.Shared;

        for (var i = 0; i < length; i++)
        {
            builder.Append(chars[random.Next(chars.Length)]);
        }

        return builder.ToString();
    }

    private static string RenderMarkdown(string content, string language)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var markdownWithHighlighting = HighlightCodeBlocks(content, language);
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseSoftlineBreakAsHardlineBreak()
            .Build();

        var html = Markdown.ToHtml(markdownWithHighlighting, pipeline);
        return Sanitizer.Sanitize(html);
    }

    private static string HighlightCodeBlocks(string markdown, string language)
    {
        var pattern = new Regex(@"```(?<lang>[A-Za-z0-9_+-]+)?\r?\n(?<code>[\s\S]*?)```", RegexOptions.Compiled);
        var matches = pattern.Matches(markdown);

        if (matches.Count == 0)
        {
            return markdown;
        }

        var result = markdown;
        foreach (Match match in matches.Cast<Match>().Reverse())
        {
            var lang = match.Groups["lang"].Success ? match.Groups["lang"].Value.Trim() : language;
            var code = match.Groups["code"].Value;
            var highlighted = HighlightSource(code, lang);
            var replacement = string.Concat(
                "<pre class=\"code-block\"><code class=\"language-",
                lang,
                "\">",
                highlighted,
                "</code></pre>");
            result = result.Remove(match.Index, match.Length).Insert(match.Index, replacement);
        }

        return result;
    }

    private static string HighlightSource(string source, string language)
    {
        var escaped = HtmlEncoder.Default.Encode(source);
        if (string.IsNullOrWhiteSpace(language))
        {
            return escaped;
        }

        return language switch
        {
            "csharp" or "cs" => ApplyKeywordStyle(escaped, new[] { "class", "public", "private", "protected", "static", "if", "else", "using", "namespace", "return", "var", "new", "await", "async", "true", "false", "string", "int", "bool", "void", "sealed", "override", "readonly" }),
            "json" => ApplyKeywordStyle(escaped, new[] { "\"[A-Za-z0-9_-]+\"", "true", "false", "null" }),
            "sql" => ApplyKeywordStyle(escaped, new[] { "SELECT", "FROM", "WHERE", "INSERT", "UPDATE", "DELETE", "JOIN", "LEFT", "RIGHT", "INNER", "OUTER", "AS", "ON", "VALUES" }),
            _ => escaped
        };
    }

    private static string ApplyKeywordStyle(string content, IEnumerable<string> keywords)
    {
        var result = content;
        foreach (var keyword in keywords)
        {
            result = Regex.Replace(
                result,
                $"(?<![A-Za-z0-9_]){Regex.Escape(keyword)}(?![A-Za-z0-9_])",
                match => string.Concat("<span class=\"token keyword\">", match.Value, "</span>"),
                RegexOptions.IgnoreCase);
        }
        return result;
    }
}
