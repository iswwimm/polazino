using CommunityToolkit.Mvvm.ComponentModel;

namespace CasinoProject.Models;

public partial class Projectile : ObservableObject
{
    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    public double Width { get; } = 4;
    public double Height { get; } = 15;
    
    [ObservableProperty]
    private bool _isVisible = true;

    public Projectile(double x, double y)
    {
        _x = x;
        _y = y;
    }
}