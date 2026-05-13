using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CasinoProject.Core;

namespace CasinoProject.ViewModels;

public partial class RouletteViewModel : ObservableObject
{
    [ObservableProperty] 
    private int _betAmount = 10;
    
    [ObservableProperty] 
    private int _selectedNumber = 0;
    
    [ObservableProperty] 
    private string _resultText = "PLACE YOUR BET AND SPIN!";
    
    [ObservableProperty] 
    private bool _isSpinning = false;
    
    [ObservableProperty] 
    private string _currentSpinNumber = "--";

    [RelayCommand]
    private async Task SpinAsync()
    {
        if (BetAmount <= 0)
        {
            ResultText = "Bet amount must be greater than zero!";
            return;
        }

        if (!SessionManager.Instance.TryDeduct(BetAmount))
        {
            ResultText = "INSUFFICIENT FUNDS! DEPOSIT FIRST.";
            return;
        }

        IsSpinning = true;
        ResultText = "SPINNING THE WHEEL... 🎡";
        
        for (int i = 0; i < 20; i++)
        {
            CurrentSpinNumber = Random.Shared.Next(0, 37).ToString();
            await Task.Delay(100);
        }
        
        int winningNumber = Random.Shared.Next(0, 37);
        CurrentSpinNumber = winningNumber.ToString();

        if (winningNumber == SelectedNumber)
        {
            int winnings = BetAmount * 36;
            SessionManager.Instance.AddWinnings(winnings);
            ResultText = $"WINNER! YOU WON ${winnings}!";
        }
        else
        {
            ResultText = $"LOSS! TRY AGAIN.";
        }

        IsSpinning = false;
    }

    [RelayCommand]
    private void GoBack()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Menu"));
    }
}