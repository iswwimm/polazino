using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CasinoProject.Core;

namespace CasinoProject.ViewModels;

public partial class MenuViewModel : ObservableObject
{
    [RelayCommand]
    private void PlaySlots()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Slots"));
    }
    
    [RelayCommand]
    private void PlayRoulette()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Roulette"));
    }
    
    [RelayCommand]
    private void PlayBlackjack()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Blackjack"));
    }

    [RelayCommand]
    private void PlayTysiac()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Tysiac"));
    }

    [RelayCommand]
    private void GoToTopUp()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("TopUp"));
    }
    
    [RelayCommand]
    private void GoToWithdraw()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Withdraw"));
    }
}