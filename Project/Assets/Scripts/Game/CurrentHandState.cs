using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace PokerCats
{
    public enum HandState
    {
        Invalid = -1,
        Preflop,
        Flop,
        Turn,
        River,
        Showdown,
        Count
    };

    public class CurrentHandState : Singleton<CurrentHandState>
	{
        private HandState _currentState;

        public void SetState(HandState state)
        {
            if (!IsStateValid(state)) {
                Debug.LogError("SetState: trying to set invalid state!");
                return;
            }

            _currentState = state;

            switch (state) {
                case HandState.Preflop:
                    break;

                case HandState.Flop:
                    break;

                case HandState.Turn:
                    break;

                case HandState.River:
                    break;

                case HandState.Showdown:
                    break;
            }
        }

        public bool IsStateValid(HandState state)
        {
            if (HandState.Invalid < state && state < HandState.Count) {
                return true;
            }

            return false;
        }

        public bool IsPreflop
        {
            get { return _currentState == HandState.Preflop; }
        }

        public bool IsFlop
        {
            get { return _currentState == HandState.Flop; }
        }

        public bool IsTurn
        {
            get { return _currentState == HandState.Turn; }
        }

        public bool IsRiver
        {
            get { return _currentState == HandState.River; }
        }

        public bool IsShowdown
        {
            get { return _currentState == HandState.Showdown; }
        }
	}
}
