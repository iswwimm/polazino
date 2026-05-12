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
    private const double PaddleHeight = 20;
    private const double PaddleSpeed = 30;

    [ObservableProperty]
    private double _paddleWidth = 100;

    [ObservableProperty]
    private ObservableCollection<BallModel> _balls = new();

    private const double DefaultBallVelocityX = 10;
    private const double DefaultBallVelocityY = -10;

    [ObservableProperty]
    private ObservableCollection<Block> _blocks = new();

    [ObservableProperty]
    private ObservableCollection<Bonus> _bonuses = new();

    [ObservableProperty]
    private ObservableCollection<Projectile> _projectiles = new();

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

    private bool _hasLasers = false;
    private bool _hasStickyPaddle = false;

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
        PaddleWidth = 100;
        PaddleX = (CanvasWidth - PaddleWidth) / 2;
        
        _hasLasers = false;
        _hasStickyPaddle = false;

        Balls.Clear();
        var initialBall = new BallModel((CanvasWidth - 20) / 2, PaddleY - 20 - 5);
        initialBall.VelocityX = DefaultBallVelocityX;
        initialBall.VelocityY = DefaultBallVelocityY;
        Balls.Add(initialBall);

        Blocks.Clear();
        Bonuses.Clear();
        Projectiles.Clear();

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
        // Update Projectiles
        foreach (var proj in Projectiles.ToList())
        {
            if (!proj.IsVisible) continue;
            
            proj.Y -= 15; // Laser speed
            if (proj.Y < 0)
            {
                proj.IsVisible = false;
                Projectiles.Remove(proj);
                continue;
            }

            foreach (var block in Blocks.Where(b => b.IsVisible))
            {
                if (proj.X + proj.Width >= block.X && proj.X <= block.X + block.Width &&
                    proj.Y <= block.Y + block.Height && proj.Y + proj.Height >= block.Y)
                {
                    proj.IsVisible = false;
                    Projectiles.Remove(proj);
                    HitBlock(block);
                    break;
                }
            }
        }

        // Update Bonuses
        foreach (var bonus in Bonuses.ToList())
        {
            if (!bonus.IsVisible) continue;

            bonus.Y += 3; // Fall speed

            if (bonus.Y > CanvasHeight)
            {
                bonus.IsVisible = false;
                Bonuses.Remove(bonus);
                continue;
            }

            if (bonus.Y + bonus.Height >= PaddleY && bonus.Y <= PaddleY + PaddleHeight &&
                bonus.X + bonus.Width >= PaddleX && bonus.X <= PaddleX + PaddleWidth)
            {
                bonus.IsVisible = false;
                Bonuses.Remove(bonus);
                ApplyBonus(bonus.Type);
            }
        }

        // Move balls
        foreach (var ball in Balls.ToList())
        {
            if (ball.IsStuck)
            {
                ball.X = PaddleX + ball.RelativeX;
                ball.Y = PaddleY - ball.Size;
                continue;
            }

            ball.X += ball.VelocityX;
            ball.Y += ball.VelocityY;

            // Wall collisions
            if (ball.X <= 0 || ball.X + ball.Size >= CanvasWidth)
            {
                ball.VelocityX *= -1;
            }
            if (ball.Y <= 0)
            {
                ball.VelocityY *= -1;
            }

            // Paddle collision
            if (ball.Y + ball.Size >= PaddleY && ball.Y + ball.Size <= PaddleY + PaddleHeight &&
                ball.X + ball.Size >= PaddleX && ball.X <= PaddleX + PaddleWidth)
            {
                if (_hasStickyPaddle)
                {
                    ball.IsStuck = true;
                    ball.RelativeX = ball.X - PaddleX;
                    ball.Y = PaddleY - ball.Size;
                }
                else
                {
                    ball.VelocityY *= -1;
                    if (ball.VelocityY > 0) ball.VelocityY *= -1; // Force up
                    double hitPoint = (ball.X + ball.Size / 2) - (PaddleX + PaddleWidth / 2);
                    ball.VelocityX = hitPoint * 0.15;
                    ball.Y = PaddleY - ball.Size;
                }
            }

            // Block collisions
            foreach (var block in Blocks.Where(b => b.IsVisible))
            {
                if (ball.X + ball.Size >= block.X && ball.X <= block.X + block.Width &&
                    ball.Y + ball.Size >= block.Y && ball.Y <= block.Y + block.Height)
                {
                    HitBlock(block);
                    ball.VelocityY *= -1;
                    break; // Only hit one block per frame per ball
                }
            }
            
            if (ball.Y > CanvasHeight)
            {
                Balls.Remove(ball);
            }
        }

        // Check lose condition
        if (!Balls.Any())
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

    private Random _rng = new Random();

    private void HitBlock(Block block)
    {
        block.IsVisible = false;
        CurrentWinnings += (int)(BetAmount * 0.1);

        if (_rng.NextDouble() < 0.10) // 10% chance
        {
            Array values = Enum.GetValues(typeof(BonusType));
            BonusType randomBonus = (BonusType)values.GetValue(_rng.Next(values.Length))!;
            Bonuses.Add(new Bonus(block.X + block.Width / 2 - 15, block.Y + block.Height / 2 - 15, randomBonus));
        }
    }
    
    private void ApplyBonus(BonusType type)
    {
        switch (type)
        {
            case BonusType.Expand:
                PaddleWidth = Math.Min(PaddleWidth + 40, CanvasWidth);
                break;
            case BonusType.Slow:
                foreach(var ball in Balls)
                {
                    ball.VelocityX *= 0.7;
                    ball.VelocityY *= 0.7;
                }
                break;
            case BonusType.MultiBall:
                var newBalls = new System.Collections.Generic.List<BallModel>();
                foreach(var ball in Balls)
                {
                    var b1 = new BallModel(ball.X, ball.Y) { VelocityX = ball.VelocityX * -1.2, VelocityY = ball.VelocityY };
                    var b2 = new BallModel(ball.X, ball.Y) { VelocityX = ball.VelocityX, VelocityY = ball.VelocityY * -1.2 };
                    newBalls.Add(b1);
                    newBalls.Add(b2);
                }
                foreach(var nb in newBalls) Balls.Add(nb);
                break;
            case BonusType.ExtraCash:
                CurrentWinnings += BetAmount;
                break;
            case BonusType.StickyPaddle:
                _hasStickyPaddle = true;
                break;
            case BonusType.Laser:
                _hasLasers = true;
                break;
        }
    }

    public void TriggerAction()
    {
        if (!IsGameRunning) return;

        bool releasedAny = false;
        foreach (var ball in Balls.Where(b => b.IsStuck))
        {
            ball.IsStuck = false;
            // Launch up
            ball.VelocityY = -Math.Abs(ball.VelocityY);
            if(ball.VelocityY == 0) ball.VelocityY = DefaultBallVelocityY;
            releasedAny = true;
        }

        if (releasedAny)
        {
            _hasStickyPaddle = false; // Optional: lose sticky after release
            return; 
        }

        if (_hasLasers)
        {
            Projectiles.Add(new Projectile(PaddleX + 10, PaddleY));
            Projectiles.Add(new Projectile(PaddleX + PaddleWidth - 10 - 4, PaddleY));
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
            double oldX = PaddleX;
            PaddleX = Math.Max(0, PaddleX - PaddleSpeed);
            double delta = PaddleX - oldX;
            
            foreach(var ball in Balls.Where(b => b.IsStuck))
            {
                ball.X += delta;
            }
        }
    }

    public void MovePaddleRight()
    {
        if (IsGameRunning)
        {
            double oldX = PaddleX;
            PaddleX = Math.Min(CanvasWidth - PaddleWidth, PaddleX + PaddleSpeed);
            double delta = PaddleX - oldX;

            foreach(var ball in Balls.Where(b => b.IsStuck))
            {
                ball.X += delta;
            }
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
