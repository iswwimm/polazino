using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CasinoProject.Core;
using CasinoProject.Models;

namespace CasinoProject.ViewModels;

public partial class ArkanoidViewModel : ObservableObject, IDisposable
{
    // Game area dimensions
    private const double CanvasWidth = 800;
    private const double CanvasHeight = 600;

    [ObservableProperty]
    private double _paddleX = 350;
    private const double PaddleY = 550;
    private const double PaddleWidth = 100;
    private const double PaddleHeight = 20;
    private const double PaddleSpeed = 30;

    [ObservableProperty]
    private double _ballX = 390;
    [ObservableProperty]
    private double _ballY = 530;
    private const double BallSize = 20;
    private double _ballVelocityX = 5;
    private double _ballVelocityY = -5;

    [ObservableProperty]
    private ObservableCollection<Block> _blocks = new();

    [ObservableProperty]
    private int _betAmount = 100;

    [ObservableProperty]
    private int _currentWinnings = 0;

    [ObservableProperty]
    private bool _isGameRunning = false;

    [ObservableProperty]
    private string _gameStatusMessage = "Place a bet to play!";

    [ObservableProperty]
    private bool _showGameStatus = true;

    public int SessionBalance => SessionManager.Instance.Balance;

    private DispatcherTimer _gameTimer;

    private void OnSessionManagerPropertyChanged(object? s, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SessionManager.Instance.Balance))
        {
            OnPropertyChanged(nameof(SessionBalance));
        }
    }

    public ArkanoidViewModel()
    {
        _gameTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _gameTimer.Tick += GameLoop;

        SessionManager.Instance.PropertyChanged += OnSessionManagerPropertyChanged;
    }

    public void Dispose()
    {
        SessionManager.Instance.PropertyChanged -= OnSessionManagerPropertyChanged;
        _gameTimer?.Stop();
    }

    [RelayCommand]
    private void StartGame()
    {
        if (IsGameRunning) return;

        if (BetAmount <= 0)
        {
            GameStatusMessage = "Bet must be greater than 0!";
            ShowGameStatus = true;
            return;
        }

        if (SessionManager.Instance.TryDeduct(BetAmount))
        {
            InitializeLevel();
            IsGameRunning = true;
            ShowGameStatus = false;
            CurrentWinnings = 0;
            _gameTimer.Start();
        }
        else
        {
            GameStatusMessage = "Not enough funds!";
            ShowGameStatus = true;
        }
    }

    private void InitializeLevel()
    {
        PaddleX = (CanvasWidth - PaddleWidth) / 2;
        BallX = (CanvasWidth - BallSize) / 2;
        BallY = PaddleY - BallSize - 5;

        _ballVelocityX = 5;
        _ballVelocityY = -5;

        Blocks.Clear();

        // Create some blocks
        int rows = 5;
        int cols = 10;
        double blockWidth = 70;
        double blockHeight = 30;
        double padding = 10;
        double startX = (CanvasWidth - (cols * blockWidth + (cols - 1) * padding)) / 2;
        double startY = 50;

        string[] colors = { "#ff007f", "#11998e", "#f5c518", "#38ef7d", "#3a1c71" };

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Blocks.Add(new Block(
                    startX + col * (blockWidth + padding),
                    startY + row * (blockHeight + padding),
                    blockWidth,
                    blockHeight,
                    colors[row % colors.Length]
                ));
            }
        }
    }

    private void GameLoop(object? sender, EventArgs e)
    {
        // Move ball
        BallX += _ballVelocityX;
        BallY += _ballVelocityY;

        // Wall collisions
        if (BallX <= 0 || BallX + BallSize >= CanvasWidth)
        {
            _ballVelocityX *= -1;
        }
        if (BallY <= 0)
        {
            _ballVelocityY *= -1;
        }

        // Paddle collision
        if (BallY + BallSize >= PaddleY && BallY + BallSize <= PaddleY + PaddleHeight &&
            BallX + BallSize >= PaddleX && BallX <= PaddleX + PaddleWidth)
        {
            // Simple bounce, adjust based on hit position
            _ballVelocityY *= -1;

            // Adjust X velocity based on where it hit the paddle
            double hitPoint = (BallX + BallSize / 2) - (PaddleX + PaddleWidth / 2);
            _ballVelocityX = hitPoint * 0.15;

            // Ensure ball is just above paddle to prevent getting stuck
            BallY = PaddleY - BallSize;
        }

        // Block collisions
        bool blockHit = false;
        foreach (var block in Blocks.Where(b => b.IsVisible))
        {
            if (BallX + BallSize >= block.X && BallX <= block.X + block.Width &&
                BallY + BallSize >= block.Y && BallY <= block.Y + block.Height)
            {
                block.IsVisible = false;
                blockHit = true;

                // Calculate winnings per block: BetAmount * 0.1 (10% of bet per block)
                CurrentWinnings += (int)(BetAmount * 0.1);

                // Simple collision response - invert Y
                _ballVelocityY *= -1;
                break; // Only hit one block per frame
            }
        }

        // Check lose condition (ball falls below paddle)
        if (BallY > CanvasHeight)
        {
            EndGame(false);
            return;
        }

        // Check win condition (all blocks destroyed)
        if (!Blocks.Any(b => b.IsVisible))
        {
            EndGame(true);
        }
    }

    private void EndGame(bool isWin)
    {
        _gameTimer.Stop();
        IsGameRunning = false;

        SessionManager.Instance.AddWinnings(CurrentWinnings);

        if (isWin)
        {
            GameStatusMessage = $"You Won! Earned: {CurrentWinnings}$";
        }
        else
        {
            GameStatusMessage = $"Game Over! Earned: {CurrentWinnings}$";
        }

        ShowGameStatus = true;
    }

    public void MovePaddleLeft()
    {
        if (IsGameRunning)
        {
            PaddleX = Math.Max(0, PaddleX - PaddleSpeed);
        }
    }

    public void MovePaddleRight()
    {
        if (IsGameRunning)
        {
            PaddleX = Math.Min(CanvasWidth - PaddleWidth, PaddleX + PaddleSpeed);
        }
    }

    [RelayCommand]
    private void ReturnToMenu()
    {
        if (IsGameRunning)
        {
            _gameTimer.Stop();
            // Optional: If you leave during game, give current winnings?
            // Requirements didn't specify leaving early, let's just add winnings
            SessionManager.Instance.AddWinnings(CurrentWinnings);
        }
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Menu"));
    }
}
