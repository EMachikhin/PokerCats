using UnityEngine;
using System;
using System.Collections.Generic;

namespace PokerCats
{
    // TODO: split AI based on difficulty levels (better split JSONs, not classes)
    public class PlayerAI : Singleton<PlayerAI>
    {
        private Game _currentGame;
        public Game CurrentGame
        {
            get { return _currentGame;  }
            set { _currentGame = value; }
        }

        private Dictionary<Position, List<HoleCards>> _preflopOpenRaiseRanges = new Dictionary<Position, List<HoleCards>>();
        private Dictionary<Position, Dictionary<Position, List<HoleCards>>> _preflopColdCallRanges = new Dictionary<Position, Dictionary<Position, List<HoleCards>>>();
        private Dictionary<Position, Dictionary<Position, List<HoleCards>>> _preflop3BetRanges = new Dictionary<Position, Dictionary<Position, List<HoleCards>>>();
        private Dictionary<Position, Dictionary<Position, List<HoleCards>>> _preflop3BetCallingRanges = new Dictionary<Position, Dictionary<Position, List<HoleCards>>>();
        private Dictionary<Position, Dictionary<Position, List<HoleCards>>> _preflop4BetRanges = new Dictionary<Position, Dictionary<Position, List<HoleCards>>>();

        public PlayerAI()
        {
            for (Position position = Position.UTG1; position < Position.Count; position++) {
                _preflopOpenRaiseRanges.Add(position, new List<HoleCards>());

                _preflopColdCallRanges.Add(position, new Dictionary<Position, List<HoleCards>>());
                for (Position vsPosition = Position.UTG1; vsPosition < position; vsPosition++) {
                    _preflopColdCallRanges[position].Add(vsPosition, new List<HoleCards>());
                }

                _preflop3BetRanges.Add(position, new Dictionary<Position, List<HoleCards>>());
                for (Position vsPosition = Position.UTG1; vsPosition < position; vsPosition++) {
                    _preflop3BetRanges[position].Add(vsPosition, new List<HoleCards>());
                }

                _preflop3BetCallingRanges.Add(position, new Dictionary<Position, List<HoleCards>>());
                for (Position vsPosition = position + 1; vsPosition < Position.Count; vsPosition++) {
                    _preflop3BetCallingRanges[position].Add(vsPosition, new List<HoleCards>());
                }

                _preflop4BetRanges.Add(position, new Dictionary<Position, List<HoleCards>>());
                for (Position vsPosition = position + 1; vsPosition < Position.Count; vsPosition++) {
                    _preflop4BetRanges[position].Add(vsPosition, new List<HoleCards>());
                }
            }
        }

        public void AddHandToPreflopOpenRaiseRange(Position position, HoleCards hand)
        {
            _preflopOpenRaiseRanges[position].Add(hand);
        }

        public void AddHandToPreflopColdCallRange(Position position, Position vsPosition, HoleCards hand)
        {
            _preflopColdCallRanges[position][vsPosition].Add(hand);
        }

        public void AddHandToPreflop3BetRange(Position position, Position vsPosition, HoleCards hand)
        {
            _preflop3BetRanges[position][vsPosition].Add(hand);
        }

        public void AddHandToPreflop3BetCallingRange(Position position, Position vsPosition, HoleCards hand)
        {
            _preflop3BetCallingRanges[position][vsPosition].Add(hand);
        }

        public void AddHandToPreflop4BetRange(Position position, Position vsPosition, HoleCards hand)
        {
            _preflop4BetRanges[position][vsPosition].Add(hand);
        }

        public TurnType MakeDecision(out int amount)
        {
            amount = 0;

            if (CurrentHandState.Instance.IsPreflop) {
                return MakePreflopDecision(out amount);
            } else {
                // TODO: temporary checking through postflop and folding to any bet
                if (_currentGame.CurrentHand.GetHighestBetNotInPot() > 0) {
                    return TurnType.Fold;
                }

                return TurnType.Check;
            }
        }

        private TurnType MakePreflopDecision(out int amount)
        {
            amount = 0;

            Player currentPlayer = _currentGame.CurrentPlayer;
            int bigBlindSize = _currentGame.BigBlindSize;

            if (!HasPlayerPutMoneyInPot()) {
                // open raising
                if (!WasPreflopRaiseMade()) {
                    if (IsHandInPreflopOpenRaiseRange()) {
                        switch (currentPlayer.Position) {
                            case Position.MP2:
                            case Position.MP3:
                            case Position.CO:
                            case Position.SB:
                                amount = (int)(bigBlindSize * 3.5);
                                break;
                            case Position.BU:
                                amount = (int)(bigBlindSize * 2.5);
                                break;
                        }

                        return TurnType.Raise;
                    }
                } else {
                    // preflop raise has been made by another player
                    // TODO: currently preflop callers/limpers are not taken into account at all
                    // TODO: preflop 3bet cold calling range should be completely different from open raise calling range!
                    Position vsPosition = _currentGame.CurrentHand.GetPlayerWithHighestBetPosition();
                    if (IsHandInPreflopColdCallRange(vsPosition)) {
                        return TurnType.Call;
                    } else if (IsHandInPreflop3BetRange(vsPosition)) {
                        // TODO: add different 3bet sizings
                        amount = _currentGame.CurrentHand.GetHighestBetNotInPot() * 3;
                        return TurnType.Raise;
                    }
                }
            } else {
                // 3bet (or more) has been made
                // TODO: check more conditions here
                // TODO: calling 4bets and so on
                Position vsPosition = _currentGame.CurrentHand.GetPlayerWithHighestBetPosition();
                if (IsHandInPreflop3BetCallRange(vsPosition)) {
                    return TurnType.Call;
                } else if (IsHandInPreflop4BetRange(vsPosition)) {
                    amount = (int)(_currentGame.CurrentHand.GetHighestBetNotInPot() * 2.5);
                    return TurnType.Raise;
                } else {
                    return TurnType.Fold;
                }
            }

            return TurnType.Fold;
        }

        private void MakeFlopDecision()
        {

        }

        private void MakeTurnDecision()
        {

        }

        private void MakeRiverDecision()
        {

        }

        private bool IsHandInPreflopOpenRaiseRange()
        {
            Position currentPlayerPosition = _currentGame.CurrentPlayer.Position;
            HoleCards holeCards = _currentGame.CurrentPlayer.HoleCards;

            foreach (HoleCards hand in _preflopOpenRaiseRanges[currentPlayerPosition]) {
                if (hand.First.Rank == holeCards.First.Rank) {
                    if (hand.Second.Rank == holeCards.Second.Rank) {
                        if (hand.IsSuited == holeCards.IsSuited) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool IsHandInPreflopColdCallRange(Position vsPosition)
        {
            Position currentPlayerPosition = _currentGame.CurrentPlayer.Position;
            HoleCards holeCards = _currentGame.CurrentPlayer.HoleCards;

            foreach (HoleCards hand in _preflopColdCallRanges[currentPlayerPosition][vsPosition]) {
                if (hand.First.Rank == holeCards.First.Rank) {
                    if (hand.Second.Rank == holeCards.Second.Rank) {
                        if (hand.IsSuited == holeCards.IsSuited) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool IsHandInPreflop3BetRange(Position vsPosition)
        {
            Position currentPlayerPosition = _currentGame.CurrentPlayer.Position;
            HoleCards holeCards = _currentGame.CurrentPlayer.HoleCards;

            foreach (HoleCards hand in _preflop3BetRanges[currentPlayerPosition][vsPosition]) {
                if (hand.First.Rank == holeCards.First.Rank) {
                    if (hand.Second.Rank == holeCards.Second.Rank) {
                        if (hand.IsSuited == holeCards.IsSuited) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool IsHandInPreflop3BetCallRange(Position vsPosition)
        {
            Position currentPlayerPosition = _currentGame.CurrentPlayer.Position;
            HoleCards holeCards = _currentGame.CurrentPlayer.HoleCards;

            foreach (HoleCards hand in _preflop3BetCallingRanges[currentPlayerPosition][vsPosition]) {
                if (hand.First.Rank == holeCards.First.Rank) {
                    if (hand.Second.Rank == holeCards.Second.Rank) {
                        if (hand.IsSuited == holeCards.IsSuited) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool IsHandInPreflop4BetRange(Position vsPosition)
        {
            Position currentPlayerPosition = _currentGame.CurrentPlayer.Position;
            HoleCards holeCards = _currentGame.CurrentPlayer.HoleCards;

            foreach (HoleCards hand in _preflop4BetRanges[currentPlayerPosition][vsPosition]) {
                if (hand.First.Rank == holeCards.First.Rank) {
                    if (hand.Second.Rank == holeCards.Second.Rank) {
                        if (hand.IsSuited == holeCards.IsSuited) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool HasPlayerPutMoneyInPot()
        {
            return (_currentGame.CurrentPlayer.CurrentBet > _currentGame.BigBlindSize);
        }

        private bool WasPreflopRaiseMade()
        {
            if (!CurrentHandState.Instance.IsPreflop) {
                return true;
            }

            return (_currentGame.CurrentHand.GetHighestBetNotInPot() > _currentGame.BigBlindSize);
        }

        // An open raise and a 3bet were made before our turn
        private bool WerePreflopRaiseAnd3BetMade()
        {
            return false;
        }
    }
}
