using CommunityToolkit.Mvvm.ComponentModel;

namespace CasinoProject.Models;

public enum BonusType
{
    Expand,
    Slow,
    MultiBall,
    ExtraCash,
    StickyPaddle,
    Laser
}

public partial class Bonus : ObservableObject
{
    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;
    
    public double Width { get; } = 30;
    public double Height { get; } = 30;

    public BonusType Type { get; }

    [ObservableProperty]
    private string _color; // e.g., representing the type

    [ObservableProperty]
    private bool _isVisible = true;

    [ObservableProperty]
    private string _symbol;

    public Bonus(double x, double y, BonusType type)
    {
        _x = x;
        _y = y;
        Type = type;

        switch (type)
        {
            case BonusType.Expand: _color = "#3498db"; _symbol = "↔"; break;
            case BonusType.Slow: _color = "#2ecc71"; _symbol = "S"; break;
            case BonusType.MultiBall: _color = "#9b59b6"; _symbol = "M"; break;
            case BonusType.ExtraCash: _color = "#f1c40f"; _symbol = "$"; break;
            case BonusType.StickyPaddle: _color = "#e67e22"; _symbol = "="; break;
            case BonusType.Laser: _color = "#e74c3c"; _symbol = "⚡"; break;
            default: _color = "#ffffff"; _symbol = "?"; break;
        }
    }
}