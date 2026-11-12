using System.ComponentModel.DataAnnotations;

namespace PasteQL.Models;

public class PasteCreateViewModel
{
    [Display(Name = "Title")]
    [MaxLength(200)]
    public string Title { get; set; } = "Untitled Paste";

    [Required]
    [Display(Name = "Content")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Language")]
    public string Language { get; set; } = "text";

    [Display(Name = "Password")]
    public string? Password { get; set; }

    [Display(Name = "Burn after read")]
    public bool BurnAfterRead { get; set; }

    [Range(1, 525600)]
    [Display(Name = "Expiry (minutes)")]
    public int ExpiresInMinutes { get; set; } = 1440;

    [Range(0, 1000)]
    [Display(Name = "Max views")]
    public int MaxViews { get; set; }
}
