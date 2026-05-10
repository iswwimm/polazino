using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace CasinoProject.Core;

public partial class SessionManager : ObservableObject
{
    private static readonly SessionManager _instance = new();
    public static SessionManager Instance => _instance;
    
    private SessionManager() { }
    
    [ObservableProperty]
    private int _balance = 1000;
    
    public bool TryDeduct(int amount)
    {
        if (Balance >= amount)
        {
            Balance -= amount;
            return true;
        }
        return false;
    }

    public void AddWinnings(int amount)
    {
        if (amount > 0)
        {
            Balance += amount;
        }
    }
}