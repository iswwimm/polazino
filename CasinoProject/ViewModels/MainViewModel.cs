using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CasinoProject.Core;

namespace CasinoProject.ViewModels;

public partial class MainViewModel : ObservableObject, IRecipient<NavigationMessage>
{
    [ObservableProperty]
    private object _currentView;

    public MainViewModel()
    {
        WeakReferenceMessenger.Default.Register(this);
        
        CurrentView = new MenuViewModel(); 
    }

    [RelayCommand]
    private void GoToTopUp()
    {
        CurrentView = new TopUpViewModel();
    }

    [RelayCommand]
    private void GoToWithdraw()
    {
        CurrentView = new WithdrawViewModel();
    }
    
    [RelayCommand]
    private void GoToMenu()
    {
        CurrentView = new MenuViewModel();
    }
    [RelayCommand]
    private void PlayThousand()
    {
        CurrentView = new TysiacViewModel();
    }
    public void Receive(NavigationMessage message)
    {
        if (message.TargetView == "TopUp")
        {
            CurrentView = new TopUpViewModel();
        }
        else if (message.TargetView == "Thousand")
        {
            CurrentView = new TysiacViewModel();
        }
        else if (message.TargetView == "Withdraw")
        {
            CurrentView = new WithdrawViewModel();
        }
        else if (message.TargetView == "Menu")
        {
            CurrentView = new MenuViewModel();
        }
        else if (message.TargetView == "Blackjack")
        {
            CurrentView = new BlackjackViewModel();
        }
        else if (message.TargetView == "Arkanoid")
        {
            CurrentView = new ArkanoidViewModel();
        }
        else if (message.TargetView == "Roulette")
        {
            CurrentView = new RouletteViewModel();
        }
    }
}