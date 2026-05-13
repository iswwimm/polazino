using CommunityToolkit.Mvvm.ComponentModel;

namespace CasinoProject.Models;

public partial class BallModel : ObservableObject
{
    [ObservableProperty]
    private double _x;
    
    [ObservableProperty]
    private double _y;
    
    public double Size { get; set; } = 20;

    public double VelocityX { get; set; } = 10;
    public double VelocityY { get; set; } = -10;

    // For sticky paddle
    public bool IsStuck { get; set; }
    public double RelativeX { get; set; }

    public BallModel(double x, double y)
    {
        _x = x;
        _y = y;
    }
}