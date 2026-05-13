using System;
using System.Collections.Generic;
using System.Linq;

namespace CasinoProject.Models;

public class Deck
{
    private List<Card> _cards = new List<Card>();
    private Random _random = new Random();

    public Deck()
    {
        Initialize();
        Shuffle();
    }

    public void Initialize()
    {
        _cards.Clear();

        string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };
        string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

        foreach (string suit in suits)
        {
            foreach (string rank in ranks)
            {
                int value = 0;

                if (rank == "A") value = 11;
                else if (rank == "K" || rank == "Q" || rank == "J") value = 10;
                else value = int.Parse(rank);

                _cards.Add(new Card(suit, rank, value));
            }
        }
    }

    public void Shuffle()
    {
        _cards = _cards.OrderBy(x => _random.Next()).ToList();
    }

    public Card Draw()
    {
        if (_cards.Count == 0)
        {
            Initialize();
            Shuffle();
        }

        Card drawnCard = _cards[0];
        _cards.RemoveAt(0);
        return drawnCard;
    }
}