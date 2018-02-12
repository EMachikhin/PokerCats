using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using UnityEngine;

namespace PokerCats
{
    public class Deck
    {
        private List<Card> _deckOfCards;

        public Deck()
        {
            _deckOfCards = new List<Card>();
        }

        public void Init()
        {
            for (Colour colour = Colour.Clubs; colour < Colour.Count; ++colour) {
                for (Rank rank = Rank.Deuce; rank < Rank.Count; ++rank) {
                    Card newCard = new Card(rank, colour);
                    // TODO: check if we should store go or it's script component in the list
                    _deckOfCards.Add(newCard);
                }
            }

            // test
            //    for (Rank rank = Rank.Deuce; rank < Rank.Count; ++rank) {
            //        for (Colour colour = Colour.Clubs; colour < Colour.Count; ++colour) {
            //        Card newCard = new Card(rank, colour);
            //        // TODO: check if we should store go or it's script component in the list
            //        _deckOfCards.Add(newCard);
            //    }
            //}
        }

        public void Shuffle()
        {
            int n = _deckOfCards.Count;

            while (n > 1) {
                n--;
                int k = RandomNumber.GetRandomNumber(n + 1);
                Card card = _deckOfCards[k];
                _deckOfCards[k] = _deckOfCards[n];
                _deckOfCards[n] = card;
            }
        }

        public void Clear()
        {
            _deckOfCards.Clear();
        }

        public void StartNewHand()
        {
            Clear();
            Init();
            Shuffle();
        }

        public Card DealTopCard()
        {
            Card cardToDeal = _deckOfCards.First();
            _deckOfCards.Remove(cardToDeal);

            return cardToDeal;
        }

        public void BurnTopCard()
        {
            _deckOfCards.RemoveAt(0);
        }

        public List<Card> DealFlop()
        {
            List<Card> flopCardsToDeal = new List<Card>();

            BurnTopCard();

            for (int flopCardIndex = 0; flopCardIndex < Defines.FLOP_CARDS_COUNT; flopCardIndex++) {
                Card flopCard = _deckOfCards.First();
                _deckOfCards.Remove(flopCard);
                flopCardsToDeal.Add(flopCard);
            }

            return flopCardsToDeal;
        }

        public Card DealTurnOrRiver()
        {
            BurnTopCard();

            return DealTopCard();
        }
    }
}
