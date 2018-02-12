using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokerCats
{
    public class Board
    {
        private List<Card> _flopCards = new List<Card>();
        private Card _turnCard;
        private Card _riverCard;

        public List<Card> Flop
        {
            get
            {
                return _flopCards;
            }

            set
            {
                if (value.Count == Defines.FLOP_CARDS_COUNT)
                {
                    _flopCards.AddRange(value);
                }
            }
        }

        public Card TurnCard
        {
            get
            {
                return _turnCard;
            }

            set
            {
                _turnCard = value;
            }
        }

        public Card RiverCard
        {
            get
            {
                return _riverCard;
            }

            set
            {
                _riverCard = value;
            }
        }

        public List<Card> FullBoard
        {
            get
            {
                List<Card> fullBoard = new List<Card>();

                if (IsFlopDealt) {
                    fullBoard.AddRange(Flop);
                }

                if (IsTurnDealt) {
                    fullBoard.Add(TurnCard);
                }

                if (IsRiverDealt) {
                    fullBoard.Add(RiverCard);
                }

                return fullBoard;
            }
        }

        public bool IsFlopDealt
        {
            get { return _flopCards.Count > 0; }
        }

        public bool IsTurnDealt
        {
            get { return _turnCard != null; }
        }

        public bool IsRiverDealt
        {
            get { return _riverCard != null; }
        }
    }
}
