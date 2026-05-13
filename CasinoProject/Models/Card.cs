namespace CasinoProject.Models;

public class Card
{
    public string Suit { get; set; }
    public string Rank { get; set; }
    public int Value { get; set; }

    public Card(string suit, string rank, int value)
    {
        Suit = suit;
        Rank = rank;
        Value = value;
    }

    public string SuitIcon
    {
        get
        {
            if (Suit == "Hearts" || Suit == "Kier") return "♥";
            if (Suit == "Diamonds" || Suit == "Karo") return "♦";
            if (Suit == "Clubs" || Suit == "Trefl") return "♣";
            if (Suit == "Spades" || Suit == "Pik") return "♠";
            return "";
        }
    }

    public string SuitColor
    {
        get
        {
            if (Suit == "Hearts" || Suit == "Diamonds" || Suit == "Kier" || Suit == "Karo")
            {
                return "Red";
            }

            return "Black";
        }
    }
}