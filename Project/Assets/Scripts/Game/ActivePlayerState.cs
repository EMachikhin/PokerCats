using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace PokerCats
{
    public enum PlayerState {
        Invalid = -1,
        IsAI,
        CanFoldCallRaise,
        CanFoldCheckBet,
        CanFoldCheckRaise,
        CanFoldCall,
        IsAllIn,
        Count
    };

	public class ActivePlayerState : Singleton<ActivePlayerState>
	{
        private PlayerState _currentState;

        public void SetState(PlayerState state)
        {
            if (!IsStateValid(state)) {
                Debug.LogError("SetState: trying to set invalid state!");
                return;
            }

            GameController.Instance.HideAllButtons();

            _currentState = state;

            switch (state) {
                case PlayerState.CanFoldCallRaise:
                    GameController.Instance.ShowFoldButton(true);
                    GameController.Instance.ShowCallButton(true);
                    GameController.Instance.ShowRaiseButton(true);
                    GameController.Instance.ShowBetSliderAndBetAmountInputField(true);
                    break;

                case PlayerState.CanFoldCheckBet:
                    GameController.Instance.ShowFoldButton(true);
                    GameController.Instance.ShowCheckButton(true);
                    GameController.Instance.ShowBetButton(true);
                    GameController.Instance.ShowBetSliderAndBetAmountInputField(true);
                    break;

                case PlayerState.CanFoldCheckRaise:
                    GameController.Instance.ShowFoldButton(true);
                    GameController.Instance.ShowCheckButton(true);
                    GameController.Instance.ShowRaiseButton(true);
                    GameController.Instance.ShowBetSliderAndBetAmountInputField(true);
                    break;

                case PlayerState.CanFoldCall:
                    GameController.Instance.ShowFoldButton(true);
                    GameController.Instance.ShowCallButton(true);
                    break;

                case PlayerState.IsAI:
                case PlayerState.IsAllIn:
                    // TODO: check if we need to do smthg here
                    break;
            }
        }

        public bool IsStateValid(PlayerState state)
        {
            if (PlayerState.Invalid < state && state < PlayerState.Count) {
                return true;
            }

            return false;
        }
	}
}
