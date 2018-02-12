using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokerCats
{
    public enum Colour
    {
        Invalid = -1,
        Clubs = 1,
        Diamonds,
        Hearts,
        Spades,
        Count
    };

    public enum Rank
    {
        Invalid = -1,
        Deuce = 2,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace,
        Count
    };

    public class Card
    {
        private Rank _rank;
        private Colour _colour;

        // TODO: check if this should be completely public
        public bool IsOpened { get; set; }

        public Card(Rank rank, Colour colour, bool isOpened = true)
        {
            _rank = rank;
            _colour = colour;
            IsOpened = isOpened;
        }

        public Rank Rank
        {
            get
            {
                return _rank;
            }

            set
            {
                _rank = value;
            }
        }

        public Colour Colour
        {
            get
            {
                return _colour;
            }

            set
            {
                _colour = value;
            }
        }

        public string GetTextInfo()
        {
            if (!IsOpened) {
                return "shirt_red";
            }

            string cardInfo = "";

            char colourSign = 'c';
            char rankSign = 'A';

            switch (_colour)
            {
                case Colour.Clubs:
                    colourSign = 'c';
                    break;

                case Colour.Diamonds:
                    colourSign = 'd';
                    break;

                case Colour.Hearts:
                    colourSign = 'h';
                    break;

                case Colour.Spades:
                    colourSign = 's';
                    break;
            }

            switch (_rank)
            {
                case Rank.Ace:
                    rankSign = 'A';
                    break;

                case Rank.Deuce:
                    rankSign = '2';
                    break;

                case Rank.Three:
                    rankSign = '3';
                    break;

                case Rank.Four:
                    rankSign = '4';
                    break;

                case Rank.Five:
                    rankSign = '5';
                    break;

                case Rank.Six:
                    rankSign = '6';
                    break;

                case Rank.Seven:
                    rankSign = '7';
                    break;

                case Rank.Eight:
                    rankSign = '8';
                    break;

                case Rank.Nine:
                    rankSign = '9';
                    break;

                case Rank.Ten:
                    rankSign = 'T';
                    break;

                case Rank.Jack:
                    rankSign = 'J';
                    break;

                case Rank.Queen:
                    rankSign = 'Q';
                    break;

                case Rank.King:
                    rankSign = 'K';
                    break;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(rankSign);
            sb.Append(colourSign);

            cardInfo = sb.ToString();

            return cardInfo;
        }
    }
}
