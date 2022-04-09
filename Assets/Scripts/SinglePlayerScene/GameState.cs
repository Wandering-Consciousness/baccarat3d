using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;

// Game state manager class.
// Referenced http://www.fizixstudios.com/labs/do/view/id/unity-game-state-manager
public class GameState : MonoBehaviour
{
    // Singleton instance of the game state manager
    private static GameState instance;

    // The Background Music
    public GameObject bgm;

    // The Google Play Games cloud manager for handling data
    public GPGCloudManager gpgCloudManager;

    // Reference to the chips manager
    public ChipsManager chipsManager;

    // Reference to the dealer
    public Dealer dealer;

    // Reference to the camears manager
    public CamerasManager camerasManager;

    // Reference to the table manager
    public TableManager tableManager;

    // Reference to the GUI controls class
    public GUIControls guiControls;

    // Reference to the Shop Manager for IAP
    public ShopManager shopManager;

     // Reference to the Tutorial Help Manager
    public TutorialHelpManager tutorialHelpManager;

    // Reference to the Roadmap Manager
    public RoadmapManager roadmapManager;

    public int roundNumber;

    // List of AR markers and their bonuses for the AR treasure hunt feature
    public class ARTreasureHuntMarker {
        public string targetName = "";
        public int id = 0;
        public int bonusValue = 0;
        public ARTreasureHuntMarker (string name, int id, int value) {
            targetName = name;
            this.id = id;
            bonusValue = value;
        }
    }
    public Dictionary<string, ARTreasureHuntMarker> arTreasureHuntMarkers = new Dictionary<string, ARTreasureHuntMarker>();

    public FingerGestures fingerGestures;

    // Bank balance for how much worth of chips we have
    private int balance = 0;

    public int currentBalance {
        get { return balance; }
        set {
            balance = value;
            if (balance < 0) {
                balance = 0;
                guiControls.displayMessage(LanguageManager.GetText("label_zero_balance"));
            }
            Debug.Log ("Setting current balance to $" + balance.ToString("n0"));
            PlayerPrefs.SetInt(PREFS_BALANCE_KEY, balance);

            // Save balance in Google Cloud
            if (Social.localUser.authenticated) {
                gpgCloudManager.saveBalance();
            }
        }
    }

    // Whether or not to show ads
    private bool showNoAds = false;
    public bool isProEdition {
        get { 
            return true; 
        }
        set {
        }
    }

    // Tutorial counter
    private int internalTutorialCounter = 0;
    public int tutorialCounter {
        get {
            //return 0;
            return internalTutorialCounter;
        }
        set {
            internalTutorialCounter = value;
            PlayerPrefs.SetInt(PREFS_TUTORIAL_SHOWN_TIMES, internalTutorialCounter);
        }
    }


    // Current state
    public enum State
    {
        CutCards,
        PlaceBets,
        DealCards,
        SqueezeCards,
        DecideWinner,
    }

    // Whether or not the tutorial was completed during this game session
    public bool tutorialEndedThisSession = false;

    bool firstChipThisRound = false;

    // TODO: implement VIP tables each with different min/max bets
    public static int MAX_BET = 500000; // $500,000
    public static int MIN_BET = 100; // $100

    private State state = State.CutCards;

    public State currentState {
        get { return state; }
        set {
            // Don't do anything if we're already in the same state
            if (value == state) {
                return;
            }
            
            // Order of allowed state changes:
            // CutCards->PlaceBets -> DealingCards -> SqueezeCards -> DecideWinner->back to PlaceBets
            switch (value) {
            case State.CutCards:
                Debug.Log ("Changing game state to CutCards");
                state = State.CutCards;
                break;
            case State.PlaceBets:
//                if (state == State.CutCards || state == State.DecideWinner) {
                    // Start new round (coup)

                // Open shop if we have no credit
                if (balance < MIN_BET) {
                    Debug.LogWarning ("Balance is less that min bet... opening shop screen");
                    LogUtils.LogEvent(Consts.FE_SHOP_OPEN_LOW_BALANCE);
                    guiControls.openShop();
                }

                Debug.Log ("Changing game state to PlaceBets");
                state = value;

                if (chipsManager != null) {
                    // Allow placing of bets
                    chipsManager.setPlaceBetsAllowed (true);
                } else {
                    Debug.LogWarning ("ChipsManager is null when telling it bets can be placed");
                }
//                } else {
//                    Debug.LogError ("Cannot place bets because current game state is " + state + ". It should be CutCards or DecideWinner.");
//                }
                firstChipThisRound = true;
                state = State.PlaceBets;
                dealer.SayPlaceBets ();

                // Hack to reset values displayed
                GUIControls.bankerCardsValueText = "";
                GUIControls.playerCardsValueText = "";
                guiControls.internalPlayerTotal = 0;
                guiControls.internalBankerTotal = 0;
                GUIControls.currentBetTexts.Clear();

                if (tutorialCounter > 1 && guiControls != null && guiControls.GetComponent<CustomizedCenteredGuiWindow>() != null 
                    && !guiControls.gameObject.GetComponent<CustomizedCenteredGuiWindow>().isDrawGui)
                    ToggleFingerGestures(true);

                // Hide stats panel
                CustomizedStatsPanel.hidePanel();

                // Hide clear and deal buttons
                if (chipsManager.clearedChipsList.Count > 0) {
                    guiControls.dealButtonState = GUIControls.DealButtonState.Rebet;
                } else {
                    guiControls.dealButtonState = GUIControls.DealButtonState.Hide;
                }
                guiControls.clearButtonState = GUIControls.ClearButtonState.Hide;

                break;

            case State.DealCards:
                // We can only deal cards if coming from PlaceBets mode
//                if (state != State.PlaceBets) {
//                    Debug.LogError ("Cannot deal cards because current game state is " + state + ". It should be PlaceBets.");
//                    return;
//                }

                // Deal cards only if we have some bets on the table
                if (betsOnTableHash.Count != 0 && getCurrentBetValue () >= GameState.MIN_BET
                    && (betsOnTableHash.Contains (BetType.Player) || betsOnTableHash.Contains (BetType.Banker) || betsOnTableHash.Contains (BetType.Tie))) {
                    Debug.Log ("Changing game state to DealCards");
                    state = State.DealCards;

                    if (chipsManager != null) {
                        // Disable any more bets
                        chipsManager.setPlaceBetsAllowed (false);
                    } else {
                        Debug.LogWarning ("ChipsManager is null when telling it no more bets");
                    }

                    // Immediately subtract betted amount from balance
                    currentBalance -= getCurrentBetValue();
                    updateGUIText();

                    if (dealer != null) {
                        // Deal the cards
                        dealer.deal ();
                        roundNumber++;
                    } else {
                        Debug.LogError ("Can't deal cards because dealer is null");
                    }
                } else if (!betsOnTableHash.Contains (BetType.Player) && !betsOnTableHash.Contains (BetType.Banker) && !betsOnTableHash.Contains(BetType.Tie)
                    && (betsOnTableHash.Contains (BetType.PlayerPair) || betsOnTableHash.Contains (BetType.BankerPair))) {
                    // Can't bet only on pairs as they are side bets
                    Debug.LogWarning("Can't bet only on side-bet banker/player pairs");
                    GameState.Instance.guiControls.displayMessage (LanguageManager.GetText ("label_insufficient_bets"));
#if !UNITY_WEBPLAYER
                    Handheld.Vibrate ();
#endif
                } else if (getCurrentBetValue () < GameState.MIN_BET) {
                    // Check we have at least the minimum bet on the table
                    Debug.LogWarning ("Can't bet less than MIN BET ($" + MIN_BET +") with the current $" + getCurrentBetValue() + " bet");
#if !UNITY_WEBPLAYER
                    Handheld.Vibrate ();
#endif
                    GameState.Instance.guiControls.displayMessage (LanguageManager.GetText ("label_min_bet") + " $" + MIN_BET);
                    dealer.SayPlaceBets(false);
                } else {
                    Debug.LogWarning ("Cannot start dealing cards until there are bets on the table");
                    dealer.SayPlaceBets();
                }
                break;
            case State.SqueezeCards:
                // We can only squeeze cards if coming from DealCards mode
//                if (state != State.DealCards) {
//                    Debug.LogError ("Cannot squeeze cards because current game state is " + state + ". It should be DealCards.");
//                    return;
//                }

                Debug.Log ("Changing game state to SqueezeCards");
                state = State.SqueezeCards;

                GUIControls.SetAutoRotate(false);

                // Move the chips on the table so they don't interfer with the cards
                if (chipsManager != null) {
                    chipsManager.clearChipsForSqueezing (true);
                }

                // Deal dealer to push the cards to the user for squeezing
                if (dealer != null) {
                    dealer.beginSqueezing ();
                }

                break;
            case State.DecideWinner:
                // We can only decide the winner once the hand the user bet on has been revealed (by squeezing or otherwise)
//                if (state != State.SqueezeCards) {
//                    Debug.LogError ("Cannot decide winner because current game state is " + state + ". It should be SqueezeCards.");
//                    return;
//                }

                Debug.Log ("Changing game state to DecideWinner");
                state = State.DecideWinner;

                GUIControls.SetAutoRotate(true);
                Card.isShowingTutorials = false;
                Chip.isShowingTutorials = false;

                // Decide the winner!
                if (dealer != null) {
                    dealer.decideWinner ();
                }

                break;
            default:
                Debug.LogWarning ("Unknown game state: " + value);
                break;
            }
        }
    }

    // Show the '2 finger zoom', '3 finger swipe', 'clear bets' and 'start dealing tutorials'
    void showTutorialSpeechBubbles() {
        Chip.isShowingTutorials = true;

        if (!Card.isShowingTutorials) {
            dealer.nextTapStopSpeechBubble = GameState.Instance.tutorialHelpManager.fingers2Zoom(true);
            dealer.nextTapObject = this.gameObject;
            dealer.nextTapMethodName = "showTutorialSpeechBubbles2";
        }
    }
    void showTutorialSpeechBubbles2() {
        if (!Card.isShowingTutorials) {
            dealer.nextTapStopSpeechBubble = GameState.Instance.tutorialHelpManager.fingers3Swipe(true);
            dealer.nextTapObject = this.gameObject;
            dealer.nextTapMethodName = "showTutorialSpeechBubbles3";
        }
    }
    void showTutorialSpeechBubbles3() {
        if (!Card.isShowingTutorials) {
            dealer.nextTapStopSpeechBubble = GameState.Instance.tutorialHelpManager.clearBets(true);
            dealer.nextTapObject = this.gameObject;
            dealer.nextTapMethodName = "showTutorialSpeechBubbles4";
        }
    }
    void showTutorialSpeechBubbles4() {
        if (!Card.isShowingTutorials) {
            dealer.nextTapStopSpeechBubble = GameState.Instance.tutorialHelpManager.startDealing(true);
            dealer.nextTapObject = this.gameObject;
            dealer.nextTapMethodName = "showTutorialSpeechBubbles5";
        }
    }
    void showTutorialSpeechBubbles5() {
        dealer.nextTapResetOnNext = true;
        Chip.isShowingTutorials = false;
    }

    // The kind of bet a chip is placed on
    public enum BetType
    {
        Player,
        Banker,
        Tie,
        BankerPair,
        PlayerPair,
        Undefined,
        InAir
    }

    // Get the current bet type (banker, player, tie etc.)
    public BetType getCurrentBetType ()
    {
        if (betsOnTableHash == null) {
            return BetType.Undefined;
        }

        if (!betsOnTableHash.Contains (BetType.Player)
            && betsOnTableHash.Contains (BetType.Banker)) {
            // There is a bet on the banker so return banker type
            // There may also be a tie bet in this case
            return BetType.Banker;
        } else if (!betsOnTableHash.Contains (BetType.Banker)
            && betsOnTableHash.Contains (BetType.Player)) {
            // There is a bet on the player so return player type
            // There may also be a tie bet in this case
            return BetType.Player;
        } else if (!betsOnTableHash.Contains (BetType.Banker)
            && !betsOnTableHash.Contains (BetType.Player)
            && betsOnTableHash.Contains (BetType.Tie)) {
            // There are no bets on banker or player - pairs or normal - but there is on tie
            return BetType.Tie;
        }

        return BetType.Undefined;
    }


    // Class to represent a bet type and the amount currently bet on it
    public class BetOnTable
    {
        public BetType type;
        public int amount;

        public BetOnTable(BetType betType, int plus) {
            this.type = betType;
            this.amount = plus;
        }

        public BetOnTable() {
        }
    }

    public Hashtable betsOnTableHash = new Hashtable ();

    // ---------------------------------------------------------------------------------------------------
    // gamestate()
    // --------------------------------------------------------------------------------------------------- 
    // Creates an instance of gamestate as a gameobject if an instance does not exist
    // ---------------------------------------------------------------------------------------------------
    public static GameState Instance {
        get {
            if (instance == null) {
                instance = (GameState)new GameObject ("GameState").AddComponent <GameState>();
                instance.currentState = State.CutCards;
            }
 
            return instance;
        }
    }   
 
    // Sets the instance to null when the application quits
    public void OnApplicationQuit ()
    {
        instance = null;
    }
    // ---------------------------------------------------------------------------------------------------
 
    void OnApplicationPause(bool pauseStatus) {
        if (!pauseStatus) {
            Debug.Log ("App coming to foreground from background");

            //if (gpgCloudManager != null) {
            //    Debug.Log("Refreshing Google cloud data");
            //    gpgCloudManager.LoadState ();
            //}
        }
    }

 
    // ---------------------------------------------------------------------------------------------------
    // startState()
    // --------------------------------------------------------------------------------------------------- 
    // Creates a new game state
    // ---------------------------------------------------------------------------------------------------
    public void startState ()
    {
        print ("Creating a new game state");

        loadPrefs ();
        updateGUIText ();

        // Add the InAir bet type as it's always gotta be there for chips to temporary add their value while they're flying in air
        betsOnTableHash.Add (BetType.InAir, new BetOnTable(BetType.InAir, 0));

        Debug.Log("Registering GPGCloudManager instance with GameState");
        gpgCloudManager = new GPGCloudManager();
    }


    private void loadPrefs ()
    {
        // Load bank balance from natively persisted storage (on Android preferences etc.)
        if (PlayerPrefs.HasKey(PREFS_BALANCE_KEY)) {
            balance = PlayerPrefs.GetInt(PREFS_BALANCE_KEY);
            Debug.Log ("Loading saved balance of $" + balance.ToString("n0"));
        } else {
            balance = 13000;
        }
        GUIControls.balanceInt = balance;
        GUIControls.lastBalanceInt = balance;

        // Load whether or not to display ads
        if (PlayerPrefs.HasKey(PREFS_NO_ADS_KEY)) {
            showNoAds = PlayerPrefs.GetInt(PREFS_NO_ADS_KEY) == 1 ? true : false;
            Debug.Log ("Loading 'display no ads' setting: " + showNoAds);
        } else {
            isProEdition = false;
        }

        // Load the counter for the number of times the tutorial has been shown
        if (PlayerPrefs.HasKey(PREFS_TUTORIAL_SHOWN_TIMES)) {
            tutorialCounter = PlayerPrefs.GetInt(PREFS_TUTORIAL_SHOWN_TIMES);
            Debug.Log ("Loading tutorial counter: " + tutorialCounter);
        } else {
            tutorialCounter = 0;
        }
    }

    // Place a bet
    public bool bet (GameState.BetType betType, int amount)
    {
        // Check we're in the right game state
        if (currentState != State.PlaceBets) {
            Debug.LogWarning ("Can't currently bet $" + amount + " on " + betType + " because not in PlaceBets mode. Current mode is " + currentState);
            return false;
        }

        // Simultaneous bets on player and banker are not allowed
        if (betType == BetType.Banker && betsOnTableHash.ContainsKey (BetType.Player)) {
            Debug.LogWarning ("Cannot bet on banker when bets for player are already on the table");
            return false;
        } else if (betType == BetType.Player && betsOnTableHash.ContainsKey (BetType.Banker)) {
            Debug.LogWarning ("Cannot bet on player when bets for banker are already on the table");
            return false;
        }

        if (firstChipThisRound) {
            if (tutorialCounter <= 1) {
                // Tutorial stuff
                showTutorialSpeechBubbles();
            }
            firstChipThisRound = false;
        }

        // If this is the first bet for this type, then create a HandBet object and add it to the hands bet hashtable
        if (!betsOnTableHash.Contains (betType)) {
            Debug.Log ("First bet on " + betType + ": $" + amount);
            BetOnTable newHandBet = new BetOnTable ();
            newHandBet.type = betType;
            newHandBet.amount = amount;
            betsOnTableHash.Add (betType, newHandBet);

            StartCoroutine(ShowPanelAndButtonsCoroutine());
            updateGUIText ();
            return true;
        }

        // Else add to the current amount betted on the specified hand
        ((BetOnTable)betsOnTableHash [betType]).amount += amount;

        updateGUIText ();
        return true;
    }

    IEnumerator ShowPanelAndButtonsCoroutine() {
        // Wait a small amount of time because I found when this methods' commands were run
        // upon the first bet been made, the corresponding chip would stop mid-air while Unity
        // gulped for a split second and tried to process the GUI commands. This hack wait
        // allows for the chip to at least drop smoothly first.
        //yield return new WaitForSeconds(0.2f); // Commented out coz wasn't working well

        // Show stats panel
        CustomizedStatsPanel.showPanel();

        // Show clear and deal buttons
        guiControls.dealButtonState = GUIControls.DealButtonState.Deal;
        guiControls.clearButtonState = GUIControls.ClearButtonState.Clear;

        yield break;
    }

    // Undo a placed bet
    public bool unbet (GameState.BetType betType, int amount)
    {
        // Check we're in the right game state
        //if (currentState != State.PlaceBets) { // COMMENTING OUT coz often we tried to unbet chips when in the wrong mode
        //    Debug.LogWarning ("Can't unbet $" + amount + " off " + betType + " because not in PlaceBets mode. Current mode is " + currentState);
        //    return false;
        //}

        // If this is the first bet for this type, then create a HandBet object and add it to the hands bet hashtable
        if (betsOnTableHash.Contains (betType)) {
            Debug.Log ("Deducted $" + amount + " from bet on " + betType);
            ((BetOnTable)betsOnTableHash [betType]).type = betType;
            ((BetOnTable)betsOnTableHash [betType]).amount -= amount;

            // If the amount bet on this hand has reached zero, then we remove it from the current hashtable of placed bet types
            if (((BetOnTable)betsOnTableHash [betType]).amount <= 0) {
                betsOnTableHash.Remove (betType);
            }

            updateGUIText ();
            return true;
        } else {
            Debug.Log ("DIDN'T deduct $" + amount + " from bet on " + betType);
        }

        return false;
    }

    // Get sum total of all bets currently on the table
    public int getCurrentBetValue() {
        int sum = 0;
        foreach (BetOnTable aBet in betsOnTableHash.Values) {
            //if (aBet.type == BetType.Undefined)
            //    continue;

            sum += aBet.amount;
        }
        Debug.Log ("Current total bet placed on table is $" + sum);
        return sum;
    }

    // Get sum total of all bets for the hand currently betted on
    public int getCurrentBetValueForBetHand() {
        int sum = 0;
        if (betsOnTableHash.Contains (BetType.Player)) {
            sum += ((BetOnTable) betsOnTableHash[BetType.Player]).amount;
        }
        if (betsOnTableHash.Contains (BetType.PlayerPair)) {
            sum += ((BetOnTable) betsOnTableHash[BetType.PlayerPair]).amount;
        }
        if (betsOnTableHash.Contains (BetType.Banker)) {
            sum += ((BetOnTable) betsOnTableHash[BetType.Banker]).amount;
        }
        if (betsOnTableHash.Contains (BetType.BankerPair)) {
            sum += ((BetOnTable) betsOnTableHash[BetType.BankerPair]).amount;
        }
        if (betsOnTableHash.Contains (BetType.Tie)) {
            sum += ((BetOnTable) betsOnTableHash[BetType.Tie]).amount;
        }
        return sum;
    }

    // Clear chips on the table
    public void clearChips ()
    {
//        if (currentState != State.PlaceBets) {
//            Debug.LogWarning ("Can't clear chips off table because not in PlaceBets mode");
//        }

        if (chipsManager != null) {
            chipsManager.clearAllChipsOnTable ();
        }

        if (betsOnTableHash != null) {
            betsOnTableHash.Clear ();

            // Add the InAir bet type as it's always gotta be there for chips to temporary add their value while they're flying in air
            betsOnTableHash.Add (BetType.InAir, new BetOnTable(BetType.InAir, 0));
        }

        // Hide window frame in stats panel
        CustomizedStatsPanel.hidePanel();

        updateGUIText ();
    }

    // Update the GUI text
    public void updateGUIText ()
    {
        // Update current bets
        GUIControls.currentBetTexts.Clear();

        // Bets on Player
        BetOnTable playerBets = (BetOnTable)betsOnTableHash [BetType.Player];
        if (playerBets != null) {
            GUIControls.currentBetTexts.Add(new string[] { LanguageManager.GetText ("label_player"), "$" + playerBets.amount.ToString("n0") });
        }

        // Bets on Player Pair
        BetOnTable playerPairBets = (BetOnTable)betsOnTableHash [BetType.PlayerPair];
        if (playerPairBets != null) {
            GUIControls.currentBetTexts.Add(new string[] { LanguageManager.GetText ("label_player_has_pair"), "$" + playerPairBets.amount.ToString("n0") } );
        }

        // Bets on Banker
        BetOnTable bankerBets = (BetOnTable)betsOnTableHash [BetType.Banker];
        if (bankerBets != null) {
            GUIControls.currentBetTexts.Add(new string[] { LanguageManager.GetText ("label_banker"), "$" + bankerBets.amount.ToString("n0") } );
        }

        // Bets on Banker Pair
        BetOnTable bankerPairBets = (BetOnTable)betsOnTableHash [BetType.BankerPair];
        if (bankerPairBets != null) {
            GUIControls.currentBetTexts.Add(new string[] { LanguageManager.GetText ("label_banker_has_pair"), "$" + bankerPairBets.amount.ToString("n0") } );
        }

        // Bets on Tie
        BetOnTable tieBets = (BetOnTable)betsOnTableHash [BetType.Tie];
        if (tieBets != null) {
            GUIControls.currentBetTexts.Add(new string[] { LanguageManager.GetText ("label_tie"), "$" + tieBets.amount.ToString("n0") } );
        }

        // Update current balance
        GUIControls.balanceInt = currentBalance;
    }


    // Dish out the winners their winnings
    public int dishOutWinnings (BetType winner, int paysout)
    {
        // Calculate how much was won
        BetOnTable bet = (BetOnTable)betsOnTableHash [winner];
        int wonAmount = 0;
        int commission = 0;
        if (bet != null) {
            wonAmount = (paysout * bet.amount);

            if (winner == BetType.Banker) {
                // When the banker hand wins we deduct a 5% commission.. which would usually goto the house but there's nothing for us to win here (as developers!)
                Debug.Log ("Amount won before deducting 5% banker commission: $" + wonAmount.ToString("n0"));
                commission = (int) Mathf.Ceil((float)wonAmount*0.05f);
                wonAmount = (int) Mathf.Ceil((float)wonAmount*0.95f);
                Debug.Log ("Amount won after deducting 5% banker commission: $" + wonAmount.ToString("n0"));
            }

            Debug.Log ("User bet a total of $" + bet.amount + " on the winning " + winner + " hand(s) so won $" + wonAmount.ToString("n0"));
            currentBalance += wonAmount;
        } else {
            Debug.Log ("User bet nothing on the winning " + winner + " hand(s)");
        }
        StartCoroutine (dishOutWinningsCoroutine (winner, paysout, wonAmount, commission));
        return wonAmount;
    }

    IEnumerator dishOutWinningsCoroutine (BetType winner, int paysout, int wonAmount, int commission)
    {
        Debug.Log ("Dishing out winnings for " + winner + " at " + paysout + "x, value is $" + wonAmount.ToString("n0") + ", commission is " + commission.ToString("n0"));
        if (winner.Equals(BetType.Tie)) {
            yield return new WaitForSeconds(Dealer.dealSpeed*12f);
        } else {
            yield return new WaitForSeconds(Dealer.dealSpeed*8f);
        }

        dealer.onWin (winner, paysout, wonAmount, commission);

        // Flash the area that won
        tableManager.doYesYesFlash (winner);
    }

    // Deduct the losses from non-winning bets
    public void deductLosses (List<BetType> winners)
    {
        StartCoroutine (deductLossesCoroutine (winners));
    }

    IEnumerator deductLossesCoroutine (List<BetType> winners)
    {
       // COMMENTED OUT coz now handled elsewhere
       //if (winners.Contains(BetType.Tie)) {
       //     // If we're a tie, then there are no losses. Instead we just return the chips to the user.
       //     Debug.Log ("deductLossesCoroutine: winners contain Tie so returning any other other chips instead of deducting");
       //     yield return new WaitForSeconds(Dealer.dealSpeed*4f);
       //     GameState.Instance.chipsManager.clearChipsExcludingWinners(winners);
       // } else {
            // Deduct loses from non-winner hands
            bool lossesDeducted = false;
            bool nothingWon = true;
            int totalLost = 0;
            Dictionary<string, string> losses = new Dictionary<string, string>();
            foreach (DictionaryEntry de in betsOnTableHash) {
                if (de.Value == null) {
                    continue;
                }
                BetOnTable betOnTable = (BetOnTable)de.Value;
                if (betOnTable == null || betOnTable.type == null) {
                    continue;
                }

                if (winners.Contains(betOnTable.type)) {
                    // do nothing as we're not a losing bet
                    currentBalance += betOnTable.amount;
                    nothingWon = false;
                } else {
                    // If the only winner was a tie then we give back (don't subtract) any other bets, i.e. we don't count them as losses
                    if (winners.Count == 1 && winners.Contains(BetType.Tie))
                        continue;
                    
                    Debug.Log ("User lost $" + betOnTable.amount + " on " + betOnTable.type + " hand");
                    //currentBalance += betOnTable.amount; // COMMENTED OUT because we shifted the logic to earlier on in the dealing process
                    totalLost += betOnTable.amount;
                    lossesDeducted = true;

                    // Calculate losses per type to send to Flurry
                    switch (betOnTable.type) {
                        case BetType.Player:
                            if (losses.ContainsKey(Consts.FEP_WINLOSE_PLAYER))
                                losses[Consts.FEP_WINLOSE_PLAYER] = (int.Parse(losses[Consts.FEP_WINLOSE_PLAYER]) - betOnTable.amount)+"";
                            else
                                losses.Add (Consts.FEP_WINLOSE_PLAYER, (-betOnTable.amount)+"");
                            break;

                        case BetType.Banker:
                            if (losses.ContainsKey(Consts.FEP_WINLOSE_BANKER))
                                losses[Consts.FEP_WINLOSE_BANKER] = (int.Parse(losses[Consts.FEP_WINLOSE_BANKER]) - betOnTable.amount)+"";
                            else
                                losses.Add (Consts.FEP_WINLOSE_BANKER, (-betOnTable.amount)+"");
                            break;
                            
                        case BetType.Tie:
                            if (losses.ContainsKey(Consts.FEP_WINLOSE_TIE))
                                losses[Consts.FEP_WINLOSE_TIE] = (int.Parse(losses[Consts.FEP_WINLOSE_TIE]) - betOnTable.amount)+"";
                            else
                                losses.Add (Consts.FEP_WINLOSE_TIE, (-betOnTable.amount)+"");
                            break;
                            
                        case BetType.PlayerPair:
                            if (losses.ContainsKey(Consts.FEP_WINLOSE_PLAYER_PAIR))
                                losses[Consts.FEP_WINLOSE_PLAYER_PAIR] = (int.Parse(losses[Consts.FEP_WINLOSE_PLAYER_PAIR]) - betOnTable.amount)+"";
                            else
                                losses.Add (Consts.FEP_WINLOSE_PLAYER_PAIR, (-betOnTable.amount)+"");
                            break;
                            
                        case BetType.BankerPair:
                            if (losses.ContainsKey(Consts.FEP_WINLOSE_BANKER_PAIR))
                                losses[Consts.FEP_WINLOSE_BANKER_PAIR] = (int.Parse(losses[Consts.FEP_WINLOSE_BANKER_PAIR]) - betOnTable.amount)+"";
                            else
                                losses.Add (Consts.FEP_WINLOSE_BANKER_PAIR, (-betOnTable.amount)+"");
                            break;
                    }
                }
            }
            if (lossesDeducted) {
                LogUtils.LogEvent(Consts.FE_WINLOSE_BET_TYPE_AMOUNT_EVENT, losses, false);
                Debug.Log("Total lost amount is $" + totalLost);
                Dictionary<string, string> lostTotals = new Dictionary<string, string>(); // for Flurry
                lostTotals.Add (Consts.FEP_LOSE_AMOUNT, totalLost+"");
                LogUtils.LogEvent (Consts.FE_WINLOSE_TOTALS_EVENT, lostTotals, false); // total lost
            }

            if (lossesDeducted && nothingWon) {
                // Clear chip loses with animation of dealer pulling in cards
                dealer.onLoss (winners);
            } else if (lossesDeducted) {
                // If there as at least one winner we don't want to animate the dealer pulling in the losses so we just clear them
                GameState.Instance.chipsManager.clearChipsExcludingWinners(winners);
            }
        //}

        updateGUIText ();

        // Begin next coup (next round)
        if (winners.Contains(BetType.Tie) && betsOnTableHash.ContainsKey(BetType.Tie)) {
            yield return new WaitForSeconds(Dealer.dealSpeed*13f);
        } else {
            yield return new WaitForSeconds(Dealer.dealSpeed*6f);
        }
        dealer.clearCards ();
        clearChips ();
        camerasManager.ResetMainCameraPos();

        // Refresh ads
#if !UNITY_EDITOR
        if (adsManager != null && !isProEdition)
            adsManager.refresh(roundNumber);
#endif

        // Update TV display of how rounds have gone by
        GameState.Instance.roadmapManager.setRoundNumber(roundNumber);

        currentState = State.PlaceBets;
    }

    // Enable/disable all finger gestures
    public void ToggleFingerGestures(bool enable) {
        Debug.Log ("Switching FingerGestures from " + fingerGestures.enabled + " -> " + enable);
        fingerGestures.enabled = enable;
    }

    ///////////////// PlayerPrefs ///////////////////////////
    public static string PREFS_BALANCE_KEY = "zndk";
    public static string PREFS_NO_ADS_KEY = "kkhj";
    public static string PREFS_TUTORIAL_SHOWN_TIMES = "ttst";
}
