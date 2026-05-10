using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CasinoProject.Core;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CasinoProject.ViewModels;

public partial class TopUpViewModel : ObservableObject
{
    public ObservableCollection<string> PaymentMethods { get; } = new()
    {
        "BLIK",
        "Credit Card",
        "Cryptocurrency (BTC/ETH)"
    };

    [ObservableProperty]
    private string _selectedMethod = "BLIK";

    [ObservableProperty]
    private int? _amount = 50;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isProcessing = false;
    

    [ObservableProperty]
    private string _blikCode = string.Empty;

    [ObservableProperty]
    private string _cardNumber = string.Empty;

    [ObservableProperty]
    private string _cardExpiry = string.Empty;

    [ObservableProperty]
    private string _cardCvv = string.Empty;

    [ObservableProperty]
    private string _cryptoAddress = string.Empty;
    
    public bool IsBlikVisible => SelectedMethod == "BLIK";
    public bool IsCardVisible => SelectedMethod == "Credit Card";
    public bool IsCryptoVisible => SelectedMethod == "Cryptocurrency (BTC/ETH)";
    
    partial void OnSelectedMethodChanged(string value)
    {
        OnPropertyChanged(nameof(IsBlikVisible));
        OnPropertyChanged(nameof(IsCardVisible));
        OnPropertyChanged(nameof(IsCryptoVisible));

        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private async Task ProcessPaymentAsync()
    {
        if (Amount == null || Amount <= 0)
        {
            StatusMessage = "Please enter a valid deposit amount!";
            return;
        }
        
        if (SelectedMethod == "BLIK" && (string.IsNullOrWhiteSpace(BlikCode) || BlikCode.Length != 6))
        {
            StatusMessage = "Please enter a valid 6-digit BLIK code!";
            return;
        }
        if (SelectedMethod == "Credit Card" && 
           (string.IsNullOrWhiteSpace(CardNumber) || string.IsNullOrWhiteSpace(CardExpiry) || string.IsNullOrWhiteSpace(CardCvv)))
        {
            StatusMessage = "Please fill in all credit card details!";
            return;
        }
        if (SelectedMethod == "Cryptocurrency (BTC/ETH)" && string.IsNullOrWhiteSpace(CryptoAddress))
        {
            StatusMessage = "Please enter your crypto wallet address!";
            return;
        }

        IsProcessing = true;
        StatusMessage = "Processing payment. Please wait...";

        await Task.Delay(2000);

        SessionManager.Instance.AddWinnings(Amount.Value);

        StatusMessage = $"Success! Added ${Amount} to your account.";
        
        Amount = 0;
        BlikCode = string.Empty;
        CardNumber = string.Empty;
        CardExpiry = string.Empty;
        CardCvv = string.Empty;
        CryptoAddress = string.Empty;
        IsProcessing = false;
    }

    [RelayCommand]
    private void GoBackToMenu()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Menu"));
    }
}