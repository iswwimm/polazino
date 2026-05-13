using System.Collections.ObjectModel;
using System.Linq;
using CasinoProject.Models;
using CasinoProject.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CasinoProject.ViewModels;

public partial class BlackjackViewModel : ObservableObject
{
    private Deck _deck = new Deck();

    [ObservableProperty]
    private string _gameStatus = string.Empty;

    [ObservableProperty]
    private int _playerScore;

    [ObservableProperty]
    private int _dealerScore;

    [ObservableProperty]
    private bool _isGameActive;

    [ObservableProperty]
    private int? _betAmount = 10;

    public ObservableCollection<Card> PlayerCards { get; } = new();
    public ObservableCollection<Card> DealerCards { get; } = new();

    public BlackjackViewModel()
    {
        GameStatus = "PLACE YOUR BET AND START!";
    }

    [RelayCommand]
    private void StartNewGame()
    {
        if ( BetAmount == null || BetAmount <= 0)
        {
            GameStatus = "BET MUST BE GREATER THAN 0!";
            return;
        }

        if (SessionManager.Instance.TryDeduct(BetAmount.Value))
        {
            _deck = new Deck();
            PlayerCards.Clear();
            DealerCards.Clear();
            IsGameActive = true;
            GameStatus = "YOUR TURN! HIT OR STAND?";

            PlayerCards.Add(_deck.Draw());
            PlayerCards.Add(_deck.Draw());
            DealerCards.Add(_deck.Draw()); 

            UpdateScores();
        }
        else
        {
            GameStatus = "INSUFFICIENT FUNDS! DEPOSIT FIRST.";
        }
    }

    [RelayCommand]
    private void Hit()
    {
        if (!IsGameActive) return;

        PlayerCards.Add(_deck.Draw());
        UpdateScores();

        if (PlayerScore > 21)
        {
            EndGame("BUST! YOU LOSE.");
        }
    }

    [RelayCommand]
    private void Stand()
    {
        if (!IsGameActive) return;
        
        while (CalculateHandScore(DealerCards) < 17)
        {
            DealerCards.Add(_deck.Draw());
        }

        UpdateScores();
        DetermineWinner();
    }

    [RelayCommand]
    private void GoBackToMenu()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Menu"));
    }

    private void UpdateScores()
    {
        PlayerScore = CalculateHandScore(PlayerCards);
        DealerScore = CalculateHandScore(DealerCards);
    }

    private int CalculateHandScore(ObservableCollection<Card> hand)
    {
        int score = hand.Sum(c => c.Value);
        int aceCount = hand.Count(c => c.Rank == "A");
        
        while (score > 21 && aceCount > 0)
        {
            score -= 10;
            aceCount--;
        }
        return score;
    }

    private void DetermineWinner()
    {
        if (DealerScore > 21)
        {
            EndGame("DEALER BUSTS! YOU WIN!");
            SessionManager.Instance.AddWinnings(BetAmount.Value * 2);
        }
        else if (PlayerScore > DealerScore)
        {
            EndGame("YOU WIN!");
            SessionManager.Instance.AddWinnings(BetAmount.Value * 2);
        }
        else if (DealerScore > PlayerScore)
        {
            EndGame("DEALER WINS.");
        }
        else
        {
            EndGame("PUSH! BET RETURNED.");
            SessionManager.Instance.AddWinnings(BetAmount.Value);
        }
    }

    private void EndGame(string message)
    {
        GameStatus = message;
        IsGameActive = false;
    }
}