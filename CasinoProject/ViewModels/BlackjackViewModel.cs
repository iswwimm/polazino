using System.Collections.ObjectModel;
using System.Linq;
using CasinoProject.Models;
using CasinoProject.Core;
using CommunityToolkit.Mvvm.Messaging;

namespace CasinoProject.ViewModels
{
    public class BlackjackViewModel : ViewModelBase
    {
        private Deck _deck = new Deck();
        private string _gameStatus = string.Empty;
        private int _playerScore;
        private int _dealerScore;
        private bool _isGameActive;
        private int _betAmount = 10;

        public string GameStatus
        {
            get => _gameStatus;
            set { _gameStatus = value; OnPropertyChanged(nameof(GameStatus)); }
        }

        public int PlayerScore
        {
            get => _playerScore;
            set { _playerScore = value; OnPropertyChanged(nameof(PlayerScore)); }
        }

        public int DealerScore
        {
            get => _dealerScore;
            set { _dealerScore = value; OnPropertyChanged(nameof(DealerScore)); }
        }

        public bool IsGameActive
        {
            get => _isGameActive;
            set { _isGameActive = value; OnPropertyChanged(nameof(IsGameActive)); }
        }

        
        public int BetAmount
        {
            get => _betAmount;
            set { _betAmount = value; OnPropertyChanged(nameof(BetAmount)); }
        }

        public ObservableCollection<Card> PlayerCards { get; } = new();
        public ObservableCollection<Card> DealerCards { get; } = new();

        public BlackjackViewModel()
        {
            GameStatus = "Wybierz stawkę i kliknij Nowa Gra!";
        }

        public void StartNewGame()
        {
            if (BetAmount <= 0)
            {
                GameStatus = "Stawka musi być większa od 0!";
                return;
            }

            if (SessionManager.Instance.TryDeduct(BetAmount))
            {
                _deck = new Deck();
                PlayerCards.Clear();
                DealerCards.Clear();
                IsGameActive = true;
                GameStatus = "Twoja tura! Dobierz (Hit) lub Pasuj (Stand)";

                PlayerCards.Add(_deck.Draw());
                PlayerCards.Add(_deck.Draw());
                DealerCards.Add(_deck.Draw());

                UpdateScores();
            }
            else
            {
                GameStatus = "Brak środków na koncie!";
            }
        }

        public void Hit()
        {
            if (!IsGameActive) return;

            PlayerCards.Add(_deck.Draw());
            UpdateScores();

            if (PlayerScore > 21)
            {
                EndGame("Przekroczyłeś 21! Przegrana.");
            }
        }

        public void Stand()
        {
            if (!IsGameActive) return;

            while (CalculateHandScore(DealerCards) < 17)
            {
                DealerCards.Add(_deck.Draw());
            }

            UpdateScores();
            DetermineWinner();
        }

        public void GoBackToMenu()
        {
            WeakReferenceMessenger.Default.Send(new NavigationMessage("Menu"));
        }

        private void UpdateScores()
        {
            PlayerScore = CalculateHandScore(PlayerCards);
            DealerScore = CalculateHandScore(DealerCards);
        }

        private int CalculateHandScore(ObservableCollection<Card> hand)
        {
            int score = hand.Sum(c => c.Value);
            int aceCount = hand.Count(c => c.Rank == "A");

            while (score > 21 && aceCount > 0)
            {
                score -= 10;
                aceCount--;
            }
            return score;
        }

        private void DetermineWinner()
        {
            if (DealerScore > 21)
            {
                EndGame("Krupier przekroczył 21! Wygrałeś!");
                SessionManager.Instance.AddWinnings(BetAmount * 2);
            }
            else if (PlayerScore > DealerScore)
            {
                EndGame("Wygrałeś!");
                SessionManager.Instance.AddWinnings(BetAmount * 2);
            }
            else if (DealerScore > PlayerScore)
            {
                EndGame("Krupier wygrywa.");
            }
            else
            {
                EndGame("Remis! Zwrot stawki.");
                SessionManager.Instance.AddWinnings(BetAmount);
            }
        }

        private void EndGame(string message)
        {
            GameStatus = message;
            IsGameActive = false;
        }
    }
}