using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

namespace PokerCats
{
    // TODO: move A LOT of methods into parent class
	public class RingGame : Game
	{
        public RingGame(GameStartingInfo gameStartingInfo)
        {
            _bigBlindSize = gameStartingInfo.BigBlindSize;
            _anteSize = gameStartingInfo.AnteSize;
            _startingChips = gameStartingInfo.StartingChips;

            _playerCount = gameStartingInfo.PlayerCount;
        }

        public override void PrepareToStart()
        {
            GameController.Instance.OnBlindsPosted = OnBlindsPosted;
            GameController.Instance.OnCardsDealt = OnCardsDealt;
            GameController.Instance.OnPlayerTurnEnded = OnPlayerTurnEnded;
            GameController.Instance.OnStreetDealt = OnStreetDealt;
            GameController.Instance.OnShowdownEnded = OnShowdownEnded;

            CreatePlayers();
            SetPlayersInitialPositions();

            _deck = new Deck();
        }

        public override void Start()
        {
            PlayHand();
        }

        public override void PlayHand()
        {
            _deck.StartNewHand();
            Hand newHand = new Hand();
            newHand.AddPlayers(_players);
            // For the first hand, players positions are already set in Start method
            if (!IsFirstHand) {
                newHand.SetPlayersPositions();
            }
            newHand.SetBlindsAndAntes(_bigBlindSize, _anteSize);
            newHand.PostBlindsAndAntes();
            _hands.Add(newHand);

            CurrentHandState.Instance.SetState(HandState.Preflop);

            Pot mainPot = new Pot();
            newHand.AddPot(mainPot);
            GameController.Instance.AddPot();

            PostBlindsAndAntes();
        }

        public override void PlayTurn()
        {
            Debug.Log("RingGame PlayTurn");
            base.PlayTurn();
            UpdateHUD();
            MakeAITurnIfNeeded();
        }

        public override bool HasEnded()
        {
            return false;
        }

        public override Player GetLocalPlayer()
        {
            foreach (Player player in _players) {
                if (player.IsLocal) {
                    return player;
                }
            }

            return null;
        }

        public override int GetLocalPlayerIndex()
        {
            for (int playerIndex = 0; playerIndex < _playerCount; playerIndex++) {
                Player player = _players[playerIndex];
                if (player.IsLocal) {
                    return playerIndex;
                }
            }

            return -1;
        }

        private void OnBlindsPosted()
        {
            PlaceButtonAndDeck();
            DealCardsToPlayers();
        }

        private void OnCardsDealt()
        {
            // TODO: move this to separate method
            Player localPlayer = GetLocalPlayer();
            PlayerHandInfo playerHandInfo;
            string handTypeString = HandChecker.Instance.GetFullHandTypeString(out playerHandInfo, localPlayer.HoleCards);

            if (string.IsNullOrEmpty(handTypeString))
            {
                Debug.LogError("DealCardsToPlayers: could not evaluate local player's hand.");
                return;
            }

            GameController.Instance.SetHandTypeHintText(handTypeString);

            BeginNewBettingRound();
        }

        private void BeginNewBettingRound()
        {
            Debug.Log("BeginNewBettingRound");
            SetPlayerFirstToAct();
            UpdateHUD();
            GameController.Instance.ClearPlayerActions();
            MakeAITurnIfNeeded();
        }

        private void UpdateHUD()
        {
            GameController.Instance.CurrentPlayerIndex = _currentPlayerIndex;
            GameController.Instance.HighlightActivePlayer(true);
            ActivePlayerState.Instance.SetState(GetActivePlayerState());
            UpdateButtonsText();
            GameController.Instance.StartTurnTimer();
        }

        private void MakeAITurnIfNeeded()
        {
            Debug.Log("MakeAITurnIfNeeded, current player: " + CurrentPlayer.Name);
            if (NeedToMakeAITurn()) {
                GameController.Instance.MakeAITurn();
            }
        }

        private bool NeedToMakeAITurn()
        {
            return CurrentPlayer.IsAI &&
                CurrentHand.IsPlayerInvolved(CurrentPlayer) &&
                !CurrentPlayer.IsAllIn;
        }

        private void CreatePlayers()
        {
            for (int playerIndex = 0; playerIndex < _playerCount; playerIndex++) {
                // TODO: this is for test, return _startingChips
                CreatePlayer(playerIndex, playerIndex * 100 + 500);
            }
        }

        private void CreatePlayer(int playerIndex, int startingChips)
        {
            Player playerToAdd;

            // TODO: this is for debug only
            if (playerIndex == 0) {
                playerToAdd = new Player("Real Player", startingChips, PlayerType.Local);
            } else {
                playerToAdd = new Player("Bot Cat " + playerIndex, startingChips, PlayerType.AI);
            }

            _players.Add(playerToAdd);
            GameController.Instance.CreatePlayer(playerToAdd, playerIndex);
        }

        private void DealCardsToPlayers()
        {
            Dictionary<int, HoleCards> cardsToDeal = new Dictionary<int, HoleCards>();

            int playerIndex = GetPlayerOnTheSmallBlindIndex();
            int playersReady = 0;

            while (playersReady < _playerCount) {
                Player player = _players[playerIndex];

                for (int cardIndex = 0; cardIndex < Defines.HOLE_CARDS_COUNT; cardIndex++) {
                    Card cardToDeal = _deck.DealTopCard();
                    player.AddHoleCard(cardToDeal);
                    // TODO: add check that we have only one local player
                    if (!player.IsLocal) {
                        cardToDeal.IsOpened = false;
                    }
                }

                cardsToDeal.Add(playerIndex, player.HoleCards);

                playerIndex++;
                playersReady++;
                if (playerIndex >= _playerCount) {
                    playerIndex = 0;
                }
            }

            GameController.Instance.DealCards(cardsToDeal);
        }

        private void PlaceButtonAndDeck()
        {
            int playerOnTheButtonIndex = GetPlayerOnTheButtonIndex();
            GameController.Instance.GiveButtonToPlayer(playerOnTheButtonIndex);
            // TODO: decide if we should have separate dealer
            GameController.Instance.GiveDeckToPlayer(playerOnTheButtonIndex);
        }

        private void PostBlindsAndAntes()
        {
            int mainPotSize = 0;
            int smallBlindIndex = 0;
            int bigBlindIndex = 0;

            for (int playerIndex = 0; playerIndex < _playerCount; playerIndex++) {
                Player player = _players[playerIndex];
                if (player != null) {
                    // TODO: in Game controller don't search player at new to post blind after ante (optimization)
                    if (_anteSize > 0) {
                        mainPotSize += _anteSize;
                        GameController.Instance.PostPlayerAnte(playerIndex, _anteSize);
                    }

                    if (player.IsOnSB) {
                        mainPotSize += _bigBlindSize / 2;
                        CurrentHand.MainPot.PlayersInPot.Add(player);
                        smallBlindIndex = playerIndex;
                    } else if (player.IsOnBB) {
                        mainPotSize += _bigBlindSize;
                        CurrentHand.MainPot.PlayersInPot.Add(player);
                        bigBlindIndex = playerIndex;
                    }

                    GameController.Instance.UpdatePlayerCard(playerIndex, player);
                }
            }

            CurrentHand.MainPot.Size += mainPotSize;
            GameController.Instance.UpdatePotSizeTexts(CurrentHand.Pots);

            GameController.Instance.PostBlinds(smallBlindIndex, bigBlindIndex, _bigBlindSize);
        }

        private PlayerState GetActivePlayerState()
        {
            if (CurrentPlayer.IsAI) {
                return PlayerState.IsAI;
            }

            int uncalledBet = CurrentHand.GetHighestBetNotInPot() - CurrentPlayer.CurrentBet;
            bool canCall = uncalledBet > 0;
            bool canCheck = uncalledBet == 0;
            bool canBet = canCheck && !CurrentHandState.Instance.IsPreflop;
            bool canRaise = uncalledBet >= 0 && CurrentPlayer.ChipCount > uncalledBet;

            if (CurrentPlayer.IsAllIn) {
                return PlayerState.IsAllIn;
            } else if (canCall && canRaise) {
                return PlayerState.CanFoldCallRaise;
            } else if (canCheck && canBet) {
                return PlayerState.CanFoldCheckBet;
            } else if (canCheck && canRaise) {
                return PlayerState.CanFoldCheckRaise;
            } else if (canCall) {
                return PlayerState.CanFoldCall;
            }

            return PlayerState.Invalid;
        }

        private void UpdateButtonsText()
        {
            SetCallButtonTextIfNeeded();
            SetBetButtonTextIfNeeded();
            SetRaiseButtonTextIfNeeded();
        }

        private void SetCallButtonTextIfNeeded()
        {
            int uncalledBet = CurrentHand.GetHighestBetNotInPot() - CurrentPlayer.CurrentBet;
            if (uncalledBet > 0) {
                GameController.Instance.SetCallButtonText(uncalledBet.ToString());
            }
        }

        private void SetBetButtonTextIfNeeded()
        {
            int uncalledBet = CurrentHand.GetHighestBetNotInPot() - CurrentPlayer.CurrentBet;
            bool canBet = uncalledBet == 0 && !CurrentHandState.Instance.IsPreflop;
            if (canBet) {
                GameController.Instance.SetBetButtonText(_bigBlindSize.ToString());
            }
        }

        private void SetRaiseButtonTextIfNeeded()
        {
            int uncalledBet = CurrentHand.GetHighestBetNotInPot() - CurrentPlayer.CurrentBet;
            bool canRaise = uncalledBet > 0 && CurrentPlayer.ChipCount > uncalledBet;
            if (canRaise) {
                int minRaiseAmount = Math.Min(CurrentHand.GetHighestBetNotInPot() * 2, CurrentPlayer.ChipCount);
                GameController.Instance.SetRaiseButtonText(minRaiseAmount.ToString());
            }
        }

        private void OnPlayerTurnEnded(TurnType turnType, int amount)
        {
            Debug.Log("OnPlayerTurnEnded");
            if (turnType == TurnType.Invalid) {
                Debug.Log("OnPlayerTurnEnded: invalid turn type.");
                return;
            }

            if (turnType == TurnType.Fold) {
                CurrentHand.RemovePlayer(CurrentPlayer);
            } else if (turnType == TurnType.Call) {
                PutChipsToPot(CurrentPlayer, amount);
                GameController.Instance.UpdatePlayerCard(CurrentPlayerIndex, CurrentPlayer);
                GameController.Instance.UpdatePotSizeTexts(CurrentHand.Pots);
            } else if (turnType == TurnType.Raise) {
                PutChipsToPot(CurrentPlayer, amount);
                GameController.Instance.UpdatePlayerCard(CurrentPlayerIndex, CurrentPlayer);
                GameController.Instance.UpdatePotSizeTexts(CurrentHand.Pots);
            } else if (turnType == TurnType.Bet) {
                PutChipsToPot(CurrentPlayer, amount);
                GameController.Instance.UpdatePlayerCard(CurrentPlayerIndex, CurrentPlayer);
                GameController.Instance.UpdatePotSizeTexts(CurrentHand.Pots);
            } else if (turnType == TurnType.Check) {
                // TODO
            }

            CurrentPlayer.LastTurn = turnType;

            GameController.Instance.HighlightActivePlayer(false);
            GameController.Instance.StopTurnTimer();

            if (!HandHasWinner(CurrentHand)) {
                if (NeedToDealNextStreet()) {
                    DealNextStreet();
                } else {
                    PlayTurn();
                }
            } else {
                EndHand(CurrentHand);
            }
        }

        // TODO: check if we should apply this function for blinds posting
        private void PutChipsToPot(Player player, int amount)
        {
            Pot potToPutChips = null;

            foreach (Pot pot in CurrentHand.Pots) {
                bool hasPlayersAllIn = false;
                foreach (Player playerInPot in pot.PlayersInPot) {
                    if (playerInPot.IsAllIn) {
                        hasPlayersAllIn = true;
                        break;
                    }
                }
                if (!hasPlayersAllIn) {
                    potToPutChips = pot;
                    break;
                }
            }

            if (potToPutChips == null) {
                Pot newPot = new Pot();
                CurrentHand.AddPot(newPot);
                GameController.Instance.AddPot();
                potToPutChips = newPot;
            }

            player.PutChipsToPot(potToPutChips, amount);
        }

        private void SetPlayerFirstToAct()
        {
            int playerFirstToActIndex = CurrentHandState.Instance.IsPreflop ? GetPlayerOnTheBigBlindIndex() + 1 : GetPlayerOnTheButtonIndex() + 1;
            if (playerFirstToActIndex >= _playerCount) {
                playerFirstToActIndex = 0;
            }
            Player playerFirstToAct = _players[playerFirstToActIndex];

            while (!CurrentHand.IsPlayerInvolved(playerFirstToAct)) {
                playerFirstToActIndex++;
                if (playerFirstToActIndex >= _playerCount) {
                    playerFirstToActIndex = 0;
                }
                playerFirstToAct = _players[playerFirstToActIndex];
            }

            _currentPlayerIndex = playerFirstToActIndex;
        }

        private bool AreTherePlayersLeftInHand(Hand hand)
        {
            return hand.HasPlayersLeft();
        }

        private bool HandHasWinner(Hand hand)
        {
            return hand.HasOnePlayerLeft();
        }

        private bool NeedToDealNextStreet()
        {
            bool allThePlayersMadeTurn = true;
            bool noUncalledBets = true;
            int playerNotAllInCount = 0;

            foreach (Player player in CurrentHand.PlayersInvolved) {
                if (player.IsAllIn) {
                    continue;
                }

                playerNotAllInCount++;

                int uncalledBet = CurrentHand.GetHighestBetNotInPot() - player.CurrentBet;
                if (uncalledBet > 0) {
                    noUncalledBets = false;
                }

                if (!player.HasMadeTurn) {
                    allThePlayersMadeTurn = false;
                }
            }

            if (noUncalledBets) {
                if (allThePlayersMadeTurn || playerNotAllInCount <= 1) {
                    return true;
                }
            }

            return false;
        }

        public void DealNextStreet()
        {
            GameController.Instance.PutChipsIntoPot();
            ResetInvolvedPlayersBets();
            ResetLastPlayersTurn();

            if (CurrentHandState.Instance.IsPreflop) {
                DealFlop();
            } else if (CurrentHandState.Instance.IsFlop && GameController.Instance.IsFlopDealt) {
                DealTurn();
            } else if (CurrentHandState.Instance.IsTurn && GameController.Instance.IsTurnDealt) {
                DealRiver();
            } else if (CurrentHandState.Instance.IsRiver && GameController.Instance.IsRiverDealt) {
                BeginShowdown();
            }

            if (!CurrentHandState.Instance.IsShowdown) {
                BeginNewBettingRound();
            }
        }

        private void ResetLastPlayersTurn()
        {
            foreach (Player player in CurrentHand.PlayersInvolved) {
                player.LastTurn = TurnType.NotMade;
            }
        }

        private void ResetInvolvedPlayersBets()
        {
            foreach (Player player in CurrentHand.PlayersInvolved) {
                player.CurrentBet = 0;
            }
        }

        private void ResetAllPlayersBets()
        {
            foreach (Player player in _players) {
                player.CurrentBet = 0;
            }
        }

        private void UpdatePlayersHands()
        {
            foreach (Player player in CurrentHand.PlayersInvolved) {
                if (player.IsLocal) {
                    UpdateLocalPlayerHand(player);
                } else {
                    PlayerHandInfo playerHandInfo = HandChecker.Instance.GetPlayerHandInfo(player.HoleCards, _board.FullBoard);

                    if (playerHandInfo.HandType == HandType.Invalid) {
                        Debug.LogError("UpdatePlayersHands: could not get player hand type.");
                        continue;
                    }

                    player.CurrentHandInfo = playerHandInfo;
                }
            }
        }

        private void UpdateLocalPlayerHand(Player localPlayer)
        {
            if (!localPlayer.IsLocal) {
                Debug.LogError("UpdateLocalPlayerHand: player is not local!");
                return;
            }

            PlayerHandInfo playerHandInfo;
            String handTypeString = HandChecker.Instance.GetFullHandTypeString(out playerHandInfo, localPlayer.HoleCards, _board.FullBoard);

            if (playerHandInfo.HandType == HandType.Invalid) {
                Debug.LogError("UpdateLocalPlayerHand: could not get local player hand type.");
                return;
            }

            localPlayer.CurrentHandInfo = playerHandInfo;

            if (string.IsNullOrEmpty(handTypeString)) {
                Debug.LogError("UpdateLocalPlayerHand: could not get local player hand type string.");
                return;
            }

            GameController.Instance.SetHandTypeHintText(handTypeString);
        }

        private void DealFlop()
        {
            _board.Flop = _deck.DealFlop();
            CurrentHandState.Instance.SetState(HandState.Flop);
            GameController.Instance.DealFlop(_board.Flop);
        }

        private void DealTurn()
        {
            _board.TurnCard = _deck.DealTurnOrRiver();
            CurrentHandState.Instance.SetState(HandState.Turn);
            GameController.Instance.DealTurn(_board.TurnCard);
        }

        private void DealRiver()
        {
            _board.RiverCard = _deck.DealTurnOrRiver();
            CurrentHandState.Instance.SetState(HandState.River);
            GameController.Instance.DealRiver(_board.RiverCard);
        }

        private void OnStreetDealt()
        {
            UpdatePlayersHands();
            if (NeedToDealNextStreet()) {
                DealNextStreet();
            }
        }

        private void BeginShowdown()
        {
            // TODO: add muck possibility
            CurrentHandState.Instance.SetState(HandState.Showdown);

            List<int> playerIndexes = new List<int>();

            for (int playerIndex = 0; playerIndex < _playerCount; playerIndex++) {
                Player player = _players[playerIndex];
                if (CurrentHand.IsPlayerInvolved(player) && !player.IsLocal) {
                    playerIndexes.Add(playerIndex);
                }
            }

            GameController.Instance.ProcessShowdown(playerIndexes);
        }

        private void OnShowdownEnded()
        {
            EndHand(CurrentHand);
        }

        private void EndHand(Hand hand)
        {
            hand.GivePotsToWinners();
            for (int playerIndex = 0; playerIndex < _playerCount; playerIndex++) {
                Player player = _players[playerIndex];
                GameController.Instance.UpdatePlayerCard(playerIndex, player);
                player.ClearHoleCards();
            }
            ResetAllPlayersBets();
            ClearBoard();

            GameController.Instance.EndHand();

            if (!HasEnded()) {
                PlayHand();
            }
        }
	}
}
