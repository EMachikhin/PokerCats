using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System;
using System.Collections;
using System.Collections.Generic;

namespace PokerCats
{
    public enum TurnType
    {
        Invalid = -1,
        NotMade,
        Fold,
        Check,
        Call,
        Bet,
        Raise,
        Count
    };

    public class GameController : SingletonMonoBehaviour<GameController>
    {
        public GameObject CardPrefab;
        public GameObject OpponentCardPrefab;

        public GameObject Board;
        public GameObject PotsParent;

        public GameObject PlayersParent;

        public GameObject ChipPrefab;
        public GameObject ButtonPrefab;
        public GameObject DeckPrefab;

        // Buttons
        public RectTransform FoldButton;
        public RectTransform CheckButton;
        public RectTransform CallButton;
        public RectTransform BetButton;
        public RectTransform RaiseButton;

        public InputField BetInputField;
        public Slider BetSlider;

        public Text HandTypeHintText;

        // Actions
        public Action OnBlindsPosted;
        public Action OnCardsDealt;
        public Action<TurnType, int> OnPlayerTurnEnded;
        public Action OnStreetDealt;
        public Action OnShowdownEnded;

        private Game _currentGame;
        private ChipDenominations[] _chipDenominations;
        private int _currentPlayerIndex = -1;
        public int CurrentPlayerIndex
        {
            get { return _currentPlayerIndex; }
            set { _currentPlayerIndex = value; }
        }

        private int _currentAmountToBet;

        public bool IsFlopDealt { get; set; }
        public bool IsTurnDealt { get; set; }
        public bool IsRiverDealt { get; set; }

        private PlayerWidget _localPlayerWidget;
        private bool _isGameInProgress;

        private bool _isAITurnDelayInProgress;

        void Start()
        {
            PrepareGame();
        }

        void Update()
        {
            if (!_isGameInProgress) {
                if (_localPlayerWidget != null) {
                    if (!_localPlayerWidget.IsSeatEmpty) {
                        _isGameInProgress = true;
                        StartGame();
                    }
                }
            }
        }

        private void PrepareGame()
        {
            _chipDenominations = (ChipDenominations[])Enum.GetValues(typeof(ChipDenominations));

            // test
            GameStartingInfo gameStartingInfo;
            gameStartingInfo.BigBlindSize = 10;
            gameStartingInfo.AnteSize = 0;
            gameStartingInfo.StartingChips = 1500;
            gameStartingInfo.PlayerCount = 6;

            new ActivePlayerState();
            new CurrentHandState();
            new HandChecker();
            new PlayerAI();

            // TODO: initialize this earlier
            new JSONReader();
            JSONReader.Instance.PrepareConfig();

            _currentGame = new RingGame(gameStartingInfo);
            _currentGame.PrepareToStart();

            PlayerAI.Instance.CurrentGame = _currentGame;
        }

        private void StartGame()
        {
            if (_currentGame != null) {
                _currentGame.Start();
            }
        }

        public void CreatePlayer(Player newPlayer, int playerIndex)
        {
            GameObject playerObject = PlayersParent.transform.Find("Player" + playerIndex).gameObject;
            PlayerWidget playerWidget = playerObject.GetComponent<PlayerWidget>();
            playerWidget.PlayerLink = newPlayer;
            playerObject.name = "Player" + playerIndex;

            Transform stackSizeTransform = playerObject.transform.Find("PlayerCard/StackSizeText");
            stackSizeTransform.gameObject.GetComponent<Text>().text = newPlayer.ChipCount.ToString();

            Transform playerNameTransform = playerObject.transform.Find("PlayerCard/PlayerNameText");
            playerNameTransform.gameObject.GetComponent<Text>().text = newPlayer.Name.ToString();

            if (newPlayer.Type == PlayerType.Local) {
                _localPlayerWidget = playerWidget;
            }
        }

        public void DealCards(Dictionary<int, HoleCards> cardsToDeal)
        {
            if (cardsToDeal == null) {
                Debug.LogError("GameController.DealCards: cardsToDeal is null!");
                return;
            }

            StartCoroutine(DealCardsCoroutine(cardsToDeal));
        }

        // TODO: create animation controller class and move this method there
        private IEnumerator DealCardsCoroutine(Dictionary<int, HoleCards> cardsToDeal)
        {
            int localPlayerIndex = _currentGame.GetLocalPlayerIndex();

            if (localPlayerIndex < 0) {
                Debug.LogError("GameController.DealCardsCoroutine: could not get local player index!");
                yield break;
            }

            // TODO: this is only for 2 hole cards case
            for (int cardIndex = 0; cardIndex < Defines.HOLE_CARDS_COUNT; cardIndex++) {
                foreach (KeyValuePair<int, HoleCards> cards in cardsToDeal) {
                    int playerIndex = cards.Key;
                    Card cardToDeal = cardIndex == 0 ? cards.Value.First : cards.Value.Second;
                    bool isLocalPlayer = (playerIndex == localPlayerIndex);

                    // TODO: check if deck object is active
                    int dealerIndex = _currentGame.GetPlayerOnTheButtonIndex();
                    Transform dealerTransform = PlayersParent.transform.Find("Player" + dealerIndex);
                    Transform deckTransform = dealerTransform.Find("Deck");

                    Transform playerTransform = PlayersParent.transform.Find("Player" + playerIndex);
                    Transform holeCardsTransform = playerTransform.Find("HoleCards");

                    GameObject cardToDealObject = GameObject.Instantiate(isLocalPlayer ? CardPrefab : OpponentCardPrefab);
                    CardWidget cardToDealWidget = cardToDealObject.GetComponent<CardWidget>();
                    cardToDealWidget.CardLink = cardToDeal;

                    cardToDealObject.transform.parent = deckTransform;

                    Vector3 startingCardPosition = deckTransform.position;
                    Vector3 finalCardPosition = holeCardsTransform.position;

                    Vector3 startingCardScale = new Vector3(0.1f, 0.1f, 0.1f);
                    Vector3 finalCardScale = Vector3.one;

                    float cardDealingTime = 0;

                    while (cardDealingTime < Defines.CARD_DEALING_TIME) {
                        cardDealingTime += Time.deltaTime;
                        float timeSpentPercentage = cardDealingTime / Defines.CARD_DEALING_TIME;
                        cardToDealObject.transform.position = Vector3.Lerp(startingCardPosition, finalCardPosition, timeSpentPercentage);
                        cardToDealObject.transform.localScale = Vector3.Lerp(startingCardScale, finalCardScale, timeSpentPercentage);
                        yield return null;
                    }

                    cardToDealObject.transform.parent = holeCardsTransform;
                }
            }

            if (OnCardsDealt != null) {
                OnCardsDealt();
            }
        }

        private TurnTimer GetCurrentPlayerTurnTimer()
        {
            Transform currentPlayerTransform = PlayersParent.transform.Find("Player" + CurrentPlayerIndex);
            if (currentPlayerTransform == null) {
                return null;
            }

            Transform timerTransform = currentPlayerTransform.Find("TurnTimer");
            if (timerTransform == null) {
                return null;
            }

            TurnTimer timer = timerTransform.gameObject.GetComponent<TurnTimer>();
            return timer;
        }

        public void StartTurnTimer()
        {
            TurnTimer timer = GetCurrentPlayerTurnTimer();
            if (timer != null) {
                timer.StartTimer();
            } else {
                Debug.LogError("GameController.StartTurnTimer: timer is null!");
            }
        }

        public void StopTurnTimer()
        {
            TurnTimer timer = GetCurrentPlayerTurnTimer();
            if (timer != null) {
                timer.StopTimer();
            } else {
                Debug.LogError("GameController.StopTurnTimer: timer is null!");
            }
        }

        public void EndPlayerTurnOnTimeout()
        {
            int uncalledBet = _currentGame.CurrentHand.GetHighestBetNotInPot() - _currentGame.CurrentPlayer.CurrentBet;
            bool canCheck = uncalledBet == 0;

            if (canCheck) {
                DisplayPlayerActionText(CurrentPlayerIndex, TurnType.Check);
                OnPlayerTurnEnded(TurnType.Check, 0);
            } else {
                DisplayPlayerActionText(CurrentPlayerIndex, TurnType.Fold);
                OnPlayerTurnEnded(TurnType.Fold, 0);
            }
        }

        public void DealFlop(List<Card> flopCards)
        {
            if (flopCards.Count != Defines.FLOP_CARDS_COUNT) {
                Debug.LogError("GameController.DealFlop: invalid amount of flop cards to deal.");
                return;
            }

            StartCoroutine(DealFlopCoroutine(flopCards));
        }

        private IEnumerator DealFlopCoroutine(List<Card> flopCards)
        {
            List<Card> cachedFlopCards = new List<Card>();
            cachedFlopCards.AddRange(flopCards);
            int flopCardIndex = 0;

            yield return new WaitForSeconds(Defines.DELAY_BETWEEN_DEALING_STREETS);

            foreach (Card flopCard in cachedFlopCards) {
                DealBoardCard(flopCardIndex, flopCard);
                flopCardIndex++;
                yield return new WaitForSeconds(Defines.DELAY_BETWEEN_DEALING_CARDS);
            }

            IsFlopDealt = true;

            if (OnStreetDealt != null) {
                OnStreetDealt();
            }
        }

        public void DealTurn(Card turnCard)
        {
            StartCoroutine(DealTurnCoroutine(turnCard));
        }

        private IEnumerator DealTurnCoroutine(Card turnCard)
        {
            Card cachedTurnCard = turnCard;
            int turnCardIndex = Defines.FLOP_CARDS_COUNT;

            while (!IsFlopDealt) {
                yield return null;
            }

            yield return new WaitForSeconds(Defines.DELAY_BETWEEN_DEALING_STREETS);

            DealBoardCard(turnCardIndex, cachedTurnCard);

            IsTurnDealt = true;

            if (OnStreetDealt != null) {
                OnStreetDealt();
            }
        }

        public void DealRiver(Card riverCard)
        {
            StartCoroutine(DealRiverCoroutine(riverCard));
        }

        private IEnumerator DealRiverCoroutine(Card riverCard)
        {
            Card cachedRiverCard = riverCard;
            int riverCardIndex = Defines.CARDS_COUNT_ON_TURN;

            while (!IsTurnDealt) {
                yield return null;
            }

            yield return new WaitForSeconds(Defines.DELAY_BETWEEN_DEALING_STREETS);

            DealBoardCard(riverCardIndex, cachedRiverCard);

            // TODO: move this somewhere else (it's to prevent immediate showdown when all the players are all in before river)
            yield return new WaitForSeconds(Defines.DELAY_BETWEEN_DEALING_STREETS);

            IsRiverDealt = true;

            if (OnStreetDealt != null) {
                OnStreetDealt();
            }
        }

        public void DealBoardCard(int cardIndex, Card card)
        {
            GameObject cardObject = GameObject.Instantiate(CardPrefab);
            CardWidget cardWidget = cardObject.GetComponent<CardWidget>();
            cardWidget.CardLink = card;

            Vector3 cardPosition = Board.transform.position;
            cardPosition.x += cardWidget.ImageWidth * cardIndex;

            cardObject.transform.parent = Board.transform;
            cardObject.transform.localPosition = cardPosition;
            cardObject.transform.localScale = Vector3.one;
        }

        public void PostBlinds(int smallBlindIndex, int bigBlindIndex, int bigBlindSize)
        {
            StartCoroutine(PostBlindsCoroutine(smallBlindIndex, bigBlindIndex, bigBlindSize));
        }

        private IEnumerator PostBlindsCoroutine(int smallBlindIndex, int bigBlindIndex, int bigBlindSize)
        {
            yield return new WaitForSeconds(Defines.DELAY_IN_BLIND_POSTING);

            PostPlayerBlind(smallBlindIndex, bigBlindSize / 2);

            yield return new WaitForSeconds(Defines.DELAY_IN_BLIND_POSTING);

            PostPlayerBlind(bigBlindIndex, bigBlindSize);

            yield return new WaitForSeconds(Defines.DELAY_IN_BLIND_POSTING);

            if (OnBlindsPosted != null) {
                OnBlindsPosted();
            }
        }

        private void PostPlayerBlind(int playerIndex, int blindSize)
        {
            Transform playerTransform = PlayersParent.transform.Find("Player" + playerIndex);
            Transform blindsTransform = playerTransform.Find("Blinds");
            PlayerWidget playerWidget = playerTransform.gameObject.GetComponent<PlayerWidget>();

            if (playerWidget == null) {
                Debug.LogError("GameController.PostPlayerBlind: PlayerWidget is null. Player index = " + playerIndex);
                return;
            }

            ChipAlignment chipAlignment = playerWidget.ChipAlignment;
            PutChipsInfrontPlayer(blindsTransform, blindSize, chipAlignment);
        }

        public void PostPlayerAnte(int playerIndex, int anteSize)
        {

        }

        public void GiveButtonToPlayer(int playerIndex)
        {
            Transform playerTransform = PlayersParent.transform.Find("Player" + playerIndex);
            Transform buttonTransform = playerTransform.Find("Button");
            GameObject buttonObject = GameObject.Instantiate(ButtonPrefab);

            buttonObject.transform.parent = buttonTransform;
            buttonObject.transform.localPosition = buttonTransform.position;
            buttonObject.transform.localScale = Vector3.one;
        }

        public void GiveDeckToPlayer(int playerIndex)
        {
            Transform playerTransform = PlayersParent.transform.Find("Player" + playerIndex);
            Transform deckTransform = playerTransform.Find("Deck");
            GameObject deckObject = GameObject.Instantiate(DeckPrefab);

            deckObject.transform.parent = deckTransform;
            deckObject.transform.localPosition = deckTransform.position;
            deckObject.transform.localScale = Vector3.one;
        }

        public void UpdatePlayerCard(int playerIndex, Player player)
        {
            // TODO: cache this transform to reduce Find calls
            Transform playerTransform = PlayersParent.transform.Find("Player" + playerIndex);

            Transform stackSizeTransform = playerTransform.Find("PlayerCard/StackSizeText");
            stackSizeTransform.gameObject.GetComponent<Text>().text = player.IsAllIn ? "All-in!" : player.ChipCount.ToString();
        }

        public void HighlightActivePlayer(bool highlight)
        {
            Transform currentPlayerTransform = PlayersParent.transform.Find("Player" + CurrentPlayerIndex);

            if (currentPlayerTransform == null) {
                return;
            }

            Transform playerAvatarFrame = currentPlayerTransform.Find("Avatar/AvatarFrame");

            if (playerAvatarFrame == null) {
                return;
            }

            Image playerAvatarFrameImage = playerAvatarFrame.GetComponent<Image>();

            if (playerAvatarFrameImage == null) {
                return;
            }

            playerAvatarFrameImage.color = highlight ? Color.green : Color.white;
        }

        public void AddPot()
        {
            for (int potIndex = 0; potIndex < PotsParent.transform.childCount; potIndex++) {
                Transform potTransform = PotsParent.transform.Find("Pot" + potIndex);
                if (potTransform == null) {
                    Debug.LogError("PutChipsIntoPot: could not find pot transform, pot index = " + potIndex);
                    break;
                }

                if (!potTransform.gameObject.activeSelf) {
                    potTransform.gameObject.SetActive(true);
                    break;
                }
            }
        }

        public void PutChipsIntoPot()
        {
            bool areThereChipsNotInPot = false;

            for (int playerIndex = 0; playerIndex < _currentGame.Players.Count; playerIndex++) {
                Player player = _currentGame.Players[playerIndex];
                Transform playerTransform = PlayersParent.transform.Find("Player" + playerIndex);
                Transform blindsTransform = playerTransform.Find("Blinds");

                for (int chipIndex = blindsTransform.childCount - 1; chipIndex >= 0; chipIndex--) {
                    if (!areThereChipsNotInPot) {
                        areThereChipsNotInPot = true;
                    }
                    Transform childTransform = blindsTransform.GetChild(chipIndex);
                    GameObject.Destroy(childTransform.gameObject);
                }

                blindsTransform.DetachChildren();
            }

            if (!areThereChipsNotInPot) {
                return;
            }

            int potIndex = 0;
            // TODO: reset chipPosition.y between pots
            foreach (Pot pot in _currentGame.CurrentHand.Pots) {
                Transform potTransform = PotsParent.transform.Find("Pot" + potIndex);
                if (potTransform == null) {
                    Debug.LogError("PutChipsIntoPot: could not find pot transform, pot index = " + potIndex);
                    break;
                }

                int potSize = pot.Size;

                for (int chipDenominationIndex = _chipDenominations.Length - 1; chipDenominationIndex >= 0; chipDenominationIndex--) {
                    int denomination = (int)_chipDenominations[chipDenominationIndex];
                    if (denomination > potSize) {
                        continue;
                    }

                    while (potSize >= denomination) {
                        GameObject chipObject = GameObject.Instantiate(ChipPrefab);
                        ChipWidget chipWidget = chipObject.GetComponent<ChipWidget>();
                        chipWidget.Denomination = _chipDenominations[chipDenominationIndex];

                        Vector3 chipPosition = potTransform.position;

                        // one child is pot size text
                        int chipsCount = potTransform.childCount - 1;
                        if (chipsCount > 0) {
                            int chipsInStack = chipsCount;
                            int stackIndex = 0;
                            if (chipsCount >= Defines.MAX_CHIPS_IN_ONE_STACK_IN_BETS) {
                                stackIndex = chipsCount / Defines.MAX_CHIPS_IN_ONE_STACK_IN_BETS;
                                chipsInStack = chipsCount % Defines.MAX_CHIPS_IN_ONE_STACK_IN_BETS;
                            }
                            // TODO: correct this magic
                            if (stackIndex > 0) {
                                chipPosition.x += (chipWidget.ImageWidth * stackIndex);
                            }
                            chipPosition.y += (chipWidget.ImageHeight * chipsInStack) / 6;
                        }
                        chipObject.transform.parent = potTransform;

                        chipObject.transform.localPosition = chipPosition;
                        chipObject.transform.localScale = Vector3.one;

                        potSize -= denomination;
                    }
                }

                if (potSize > 0) {
                    Debug.LogError("PutChipsIntoPot: wrong bet amount (could not be presented with current chip denominations).");
                }

                potIndex++;
            }

                // old variant with all the players chips in pot unchanged
                //if (player.CurrentBet > 0) {
                //    Transform playerTransform = PlayersParent.transform.Find("Player" + playerIndex);
                //    Transform blindsTransform = playerTransform.Find("Blinds");

                //    for (int chipIndex = blindsTransform.childCount - 1; chipIndex >= 0; chipIndex--) {
                //        Transform childTransform = blindsTransform.GetChild(chipIndex);
                //        GameObject chipObject = childTransform.gameObject;
                //        ChipWidget chipWidget = chipObject.GetComponent<ChipWidget>();
                //        Vector3 chipPosition = Pot.transform.position;
                //        int chipsCountInPot = Pot.transform.childCount;

                //        if (chipsCountInPot > 0) {
                //            int chipsInStack = chipsCountInPot;
                //            int stackColumn = 0, stackRow = 0;
                //            if (chipsCountInPot >= Defines.MAX_CHIPS_IN_ONE_STACK_IN_BETS) {
                //                stackColumn = (chipsCountInPot / Defines.MAX_CHIPS_IN_ONE_STACK_IN_BETS) % Defines.MAX_STACKS_IN_ONE_ROW_IN_BETS;
                //                stackRow = (chipsCountInPot / Defines.MAX_CHIPS_IN_ONE_STACK_IN_BETS) / Defines.MAX_STACKS_IN_ONE_ROW_IN_BETS;
                //                chipsInStack = chipsCountInPot % Defines.MAX_CHIPS_IN_ONE_STACK_IN_BETS;
                //            }
                //            // TODO: correct this magic
                //            if (stackColumn > 0) {
                //                chipPosition.x += (chipWidget.ImageWidth * stackColumn);
                //            }
                //            if (stackRow > 0) {
                //                chipPosition.y -= (chipWidget.ImageHeight * stackRow);
                //            }
                //            chipPosition.y += (chipWidget.ImageHeight * chipsInStack) / 6;
                //        }

                //        childTransform.parent = Pot.transform;
                //        childTransform.localPosition = chipPosition;
                //    }

                //    blindsTransform.DetachChildren();
                //}
            //}

            _currentAmountToBet = 0;
        }

        public void ClearPlayerActions()
        {
            for (int playerIndex = 0; playerIndex < _currentGame.Players.Count; playerIndex++) {
                Transform playerTransform = PlayersParent.transform.Find("Player" + playerIndex);
                Transform playerCard = playerTransform.Find("PlayerCard");
                if (playerCard != null) {
                    Transform playerActionTransform = playerTransform.Find("PlayerCard/LastActionText");
                    if (playerActionTransform != null) {
                        Text playerActionText = playerActionTransform.gameObject.GetComponent<Text>();
                        if (playerActionText != null) {
                            playerActionText.text = "";
                        }
                    }
                }
            }
        }

        public void ProcessShowdown(List<int> playerIndexes)
        {
            StartCoroutine(ShowPlayersCards(playerIndexes));
        }

        private IEnumerator ShowPlayersCards(List<int> playerIndexes)
        {
            // TODO: check the order of players
            foreach (int playerIndex in playerIndexes)
            {
                Transform playerTransform = PlayersParent.transform.Find("Player" + playerIndex);
                Transform holeCardsTransform = playerTransform.Find("HoleCards");

                for (int cardIndex = holeCardsTransform.childCount - 1; cardIndex >= 0; cardIndex--)
                {
                    Transform childTransform = holeCardsTransform.GetChild(cardIndex);
                    CardWidget cardWidget = childTransform.gameObject.GetComponent<CardWidget>();

                    if (cardWidget != null)
                    {
                        // TODO: move to separate method
                        Card card = cardWidget.CardLink;
                        card.IsOpened = true;
                        cardWidget.CardLink = card;
                    }
                }

                yield return new WaitForSeconds(Defines.SHOW_CARDS_TIME);
            }

            if (OnShowdownEnded != null){
                OnShowdownEnded();
            }
        }

        public void EndHand()
        {
            ClearPlayers();
            ClearBoard();
            ResetBetSliderAndInput();
        }

        public void UpdatePotSizeTexts(List<Pot> pots)
        {
            int potIndex = 0;
            foreach (Pot pot in pots) {
                Transform potTransform = PotsParent.transform.Find("Pot" + potIndex + "/PotSizeText");
                if (potTransform == null) {
                    Debug.LogError("UpdatePotSizeTexts: could not find pot transform, pot index = " + potIndex);
                    break;
                }

                Text potSizeText = potTransform.gameObject.GetComponent<Text>();
                if (potSizeText == null) {
                    Debug.LogError("UpdatePotSizeTexts: could not find pot size text, pot index = " + potIndex);
                    break;
                }

                string potSizeString = (potIndex == 0) ? "Main pot: " : "Side pot: ";
                potSizeString += pot.Size.ToString();

                potSizeText.text = potSizeString;

                potIndex++;
            }
        }

        public void HideAllButtons()
        {
            ShowFoldButton(false);
            ShowCheckButton(false);
            ShowCallButton(false);
            ShowBetButton(false);
            ShowRaiseButton(false);
            ShowBetSliderAndBetAmountInputField(false);
        }

        public void ShowFoldButton(bool show)
        {
            if (FoldButton != null) {
                FoldButton.gameObject.SetActive(show);
            }
        }

        public void ShowCheckButton(bool show)
        {
            if (CheckButton != null) {
                CheckButton.gameObject.SetActive(show);
            }
        }

        public void ShowCallButton(bool show)
        {
            if (CallButton != null) {
                CallButton.gameObject.SetActive(show);
            }
        }

        public void SetCallButtonText(string text)
        {
            if (CallButton != null) {
                Text CallButtonText = CallButton.GetComponentInChildren<Text>();
                CallButtonText.text = "Call " + text;
            }
        }

        public void ShowBetButton(bool show)
        {
            if (BetButton != null) {
                BetButton.gameObject.SetActive(show);
            }
        }

        public void SetBetButtonText(string text)
        {
            if (BetButton != null) {
                Text BetButtonText = BetButton.GetComponentInChildren<Text>();
                if (BetButtonText != null) {
                    BetButtonText.text = "Bet " + text;
                }
            }
        }

        public void ShowRaiseButton(bool show)
        {
            if (RaiseButton != null) {
                RaiseButton.gameObject.SetActive(show);
            }
        }

        public void SetRaiseButtonText(string text)
        {
            if (RaiseButton != null) {
                Text RaiseButtonText = RaiseButton.GetComponentInChildren<Text>();
                if (RaiseButtonText != null) {
                    RaiseButtonText.text = "Raise " + text;
                }
            }
        }

        public void ShowBetSliderAndBetAmountInputField(bool show)
        {
            if (BetSlider != null) {
                BetSlider.gameObject.SetActive(show);
            }

            if (BetInputField != null) {
                BetInputField.gameObject.SetActive(show);
            }
        }

        public void OnBackClicked()
        {
            SceneManager.LoadScene("Main");
        }

        public void OnFoldClicked()
        {
            if (CurrentPlayerIndex < 0) {
                Debug.LogError("OnFoldClicked: invalid current player index.");
                return;
            }

            DisplayPlayerActionText(CurrentPlayerIndex, TurnType.Fold);

            StartCoroutine(FoldAnimation());
        }

        private IEnumerator FoldAnimation()
        {
            int dealerIndex = _currentGame.GetPlayerOnTheButtonIndex();
            Transform dealerTransform = PlayersParent.transform.Find("Player" + dealerIndex);
            Transform deckTransform = dealerTransform.Find("Deck");

            Transform playerTransform = PlayersParent.transform.Find("Player" + CurrentPlayerIndex);
            Transform holeCardsTransform = playerTransform.Find("HoleCards");

            Vector3 startingCardPosition = holeCardsTransform.position;
            Vector3 finalCardPosition = deckTransform.position;

            Vector3 startingCardScale = Vector3.one;
            Vector3 finalCardScale = new Vector3(0.1f, 0.1f, 0.1f);

            for (int cardIndex = holeCardsTransform.childCount - 1; cardIndex >= 0; cardIndex--) {
                Transform cardTransform = holeCardsTransform.GetChild(cardIndex);

                float cardDealingTime = 0;

                while (cardDealingTime < Defines.CARD_DEALING_TIME) {
                    cardDealingTime += Time.deltaTime;
                    float timeSpentPercentage = cardDealingTime / Defines.CARD_DEALING_TIME;
                    cardTransform.position = Vector3.Lerp(startingCardPosition, finalCardPosition, timeSpentPercentage);
                    cardTransform.localScale = Vector3.Lerp(startingCardScale, finalCardScale, timeSpentPercentage);
                    yield return null;
                }

                GameObject.Destroy(cardTransform.gameObject);
            }

            holeCardsTransform.DetachChildren();

            if (OnPlayerTurnEnded != null) {
                OnPlayerTurnEnded(TurnType.Fold, 0);
            }
        }

        public void OnCheckClicked()
        {
            DisplayPlayerActionText(CurrentPlayerIndex, TurnType.Check);

            if (OnPlayerTurnEnded != null) {
                OnPlayerTurnEnded(TurnType.Check, 0);
            }
        }

        public void OnCallClicked()
        {
            Transform playerTransform = PlayersParent.transform.Find("Player" + CurrentPlayerIndex);
            Transform blindsTransform = playerTransform.Find("Blinds");

            PlayerWidget playerWidget = playerTransform.gameObject.GetComponent<PlayerWidget>();

            if (playerWidget == null) {
                Debug.LogError("GameController.OnCallClicked: PlayerWidget is null. Player index = " + CurrentPlayerIndex);
                return;
            }

            ChipAlignment chipAlignment = playerWidget.ChipAlignment;

            Hand currentHand = _currentGame.CurrentHand;
            Player currentPlayer = _currentGame.CurrentPlayer;

            int amountToCall = Math.Min(currentHand.GetHighestBetNotInPot() - currentPlayer.CurrentBet, currentPlayer.ChipCount);
            PutChipsInfrontPlayer(blindsTransform, amountToCall, chipAlignment);

            DisplayPlayerActionText(CurrentPlayerIndex, TurnType.Call, amountToCall);

            if (OnPlayerTurnEnded != null) {
                OnPlayerTurnEnded(TurnType.Call, amountToCall);
            }
        }

        public void OnBetClicked()
        {
            Transform playerTransform = PlayersParent.transform.Find("Player" + CurrentPlayerIndex);
            Transform blindsTransform = playerTransform.Find("Blinds");

            PlayerWidget playerWidget = playerTransform.gameObject.GetComponent<PlayerWidget>();

            if (playerWidget == null) {
                Debug.LogError("GameController.OnBetClicked: PlayerWidget is null. Player index = " + CurrentPlayerIndex);
                return;
            }

            ChipAlignment chipAlignment = playerWidget.ChipAlignment;

            int amountToBet = _currentAmountToBet > 0 ? _currentAmountToBet : _currentGame.BigBlindSize;
            PutChipsInfrontPlayer(blindsTransform, amountToBet, chipAlignment);

            DisplayPlayerActionText(CurrentPlayerIndex, TurnType.Bet, amountToBet);

            if (OnPlayerTurnEnded != null) {
                OnPlayerTurnEnded(TurnType.Bet, amountToBet);
            }
        }

        public void OnRaiseClicked()
        {
            Transform playerTransform = PlayersParent.transform.Find("Player" + CurrentPlayerIndex);
            Transform blindsTransform = playerTransform.Find("Blinds");

            PlayerWidget playerWidget = playerTransform.gameObject.GetComponent<PlayerWidget>();

            if (playerWidget == null) {
                Debug.LogError("GameController.OnRaiseClicked: PlayerWidget is null. Player index = " + CurrentPlayerIndex);
                return;
            }

            ChipAlignment chipAlignment = playerWidget.ChipAlignment;

            Hand currentHand = _currentGame.CurrentHand;
            Player currentPlayer = _currentGame.CurrentPlayer;

            int defaultAmountToRaise = Math.Min(currentHand.GetHighestBetNotInPot() * 2 - currentPlayer.CurrentBet, currentPlayer.ChipCount);
            int amountToRaise = _currentAmountToBet > 0 ? _currentAmountToBet : defaultAmountToRaise;
            PutChipsInfrontPlayer(blindsTransform, amountToRaise, chipAlignment);

            DisplayPlayerActionText(CurrentPlayerIndex, TurnType.Raise, amountToRaise);

            if (OnPlayerTurnEnded != null) {
                OnPlayerTurnEnded(TurnType.Raise, amountToRaise);
            }
        }

        public void OnBetAmountEntered()
        {
            Debug.Log("OnBetAmountEntered");
            string amountString = BetInputField.text;
            int amount;
            bool isParsed = int.TryParse(amountString, out amount);

            if (!isParsed) {
                Debug.LogError("OnBetAmountEntered: invalid amount entered!");
                return;
            }

            _currentAmountToBet = amount;

            if (BetButton.gameObject.activeSelf) {
                SetBetButtonText(amountString);
            } else if (RaiseButton.gameObject.activeSelf) {
                SetRaiseButtonText(amountString);
            }

            // TODO: adjust bet slider(incorrect now!)
            if (BetSlider != null && BetSlider.gameObject.activeSelf) {
                int maxAmount = _currentGame.CurrentPlayer.ChipCount;
                float value = (float)amount / (float)maxAmount;

                if (BetSlider.value != value) {
                    BetSlider.value = value;
                }
            }
        }

        public void OnBetSliderMoved()
        {
            Debug.Log("OnBetSliderMoved");
            Hand currentHand = _currentGame.CurrentHand;
            Player currentPlayer = _currentGame.CurrentPlayer;

            int minAmount = currentHand.GetHighestBetNotInPot() * 2;
            int maxAmount = _currentGame.CurrentPlayer.ChipCount;
            float value = BetSlider.value;
            int amount = (int)(maxAmount * value);
            int amountInBB = amount / _currentGame.BigBlindSize;
            amount = amountInBB * _currentGame.BigBlindSize;

            amount = Math.Max(amount, minAmount);

            // adjust bet input field
            if (BetInputField != null && BetInputField.gameObject.activeSelf) {
                int currentBetFieldValue;
                // TODO: do we need isParsed here?
                bool isParsed = int.TryParse(BetInputField.text, out currentBetFieldValue);
                if (currentBetFieldValue != amount) {
                    BetInputField.text = amount.ToString();
                }
            }
        }

        public void SetHandTypeHintText(string text)
        {
            if (HandTypeHintText != null) {
                HandTypeHintText.text = text;
            }
        }

        public void MakeAITurn()
        {
            Debug.Log("GameController.MakeAITurn, current player: " + _currentGame.CurrentPlayer.Name);
            if (!_currentGame.CurrentPlayer.IsAI){
                Debug.LogError("GameController.MakeAITurn: current player is not AI!");
                return;
            }

            StartCoroutine(WaitBeforeAITurn());
            StartCoroutine(ProcessAITurn());
        }

        private IEnumerator WaitBeforeAITurn()
        {
            _isAITurnDelayInProgress = true;
            yield return new WaitForSeconds(Defines.AI_TURN_TIME);
            _isAITurnDelayInProgress = false;
        }

        private IEnumerator ProcessAITurn()
        {
            Debug.Log("GameController.ProcessAITurn start, current player: " + _currentGame.CurrentPlayer.Name);
            yield return new WaitWhile(() => _isAITurnDelayInProgress);

            if (!_currentGame.CurrentPlayer.IsAI) {
                Debug.LogError("GameController.ProcessAITurn: current player is not AI!");
                yield break;
            }

            int amount = 0;
            TurnType turnType = PlayerAI.Instance.MakeDecision(out amount);

            if (turnType == TurnType.Bet || turnType == TurnType.Raise) {
                _currentAmountToBet = amount;
            }

            Debug.Log("ProcessAITurn end, current player: " + _currentGame.CurrentPlayer.Name);

            switch (turnType) {
                case TurnType.Check:
                    OnCheckClicked();
                    break;
                case TurnType.Fold:
                    OnFoldClicked();
                    break;
                case TurnType.Call:
                    OnCallClicked();
                    break;
                case TurnType.Raise:
                    OnRaiseClicked();
                    break;
                case TurnType.Bet:
                    OnBetClicked();
                    break;
            }
        }

        private void DisplayPlayerActionText(int playerIndex, TurnType moveType, int amount = 0)
        {
            Transform playerTransform = PlayersParent.transform.Find("Player" + playerIndex);

            if (playerTransform == null) {
                // TODO: log error
                return;
            }

            Transform playerActionTransform = playerTransform.Find("PlayerCard/LastActionText");

            if (playerActionTransform == null) {
                return;
            }

            Text playerActionText = playerActionTransform.gameObject.GetComponent<Text>();

            if (playerActionText == null) {
                return;
            }

            string amountString = amount > 0 ? amount.ToString() : string.Empty;

            switch (moveType) {
                case TurnType.Fold:
                    playerActionText.text = "Fold";
                    break;
                case TurnType.Check:
                    playerActionText.text = "Check";
                    break;
                case TurnType.Call:
                    playerActionText.text = "Call " + amountString;
                    break;
                case TurnType.Bet:
                    playerActionText.text = "Bet " + amountString;
                    break;
                case TurnType.Raise:
                    playerActionText.text = "Raise " + amountString;
                    break;
            }
        }

        private void PutChipsInfrontPlayer(Transform place, int betAmount, ChipAlignment chipAlignment = ChipAlignment.Right)
        {
            if (chipAlignment == ChipAlignment.Invalid) {
                Debug.LogWarning("PutChipsInfrontPlayer: chipAlignment is invalid, applying right alignment.");
                chipAlignment = ChipAlignment.Right;
            }

            for (int chipDenominationIndex = _chipDenominations.Length - 1; chipDenominationIndex >= 0; chipDenominationIndex--) {
                int denomination = (int)_chipDenominations[chipDenominationIndex];
                if (denomination > betAmount) {
                    continue;
                }

                while (betAmount >= denomination) {
                    GameObject chipObject = GameObject.Instantiate(ChipPrefab);
                    ChipWidget chipWidget = chipObject.GetComponent<ChipWidget>();
                    chipWidget.Denomination = _chipDenominations[chipDenominationIndex];

                    Vector3 chipPosition = place.position;
                    int chipsCount = place.childCount;
                    if (chipsCount > 0) {
                        int chipsInStack = chipsCount;
                        int stackIndex = 0;
                        if (chipsCount >= Defines.MAX_CHIPS_IN_ONE_STACK_IN_BETS) {
                            stackIndex = chipsCount / Defines.MAX_CHIPS_IN_ONE_STACK_IN_BETS;
                            chipsInStack = chipsCount % Defines.MAX_CHIPS_IN_ONE_STACK_IN_BETS;
                        }
                        // TODO: correct this magic
                        if (stackIndex > 0) {
                            if (chipAlignment == ChipAlignment.Right) {
                                chipPosition.x += (chipWidget.ImageWidth * stackIndex);
                            } else if (chipAlignment == ChipAlignment.Left) {
                                chipPosition.x -= (chipWidget.ImageWidth * stackIndex);
                            }
                        }
                        chipPosition.y += (chipWidget.ImageHeight * chipsInStack) / 6;
                    }
                    chipObject.transform.parent = place;

                    chipObject.transform.localPosition = chipPosition;
                    chipObject.transform.localScale = Vector3.one;

                    betAmount -= denomination;
                }
            }

            if (betAmount > 0) {
                Debug.LogError("PutChipsInfrontPlayer: wrong bet amount (could not be presented with current chip denominations).");
            }
        }

        private void ClearPlayers()
        {
            for (int playerIndex = 0; playerIndex < _currentGame.Players.Count; playerIndex++) {
                Transform playerTransform = PlayersParent.transform.Find("Player" + playerIndex);
                Transform blindsTransform = playerTransform.Find("Blinds");
                Transform holeCardsTransform = playerTransform.Find("HoleCards");
                Transform buttonTransform = playerTransform.Find("Button");
                Transform deckTransform = playerTransform.Find("Deck");

                for (int chipIndex = blindsTransform.childCount - 1; chipIndex >= 0; chipIndex--) {
                    Transform childTransform = blindsTransform.GetChild(chipIndex);
                    GameObject.Destroy(childTransform.gameObject);
                }

                blindsTransform.DetachChildren();

                for (int cardIndex = holeCardsTransform.childCount - 1; cardIndex >= 0; cardIndex--) {
                    Transform childTransform = holeCardsTransform.GetChild(cardIndex);
                    GameObject.Destroy(childTransform.gameObject);
                }

                holeCardsTransform.DetachChildren();

                for (int buttonIndex = buttonTransform.childCount - 1; buttonIndex >= 0; buttonIndex--) {
                    Transform childTransform = buttonTransform.GetChild(buttonIndex);
                    GameObject.Destroy(childTransform.gameObject);
                }

                buttonTransform.DetachChildren();

                for (int deckIndex = deckTransform.childCount - 1; deckIndex >= 0; deckIndex--) {
                    Transform childTransform = deckTransform.GetChild(deckIndex);
                    GameObject.Destroy(childTransform.gameObject);
                }

                deckTransform.DetachChildren();

                Transform playerCard = playerTransform.Find("PlayerCard");
                if (playerCard != null) {
                    Transform playerActionTransform = playerTransform.Find("PlayerCard/LastActionText");
                    if (playerActionTransform != null) {
                        Text playerActionText = playerActionTransform.gameObject.GetComponent<Text>();
                        if (playerActionText != null) {
                            playerActionText.text = "";
                        }
                    }
                }
            }
        }

        private void ClearBoard()
        {
            for (int cardIndex = Board.transform.childCount - 1; cardIndex >= 0; cardIndex--) {
                Transform childTransform = Board.transform.GetChild(cardIndex);
                GameObject.Destroy(childTransform.gameObject);
            }

            for (int potIndex = PotsParent.transform.childCount - 1; potIndex >= 0; potIndex--) {
                Transform potTransform = PotsParent.transform.Find("Pot" + potIndex);

                // first child is pot size text (should not be destoyed)
                for (int chipIndex = potTransform.childCount - 1; chipIndex >= 1; chipIndex--) {
                    Transform childTransform = potTransform.GetChild(chipIndex);
                    GameObject.Destroy(childTransform.gameObject);
                }

                potTransform.gameObject.SetActive(false);
            }

            IsFlopDealt = false;
            IsTurnDealt = false;
            IsRiverDealt = false;
        }

        private void ResetBetSliderAndInput()
        {
            if (BetSlider != null) {
                BetSlider.value = BetSlider.minValue;
            }

            SetBetButtonText("");
            SetRaiseButtonText("");

            if (BetInputField != null) {
                BetInputField.text = "";
            }
        }
    }
}
