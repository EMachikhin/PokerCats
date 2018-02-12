using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace PokerCats
{
    public enum PlayerType
    {
        Invalid = -1,
        Local,
        Remote,
        AI,
        Count
    };

    public enum Position
    {
        Invalid = -1,
        UTG1,
        UTG2,
        MP1,
        MP2,
        MP3,
        CO,
        BU,
        SB,
        BB,
        Count
    };

    public struct HoleCards
    {
        public Card First;
        public Card Second;

        public bool IsPocketPair
        {
            get
            {
                if (First == null || Second == null) {
                    Debug.LogError("HoleCards.IsPocketPair: cards are null!");
                    return false;
                }

                return (First.Rank == Second.Rank);
            }
        }

        public bool IsSuited
        {
            get
            {
                if (First == null || Second == null) {
                    Debug.LogError("HoleCards.IsSuited: cards are null!");
                    return false;
                }

                return (First.Colour == Second.Colour);
            }
        }
    };

    public class Player
    {
        private HoleCards _holeCards = new HoleCards();
        private string _playerName;
        private int _chipCount;
        private int _currentBet;
        private Position _position;

        private TurnType _lastTurn = TurnType.NotMade;

        private PlayerType _playerType = PlayerType.Invalid;

        private PlayerHandInfo _currentHandInfo;

        public Player(string playerName, int startingChips, PlayerType playerType)
        {
            _playerName = playerName;
            _chipCount = startingChips;

            _playerType = playerType;
        }

        public PlayerType Type
        {
            get { return _playerType; }
            set { _playerType = value; }
        }

        public string Name
        {
            get { return _playerName; }
            set { _playerName = value; }
        }

        public int ChipCount
        {
            get { return _chipCount; }
            set {
                _chipCount = value;
                if (_chipCount < 0) {
                    _chipCount = 0;
                    Debug.LogError("ChipCount set: attempt to set negative amount.");
                }
            }
        }

        public PlayerHandInfo CurrentHandInfo
        {
            get { return _currentHandInfo; }
            set { _currentHandInfo = value; }
        }

        public bool IsAllIn
        {
            get { return ChipCount == 0; }
        }

        public int CurrentBet
        {
            get { return _currentBet; }
            set { _currentBet = value; }
        }

        public HoleCards HoleCards
        {
            get { return _holeCards; }
        }

        public Position Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public bool HasButton
        {
            get { return Position == Position.BU; }
        }

        public bool IsOnSB
        {
            get { return Position == Position.SB; }
        }

        public bool IsOnBB
        {
            get { return Position == Position.BB; }
        }

        public TurnType LastTurn
        {
            get { return _lastTurn; }
            set { _lastTurn = value; }
        }

        public bool IsLocal
        {
            get { return _playerType == PlayerType.Local; }
        }

        public bool IsAI
        {
            get { return _playerType == PlayerType.AI; }
        }

        public bool HasMadeTurn
        {
            get { return LastTurn != TurnType.NotMade; }
        }

        public bool HasFolded
        {
            get { return LastTurn == TurnType.Fold; }
        }

        public bool HasChecked
        {
            get { return LastTurn == TurnType.Check; }
        }

        public void AddHoleCard(Card cardToAdd)
        {
            if (cardToAdd == null) {
                Debug.LogError("AddHoleCard: card to add is null!");
                return;
            }

            if (_holeCards.First == null) {
                _holeCards.First = cardToAdd;
            } else if (_holeCards.Second == null) {
                if (_holeCards.First.Rank == cardToAdd.Rank && _holeCards.First.Colour == cardToAdd.Colour) {
                    Debug.LogError("AddHoleCard: attempt to add the same hole card!");
                    return;
                }

                _holeCards.Second = cardToAdd;

                // sort cards in hand by rank
                if (_holeCards.Second.Rank > _holeCards.First.Rank) {
                    Card card = _holeCards.First;
                    _holeCards.First = _holeCards.Second;
                    _holeCards.Second = card;
                }
            } else {
                Debug.LogError("AddHoleCard: attempt to add redundant hole card!");
            }
        }

        public void ClearHoleCards()
        {
            _holeCards.First = null;
            _holeCards.Second = null;
        }

        public void PostBlind(int blindSize)
        {
            if (!Utils.IsOnBlinds(Position)) {
                Debug.LogError("PostBlind: attempt to post blind for player with wrong position!");
                return;
            }

            CurrentBet += blindSize;
            int newStackSize = ChipCount - blindSize;
            ChipCount = newStackSize;
        }

        public void PostAnte(int anteSize)
        {
            // TODO: check if we should add ante to _currentBet
            int newStackSize = ChipCount - anteSize;
            ChipCount = newStackSize;
        }

        public void PutChipsToPot(Pot pot, int amount)
        {
            int newStackSize = ChipCount - amount;
            ChipCount = newStackSize;

            CurrentBet += amount;
            if (!pot.IsPlayerInPot(this)) {
                pot.PlayersInPot.Add(this);
            }
            pot.Size += amount;
        }

        public void AddChips(int amount)
        {
            int newStackSize = ChipCount + amount;
            ChipCount = newStackSize;
        }
    }
}
