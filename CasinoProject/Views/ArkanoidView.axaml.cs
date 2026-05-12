using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CasinoProject.ViewModels;
using Avalonia.Threading;
using System;

namespace CasinoProject.Views;

public partial class ArkanoidView : UserControl
{
    private DispatcherTimer _paddleTimer;
    private bool _moveLeft;
    private bool _moveRight;

    public ArkanoidView()
    {
        InitializeComponent();
        _paddleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _paddleTimer.Tick += (s, e) =>
        {
            if (DataContext is ArkanoidViewModel vm)
            {
                if (_moveLeft) vm.MovePaddleLeft();
                if (_moveRight) vm.MovePaddleRight();
            }
        };
        _paddleTimer.Start();
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // Focus the control to receive key events
        this.Focus();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Left) _moveLeft = true;
        if (e.Key == Key.Right) _moveRight = true;
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Left) _moveLeft = false;
        if (e.Key == Key.Right) _moveRight = false;
    }
}
