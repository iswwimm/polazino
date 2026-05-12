using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CasinoProject.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CasinoProject.ViewModels;

public partial class TysiacViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _betAmount = 10;

    [ObservableProperty]
    private string _statusMessage = "Postaw zakład i kliknij Graj!";

    [ObservableProperty]
    private string _playerCardsText = string.Empty;

    [ObservableProperty]
    private string _dealerCardsText = string.Empty;

    [ObservableProperty]
    private int _playerScore = 0;

    [ObservableProperty]
    private int _dealerScore = 0;

    [ObservableProperty]
    private bool _isProcessing = false;

    public int CurrentBalance => SessionManager.Instance.Balance;

    [RelayCommand]
    private async Task PlayRoundAsync()
    {
        if (BetAmount <= 0)
        {
            StatusMessage = "Stawka musi być większa od 0!";
            return;
        }

        if (!SessionManager.Instance.TryDeduct(BetAmount))
        {
            StatusMessage = "Brak wystarczających środków na koncie!";
            return;
        }

        IsProcessing = true;
        StatusMessage = "Tasowanie i rozdawanie kart...";
        OnPropertyChanged(nameof(CurrentBalance));

        var deck = GenerateDeck();
        Shuffle(deck);

        var playerHand = deck.Take(6).ToList();
        var dealerHand = deck.Skip(6).Take(6).ToList();

        PlayerCardsText = string.Join(" | ", playerHand.Select(c => c.ToString()));
        DealerCardsText = string.Join(" | ", dealerHand.Select(c => c.ToString()));

        PlayerScore = CalculateScore(playerHand);
        DealerScore = CalculateScore(dealerHand);

        ResolveWinner();

        IsProcessing = false;
        OnPropertyChanged(nameof(CurrentBalance));
    }

    private void ResolveWinner()
    {
        if (PlayerScore > DealerScore)
        {
            int winnings = BetAmount * 2;
            SessionManager.Instance.AddWinnings(winnings);
            StatusMessage = $"Wygrałeś! Zgarniasz {winnings}$. (Wynik: {PlayerScore} do {DealerScore})";
        }
        else if (PlayerScore == DealerScore)
        {
            SessionManager.Instance.AddWinnings(BetAmount);
            StatusMessage = $"Remis! Stawka {BetAmount}$ wraca na konto. (Wynik: {PlayerScore} do {DealerScore})";
        }
        else
        {
            StatusMessage = $"Przegrałeś. Krupier miał więcej punktów. (Wynik: {PlayerScore} do {DealerScore})";
        }
    }

    private int CalculateScore(List<Card> hand)
    {
        int score = hand.Sum(c => c.Value);
        var suits = new[] { "Pik", "Trefl", "Karo", "Kier" };
        var marriageValues = new Dictionary<string, int>
        {
            { "Pik", 40 },
            { "Trefl", 60 },
            { "Karo", 80 },
            { "Kier", 100 }
        };

        foreach (var suit in suits)
        {
            bool hasQueen = hand.Any(c => c.Suit == suit && c.Rank == "Q");
            bool hasKing = hand.Any(c => c.Suit == suit && c.Rank == "K");

            if (hasQueen && hasKing)
            {
                score += marriageValues[suit];
            }
        }

        return score;
    }

    private List<Card> GenerateDeck()
    {
        var suits = new[] { "Pik", "Trefl", "Karo", "Kier" };
        var ranks = new Dictionary<string, int>
        {
            { "9", 0 }, { "J", 2 }, { "Q", 3 }, { "K", 4 }, { "10", 10 }, { "A", 11 }
        };

        var deck = new List<Card>();
        foreach (var suit in suits)
        {
            foreach (var rank in ranks)
            {
                deck.Add(new Card { Suit = suit, Rank = rank.Key, Value = rank.Value });
            }
        }
        return deck;
    }

    private void Shuffle(List<Card> deck)
    {
        var rng = new Random();
        int n = deck.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (deck[k], deck[n]) = (deck[n], deck[k]);
        }
    }

    [RelayCommand]
    private void GoBackToMenu()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Menu"));
    }
}
public class Card
{
    public string Suit { get; set; } = string.Empty;
    public string Rank { get; set; } = string.Empty;
    public int Value { get; set; }

    public override string ToString() => $"{Rank} {Suit}";
}