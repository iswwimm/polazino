using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CasinoProject.Core;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CasinoProject.ViewModels;

public partial class WithdrawViewModel : ObservableObject
{
    public ObservableCollection<string> PayoutMethods { get; } = new()
    {
        "Bank Transfer",
        "PayPal",
        "Cryptocurrency (USDT/BTC)"
    };

    [ObservableProperty]
    private string _selectedMethod = "Bank Transfer";

    [ObservableProperty]
    private int? _amount = 100;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isProcessing = false;
    
    [ObservableProperty]
    private string _bankAccount = string.Empty;

    [ObservableProperty]
    private string _paypalEmail = string.Empty;

    [ObservableProperty]
    private string _cryptoWallet = string.Empty;
    
    public bool IsBankVisible => SelectedMethod == "Bank Transfer";
    public bool IsPayPalVisible => SelectedMethod == "PayPal";
    public bool IsCryptoVisible => SelectedMethod == "Cryptocurrency (USDT/BTC)";
    
    public int CurrentBalance => SessionManager.Instance.Balance;

    partial void OnSelectedMethodChanged(string value)
    {
        OnPropertyChanged(nameof(IsBankVisible));
        OnPropertyChanged(nameof(IsPayPalVisible));
        OnPropertyChanged(nameof(IsCryptoVisible));
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private async Task ProcessWithdrawalAsync()
    {
        if (Amount == null || Amount <= 0)
        {
            StatusMessage = "Please enter a valid withdrawal amount!";
            return;
        }
        
        if (Amount > SessionManager.Instance.Balance)
        {
            StatusMessage = "Insufficient funds in your account!";
            return;
        }
        
        if (SelectedMethod == "Bank Transfer" && string.IsNullOrWhiteSpace(BankAccount))
        {
            StatusMessage = "Please enter your bank account number (IBAN)!";
            return;
        }
        if (SelectedMethod == "PayPal" && (!PaypalEmail.Contains("@") || string.IsNullOrWhiteSpace(PaypalEmail)))
        {
            StatusMessage = "Please enter a valid PayPal email address!";
            return;
        }
        if (SelectedMethod == "Cryptocurrency (USDT/BTC)" && string.IsNullOrWhiteSpace(CryptoWallet))
        {
            StatusMessage = "Please enter your crypto wallet address!";
            return;
        }

        IsProcessing = true;
        StatusMessage = "Processing withdrawal. Please wait...";

        await Task.Delay(2000); 
        
        if (SessionManager.Instance.TryDeduct(Amount.Value))
        {
            StatusMessage = $"Success! Withdrawn ${Amount}. Funds will reach your account soon.";
            
            OnPropertyChanged(nameof(CurrentBalance));
            
            Amount = 0;
            BankAccount = string.Empty;
            PaypalEmail = string.Empty;
            CryptoWallet = string.Empty;
        }
        else
        {
            StatusMessage = "Transaction error! Not enough funds.";
        }

        IsProcessing = false;
    }

    [RelayCommand]
    private void GoBackToMenu()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Menu"));
    }
}