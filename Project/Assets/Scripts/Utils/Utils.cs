using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace PokerCats
{
	public class Utils
	{
        public static bool IsPositionValid(Position position)
        {
            return (Position.Invalid < position && position < Position.Count);
        }

        public static bool IsOnBlinds(Position position)
        {
            return (Position.SB <= position && position <= Position.BB);
        }

        public static bool IsValidHandType(HandType handType)
        {
            return HandType.Invalid < handType && handType < HandType.Count;
        }

        public static bool IsValidRank(Rank rank)
        {
            return Rank.Invalid < rank && rank < Rank.Count;
        }

        public static Position GetPositionByString(string positionString)
        {
            Position position = Position.Invalid;

            switch (positionString) {
                case "MP2":
                    position = Position.MP2;
                    break;
                case "MP3":
                    position = Position.MP3;
                    break;
                case "CO":
                    position = Position.CO;
                    break;
                case "BU":
                    position = Position.BU;
                    break;
                case "SB":
                    position = Position.SB;
                    break;
                case "BB":
                    position = Position.BB;
                    break;
            }

            if (position == Position.Invalid) {
                Debug.LogError("GetPositionByName: invalid position string!");
            }

            return position;
        }

        public static Card GetCardByChar(char cardChar)
        {
            Rank rank = Rank.Invalid;
            Colour colour = Colour.Spades;

            switch (cardChar) {
                case 'A':
                    rank = Rank.Ace;
                    break;
                case 'K':
                    rank = Rank.King;
                    break;
                case 'Q':
                    rank = Rank.Queen;
                    break;
                case 'J':
                    rank = Rank.Jack;
                    break;
                case 'T':
                    rank = Rank.Ten;
                    break;
                case '9':
                    rank = Rank.Nine;
                    break;
                case '8':
                    rank = Rank.Eight;
                    break;
                case '7':
                    rank = Rank.Seven;
                    break;
                case '6':
                    rank = Rank.Six;
                    break;
                case '5':
                    rank = Rank.Five;
                    break;
                case '4':
                    rank = Rank.Four;
                    break;
                case '3':
                    rank = Rank.Three;
                    break;
                case '2':
                    rank = Rank.Deuce;
                    break;
            }

            if (rank == Rank.Invalid) {
                Debug.LogError("GetCardByChar: invalid card char!");
                return null;
            }

            Card card = new Card(rank, colour);

            return card;
        }

        public static HoleCards GetHandByString(string handString)
        {
            HoleCards holeCards = new HoleCards();

            Card firstCard = GetCardByChar(handString[0]);
            Card secondCard = GetCardByChar(handString[1]);

            if (firstCard == null || secondCard == null) {
                Debug.LogError("GetHandByString: could not create cards!");
                return holeCards;
            }

            holeCards.First = firstCard;
            holeCards.Second = secondCard;

            // TODO: 3rd simbol can be o, s or *, 4th can be *. Process all this

            // just in case, make cards colour different for pocket pairs
            if (firstCard.Rank == secondCard.Rank) {
                secondCard.Colour = Colour.Clubs;
            } else if (handString.Length > 2) {
                // change second card colour for offsuit hands
                if (handString[2] == 'o') {
                    secondCard.Colour = Colour.Clubs;
                }
            }

            return holeCards;
        }
    }
}
