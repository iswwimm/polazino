namespace CasinoProject.Core;

public class NavigationMessage
{
    public string TargetView { get; }
    public NavigationMessage(string targetView)
    {
        TargetView = targetView;
    }
}