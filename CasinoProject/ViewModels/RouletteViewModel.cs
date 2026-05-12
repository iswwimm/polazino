using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CasinoProject.Core;

namespace CasinoProject.ViewModels
{
    public partial class RouletteViewModel : ObservableObject
    {
        [ObservableProperty] private int _stawka = 10;
        [ObservableProperty] private int _wybranaLiczba = 0;
        [ObservableProperty] private string _wynikTekst = "Postaw zakład i zakręć!";

        private readonly Random _random = new Random();

        [RelayCommand]
        private void Spin()
        {
            if (SessionManager.Instance.TryDeduct(Stawka))
            {
                int wylosowana = _random.Next(0, 37);

                if (wylosowana == WybranaLiczba)
                {
                    int wygrana = Stawka * 36;
                    SessionManager.Instance.AddWinnings(wygrana);
                    WynikTekst = $"WYGRANA! Wypadło {wylosowana}. Zarabiasz {wygrana}$!";
                }
                else
                {
                    WynikTekst = $"Pudło! Wypadło {wylosowana}. Przegrywasz stawkę.";
                }
            }
            else
            {
                WynikTekst = "Brak kasy! Doładuj portfel.";
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            WeakReferenceMessenger.Default.Send(new NavigationMessage("Menu"));
        }
    }
}