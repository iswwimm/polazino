using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CasinoProject.Core;
using CasinoProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CasinoProject.ViewModels;

public partial class TysiacViewModel : ObservableObject
{
    [ObservableProperty] private int _betAmount = 10;
    [ObservableProperty] private string _statusMessage = "PLACE YOUR BET AND PLAY!";
    [ObservableProperty] private string _playerCardsText = string.Empty;
    [ObservableProperty] private string _dealerCardsText = string.Empty;
    [ObservableProperty] private int _playerScore = 0;
    [ObservableProperty] private int _dealerScore = 0;
    [ObservableProperty] private bool _isProcessing = false;

    [RelayCommand]
    private async Task PlayRoundAsync()
    {
        if (BetAmount <= 0) { StatusMessage = "BET MUST BE GREATER THAN 0!"; return; }

        if (!SessionManager.Instance.TryDeduct(BetAmount))
        {
            StatusMessage = "INSUFFICIENT FUNDS!";
            return;
        }

        IsProcessing = true;
        StatusMessage = "SHUFFLING AND DEALING...";

        await Task.Delay(1000);

        var deck = GenerateTysiacDeck();
        Shuffle(deck);

        var playerHand = deck.Take(6).ToList();
        var dealerHand = deck.Skip(6).Take(6).ToList();

        PlayerCardsText = string.Join(" | ", playerHand.Select(c => $"{c.Rank}{c.SuitIcon}"));
        DealerCardsText = string.Join(" | ", dealerHand.Select(c => $"{c.Rank}{c.SuitIcon}"));

        PlayerScore = CalculateScore(playerHand);
        DealerScore = CalculateScore(dealerHand);

        ResolveWinner();
        IsProcessing = false;
    }

    private void ResolveWinner()
    {
        if (PlayerScore > DealerScore)
        {
            int winnings = BetAmount * 2;
            SessionManager.Instance.AddWinnings(winnings);
            StatusMessage = $"WINNER! YOU WON ${winnings}!";
        }
        else if (PlayerScore == DealerScore)
        {
            SessionManager.Instance.AddWinnings(BetAmount);
            StatusMessage = $"DRAW! ${BetAmount} RETURNED.";
        }
        else
        {
            StatusMessage = $"DEALER WINS! TRY AGAIN.";
        }
    }

    private int CalculateScore(List<CasinoProject.Models.Card> hand)
    {
        int score = hand.Sum(c => c.Value);
        var marriages = new Dictionary<string, int> { 
            { "♠", 40 }, { "♣", 60 }, { "♦", 80 }, { "♥", 100 } 
        };

        foreach (var m in marriages)
        {
            if (hand.Any(c => c.SuitIcon == m.Key && c.Rank == "Q") && 
                hand.Any(c => c.SuitIcon == m.Key && c.Rank == "K"))
            {
                score += m.Value;
            }
        }
        return score;
    }

    private List<CasinoProject.Models.Card> GenerateTysiacDeck()
    {
        var suits = new[] { 
            new { Name="Pik", Icon="♠", Color="Black" },
            new { Name="Trefl", Icon="♣", Color="Black" },
            new { Name="Karo", Icon="♦", Color="Red" },
            new { Name="Kier", Icon="♥", Color="Red" }
        };
        var ranks = new Dictionary<string, int> {
            { "9", 0 }, { "J", 2 }, { "Q", 3 }, { "K", 4 }, { "10", 10 }, { "A", 11 }
        };

        var deck = new List<CasinoProject.Models.Card>();
        foreach (var s in suits)
        {
            foreach (var r in ranks)
            {
                var card = new CasinoProject.Models.Card(s.Name, r.Key, r.Value);

                deck.Add(card);
            }
        }

        return deck;
    }

    private void Shuffle<T>(List<T> list)
    {
        var rng = new Random();
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    [RelayCommand]
    private void GoBackToMenu() => WeakReferenceMessenger.Default.Send(new NavigationMessage("Menu"));
}