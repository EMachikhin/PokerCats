using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace PokerCats
{
    public enum HandType
    {
        Invalid = -1,
        HighCard,
        Pair,
        TwoPairs,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush,
        RoyalFlush,
        Count
    };

    public struct PlayerHandInfo
    {
        public HandType HandType;
        public Colour HandColour;
        public Rank MainRank;
        public Rank SecondRank;
    }

    public class HandChecker : Singleton<HandChecker>
    {
        // Hand Types
        public static readonly string HIGH_CARD_STRING = "High card ";
        public static readonly string ONE_PAIR_STRING = "Pair of ";
        public static readonly string TWO_PAIRS_STRING = "Two pairs: ";
        public static readonly string THREE_OF_A_KIND_STRING = "Three of a kind, ";
        public static readonly string STRAIGHT_STRING = "Straight, ";
        public static readonly string FLUSH_STRING = "Flush, ";
        public static readonly string FULL_HOUSE_STRING = "Full house, ";
        public static readonly string FOUR_OF_A_KIND_STRING = "Four of a kind, ";
        public static readonly string STRAIGHT_FLUSH_STRING = "Straight flush, ";
        public static readonly string ROYAL_FLUSH_STRING = "ROYAL FLUSH!!!";

        // Card ranks
        public static readonly string ACE_STRING = "A";
        public static readonly string DEUCE_STRING = "2";
        public static readonly string THREE_STRING = "3";
        public static readonly string FOUR_STRING = "4";
        public static readonly string FIVE_STRING = "5";
        public static readonly string SIX_STRING = "6";
        public static readonly string SEVEN_STRING = "7";
        public static readonly string EIGHT_STRING = "8";
        public static readonly string NINE_STRING = "9";
        public static readonly string TEN_STRING = "10";
        public static readonly string JACK_STRING = "J";
        public static readonly string QUEEN_STRING = "Q";
        public static readonly string KING_STRING = "K";

        public string GetFullHandTypeString(out PlayerHandInfo playerHandInfo, HoleCards holeCards, List<Card> boardCards = null)
        {
            HandType handType = HandType.Invalid;

            Colour handColour = Colour.Invalid;
            Rank mainRank = Rank.Invalid;
            Rank secondRank = Rank.Invalid;

            if (boardCards != null) {
                handType = EvaluateHand(holeCards, boardCards, out handColour, out mainRank, out secondRank);
            } else {
                handType = EvaluateHand(holeCards);
            }

            playerHandInfo.HandType = handType;
            playerHandInfo.HandColour = handColour;
            playerHandInfo.MainRank = mainRank;
            playerHandInfo.SecondRank = secondRank;

            if (handType == HandType.Invalid) {
                Debug.LogError("GetFullHandTypeString: could not evaluate hand type properly!");
                return "ERROR!";
            }

            string handTypeString = GetHandTypeString(handType);

            if (handType == HandType.HighCard) {
                if (boardCards == null) {
                    Rank highestRank = GetHighestCardRank(holeCards);
                    string highestRankString = GetRankString(highestRank);

                    return handTypeString + highestRankString;
                } else {
                    List<Card> holeAndBoardCards = new List<Card>();
                    holeAndBoardCards.Add(holeCards.First);
                    holeAndBoardCards.Add(holeCards.Second);
                    holeAndBoardCards.AddRange(boardCards);
                    Rank highestRank = GetHighestCardRank(holeAndBoardCards);
                    string highestRankString = GetRankString(highestRank);

                    return handTypeString + highestRankString;
                }
            } else if (handType == HandType.Pair) {
                if (boardCards == null) {
                    Rank rank = holeCards.First.Rank;
                    string rankString = GetRankString(rank);

                    return handTypeString + rankString + "s";
                } else {
                    if (mainRank == Rank.Invalid) {
                        Debug.LogError("GetFullHandTypeString: one pair - invalid rank.");
                        return "ERROR!";
                    }

                    return handTypeString + GetRankString(mainRank) + "s";
                }
            } else if (handType == HandType.TwoPairs) {
                if (mainRank == Rank.Invalid || secondRank == Rank.Invalid) {
                    Debug.LogError("GetFullHandTypeString: two pairs - invalid rank.");
                    return "ERROR!";
                }

                return handTypeString + GetRankString(mainRank) + "s and " + GetRankString(secondRank) + "s";
            } else if (handType == HandType.ThreeOfAKind) {
                if (mainRank == Rank.Invalid) {
                    Debug.LogError("GetFullHandTypeString: set - invalid rank.");
                    return "ERROR!";
                }

                return handTypeString + GetRankString(mainRank) + "s";
            } else if (handType == HandType.Straight) {
                if (mainRank == Rank.Invalid) {
                    Debug.LogError("GetFullHandTypeString: straight - invalid rank.");
                    return "ERROR!";
                }

                return handTypeString + GetRankString(mainRank) + " high";
            } else if (handType == HandType.Flush) {
                if (mainRank == Rank.Invalid) {
                    Debug.LogError("GetFullHandTypeString: flush - invalid rank.");
                    return "ERROR!";
                }

                if (handColour == Colour.Invalid) {
                    Debug.LogError("GetFullHandTypeString: flush - invalid colour.");
                    return "ERROR!";
                }

                return handTypeString + GetRankString(mainRank) + " high";
            } else if (handType == HandType.FullHouse) {
                if (mainRank == Rank.Invalid || secondRank == Rank.Invalid) {
                    Debug.LogError("GetFullHandTypeString: full house - invalid rank.");
                    return "ERROR!";
                }

                return handTypeString + GetRankString(mainRank) + "s full of " + GetRankString(secondRank) + "s";
            } else if (handType == HandType.FourOfAKind) {
                if (mainRank == Rank.Invalid) {
                    Debug.LogError("GetFullHandTypeString: quads - invalid rank.");
                    return "ERROR!";
                }

                return handTypeString + GetRankString(mainRank) + "s";
            } else if (handType == HandType.StraightFlush) {
                if (mainRank == Rank.Invalid) {
                    Debug.LogError("GetFullHandTypeString: straight flush - invalid rank.");
                    return "ERROR!";
                }

                if (handColour == Colour.Invalid) {
                    Debug.LogError("GetFullHandTypeString: straight flush - invalid colour.");
                    return "ERROR!";
                }

                return handTypeString + GetRankString(mainRank) + "high";
            } else if (handType == HandType.RoyalFlush) {
                if (handColour == Colour.Invalid) {
                    Debug.LogError("GetFullHandTypeString: royal flush - invalid colour.");
                    return "ERROR!";
                }

                return handTypeString;
            }

            return "";
        }

        public PlayerHandInfo GetPlayerHandInfo(HoleCards holeCards, List<Card> boardCards = null)
        {
            HandType handType = HandType.Invalid;

            Colour handColour = Colour.Invalid;
            Rank mainRank = Rank.Invalid;
            Rank secondRank = Rank.Invalid;

            if (boardCards != null) {
                handType = EvaluateHand(holeCards, boardCards, out handColour, out mainRank, out secondRank);
            } else {
                handType = EvaluateHand(holeCards);
            }

            PlayerHandInfo playerHandInfo;
            playerHandInfo.HandType = handType;
            playerHandInfo.HandColour = handColour;
            playerHandInfo.MainRank = mainRank;
            playerHandInfo.SecondRank = secondRank;

            return playerHandInfo;
        }

        private HandType EvaluateHand(HoleCards holeCards)
        {
            //if (!IsHoleCardsNumberCorrect(holeCards)) {
            //    Debug.LogError("EvaluateHand: incorrect number of hole cards!");
            //    return HandType.Invalid;
            //}

            if (holeCards.IsPocketPair) {
                return HandType.Pair;
            } else {
                return HandType.HighCard;
            }
        }

        private HandType EvaluateHand(HoleCards holeCards, List<Card> boardCards, out Colour handColour, out Rank mainRank, out Rank secondRank)
        {
            handColour = Colour.Invalid;
            mainRank = Rank.Invalid;
            secondRank = Rank.Invalid;

            if (/*!IsHoleCardsNumberCorrect(holeCards) || */!IsBoardCardsNumberCorrect(boardCards)) {
                Debug.LogError("EvaluateHand: incorrect number of hole or board cards!");
                return HandType.Invalid;
            }

            List<Card> holeAndBoardCards = new List<Card>();
            holeAndBoardCards.Add(holeCards.First);
            holeAndBoardCards.Add(holeCards.Second);
            holeAndBoardCards.AddRange(boardCards);

            // TODO: optimization: check set, two pair etc. in one loop through cards

            if (HasRoyalFlush(holeAndBoardCards, out handColour)) {
                return HandType.RoyalFlush;
            } else if (HasStraightFlush(holeAndBoardCards, out handColour, out mainRank)) {
                return HandType.StraightFlush;
            } else if (HasFourOfAKind(holeAndBoardCards, out mainRank)) {
                return HandType.FourOfAKind;
            } else if (HasFullHouse(holeAndBoardCards, out mainRank, out secondRank)) {
                return HandType.FullHouse;
            } else if (HasFlush(holeAndBoardCards, out handColour, out mainRank)) {
                return HandType.Flush;
            } else if (HasStraight(holeAndBoardCards, out mainRank)) {
                return HandType.Straight;
            } else if (HasThreeOfAKind(holeAndBoardCards, out mainRank)) {
                return HandType.ThreeOfAKind;
            } else if (HasTwoPairs(holeAndBoardCards, out mainRank, out secondRank)) {
                return HandType.TwoPairs;
            } else if (HasOnePair(holeAndBoardCards, out mainRank)) {
                return HandType.Pair;
            } else {
                return HandType.HighCard;
            }
        }

        private bool HasRoyalFlush(List<Card> cards, out Colour royalFlushColour)
        {
            Colour straightFlushColour;
            Rank highestStraightFlushCardRank;

            royalFlushColour = Colour.Invalid;

            if (!HasStraightFlush(cards, out straightFlushColour, out highestStraightFlushCardRank)) {
                return false;
            }

            if (straightFlushColour == Colour.Invalid || highestStraightFlushCardRank == Rank.Invalid) {
                Debug.LogError("HasRoyalFlush: HasStraightFlush returned true, but invalid colour or highest card rank.");
                return false;
            }

            if (highestStraightFlushCardRank == Rank.Ace) {
                royalFlushColour = straightFlushColour;
                return true;
            }

            return false;
        }

        private bool HasStraightFlush(List<Card> cards, out Colour straightFlushColour, out Rank highestCardRank)
        {
            Colour flushColour;
            Rank highestFlushCardRank;

            straightFlushColour = Colour.Invalid;
            highestCardRank = Rank.Invalid;

            if (!HasFlush(cards, out flushColour, out highestFlushCardRank)) {
                return false;
            }

            if (flushColour == Colour.Invalid || highestFlushCardRank == Rank.Invalid) {
                Debug.LogError("HasStraightFlush: HasFlush returned true, but invalid colour or highest card rank.");
                return false;
            }

            List<Card> listOfOneColourCards = new List<Card>();
            foreach (Card card in cards) {
                if (card.Colour == flushColour) {
                    listOfOneColourCards.Add(card);
                }
            }

            Rank highestStraighFlushCardRank = Rank.Invalid;

            bool isInSequence = HasStraight(listOfOneColourCards, out highestStraighFlushCardRank);
            if (isInSequence) {
                straightFlushColour = flushColour;
                highestCardRank = highestStraighFlushCardRank;
                return true;
            }

            return false;
        }

        private bool HasFourOfAKind(List<Card> cards, out Rank rank)
        {
            rank = Rank.Invalid;

            for (int firstCardIndex = 0; firstCardIndex < cards.Count; firstCardIndex++) {
                Card firstCard = cards[firstCardIndex];
                int matchesCount = 0;

                for (int secondCardIndex = firstCardIndex + 1; secondCardIndex < cards.Count; secondCardIndex++) {
                    Card secondCard = cards[secondCardIndex];

                    if (firstCard.Rank == secondCard.Rank) {
                        matchesCount++;
                    }
                }

                if (matchesCount == 3) {
                    rank = firstCard.Rank;
                    return true;
                }
            }
            
            return false;
        }

        private bool HasFullHouse(List<Card> cards, out Rank setRank, out Rank pairRank)
        {
            setRank = Rank.Invalid;
            pairRank = Rank.Invalid;

            bool isSetFound = false;
            bool isPairFound = false;

            for (int firstCardIndex = 0; firstCardIndex < cards.Count; firstCardIndex++) {
                Card firstCard = cards[firstCardIndex];
                int matchesCount = 0;

                for (int secondCardIndex = firstCardIndex + 1; secondCardIndex < cards.Count; secondCardIndex++) {
                    Card secondCard = cards[secondCardIndex];

                    if (firstCard.Rank == secondCard.Rank) {
                        matchesCount++;
                    }
                }

                if (matchesCount == 2) {
                    if (!isSetFound) {
                        setRank = firstCard.Rank;
                        isSetFound = true;
                    } else {
                        if (firstCard.Rank > setRank) {
                            if (isPairFound) {
                                if (setRank > pairRank) {
                                    pairRank = setRank;
                                    setRank = firstCard.Rank;
                                }
                            } else {
                                pairRank = setRank;
                                isPairFound = true;
                            }
                        }
                    }
                } else if (matchesCount == 1) {
                    if (isSetFound) {
                        if (firstCard.Rank != setRank) {
                            pairRank = firstCard.Rank;
                            isPairFound = true;
                        }
                    } else {
                        pairRank = firstCard.Rank;
                        isPairFound = true;
                    }
                }
            }

            if (isSetFound && isPairFound) {
                return true;
            }

            return false;
        }

        private bool HasFlush(List<Card> cards, out Colour flushColour, out Rank highestCardRank)
        {
            int maxCardsOfOneColourCount = 0;
            Colour colourWithMaxCards = Colour.Invalid;
            Rank highestCardInColourWithMaxCards = Rank.Invalid;

            flushColour = Colour.Invalid;
            highestCardRank = Rank.Invalid;

            for (Colour colour = Colour.Clubs; colour < Colour.Count; colour++) {
                int cardsOfThisColourCount = 0;
                Rank highestRankInColour = Rank.Invalid;

                foreach (Card card in cards) {
                    if (card.Colour == colour) {
                        cardsOfThisColourCount++;
                        if (card.Rank > highestRankInColour) {
                            highestRankInColour = card.Rank;
                        }
                    }
                }

                if (cardsOfThisColourCount > maxCardsOfOneColourCount) {
                    maxCardsOfOneColourCount = cardsOfThisColourCount;
                    colourWithMaxCards = colour;
                    highestCardInColourWithMaxCards = highestRankInColour;
                }
            }

            if (maxCardsOfOneColourCount >= Defines.CARDS_IN_HAND_COUNT) {
                flushColour = colourWithMaxCards;
                highestCardRank = highestCardInColourWithMaxCards;
                return true;
            }

            return false;
        }

        private bool HasStraight(List<Card> cards, out Rank highestCardRank)
        {
            highestCardRank = Rank.Invalid;

            List<Card> sortedCardsList = new List<Card>();
            foreach (Card card in cards) {
                sortedCardsList.Add(card);
            }

            sortedCardsList.Sort(CompareByRank);

            for (int firstCardIndex = 0; firstCardIndex <= sortedCardsList.Count - Defines.CARDS_IN_HAND_COUNT; firstCardIndex++) {
                int cardsInSequence = 1;
                Rank highestCardRankInSequence = Rank.Invalid;

                for (int secondCardIndex = firstCardIndex + 1; secondCardIndex < sortedCardsList.Count; secondCardIndex++) {
                    Card card = sortedCardsList[secondCardIndex];
                    Card previousCard = sortedCardsList[secondCardIndex - 1];

                    bool isInSequence = false;
                    if (card.Rank == previousCard.Rank + 1) {
                        isInSequence = true;
                        cardsInSequence++;
                        highestCardRankInSequence = card.Rank;
                    } else if (card.Rank == previousCard.Rank) {
                        isInSequence = true;
                    }

                    if (!isInSequence) {
                        // break the inner loop and try to begin new sequence
                        break;
                    }
                }

                if (cardsInSequence >= Defines.CARDS_IN_HAND_COUNT) {
                    highestCardRank = highestCardRankInSequence;
                    return true;
                }

                // possible A - 5 straight case
                // if we have 2 to 5 sequence at the beginning of the list
                if (cardsInSequence == Defines.CARDS_IN_HAND_COUNT - 1 && highestCardRankInSequence == Rank.Five) {
                    // and we have an ace at the end of the list
                    if (sortedCardsList[sortedCardsList.Count - 1].Rank == Rank.Ace) {
                        highestCardRank = highestCardRankInSequence;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool HasThreeOfAKind(List<Card> cards, out Rank rank)
        {
            rank = Rank.Invalid;

            for (int firstCardIndex = 0; firstCardIndex < cards.Count; firstCardIndex++) {
                Card firstCard = cards[firstCardIndex];
                int matchesCount = 0;

                for (int secondCardIndex = firstCardIndex + 1; secondCardIndex < cards.Count; secondCardIndex++) {
                    Card secondCard = cards[secondCardIndex];

                    if (firstCard.Rank == secondCard.Rank) {
                        matchesCount++;
                    }
                }

                if (matchesCount == 2) {
                    rank = firstCard.Rank;
                    return true;
                }
            }
 
            return false;
        }

        private bool HasTwoPairs(List<Card> cards, out Rank firstPairRank, out Rank secondPairRank)
        {
            firstPairRank = Rank.Invalid;
            secondPairRank = Rank.Invalid;

            bool isFirstPairFound = false;
            bool isSecondPairFound = false;

            for (int firstCardIndex = 0; firstCardIndex < cards.Count; firstCardIndex++) {
                Card firstCard = cards[firstCardIndex];
                int matchesCount = 0;

                for (int secondCardIndex = firstCardIndex + 1; secondCardIndex < cards.Count; secondCardIndex++) {
                    Card secondCard = cards[secondCardIndex];

                    if (firstCard.Rank == secondCard.Rank) {
                        matchesCount++;
                    }
                }

                if (matchesCount == 1) {
                    if (!isFirstPairFound) {
                        firstPairRank = firstCard.Rank;
                        isFirstPairFound = true;
                    } else if (!isSecondPairFound) {
                        if (firstCard.Rank != firstPairRank) {
                            if (firstCard.Rank > firstPairRank) {
                                secondPairRank = firstPairRank;
                                firstPairRank = firstCard.Rank;
                            } else {
                                secondPairRank = firstCard.Rank;
                            }
                            isSecondPairFound = true;
                        }
                    } else {
                        // we found a pair which is bigger than either of two that we already have
                        if (firstCard.Rank > secondPairRank) {
                            secondPairRank = firstCard.Rank;
                        }
                        // exchange pairs if needed (first should always be higher)
                        if (secondPairRank > firstPairRank) {
                            Rank tempRank = firstPairRank;
                            firstPairRank = secondPairRank;
                            secondPairRank = tempRank;
                        }
                    }
                }
            }

            if (isFirstPairFound && isSecondPairFound) {
                return true;
            }

            return false;
        }

        private bool HasOnePair(List<Card> cards, out Rank rank)
        {
            rank = Rank.Invalid;

            for (int firstCardIndex = 0; firstCardIndex < cards.Count; firstCardIndex++) {
                Card firstCard = cards[firstCardIndex];
                int matchesCount = 0;

                for (int secondCardIndex = firstCardIndex + 1; secondCardIndex < cards.Count; secondCardIndex++) {
                    Card secondCard = cards[secondCardIndex];

                    if (firstCard.Rank == secondCard.Rank) {
                        matchesCount++;
                    }
                }

                if (matchesCount == 1) {
                    rank = firstCard.Rank;
                    return true;
                }
            }

            return false;
        }

        private int CompareByRank(Card firstCard, Card secondCard)
        {
            if (firstCard.Rank > secondCard.Rank) {
                return 1;
            } else if (firstCard.Rank < secondCard.Rank) {
                return -1;
            } else {
                return 0;
            }
        }

        private string GetHandTypeString(HandType handType)
        {
            if (!Utils.IsValidHandType(handType)) {
                Debug.LogError("GetHandTypeString: invalid hand type.");
                return "";
            }

            switch (handType) {
                case HandType.HighCard:
                    return HIGH_CARD_STRING;
                case HandType.Pair:
                    return ONE_PAIR_STRING;
                case HandType.TwoPairs:
                    return TWO_PAIRS_STRING;
                case HandType.ThreeOfAKind:
                    return THREE_OF_A_KIND_STRING;
                case HandType.Straight:
                    return STRAIGHT_STRING;
                case HandType.Flush:
                    return FLUSH_STRING;
                case HandType.FullHouse:
                    return FULL_HOUSE_STRING;
                case HandType.FourOfAKind:
                    return FOUR_OF_A_KIND_STRING;
                case HandType.StraightFlush:
                    return STRAIGHT_FLUSH_STRING;
                case HandType.RoyalFlush:
                    return ROYAL_FLUSH_STRING;
                default:
                    return "";
            }
        }

        private string GetRankString(Rank rank)
        {
            if (!Utils.IsValidRank(rank)) {
                Debug.LogError("GetRankString: invalid rank.");
                return "";
            }

            switch (rank) {
                case Rank.Ace:
                    return ACE_STRING;
                case Rank.Deuce:
                    return DEUCE_STRING;
                case Rank.Three:
                    return THREE_STRING;
                case Rank.Four:
                    return FOUR_STRING;
                case Rank.Five:
                    return FIVE_STRING;
                case Rank.Six:
                    return SIX_STRING;
                case Rank.Seven:
                    return SEVEN_STRING;
                case Rank.Eight:
                    return EIGHT_STRING;
                case Rank.Nine:
                    return NINE_STRING;
                case Rank.Ten:
                    return TEN_STRING;
                case Rank.Jack:
                    return JACK_STRING;
                case Rank.Queen:
                    return QUEEN_STRING;
                case Rank.King:
                    return KING_STRING;
                default:
                    return "";
            }
        }

        private Rank GetHighestCardRank(HoleCards holeCards)
        {
            if (holeCards.First.Rank > holeCards.Second.Rank) {
                return holeCards.First.Rank;
            } else {
                return holeCards.Second.Rank;
            }
        }

        private Rank GetHighestCardRank(List<Card> cards)
        {
            Rank highestRank = Rank.Invalid;

            foreach (Card card in cards) {
                if (card.Rank > highestRank) {
                    highestRank = card.Rank;
                }
            }

            return highestRank;
        }

        private bool IsHoleCardsNumberCorrect(List<Card> holeCards)
        {
            return holeCards.Count == Defines.HOLE_CARDS_COUNT;
        }

        private bool IsBoardCardsNumberCorrect(List<Card> boardCards)
        {
            return Defines.FLOP_CARDS_COUNT <= boardCards.Count && boardCards.Count <= Defines.CARDS_COUNT_ON_RIVER;
        }
    }
}
