using CommunityToolkit.Mvvm.ComponentModel;

namespace CasinoProject.Models;

public partial class Block : ObservableObject
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    [ObservableProperty]
    private string _color;

    [ObservableProperty]
    private bool _isVisible = true;

    public Block(double x, double y, double width, double height, string color)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Color = color;
    }
}
