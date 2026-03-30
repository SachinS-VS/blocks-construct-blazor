namespace Client.Models.Language;

public class LanguageItem
{
    public string ItemId { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string ProjectKey { get; set; } = string.Empty;
}

public class LanguageModule
{
    public string ItemId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
