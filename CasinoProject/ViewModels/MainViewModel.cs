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
    
    public void Receive(NavigationMessage message)
    {
        if (message.TargetView == "TopUp")
        {
            CurrentView = new TopUpViewModel();
        }
        else if (message.TargetView == "Tysiac")
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
    }
}