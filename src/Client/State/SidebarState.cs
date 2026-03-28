namespace Client.State;

public class SidebarState
{
    public bool IsOpen { get; private set; } = true;
    public bool IsMobileOpen { get; private set; }
    public event Action? OnChange;

    public void Toggle()
    {
        IsOpen = !IsOpen;
        NotifyStateChanged();
    }

    public void ToggleMobile()
    {
        IsMobileOpen = !IsMobileOpen;
        NotifyStateChanged();
    }

    public void OpenMobile()
    {
        if (IsMobileOpen)
        {
            return;
        }

        IsMobileOpen = true;
        NotifyStateChanged();
    }

    public void CloseMobile()
    {
        if (!IsMobileOpen)
        {
            return;
        }

        IsMobileOpen = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
