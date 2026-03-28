namespace Client.Models;

public class MenuItemModel
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsIntegrated { get; set; }
    public List<MenuItemModel> Children { get; set; } = [];
    public bool HasChildren => Children.Count > 0;
}
