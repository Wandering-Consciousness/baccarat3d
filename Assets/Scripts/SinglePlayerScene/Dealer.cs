using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.SocialPlatforms;

// This has methods for dealer actions and managing the shoe and all the decks of cards in it
public class Dealer : MonoBehaviour
{
    private static int CUT_CARD_START_IDX = 6;
    public static float CARD_ENLARGEN_SCALEF = 3f;
    public static float CARD_ENLARGEN_SCALEF_TIME = 0.5f;

    public static int AR_BONUS = 1000; // win $1000 when win a hand using AR
    public static int FRIEND_INVITED_BONUS = 500; // $500 for each friend invited
    public static int GPG_LOGON_BONUS = 10000; // $10,000 for joining Google Play Games
    public static int AR_TREASURE_HUNT_BONUS1 = 100000; // for cigarettes and beer etc
    public static int AR_TREASURE_HUNT_BONUS2 = 1000000; // for luxury brand items like Prada

    public Deck deckPrefab;
    public GameObject cardPrefab;
    public GameObject cardCutPrefab;
    public int numDecks = 8;
    public int numDecksToCut = 1;
    public GameObject cardParentGO; // Deck place holder game object where instantiated player/banker card GO children go

    // Three speed levels: slow - 1, normal - 2, fast - 3
    // TODO: add to preferences
    private static int speedLevel = 3;
    public static float dealSpeed  // seconds for iTween animation
    {
        get
        {
            switch (speedLevel) {
                case 1: // slow
                    return 2f;

                case 3: // fast
                    return 0.3f;

                case 2: // normal
                default:
                    return 0.6f;
            }
        }
        set
        {
           // do nothing
        }
    }
    public static float animatorSpeed  // speed factor for animator controller for dealer character
    {
        get
        {
            switch (speedLevel) {
                case 1: // slow
                    return 3f;
                case 3: // fast
                    return 13f;
                case 2: // normal
                default:
                    return 8f;
            }
        }
        set
        {
           // do nothing
        }
    }

    // References to the place holder planes used to position cards
    public GameObject DealerBanker1PlaceHolder;
    public GameObject DealerBanker2PlaceHolder;
    public GameObject DealerBanker3PlaceHolder;
    public GameObject DealerPlayer1PlaceHolder;
    public GameObject DealerPlayer2PlaceHolder;
    public GameObject DealerPlayer3PlaceHolder;
    public GameObject DeckPlaceHolder;
    public GameObject Player3Banker1PlaceHolder;
    public GameObject Player3Banker2PlaceHolder;
    public GameObject Player3Banker3PlaceHolder;
    public GameObject Player3Player1PlaceHolder;
    public GameObject Player3Player2PlaceHolder;
    public GameObject Player3Player3PlaceHolder;

    public GameObject DealersChips;

    // For tutorials
    public GameObject moveChipHand;
    public GameObject tapRotateSqueezeHand;
    public GameObject singleFingerSqueezeHand;
    public GameObject doubleFingerSqueezeHand;

    // How much a kind of win pays
    public int playerWinPayback = 1;
    public int bankerWinPayback = 1;
    public int tieWinPayback = 8;
    public int playerPairWinPayback = 11;
    public int bankerPairWinPayback = 11;

    private iBlow blowDetector;
    ArrayList decks = new ArrayList ();
    public Hashtable dealtCards;
    public bool squeezing = false;
    int playerCard1Value = 0;
    int playerCard2Value = 0;
    int playerCard3Value = 0;
    int bankerCard1Value = 0;
    int bankerCard2Value = 0;
    int bankerCard3Value = 0;
    int playerTotal = 0;
    int bankerTotal = 0;
    bool playerDrew3rdCard = false;
    int players3rdCardValue = 0;
    bool isDrawing3rdCards = false;
    bool skipSqueezing = false;
    bool need3rdBankerCardOn3rdPlayerCardCallback = false;
    bool isCut = false;
    bool lastRound = false;
    int shoeNumber = 1;
    bool lastRoundNext = false;
    GameObject cutCardGameObject = null;
    List<GameObject> m_CutCards = new List<GameObject>();
    int cutCardIdx = CUT_CARD_START_IDX;
    public GameObject cardDisplaying = null;
    private bool otherCardsRevealed = false;
    private bool hasNatural = false;
    public bool hasPlayerNatural = false;
    public bool hasBankerNatural = false;
    public bool dealtPlayerPair = false;
    public bool dealtBankerPair = false;
    bool isCalculating3rdCardTotals = false;

    // Animator variables
    public GameObject dealerCharacter;
    public Animator dealerAnimator;
    public GameObject dress;
    public GameObject bikiniTop;
    public GameObject sexyTop;
    public GameObject miniSkirt;

    // Use this for initialization
    void Start ()
    {
        // Register ourselves with the game state manager
        GameState.Instance.dealer = this;

        // Blow detector for good luck kisses while squeezing cards
        //blowDetector = this.gameObject.GetComponent<iBlow>();

        loadDealerVoiceAudio();

        dealtCards = new Hashtable();

        // Dealer animator initialization
        dealerAnimator = dealerCharacter.GetComponent<Animator>();
        dealerAnimator.speed = animatorSpeed;

        StartCoroutine(initDealer());
    }

    void displayMessage(string msg, float delay) {
        GameState.Instance.guiControls.displayMessage(msg, delay);
    }

    void displayMessage(string msg) {
        GameState.Instance.guiControls.displayMessage(msg);
    }

    IEnumerator initDealer() {
        Debug.Log ("Initializing Dealer");

        // Wait a little bit before we play the welcome message so the scene is actually loaded and displaying
        // before the dealer says anything. I found that the speaking would begin while still on the splash screen sometimes.
        yield return new WaitForSeconds(vwelcome.clip.length);

        // Welcome message
        vwelcome.Play();
        displayMessage (LanguageManager.GetText("label_welcome"), vwelcome.clip.length);
        yield return new WaitForSeconds(vwelcome.clip.length);
        changeClothes();

        initializeDecks();
    }

    // Initialize the shoe by populating it with decks of cards
    void initializeDecks ()
    {
        // Clear the decks (needed for shoes #2 and onwards)...
        decks.Clear();

        GameState.Instance.roundNumber = 1;

        // Reset the roadmap
        GameState.Instance.roadmapManager.resetRoadmap();

        int i;
        for (i = 0; i < numDecks; i++) {
            Deck newDeck = Instantiate (deckPrefab) as Deck;
            newDeck.Initialize ();
            newDeck.Reset ();
            newDeck.Shuffle (); // shuffles each deck individually
            decks.Add (newDeck);
        }

        // Now shuffle all 8 decks together with Knuth shuffle
        for (int k = 0; k < 7; k++) { // I read somewhere that 7 Knuth shuffles was pretty random :)
            int deckNumI = 0, deckNumJ = 0, ii;
            for ( i = 417; i > 1; i--)
            {
                 // Pick random element to swap.
                 int j = Random.Range(0, i); // 0 <= j <= i-1
    
                deckNumI = getNormalizedDeckNumber(i);
                deckNumJ = getNormalizedDeckNumber(j);
    
                ii = i % 51;
                j %= 51;
                ii++; j++;
    
                //Debug.LogError("Swapping deck["+(deckNumI)+"],card["+(ii)+"] with deck["+(deckNumJ)+"],card["+(j)+"]");
    
                CardDef temp1 = ((Deck) decks[deckNumJ]).getCardAtPos(j);
                ((Deck) decks[deckNumJ]).replaceCardAtPos(((Deck) decks[deckNumI]).getCardAtPos(ii-1), j);
                ((Deck) decks[deckNumI]).replaceCardAtPos(temp1, ii-1);
            }
        }

        // TEST check for valid shoe
        int t1 = 1, t2 = 1;
        Dictionary<string, Dictionary<string, int>> all = new Dictionary<string, Dictionary<string, int>>();
        for (i = 0; i < numDecks; i++) {
            for (int j = 0; j < 52; j++) {
                CardDef cd = ((Deck) decks[i]).getCardAtPos(j);
                string s = cd.Symbol;
                string v = cd.Text;
                if (!all.ContainsKey(s))
                    all[s] = new Dictionary<string, int>();
                if (!all[s].ContainsKey(v))
                    all[s][v] = 1;
                else
                    all[s][v]++;
                t1++;
            }
        }
        bool acceptable = true;
        foreach (string key1 in all.Keys) {
            foreach (string key2 in all[key1].Keys) {
                if (all[key1][key2] != numDecks) {
                    Debug.LogError (key1 + " " + key2 + " == " + all[key1][key2] + " failed shoe validation test!");
                    acceptable = false;
                }
                t2++;
            }
        }
        if (!acceptable)
            Debug.LogError (numDecks + " DECK SHOE VALIDATION FAILED! t1="+t1+", t2="+t2);
        else
            Debug.Log (numDecks + " DECK SHOE VALIDATION SUCCEEDED! t1="+t1+", t2="+t2);

        Debug.Log ("Initialized the shoe with " + numDecks + " shuffled decks");

        // Cut the cards if just started a new game
        if (!isCut) {
            CutCards();
        }

       
        // TEST! Use up most of the shoe so we can test the end of one game
        /*
        for (int j = 0; i < 363; i++) {
           popShoe();
        }
//        Debug.LogWarning("Deck 1 Remaining: " + ((CardDeck) decks[0]).Remaining());
//        Debug.LogWarning("Deck 2 Remaining: " + ((CardDeck) decks[1]).Remaining());
//        Debug.LogWarning("Deck 3 Remaining: " + ((CardDeck) decks[2]).Remaining());
//        Debug.LogWarning("Deck 4 Remaining: " + ((CardDeck) decks[3]).Remaining());
//        Debug.LogWarning("Deck 5 Remaining: " + ((CardDeck) decks[4]).Remaining());
//        Debug.LogWarning("Deck 6 Remaining: " + ((CardDeck) decks[5]).Remaining());
//        Debug.LogWarning("Deck 7 Remaining: " + ((CardDeck) decks[6]).Remaining());
//        Debug.LogWarning("Deck 8 Remaining: " + ((CardDeck) decks[7]).Remaining());
        Deck dec = (Deck) decks[currentDeckNum];
        int r = dec.Remaining();
        int jj = 0;
        for (; jj < r; jj++) {
            CardDef c = dec.getCardAtPos(jj);
            //Debug.Log ("** Card #"  + (jj+1) + " deck: " + (currentDeckNum+1));
            if (c.markedAsCutCard)
                Debug.LogWarning (">>>>>>>> Card "  + (jj+1) + " is the cut card in in deck #" + (currentDeckNum+1));
        }
        */
    }

    /** Get the individual deck number relative to a global index in an ASSUMED 8 deck shoe */
    private int getNormalizedDeckNumber(int idx) {
        if (idx >= 0 && idx <= 52)
            return 0;
        else if (idx >= 53 && idx <= 104)
            return 1;
        else if (idx >= 105 && idx <= 1156)
            return 2;
        else if (idx >= 157 && idx <= 208)
            return 3;
        else if (idx >= 209 && idx <= 260)
            return 4;
        else if (idx >= 261 && idx <= 312)
            return 5;
        else if (idx >= 313 && idx <= 364)
            return 6;
        else if (idx >= 365 && idx <= 417)
            return 7;

        Debug.LogWarning("Invalid shoe index " + idx + ", can't work out normalized deck number");
        return 0;
    }

    void CutCards() {
       // TODO: add skip card counting to preferences??
       //if (skipCutting) {
            // Until we get card cutting work generate a random number somewhere in the last deck to designate as the cut card
            int rndIndx = Random.Range (10, 30);
            cutCardIdx = (numDecks-1)*52+rndIndx;
            markCutCardInShoe();
            StartCoroutine(PlaceBets());isCut=true;
            return;
       // }

        // Cut the cards
        vplease_cut.Play();
        displayMessage(LanguageManager.GetText("label_please_cut"), vplease_cut.clip.length);

        // Layout some cards on the table and mark one as the cut card which the user can then reinsert into the decks
        for (int i = 0, m = 0; i < numDecksToCut; i++) {
            for (int j = 0; j < 52; j++, m++) {
                // Instantiate the card and move it into position
                CardDef c1 = ((Deck) decks[i]).getCardAtPos(j);
                GameObject newObj = Instantiate (cardPrefab) as GameObject;
                newObj.name = "CutCard" + m; // important to find indexes later
                newObj.transform.parent = DeckPlaceHolder.transform;

                // Make it the cut card?
                Card newCard = newObj.GetComponent<Card> () as Card;
                newCard.Definition = c1;
                if (m == CUT_CARD_START_IDX) {
                    newCard.CutCard = true;
                }

                m_CutCards.Add(newObj);

                // Take note of a card near the right to help set the right boundary for sliding
                if (m == Mathf.Round (0.95f*(52f*numDecksToCut))) {
                    Debug.Log ("Using "+m+" card as marker for right-most slide boundary");
                    m_CutCards[CUT_CARD_START_IDX].GetComponent<Card>().rightBoundaryCardObj = newObj;
                }

                // Prepare a callback with params after the itween to move the cards half way is done
                Hashtable multiParams = new Hashtable();
                multiParams.Add("gameObject", newObj);
                multiParams.Add("m", m);

                // First leg of cards: move from deck close to player
                iTween.MoveTo (newObj, iTween.Hash("time", 1.0f, "delay", m*0.03f, "path", iTweenPath.GetPath ("CutCardsPath"),
                    "onComplete", "CutCardsLayout",
                    "onCompleteTarget", this.gameObject,
                    "onCompleteParams", multiParams));

                newCard.playSlidingSound();
            }
        }
    }

    // Callback to layout the cards for card cutting
    public void CutCardsLayout(Hashtable multiParams) {
        if (multiParams == null) {
            return;
        }

        // Extract them
        GameObject cardGameObject = (GameObject) multiParams["gameObject"];
        int m = (int) multiParams["m"];

        // Get the Card object and store a reference to it
        Card card = cardGameObject.GetComponent<Card>();
        if (card.isCutCard) {
            cutCardGameObject = cardGameObject;
        }

        if (m == (52*numDecksToCut)-1) {
            // "Callback" for when the last card has finished its tweening
            iTween.MoveBy (cardGameObject, iTween.Hash ("x", m*(numDecksToCut/175f), "time", 0.014, "space", "world",
                "onComplete", "CutCard",
                "onCompleteTarget", this.gameObject));
        } else {
            // No callback needed
            iTween.MoveBy (cardGameObject, iTween.Hash ("x", m*(numDecksToCut/175f), "time", 0.014,  "space", "world"));
        }

        card.playRevealingSound();
    }

    // Callback to pop the cut card and configure it so the user can move it
    float cutCardOffset = 0.02f;
    float cutCardStartTime = 0f;
    public void CutCard() {
        if (cutCardGameObject == null) {
            return;
        }

        Debug.Log ("Beginning card cutting");
        cutCardStartTime = Time.time;

        Card card = cutCardGameObject.GetComponent<Card>();
        cutCardGameObject.GetComponent<Card> ().setGestureRecognizerStates(true);
        iTween.MoveBy (cutCardGameObject, iTween.Hash ("y", cutCardOffset/2, "x", cutCardOffset, "time", 1.2f, "space", "self",
            "onComplete", "startCutCardMoving"));
        card.playSlidingSound();
    }

    // Ending of cut cutting...
    public void endCardCut() {
        Debug.Log ("Ending card cutting");
        // Inserts red cut card back into the deck(s) of cards
        iTween.MoveBy (cutCardGameObject, iTween.Hash ("y", -cutCardOffset/2, "x", -cutCardOffset, "time", 1.2f, "space", "self",
            "onComplete", "endCardCutPart2",
            "onCompleteTarget", this.gameObject));
        cutCardGameObject.GetComponent<Card>().playReturningSound();
    }

    public void endCardCutPart2() {
        StartCoroutine (endCardCutPart2Coroutine());
    }

    // Repack cards after cut card has been "inserted" back into laid out cards
    IEnumerator endCardCutPart2Coroutine() {
        isCut = true;

        // Get index of where the cut card was inserted
        Card card = m_CutCards[CUT_CARD_START_IDX].GetComponent<Card>();
        card.detectCutCardHit = false;
        int origIdx = CUT_CARD_START_IDX; // indx in original pack(s) of cards actually laid out on table
        cutCardIdx = 39; // default
        if (card.lastCutCardCollider != null) {
            // Get index from card object name
            Debug.Log ("Red card last hit card collider " + card.lastCutCardCollider.name);
            cutCardIdx = int.Parse(Regex.Match(card.lastCutCardCollider.name, @"\d+").Value)-1;
            origIdx = cutCardIdx;
            cutCardIdx *= numDecks-numDecksToCut; // even if we don't layout all 8 decks of cards on the table we can simulate cut card going near the back
            if (cutCardIdx <= 6) // min check
                cutCardIdx = 30 * (numDecks-numDecksToCut);
            if (cutCardIdx >= (numDecks*52)-7) // max check
                cutCardIdx = (numDecks*52)-7;
        }
        Debug.Log ("Cut card is no. " + cutCardIdx);

        // Play a nice cut if they placed it nearer the right half of the cards on the table and within a nice time
        if (cutCardIdx > (numDecks*52)*0.66f && (Time.time - cutCardStartTime) < 4.0f) {
            vnice_cut.Play();
            displayMessage(LanguageManager.GetText("label_nice_cut"), vnice_cut.clip.length);
            yield return new WaitForSeconds(vnice_cut.clip.length);
        }

        // As cut card has been shifted, we update its index to ensure reverse animation order of cards looks right
        Debug.Log("cutCardIdx:"+cutCardIdx+", origIdx:"+origIdx+", mCut_Cards.Count:"+m_CutCards.Count+", CUT_CARDS_START_IDX:"+CUT_CARD_START_IDX+ ", m_CutCards[CUT_CARD_START_IDX]:"+m_CutCards[CUT_CARD_START_IDX]);
        m_CutCards.Insert (origIdx, m_CutCards[CUT_CARD_START_IDX]);

        // Reverse animations so the cut cards fly back into the deck
        Vector3[] firstPathNodes = iTweenPath.GetPath("CutCardsPath");
        int m = 0;
        foreach (GameObject cardObj in m_CutCards) {
            Hashtable multiParams = new Hashtable();
            cardObj.GetComponent<Card>().playSlidingSound();
            multiParams.Add("gameObject", cardObj);
            multiParams.Add ("m", m);
            iTween.MoveTo(cardObj,  iTween.Hash (
                "x", firstPathNodes[firstPathNodes.Length-1].x,
                "y", firstPathNodes[firstPathNodes.Length-1].y,
                "z", firstPathNodes[firstPathNodes.Length-1].z,
                "time", 0.5f,
                "onComplete", "endCardCutPart3", "space", "world",
                "onCompleteTarget", this.gameObject,
                "onCompleteParams", multiParams));
            m++;
        }
    }

    public void endCardCutPart3(Hashtable multiParams) {
        GameObject cardGameObject = (GameObject) multiParams["gameObject"];
        int m = (int) multiParams["m"];

        if (m == (52*numDecksToCut)-1) {
            // "Callback" for when the last card has finished its tweening
            iTween.MoveTo (cardGameObject, iTween.Hash( "time", 0.3f, "path", iTweenPath.GetPathReversed ("CutCardsPath"),
                "onComplete", "DestroyCutCards",
                "onCompleteTarget", this.gameObject));
        } else {
            // No callback needed
            cardGameObject.GetComponent<Card>().playSlidingSound();
            iTween.MoveTo (cardGameObject, iTween.Hash("time", 0.8f+1f/m, "path", iTweenPath.GetPathReversed ("CutCardsPath")));
        }
    }

    public void DestroyCutCards() {
        foreach (GameObject cardObj in m_CutCards) {
            DestroyImmediate (cardObj.GetComponent<MeshFilter>().mesh);
            Destroy(cardObj);
        }
        m_CutCards.Clear();
        m_CutCards = null;
        Debug.Log ("All cards used for cutting destroyed");

        // Update shoe of which card is now the cut card
        markCutCardInShoe();

//        // Test for location of marked cut card
//        for (int i = 0, m = 0; i < numDecks; i++) {
//            List<CardDef> m_cards = ((Deck)decks[i]).m_cards;
//            for (int j = 0; j < m_cards.Count; j++, m++) {
//                Debug.Log ("m_cards["+m+"] : " + m_cards[j].Symbol + " " + m_cards[j].Text + ".is marked? " + m_cards[j].markedAsCutCard);
//            }
//        }

        StartCoroutine(PlaceBets());
    }

    void markCutCardInShoe() {
        for (int i = 0, m = 0; i < numDecks; i++) {
            for (int j = 0; j < 52; j++, m++) {
                if (m == cutCardIdx) {
                    CardDef c1 = ((Deck) decks[i]).getCardAtPos(j);
                    c1.markedAsCutCard = true;
                    Debug.Log ("Shoe updated to mark cut card at position " + cutCardIdx + " ("+c1.Symbol + " " + c1.Text
                        + ") [deck: " + (i+1) + ", card: " + (j+1) + ", " + c1.GetHashCode() + "]");
                }
            }
        }
    }

    IEnumerator PlaceBets() {
        GameState.Instance.currentState = GameState.State.PlaceBets;
        yield return new WaitForSeconds(.5f);
//        SayPlaceBets();
    }

    public void SayPlaceBets() {
        SayPlaceBets(true);
    }

    public void SayPlaceBets(bool displayMesg) {
        if (GameState.Instance.currentState != GameState.State.CutCards) {
            vplace_bets.Play();
            if (displayMesg)
                displayMessage(LanguageManager.GetText("label_place_bets"), vplace_bets.clip.length);
        }
    }

    // >>> Unit test code for forced hands <<<
    int xx = 0;

    // Pop a card from the shoe
    int currentDeckNum = 0;
    CardDef popShoe ()
    {
        // Check if there remains any cards in this deck
        Deck currentDeck = (Deck) decks[currentDeckNum];
        if (currentDeck.Remaining() <= 0) {
            if (currentDeckNum >= numDecks-1 && currentDeck.Remaining() <= 1) {
                // We should never get this far coz the cut card will end the game prior to the very last card in the shoe,
                // but on the safe side we have this logic to restart the shoe silently
                Debug.LogWarning("Uh-oh, we reached the last card in the shoe somehow!! Resetting shoe...");
                reset ();
                cutCardGameObject = null;
                lastRoundNext = false;
                lastRound = false;
                currentDeckNum = 0;
                isCut = false;
                initializeDecks();
                GameState.Instance.currentState = GameState.State.CutCards;
                return null;
            }

            Debug.Log ("Shoe changing to deck #"+(currentDeckNum+1)+" because number of cards remaining in previous deck is "
                + currentDeck.Remaining());
            currentDeck = (Deck) decks[++currentDeckNum];
        }

        // Update TV display of how many cards left
        GameState.Instance.roadmapManager.setCardsRemaining();
        Debug.Log ("Popping card " + currentDeck.Remaining() + " from deck " + (currentDeckNum+1) + " in the shoe");
        CardDef d = currentDeck.Pop ();
        Debug.Log (currentDeck.Remaining() + " cards left in current deck #"+currentDeckNum+1);

          // >>> Forced hand unit testing code
          // Switch 1st and 2nd to make player/banker get a 3rd card
//            if (xx==0){
//                d.Text = "A";
//            } else if (xx==1){
//                d.Text = "5";
//            } else if (xx==2){
//                d.Text = "A";
//            } else if (xx==3){
//                d.Text = "2";
//            } else if (xx==4){
//                d.Text = "2";
//            } else if (xx==5){
//                d.Text = "A";
//            }
         // Both player and banker have 3 cards
//        if (xx==0){
//            d.Text = "A";
//        } else if (xx==1){
//            d.Text = "A";
//        } else if (xx==2){
//            d.Text = "A";
//        } else if (xx==3){
//            d.Text = "A";
//        } else if (xx==4){
//            d.Text = "A";
//        } else if (xx==5){
//            d.Text = "A";
//        }
//        // Natural 9 & 0
//        if (xx==0){
//            d.Text = "7";
//        } else if (xx==1){
//            d.Text = "K";
//        } else if (xx==2){
//            d.Text = "2";
//        } else if (xx==3){
//            d.Text = "10";
//        } else if (xx==4){
//            d.Text = "A";
//        } else if (xx==5){
//            d.Text = "A";
//        }
//        // Tie 6
        if (xx==0){
            d.Text = "3";
        } else if (xx==1){
            d.Text = "4";
        } else if (xx==2){
            d.Text = "4";
        } else if (xx==3){
            d.Text = "3";
        } else if (xx==4){
            d.Text = "1";
        } else if (xx==5){
            d.Text = "1";
        }
//        // Player pair 4/Banker pair 0
//        if (xx==0){
//            d.Text = "4";
//        } else if (xx==1){
//            d.Text = "Q";
//        } else if (xx==2){
//            d.Text = "4";
//        } else if (xx==3){
//            d.Text = "Q";
//        } else if (xx==4){
//            d.Text = "K";
//        } else if (xx==5){
//            d.Text = "K";
//        }
        // Player pair 0/Banker pair 4
//        if (xx==0){
//            d.Text = "Q";
//        } else if (xx==1){
//            d.Text = "4";
//        } else if (xx==2){
//            d.Text = "Q";
//        } else if (xx==3){
//            d.Text = "4";
//        } else if (xx==4){
//            d.Text = "K";
//        } else if (xx==5){
//            d.Text = "K";
//        }
        // Player pair ?/Banker pair ?
//        if (xx==0){
//            d.Text = "4";
//        } else if (xx==1){
//            d.Text = "2";
//        } else if (xx==2){
//            d.Text = "4";
//        } else if (xx==3){
//            d.Text = "2";
//        } else if (xx==4){
//            d.Text = "K";
//        } else if (xx==5){
//            d.Text = "K";
//        }
        // Free
//        if (xx==0){
//            d.Text = "A";
//        } else if (xx==1){
//            d.Text = "8";
//        } else if (xx==2){
//            d.Text = "A";
//        } else if (xx==3){
//            d.Text = "A";
//        } else if (xx==4){
//            d.Text = "A";
//        } else if (xx==5){
//            d.Text = "A";
//        }
        xx++;
        // <<< EOF forced hand unit test coding

        // TODO: add burning (cut card etc)

        return d;
    }

    // Deal the cards
    public void deal ()
    {
        // dealPlayer(pos==1) deals the player's 1st card, then on completion invokes callback to dealBanker's 1st card,
        // which then does the players 2nd card and so on...
        // If the only bet is on a Tie then we skip squeezing
        if (GameState.Instance.getCurrentBetType() == GameState.BetType.Tie) {
            // TODO: insert check logic if squeeze is disabled or not
            skipSqueezing = true;
        }

        // Check if we're the second to last round
        if (lastRoundNext && !lastRound) {
            Debug.Log ("This is the last round as the cut card was dealt in the previous round");
            lastRound = true;
        }

        // Cut the cards if just started a new shoe
        if (!isCut) {
            CutCards();
        }

        nextPlayerPos = 1;
        isDrawing3rdCards = false;

        // Start the dealer moving her hands
        // which will in turn trigger the events to start dealing cards
        if (dealerAnimator != null && dealerCharacter.activeSelf) {
            dealerAnimator.SetBool("Deal4Cards", true);
        } else {
            // Rely on iTween for no-dealer-hand assisted dealing of cards
            dealPlayer ();
        }
    }

    private int nextPlayerPos = 0;

    // Callback(?)
    public void dealPlayer ()
    {
        StartCoroutine(dealPlayerCoroutine (null, false));
    }

    public void dealPlayer (string callback)
    {
        StartCoroutine(dealPlayerCoroutine (callback, false));
    }

    // Called from DealerAnimatorEventReceiver.
    public void dealPlayer (int number)
    {
        // Trigger card dealing based on animation events from the animated dealer.
        // Start dealing a card when their hand reaches the shoe.
        nextPlayerPos = number;
        StartCoroutine(dealPlayerCoroutine (null, true));
    }

    IEnumerator dealPlayerCoroutine (string callback, bool dealerTriggered)
    {
        // Determine the iTween path to use based on the position
        string iTweenPathName = "";
        switch (nextPlayerPos) {
        case 3:
            iTweenPathName = (GameState.Instance.getCurrentBetType() == GameState.BetType.Player ? "Deck2DealerPlayer3Animator" : "Deck2DealerPlayer3");
            break;
        case 2:
            iTweenPathName = "Deck2DealerPlayer2";
            break;
        case 1:
            iTweenPathName = "Deck2DealerPlayer1";
            break;
        case -1:
            if (!isDrawing3rdCards) {
                notifyGameStateCardsDealt ();
                yield break;
            }
            break;
        case 0:
        default:
            yield break;

        }

        // Check if it's the last round, and say so if-so
        if (lastRound && nextPlayerPos == 1) {
            Debug.Log ("This is the last round");
            displayMessage(LanguageManager.GetText("label_last_round"), vlast_round.clip.length);
            vlast_round.Play();
            //yield return new WaitForSeconds(vlast_round.clip.length);
        }

        // Instantiate the card and move it into position
        CardDef c1 = popShoe ();
        if (c1 == null) {
            Debug.LogWarning("Shoe returned null card on pop"
                 + " Deck no. " + (currentDeckNum+1) + " has " + ((Deck) decks[currentDeckNum]).Remaining() + " cards remaining.");
            yield break;
        }

        // We need to know the value of the 3rd player card ahead of tweening to figure out if we need a banker 3rd card too
        if (nextPlayerPos == 3) {
            players3rdCardValue = c1.getValue();
        }

        Debug.Log ("Popped card is " + c1.Symbol + " " + c1.Text);
        GameObject newObj = Instantiate (cardPrefab) as GameObject;

        // Below is the failed attempt to use a hinge joint on the dealer's index finger
        // to move the card instead of iTween.
        //Rigidbody rigidbody = leftFingerNub.GetComponent<Rigidbody>();
        //HingeJoint hingeJoint = newObj.GetComponent<HingeJoint>();
        //hingeJoint.connectedBody = rigidbody;

        float dealeredSpeed = dealSpeed;
        if (nextPlayerPos == 1) {
            vplayer_card.Play();
            dealeredSpeed *= 1.6f;
        } else if (nextPlayerPos == 2) {
            if (!Utils.isEnglish()) // don't play the English one because it overlaps the previous wav playback
                vplayer_card2.Play();
            dealeredSpeed *= 1.5f;
        } else if (nextPlayerPos == 3) {
            vplayer_card3.Play();
        }

        newObj.name = "PlayerCard" + nextPlayerPos;
        if (dealtCards.ContainsKey(newObj.name)) {
            dealtCards.Remove(newObj.name);
        }
        dealtCards.Add (newObj.name, newObj);
        Card newCard = newObj.GetComponent<Card> () as Card;
        newCard.cardType = GameState.BetType.Player;
        newCard.Definition = c1;
        newCard.Rebuild ();

        // Check if this card is the cut card
        StartCoroutine(checkForCutCard(newCard));

        if (dealerAnimator != null && dealerCharacter.activeSelf
            && (GameState.Instance.getCurrentBetType() == GameState.BetType.Player)) {
            if (dealerTriggered && nextPlayerPos == 3 ) {
                // Animate the dealer dishing out the 3rd player card
                dealerAnimator.SetBool("Deal3rdCard", true);
            }
        }

        nextBankerPos++;
        if (nextBankerPos > 2) {
            nextPlayerPos = 0;
        }

        Hashtable callHash;
        if (dealerTriggered) {
            if (nextPlayerPos == 0) { // player card #3
                callHash = iTween.Hash("time", dealSpeed*2f, "path", iTweenPath.GetPath (iTweenPathName),
                                       "onComplete", callback, "onCompleteTarget", gameObject);
            } else {
                callHash = iTween.Hash("time", dealeredSpeed*2f, "path", iTweenPath.GetPath (iTweenPathName));
            }
        } else {
            callHash = iTween.Hash("time", dealSpeed*2f, "path", iTweenPath.GetPath (iTweenPathName),
                "onComplete", (callback == null ? "dealBanker" : callback), "onCompleteTarget", gameObject);
        }
        newCard.slideSelf(callHash);
    }

    private int nextBankerPos = 0;

    public void dealBanker ()
    {
        StartCoroutine(dealBankerCoroutine (null, false));
    }

    public void dealBanker (string callback)
    {
        StartCoroutine(dealBankerCoroutine (callback, false));
    }

    // Called from DealerAnimatorEventReceiver.
    public void dealBanker (int number)
    {
        // Trigger card dealing based on animation events from the animated dealer.
        // Start dealing a card when their hand reaches the shoe.
        nextBankerPos = number;
        StartCoroutine(dealBankerCoroutine (null, true));
    }

    IEnumerator dealBankerCoroutine (string callback, bool dealerTriggered)
    {
        // Determine the iTween path to use based on the position
        string iTweenPathName = "";
        switch (nextBankerPos) {
        case 3:
            iTweenPathName = (GameState.Instance.getCurrentBetType() == GameState.BetType.Banker ? "Deck2DealerBanker3Animator" : "Deck2DealerBanker3");
            break;
        case 2:
            iTweenPathName = "Deck2DealerBanker2";
            break;
        case 1:
            iTweenPathName = "Deck2DealerBanker1";
            break;
        case 0:
        default:
            yield break;
        }

        // Instantiate the card and move it into position
        CardDef c1 = popShoe ();
        if (c1 == null) {
            Debug.LogWarning("Shoe returned null card on pop"
                + " Deck no. " + (currentDeckNum+1) + " has " + ((Deck) decks[currentDeckNum]).Remaining() + " cards remaining.");
            yield break;
        }

        Debug.Log ("Popped card is " + c1.Symbol + " " + c1.Text);
        GameObject newObj = Instantiate (cardPrefab) as GameObject;
        float dealeredSpeed = dealSpeed;

        if (nextBankerPos == 1) {
            playWithDelay(vbanker_card, 0.3f);
            dealeredSpeed *= 1.7f;
        } else if (nextBankerPos == 2) {
            if (!Utils.isEnglish()) // don't play the English one because it overlaps the previous wav playback
                vbanker_card2.Play();
        } else if (nextBankerPos == 3) {
            if (dealtCards["PlayerCard3"] != null)
                playWithDelay(vbanker_card3, 0.3f);
            else
                vbanker_card3.Play();
        }

        newObj.name = "BankerCard" + nextBankerPos;
        if (dealtCards.ContainsKey(newObj.name)) {
            dealtCards.Remove(newObj.name);
        }
        dealtCards.Add (newObj.name, newObj);
        Card newCard = newObj.GetComponent<Card> () as Card;
        newCard.cardType = GameState.BetType.Banker;
        newCard.Definition = c1;
        newCard.Rebuild ();

        // Check if this card is the cut card
        StartCoroutine(checkForCutCard(newCard));

        if (dealerAnimator != null && dealerCharacter.activeSelf
            && GameState.Instance.getCurrentBetType() == GameState.BetType.Banker) {
            if (dealerTriggered && nextBankerPos == 3) {
                // Animate the dealer dishing out the 3rd banker card
                dealerAnimator.SetBool("Deal3rdCard", true);
            }
        }

        nextPlayerPos++;
        if (nextPlayerPos > 2) {
            nextPlayerPos = -1;
        }

        Hashtable callHash;
        if (dealerTriggered && nextPlayerPos != -1) {
             if (nextBankerPos == 3) { // banker card #3
                callHash = iTween.Hash("time", dealSpeed*2f, "path", iTweenPath.GetPath (iTweenPathName),
                "onComplete", callback, "onCompleteTarget", gameObject);
            } else {
                callHash = iTween.Hash("time", dealeredSpeed*2f, "path", iTweenPath.GetPath (iTweenPathName));
            }
        } else {
            callHash = iTween.Hash("time", dealSpeed*2f, "path", iTweenPath.GetPath (iTweenPathName),
                "onComplete", (callback == null ? "dealPlayer" : callback), "onCompleteTarget", gameObject);
        }
        newCard.slideSelf(callHash);
    }

    // Check if the cut card as come out of the shoe
    IEnumerator checkForCutCard(Card card) {
        if (card != null) {
            // Check if we're a cut card first
            if (card.Definition.markedAsCutCard) {
                Debug.Log ("The cut card has been dealt (" + card.gameObject.name + ")");
                displayMessage(LanguageManager.GetText("label_cut_card_appears"), vcut_card_appears.clip.length);
                vcut_card_appears.Play();
                yield return new WaitForSeconds(vcut_card_appears.clip.length);

                // If the first card in this round was the cut card, then this is the last round,
                // otherwise the next round is the last
                if (card.gameObject.name == "PlayerCard1") {
                    Debug.Log ("The cut card is the first card this round so this is the last round");
                    lastRound = true;
                    displayMessage(LanguageManager.GetText("label_last_round"), vlast_round.clip.length);
                    vlast_round.Play();
                    yield return new WaitForSeconds(vlast_round.clip.length);
                } else {
                    Debug.Log ("The cut card is NOT the first card this round so next round is the last");
                    lastRoundNext = true;
                    //displayMessage(LanguageManager.GetText("label_last_round_next"));
                    //vlast_round_next.Play();
                    //yield return new WaitForSeconds(vlast_round_next.clip.length);
                }
            }
        }
    }

    // Callback method
    public void notifyGameStateCardsDealt ()
    {
        // GameState will decide whether to allow squeezing of the cards or reveal them straight away
        GameState.Instance.currentState = GameState.State.SqueezeCards;

        // Stop the dealer trying to deal the 4 cards again
        if (dealerAnimator != null && dealerCharacter.activeSelf) {
            dealerAnimator.SetBool("Deal4Cards", false);
        }
    }

    // Beging squeezing the first 2 cards
    public void beginSqueezing () {
        StartCoroutine(beginSqueezingCoroutine());
    }

    // Beging squeezing the first 2 cards
    IEnumerator beginSqueezingCoroutine ()
    {
        // Reveal the other cards first so the user can see their value and enjoy squeezing a better hand! Yay for squeezing fun!
        // Anticipate the moment!

        // Reveal other cards
        //revealOtherCards(); // Commented out coz now there's a button for the user to reveal them themselves

        squeezing = true;

        // COMMENTED OUT to try handling in GameState instead
        //GUIControls.SetAutoRotate(false);

        // Tutorial stuff
        GameState.Instance.tutorialHelpManager.fingers2Zoom(false);
        GameState.Instance.tutorialHelpManager.fingers3Swipe(false);
        GameState.Instance.tutorialHelpManager.clearBets(false);
        GameState.Instance.tutorialHelpManager.startDealing(false);

        // Report stats to Flurry about our betting habits
        Dictionary<string, string> betAmounts = new Dictionary<string, string>();
        if (GameState.Instance.betsOnTableHash.ContainsKey(GameState.BetType.Player)) {
            LogUtils.LogEvent(Consts.FE_BET_ON_EVENT, new string[] { Consts.FEP_BET_ON_PLAYER }, false);
            betAmounts.Add(Consts.FEP_BET_AMOUNT_PLAYER, ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.Player]).amount+"");
        }
        if (GameState.Instance.betsOnTableHash.ContainsKey(GameState.BetType.Banker)) {
            LogUtils.LogEvent(Consts.FE_BET_ON_EVENT, new string[] { Consts.FEP_BET_ON_BANKER }, false);
            betAmounts.Add(Consts.FEP_BET_AMOUNT_BANKER, ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.Banker]).amount+"");
        }
        if (GameState.Instance.betsOnTableHash.ContainsKey(GameState.BetType.Tie)) {
            LogUtils.LogEvent(Consts.FE_BET_ON_EVENT, new string[] { Consts.FEP_BET_ON_TIE }, false); 
            betAmounts.Add(Consts.FEP_BET_AMOUNT_TIE, ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.Tie]).amount+"");
        }
        if (GameState.Instance.betsOnTableHash.ContainsKey(GameState.BetType.PlayerPair)) {
            LogUtils.LogEvent(Consts.FE_BET_ON_EVENT, new string[] { Consts.FEP_BET_ON_PLAYER_PAIR }, false);
            betAmounts.Add(Consts.FEP_BET_AMOUNT_PLAYER_PAIR, ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.PlayerPair]).amount+"");
        }
        if (GameState.Instance.betsOnTableHash.ContainsKey(GameState.BetType.BankerPair)) {
            LogUtils.LogEvent(Consts.FE_BET_ON_EVENT, new string[] { Consts.FEP_BET_ON_BANKER_PAIR }, false);
            betAmounts.Add(Consts.FEP_BET_AMOUNT_BANKER_PAIR, ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.BankerPair]).amount+"");
        }
        LogUtils.LogEvent(Consts.FE_BET_AMOUNT_EVENT, betAmounts, false);
        int chips1Amount = 0;
        int chips2Amount = 0;
        int chips3Amount = 0;
        int chips4Amount = 0;
        int chips5Amount = 0;
        Dictionary<string, string> bettedChips = new Dictionary<string, string>();
        foreach (Chip chip in GameState.Instance.chipsManager.chipsOnTable) {
            if (chip.chipValue == 100)
                chips1Amount += chip.chipValue;
            if (chip.chipValue == 500)
                chips2Amount += chip.chipValue;
            if (chip.chipValue == 1000)
                chips3Amount += chip.chipValue;
            if (chip.chipValue == 10000)
                chips4Amount += chip.chipValue;
            if (chip.chipValue == 10000)
                chips5Amount += chip.chipValue;
        }
        if (chips1Amount > 0)
            bettedChips.Add (Consts.FEP_BET_CHIP1, chips1Amount+"");
        if (chips2Amount > 0)
            bettedChips.Add (Consts.FEP_BET_CHIP2, chips2Amount+"");
        if (chips3Amount > 0)
            bettedChips.Add (Consts.FEP_BET_CHIP3, chips3Amount+"");
        if (chips4Amount > 0)
            bettedChips.Add (Consts.FEP_BET_CHIP4, chips4Amount+"");
        if (chips5Amount > 0)
            bettedChips.Add (Consts.FEP_BET_CHIP5, chips5Amount+"");
        LogUtils.LogEvent(Consts.FE_BET_CHIP_EVENT, bettedChips, false);

        // If the only bet is on a Tie then we skip squeezing
        if (skipSqueezing) {
            GameState.Instance.camerasManager.ToggleTieCameras(true);
            end2CardsSqueezing();
            yield break;
        }


        // For the drag recognizer to register squeeze events from finger gestures
        cardParentGO.GetComponent<DragRecognizer>().enabled = true;

        // Switch to the squeeze camera so we can see the cards up close
        GameState.Instance.camerasManager.ToggleSqueezeCamera (true);

        // Show the button that can be used to reveal the other cards before squeezing your own
        GameState.Instance.guiControls.ToggleRevealOtherButton(true);

        // Start by position the squeeze camera on the first card
        GameState.Instance.camerasManager.moveSqueezeCamera ("left");

        // Show butons
        GameState.Instance.guiControls.clearButtonState = GUIControls.ClearButtonState.OtherCard;
        GameState.Instance.guiControls.dealButtonState = GUIControls.DealButtonState.ReturnCard;

        // Tutorial stuff
        if (GameState.Instance.tutorialCounter <= 1) {
            Card.isShowingTutorials = true; // disallows the pressing of the Other and Return buttons while showing the following two speech bubbles
            nextTapStopSpeechBubble = GameState.Instance.tutorialHelpManager.otherCard(true);
            nextTapObject = this.gameObject;
            nextTapMethodName = "showTutorialSpeechBubbleReturnCard";
        } else {
            // No tutorial to show, continue on to part 2 immediately
            beginSqueezingPart2();
        }
    }

    // This method is called after showing the tutorial speech bubble for "reval other card"
    void showTutorialSpeechBubbleReturnCard() {
        nextTapStopSpeechBubble = GameState.Instance.tutorialHelpManager.returnCard(true);
        nextTapObject = this.gameObject;
        nextTapMethodName = "beginSqueezingPart2";
    }

    // Beging squeezing the first 2 cards - part 2
    public void beginSqueezingPart2 () {
        // Tutorial stuff
        nextTapObject = null;
        nextTapMethodName = null;
        nextTapResetOnNext = true;

        StartCoroutine(beginSqueezingCoroutinePart2());
    }

    // Beging squeezing the first 2 cards - part 2
    IEnumerator beginSqueezingCoroutinePart2 ()
    {
        // Check if there are any bets on the banker or player and if so push the cards to the user so they can squeeze
        if (GameState.Instance.getCurrentBetType () == GameState.BetType.Player) {
            GameObject playerCard1 = dealtCards ["PlayerCard1"] as GameObject;
            GameObject playerCard2 = dealtCards ["PlayerCard2"] as GameObject;

            if (Utils.isJapanese())
                vplayer_customer.Play();

            Debug.Log ("Pushing user the first 2 player cards to squeeze");

            // Add the two cards to the list of objects that can be iBlowed
            //blowDetector.start(playerCard1, playerCard2);

            if (dealerAnimator != null && dealerCharacter.activeSelf) {
                dealerAnimator.SetBool("PushPlayer2Cards", true);
            }
            playerCard1.GetComponent<Card>().slideSelf(iTween.Hash("time", 2*dealSpeed, "path", iTweenPath.GetPath ("Dealer2Player3Player1")));
            playerCard2.GetComponent<Card>().slideSelf(iTween.Hash("time", 2*dealSpeed, "path", iTweenPath.GetPath ("Dealer2Player3Player2"),
                "onCompleteTarget", gameObject,
                "onComplete", "endPushPlayer2CardsAnimator"));

            // Scale size up a bit to avoid jitter/shakiness when squeezing
            ((GameObject) dealtCards["PlayerCard1"]).GetComponent<Card>().ToggleEnlarge();
            ((GameObject) dealtCards["PlayerCard2"]).GetComponent<Card>().ToggleEnlarge();

            // Tutorial stuff
            if (GameState.Instance.tutorialCounter <= 1) {
                yield return new WaitForSeconds(7);
            }

            //playerCard1.GetComponent<Card> ().setGestureRecognizerStates(true); // Now done by tutorial logic
            playerCard2.GetComponent<Card> ().setGestureRecognizerStates(true);
        } else if (GameState.Instance.getCurrentBetType () == GameState.BetType.Banker) {
            GameObject bankerCard1 = dealtCards ["BankerCard1"] as GameObject;
            GameObject bankerCard2 = dealtCards ["BankerCard2"] as GameObject;

            if (Utils.isJapanese())
                vbanker_customer.Play();

            Debug.Log ("Pushing user the first 2 banker cards to squeeze");

            // Add the two cards to the list of objects that can be iBlowed
            //blowDetector.start(bankerCard1, bankerCard2);

            if (dealerAnimator != null && dealerCharacter.activeSelf) {
                dealerAnimator.SetBool("PushBanker2Cards", true);
            }

            bankerCard1.GetComponent<Card>().slideSelf(iTween.Hash("time", 2*dealSpeed, "path", iTweenPath.GetPath ("Dealer2Player3Banker1")));
            bankerCard2.GetComponent<Card>().slideSelf(iTween.Hash("time", 2*dealSpeed, "path", iTweenPath.GetPath ("Dealer2Player3Banker2"),
                "onCompleteTarget", gameObject,
                "onComplete", "endPushBanker2CardsAnimator"));

             // Scale size up a bit to avoid jitter/shakiness when squeezing
            ((GameObject) dealtCards["BankerCard1"]).GetComponent<Card>().ToggleEnlarge();
            ((GameObject) dealtCards["BankerCard2"]).GetComponent<Card>().ToggleEnlarge();

            // Tutorial stuff
            if (GameState.Instance.tutorialCounter <= 1)
                yield return new WaitForSeconds(7);

            //bankerCard1.GetComponent<Card> ().setGestureRecognizerStates(true); // Now done by tutorial logic
            bankerCard2.GetComponent<Card> ().setGestureRecognizerStates(true);
        }
    }

    public void endPushBanker2CardsAnimator() {
        if (dealerAnimator != null && dealerCharacter.activeSelf) {
            dealerAnimator.SetBool("PushBanker2Cards", false);
        }

        // Tutorial stuff
        GameObject bankerCard1GO = dealtCards ["BankerCard1"] as GameObject;
        ((Card) bankerCard1GO.GetComponent<Card>()).StartSqueezeTutorial();
    }

    public void endPushPlayer2CardsAnimator() {
        if (dealerAnimator != null && dealerCharacter.activeSelf) {
            dealerAnimator.SetBool("PushPlayer2Cards", false);
        }

        // Tutorial stuff
        GameObject playerCard1GO = dealtCards ["PlayerCard1"] as GameObject;
        ((Card) playerCard1GO.GetComponent<Card>()).StartSqueezeTutorial();
    }

    // Stop any squeeze tutorials we may be in the middle of
    public void StopCardSqueezeTutorial() {
        if (GameState.Instance.getCurrentBetType() == GameState.BetType.Banker) {
            GameObject bankerCard1GO = dealtCards ["BankerCard1"] as GameObject;
            ((Card) bankerCard1GO.GetComponent<Card>()).StopSqueezeTutorial();
        } else {
            GameObject playerCard1GO = dealtCards ["PlayerCard1"] as GameObject;
            ((Card) playerCard1GO.GetComponent<Card>()).StopSqueezeTutorial();
        }
    }

    // Determine whether we're ending squeezing for the first 2 cards or 3rd cards
    public void endSqueezing() {
        // Avoid multiple calls to endSqueeze if being triggered by finger gestures
        if (!squeezing) {
            return;
        }

        // No more iBlowing if we're not squeezing
        //blowDetector.end();

        if (isDrawing3rdCards) {
            end3rdCardSqueezing();
        } else {
            // End squeezing only after both player or banker 1st and 2nd cards have been revealed
            end2CardsSqueezing();
        }
    }

    // Callback method to call when squeezing the first two cards is done
    public void end2CardsSqueezing ()
    {
        GameObject playerCard1GO = dealtCards ["PlayerCard1"] as GameObject;
        Card playerCard1 = playerCard1GO.GetComponent<Card> ();
        GameObject playerCard2GO = dealtCards ["PlayerCard2"] as GameObject;
        Card playerCard2 = playerCard2GO.GetComponent<Card> ();
        GameObject bankerCard1GO = dealtCards ["BankerCard1"] as GameObject;
        Card bankerCard1 = bankerCard1GO.GetComponent<Card> ();
        GameObject bankerCard2GO = dealtCards ["BankerCard2"] as GameObject;
        Card bankerCard2 = bankerCard2GO.GetComponent<Card> ();

        // If the only bet is on a Tie then we skip returning the squeeze cards and just goto reveal everything straight away
        if (skipSqueezing) {
            // Reveal all 2 banker and player cards (4 total)
            revealCardsTogether(playerCard1GO, playerCard2GO);
            revealCardsTogether(bankerCard1GO, bankerCard2GO);

            StartCoroutine(calculate2CardsTotals());
        } else {
            // Choose to return the squeezed player or banker cards
            if (GameState.Instance.getCurrentBetType () == GameState.BetType.Player) {
                if (playerCard1.readyToReturn && !playerCard2.readyToReturn) { // Return 1st player card, 2nd card is still not revealed
                    playerCard1.setGestureRecognizerStates(false);

                    // Reveal the card completely, then return it to the dealer
                    Debug.Log ("Revealing and returning PlayerCard1");
                    revealCardWithReturnCallback (playerCard1GO, iTween.Hash ("returnPath", "Dealer2Player3Player1"));
                } else if (!playerCard1.readyToReturn && playerCard2.readyToReturn) { // Return 2nd player card, 1st card is still not revealed
                    playerCard2.setGestureRecognizerStates(false);

                    // Reveal the card completely, then return it to the dealer
                    Debug.Log ("Revealing and returning PlayerCard2");
                    revealCardWithReturnCallback (playerCard2GO, iTween.Hash ("returnPath", "Dealer2Player3Player2"));
                } else if (playerCard1.readyToReturn && playerCard2.readyToReturn) { // One card is already returned, this is the last one of the first two
                    squeezing = false;
                    setCurrentCard();

                    // For the drag recognizer to register squeeze events from finger gestures
                    cardParentGO.GetComponent<DragRecognizer>().enabled = false;

                    if (cardDisplaying != null) {
                        Debug.Log ("Revealing and returning last card between PlayerCard1 and PlayerCard2... " + cardDisplaying.name);
    
                        // Chose the right return path for the currently displaying card
                        string returnPath = null;
                        if (cardDisplaying.name == "PlayerCard1") {
                            returnPath = "Dealer2Player3Player1";
                        } else {
                            returnPath = "Dealer2Player3Player2";
                        }
    
                        // Full return and reveal last of the first two player cards
                        Debug.Log ("Returning " + cardDisplaying.name + " along path " +  returnPath);

                        // Last of first two cards, return and reveal itself
                        revealCardWithReturnCallback (cardDisplaying, iTween.Hash("returnPath", returnPath), true);

                        // COMMENTED OUT to try handling in GameState instead
                        //GUIControls.SetAutoRotate(true);
                    }
                }
            } else if (GameState.Instance.getCurrentBetType () == GameState.BetType.Banker) {
                if (bankerCard1.readyToReturn && !bankerCard2.readyToReturn) { // Return 1st banker card
                    bankerCard1.setGestureRecognizerStates(false);

                    // Reveal the card completely, then return it to the dealer
                    Debug.Log ("Revealing and returning BankerCard1");
                    revealCardWithReturnCallback (bankerCard1GO, iTween.Hash ("returnPath", "Dealer2Player3Banker1"));
                } else if (!bankerCard1.readyToReturn && bankerCard2.readyToReturn) { // Return 2nd banker card
                    bankerCard2.setGestureRecognizerStates(false);

                    // Reveal the card completely, then return it to the dealer
                    Debug.Log ("Revealing and returning BankerCard2");
                    revealCardWithReturnCallback (bankerCard2GO, iTween.Hash ("returnPath", "Dealer2Player3Banker2"));
                } else if (bankerCard1.readyToReturn && bankerCard2.readyToReturn) { // One card is already returned, this is the last one of the first two
                    squeezing = false;
                    setCurrentCard();

                    // For the drag recognizer to register squeeze events from finger gestures
                    cardParentGO.GetComponent<DragRecognizer>().enabled = false;

                    if (cardDisplaying != null) {
                        Debug.Log ("Revealing and returning last card between BankerCard1 and BankerCard2... " + cardDisplaying.name);

                        // Chose the right return path for the currently displaying card
                        string returnPath = null;
                        if (cardDisplaying.name == "BankerCard1") {
                            returnPath = "Dealer2Player3Banker1";
                        } else {
                            returnPath = "Dealer2Player3Banker2";
                        }
    
                        // Full return and reveal last of the first two banker cards
                        Debug.Log ("Returning " + cardDisplaying.name + " along path " +  returnPath);

                        // Last of first two cards, return and reveal itself
                        revealCardWithReturnCallback (cardDisplaying, iTween.Hash("returnPath", returnPath), true);

                        // COMMENTED OUT to try handling in GameState instead
                        //GUIControls.SetAutoRotate(true);
                    }
                }
            }
        }
    }

    public void returnCurrentCard() {
        setCurrentCard();

        cardDisplaying.GetComponent<Card>().readyToReturn = true;
        endSqueezing();
    }

    // Set the current card that's being displayed for squeezing
    void setCurrentCard() {
         // Find out which card the squeeze camera is currently focused on and return that
        if (GameState.Instance.getCurrentBetType() == GameState.BetType.Banker) {
            // Find out which banker card
            if (!isDrawing3rdCards) {
                GameState.Instance.guiControls.hideLeftRightCardButtons(); // needed to prevent being pressed just after returning one of the first two cards and mixing up the camera order

                // First or second banker card
                if (GameState.Instance.camerasManager.getSqueezeCameraCurrentPosition() == "left") {
                    cardDisplaying = (GameObject) dealtCards["BankerCard1"];
                } else if (GameState.Instance.camerasManager.getSqueezeCameraCurrentPosition() == "right") {
                    cardDisplaying = (GameObject) dealtCards["BankerCard2"];
                }
            } else {
                // Third banker card
                cardDisplaying = (GameObject) dealtCards["BankerCard3"];
            }
        } else if (GameState.Instance.getCurrentBetType() == GameState.BetType.Player) {
            // Find out which player card
            if (!isDrawing3rdCards) {
                GameState.Instance.guiControls.hideLeftRightCardButtons(); // needed to prevent being pressed just after returning one of the first two cards and mixing up the camera order

                // First or second player card
                if (GameState.Instance.camerasManager.getSqueezeCameraCurrentPosition() == "left") {
                    cardDisplaying = (GameObject) dealtCards["PlayerCard1"];
                } else if (GameState.Instance.camerasManager.getSqueezeCameraCurrentPosition() == "right") {
                    cardDisplaying = (GameObject) dealtCards["PlayerCard2"];
                }
            } else {
                // Third player card
                cardDisplaying = (GameObject) dealtCards["PlayerCard3"];
            }
        }
    }

    // Fully reveal two cards at the same time
    void revealCardsTogether (GameObject cardGameObject1, GameObject cardGameObject2)
    {
        // Reveal card 1
        if (cardGameObject1 != null) {
            Card card1 = cardGameObject1.GetComponent<Card> ();
            if (card1 != null) {
                card1.revealSelf ();
            }
        }

        // Reveal card 2
        if (cardGameObject2 != null) {
            Card card2 = cardGameObject2.GetComponent<Card> ();
            if (card2 != null) {
                card2.revealSelf ();
            }
        }
    }

    // Fully reveal a card, used for 1 and 2 cards
    void revealCard (GameObject cardGameObject)
    {
        if (cardGameObject == null) {
            return;
        }

        Card card = cardGameObject.GetComponent<Card> ();
        if (card != null) {
            card.revealSelf ();
        }
    }

     // Fully reveal a card, and then following up withOUT the return to dealer callback
    void revealCardWithReturnCallback(GameObject cardGameObject, Hashtable paramz) {
        revealCardWithReturnCallback(cardGameObject, paramz, false);
    }

    // Fully reveal a card, and then following up with a return to dealer callback
    void revealCardWithReturnCallback(GameObject cardGameObject, Hashtable paramz, bool calculate2CardTotals) {
        paramz["delay"] = dealSpeed; // wait after flipping card before sending back to dealer
        paramz["time"] = dealSpeed;
        paramz["path"] = iTweenPath.GetPathReversed((string)paramz["returnPath"]);
        paramz["dealerCallback"] = calculate2CardTotals ? "startCalculate2CardTotalsCoroutine" : "";
        cardGameObject.GetComponent<Card>().revealSelf(cardGameObject, "returnSelf", paramz, true);
    }

    // Return a card to the dealer specifying the next card to return afterwards
    public void returnCards(GameObject cardGameObject, string path1, GameObject nextCardGameObject, string path2) {
        // Wrap the multiple arguments for the callback into a hashtable
        Hashtable multiParams = new Hashtable();
        multiParams.Add("path", path2);
        multiParams.Add("gameObject", nextCardGameObject);

        Debug.Log ("Returning card " + cardGameObject.name);
        cardGameObject.GetComponent<Card>().returnSelf(iTween.Hash("time", dealSpeed, "path", iTweenPath.GetPathReversed (path1),
            "onCompleteTarget", gameObject,
            "onComplete", "returnCard",
            "onCompleteParams", multiParams));
    }

    // Return card to the dealer
    public void returnCard(GameObject cardGameObject, string path) {
        Debug.Log ("Returning card " + cardGameObject.name);

        cardGameObject.GetComponent<Card>().returnSelf(iTween.Hash("time", dealSpeed, "path", iTweenPath.GetPathReversed (path)));
    }

    // Reveal the other hand's cards (the ones that weren't squeezed)
    public void revealOtherCards(bool flurryOrNot) {
        if (otherCardsRevealed && !isDrawing3rdCards) {
            return;
        }

        if (GameState.Instance.getCurrentBetType () == GameState.BetType.Banker) {
            if (isDrawing3rdCards) {
                if (flurryOrNot)
                    LogUtils.LogEvent(Consts.FEP_BTN_REVEAL_EVENT, new string[] { Consts.FEP_BTN_REVEAL_PLAYER_3RD_CARD }, false);

                ((GameObject) dealtCards["PlayerCard3"]).GetComponent<Card> ().revealSelf();
            } else {
                if (flurryOrNot)
                    LogUtils.LogEvent(Consts.FEP_BTN_REVEAL_EVENT, new string[] { Consts.FEP_BTN_REVEAL_PLAYER_2_CARDS }, false);

                ((GameObject) dealtCards["PlayerCard1"]).GetComponent<Card> ().revealSelf();
                ((GameObject) dealtCards["PlayerCard2"]).GetComponent<Card> ().revealSelf();
            }
        } else if (GameState.Instance.getCurrentBetType () == GameState.BetType.Player) {
            if (isDrawing3rdCards) {
                if (flurryOrNot)
                    LogUtils.LogEvent(Consts.FEP_BTN_REVEAL_EVENT, new string[] { Consts.FEP_BTN_REVEAL_BANKER_3RD_CARD }, false);

                ((GameObject) dealtCards["BankerCard3"]).GetComponent<Card> ().revealSelf();
            } else {
               
                if (flurryOrNot)
                    LogUtils.LogEvent(Consts.FEP_BTN_REVEAL_EVENT, new string[] { Consts.FEP_BTN_REVEAL_BANKER_2_CARDS }, false);

                ((GameObject) dealtCards["BankerCard1"]).GetComponent<Card> ().revealSelf();
                ((GameObject) dealtCards["BankerCard2"]).GetComponent<Card> ().revealSelf();
            }
        }

        otherCardsRevealed = true;
    }

    // Shortcut for a callback
    public void startCalculate2CardTotalsCoroutine() {
        // All first 2 cards have been revealed, calculate their totals
        StartCoroutine(calculate2CardsTotals());
    }

    // Calculate the totals of the cards once the 2 banker and dealer cards have been revealed
    IEnumerator calculate2CardsTotals() {
        Debug.Log ("Calculating 2 card totals");

        // Switch to the camera where we can see the dealt cards up close
        //GameState.Instance.camerasManager.ToggleSqueezeCamera(false);

        // Reveal other cards if they aren't already
        revealOtherCards(false);
        GameState.Instance.guiControls.ToggleRevealOtherButton(false);

        // Hide butons
        GameState.Instance.guiControls.clearButtonState = GUIControls.ClearButtonState.Hide;
        GameState.Instance.guiControls.dealButtonState = GUIControls.DealButtonState.Hide;

        /* COMMENTING OUT to improve camera transitions so they're easier on the eye
        if (GUIControls.isPortrait && GameState.Instance.getCurrentBetType() != GameState.BetType.Tie) {
            // If in portrait move the camera to the player or banker position
            GameState.Instance.camerasManager.ToggleDealtCardsCamera(true,
                (GameState.Instance.getCurrentBetType() == GameState.BetType.Player || GameState.Instance.getCurrentBetType() == GameState.BetType.PlayerPair) ? "player" : "banker");
        } else if (GameState.Instance.getCurrentBetType() != GameState.BetType.Tie) {
            // If in landscape just center the camera
            GameState.Instance.camerasManager.ToggleDealtCardsCamera(true, "center");
        }
        */

        Card playerCard1 = ((GameObject) dealtCards ["PlayerCard1"]).GetComponent<Card> () as Card;
        Card playerCard2 = ((GameObject) dealtCards ["PlayerCard2"]).GetComponent<Card> () as Card;
        Card bankerCard1 = ((GameObject) dealtCards ["BankerCard1"]).GetComponent<Card> () as Card;
        Card bankerCard2 = ((GameObject) dealtCards ["BankerCard2"]).GetComponent<Card> () as Card;

        // Player 1st card
        playerCard1Value = playerCard1.Definition.getValue();
        string playerCard1Text = playerCard1.Definition.Text;
        Debug.Log ("Player 1st card ("+playerCard1Text+") is worth " + playerCard1Value);

        // Player 2nd card
        playerCard2Value = playerCard2.Definition.getValue();
        string playerCard2Text = playerCard2.Definition.Text;
        Debug.Log ("Player 2nd card ("+playerCard2Text+") is worth " + playerCard2Value);

        // Banker 1st card
        bankerCard1Value = bankerCard1.Definition.getValue();
        string bankerCard1Text = bankerCard1.Definition.Text;
        Debug.Log ("Banker 1st card ("+bankerCard1Text+") is worth " + bankerCard1Value);

        // Banker 2nd card
        bankerCard2Value = bankerCard2.Definition.getValue();
        string bankerCard2Text = bankerCard2.Definition.Text;
        Debug.Log ("Banker 2nd card ("+bankerCard2Text+") is worth " + bankerCard2Value);

        // Calculate baccarat sums
        playerTotal = playerCard1Value + playerCard2Value;
        if (playerTotal > 9) {
            playerTotal %= 10;
        }
        Debug.Log ("Player 2 cards total is " + playerTotal);
        GUIControls.playerCardsValueText = ""+playerTotal;

        bankerTotal = bankerCard1Value + bankerCard2Value;
        if (bankerTotal > 9) {
            bankerTotal %= 10;
        }
        Debug.Log ("Banker 2 cards total is " + bankerTotal);
        GUIControls.bankerCardsValueText = ""+bankerTotal;

        // Check for player pair
        if (playerCard1.Definition.Text.Equals(playerCard2.Definition.Text)) {
            Debug.Log ("Player has pair " + playerCard1Value);
            displayMessage(LanguageManager.GetText("label_player_has_pair") + " " + playerTotal, vplayer.clip.length + vpair.clip.length + getNumWav(playerTotal).clip.length);

            // Update TV display
            GameState.Instance.roadmapManager.setPlayerPairs();

            /* COMMENTING OUT coz only need to say the value at the end of the round
            vplayer.Play();
            yield return new WaitForSeconds(vplayer.clip.length);
            vpair.Play();
            yield return new WaitForSeconds(vpair.clip.length);
            getNumWav(playerTotal).Play();
            yield return new WaitForSeconds(getNumWav(playerTotal).clip.length);
            */

            dealtPlayerPair = true;
            LogUtils.LogEvent(Consts.FE_PAIR_EVENT, new string[] { Consts.FEP_PLAYER_PAIR }, false);
        }

        // Check for banker pair
        if (bankerCard1.Definition.Text.Equals(bankerCard2.Definition.Text)) {
            Debug.Log ("Banker has pair " + playerCard1Value);
            displayMessage(LanguageManager.GetText("label_banker_has_pair") + " " + bankerTotal, vbanker.clip.length + vpair.clip.length + getNumWav(bankerTotal).clip.length);

            // Update TV display
            GameState.Instance.roadmapManager.setBankerPairs();

            /* COMMENTING OUT coz only need to say the value at the end of the round
            vbanker.Play();
            yield return new WaitForSeconds(vbanker.clip.length);
            vpair.Play();
            yield return new WaitForSeconds(vpair.clip.length);
            getNumWav(bankerTotal).Play();
            yield return new WaitForSeconds(getNumWav(bankerTotal).clip.length);
            */

            dealtBankerPair = true;
            LogUtils.LogEvent(Consts.FE_PAIR_EVENT, new string[] { Consts.FEP_BANKER_PAIR } , false);
        }

        // Draw 3rd cards or not
        draw3rdCardsLogic();

        yield break;
    }

    // Determine whether 3rd player and banker cards need to be drawn
    // Rule sources: http://www.ildado.com/baccarat_rules.html and http://en.wikipedia.org/wiki/Baccarat
    void draw3rdCardsLogic() {
        StartCoroutine(draw3rdCardsLogicCoroutine());
    }

    IEnumerator draw3rdCardsLogicCoroutine() {
        // We can end here if either the banker or player have an 8 or 9
        hasNatural = hasPlayerNatural = hasBankerNatural = false;
        if (playerTotal == 8 || playerTotal == 9) {
            // Player natural
            hasNatural = hasPlayerNatural = true;

            LogUtils.LogEvent(Consts.FE_NATURAL_EVENT, new string[] { Consts.FEP_PLAYER_NATURAL }, false);

            // Update TV display
            GameState.Instance.roadmapManager.setNaturals();
        }
        if (bankerTotal == 8 || bankerTotal == 9) {
            // Banker natural
            hasNatural = hasBankerNatural = true;

            LogUtils.LogEvent(Consts.FE_NATURAL_EVENT, new string[] { Consts.FEP_BANKER_NATURAL }, false);

            // Update TV display
            GameState.Instance.roadmapManager.setNaturals();
        }

        // No 3rd cards needed if we have a natural
        if (hasNatural) {
           isDrawing3rdCards = false;
           calculate3CardsTotals();
        } else {
            // Proceed to check if player and banker get 3rd cards or not
            isDrawing3rdCards = true;
            bool need3rdPlayerCard = isNeedToDraw3rdPlayerCard();
            bool need3rdBankerCard = false;
            if (need3rdPlayerCard) {
                yield return new WaitForSeconds(dealSpeed*2F);
                draw3rdPlayerCard();
            }

            need3rdBankerCard = isNeedToDraw3rdBankerCard();

            if (need3rdPlayerCard && need3rdBankerCard) {
                // Signal for the callback invoked after 3rd player card has been revealed to
                // trigger the dealing of the 3rd banker card
                need3rdBankerCardOn3rdPlayerCardCallback = true;
            } else if (need3rdBankerCard) {
                // We don't need a 3rd player card, just a 3rd banker card so go and deal it straight away
                draw3rdBankerCard();
            } else if (!need3rdPlayerCard && !need3rdBankerCard) {
                // We don't need any 3rd cards
                calculate3CardsTotals();
            }
//            } else if (skipSqueezing) {
//                // We don't need a 3rd banker card but we do need a 3rd player card
//                reveal3rdCardWithCallback(((GameObject) dealtCards["PlayerCard3"]), "calculate3CardsTotals");
        }
    }

    bool isNeedToDraw3rdPlayerCard() {
        if (playerTotal <= 5) {
            // Player draws 3rd card on total of 5 or less
            return true;
        } else {
            // Else player stands on 6 or 7
            //playStandOn(playerTotal);
            return false;
        }
    }

    bool isNeedToDraw3rdBankerCard() {
        // If player stood pat, i.e. didn't draw a 3rd card and thus has a 6 or a 7 total, we apply the same rules
        // to the banker to see if we draw a 3rd card
        if (!playerDrew3rdCard) {
            Debug.Log ("Checking banker 3rd card logic with NO 3rd player card dealt");

            if (bankerTotal <= 5) {
                // Banker draws 3rd card on total of 5 or less
                Debug.Log ("Banker draws 3rd card on bankerTotal >= 0 && bankerTotal <= 4");
                return true;
            } else if (bankerTotal == 6 || bankerTotal == 7) {
                // Else banker stands on 6 or 7 so we next decide the winner
                Debug.Log ("Banker stands on 6 >= total <= 7");
                //playStandOn(bankerTotal);
                return false;
            }
        } else { // playerDrew3rdCard == true
            // Complex rules for choosing whether banker draws 3rd card based on what the player's 3rd card was
            Debug.Log ("Checking banker 3rd card logic WITH 3rd player card dealt");

            // If Player drew a 2 or 3, Banker draws with 0â€“4, and stands with 5â€“7.
            if (players3rdCardValue == 2 || players3rdCardValue == 3) {
                Debug.Log ("Player 3rd card drew 2 || 3");
                if (bankerTotal >= 0 && bankerTotal <= 4) {
                    // Banker draws 3rd card
                    Debug.Log ("Banker draws 3rd card on bankerTotal >= 0 && bankerTotal <= 4");
                    return true;
                } else if (bankerTotal >= 5 && bankerTotal <= 7) {
                    // Banker stands
                    Debug.Log ("Banker stands on 5 >= total <= 7");
                    return false;
                }
            }
            // If Player drew a 4 or 5, Banker draws with 0â€“5, and stands with 6â€“7.
            else if (players3rdCardValue == 4 || players3rdCardValue == 5) {
                Debug.Log ("Player 3rd card drew 4 || 5");
                if (bankerTotal >= 0 && bankerTotal <= 5) {
                    // Banker draws 3rd card
                    Debug.Log ("Banker draws 3rd card on bankerTotal >= 0 && bankerTotal <= 5");
                    return true;
                } else if (bankerTotal >= 6 && bankerTotal <= 7) {
                    // Banker stands
                    Debug.Log ("Banker stands on 6 >= total <= 7");
                    return false;
                }
            }
            // If Player drew a 6 or 7, Banker draws with 0â€“6, and stands with 7.
            else if (players3rdCardValue == 6 || players3rdCardValue == 7) {
                Debug.Log ("Player 3rd card drew 6 || 7");
                if (bankerTotal >= 0 && bankerTotal <= 6) {
                    // Banker draws 3rd card
                    Debug.Log ("Banker draws 3rd card on bankerTotal >= 0 && bankerTotal <= 6");
                    return true;
                } else if (bankerTotal == 7) {
                    // Banker stands
                    Debug.Log ("Banker stands on total == 7");
                    return false;
                }
            }
            // If Player drew an 8, Banker draws with 0â€“2, and stands with 3â€“7.
            else if (players3rdCardValue == 8) {
                Debug.Log ("Player 3rd card drew 8");
                if (bankerTotal >= 0 && bankerTotal <= 2) {
                    // Banker draws 3rd card
                    Debug.Log ("Banker draws 3rd card on bankerTotal >= 0 && bankerTotal <= 2");
                    return true;
                } else {
                    // Banker stands
                    Debug.Log ("Banker stands on 3 >= total <= 7");
                    return false;
                }
            }
            // If Player drew an ace, 9, 10, or face-card, the Banker draws with 0â€“3, and stands with 4â€“7.
            else if (players3rdCardValue == 1 || players3rdCardValue == 9 || players3rdCardValue == 10 || players3rdCardValue == 0) {
                Debug.Log ("Player 3rd card drew 1 (ace) || 9 || 10 || 0 (face card)");
                if (bankerTotal >= 0 && bankerTotal <= 3) {
                    // Banker draws 3rd card
                    Debug.Log ("Banker draws 3rd card on bankerTotal >= 0 && bankerTotal <= 3");
                    return true;
                } else if (bankerTotal >= 4 && bankerTotal <= 7) {
                    // Banker stands
                    Debug.Log ("Banker stands on 4 >= total <= 7");
                    return false;
                }
            }
        }

        // Uh-oh, what've we missed?!
        Debug.LogError("LOGIC ERROR!! Shouldn't get this far in deciding 3rd banker card's logic");
        return false;
    }

    // Draw the player's 3rd card
    void draw3rdPlayerCard() {
        Debug.Log ("Drawing 3rd player card");

        playerDrew3rdCard = true;
        nextPlayerPos = 3;

        // Dealer draws and gives us the 3rd player card
        if (dealerAnimator != null && dealerCharacter.activeSelf) {
            //GameState.Instance.camerasManager.ToggleDealtCardsCamera(false, null);
            StartCoroutine(dealPlayerCoroutine("draw3rdPlayerCardCallback", true));
        } else {
            dealPlayer("draw3rdPlayerCardCallback");
        }
    }

    // Callback after 3rd player card has been dealt from deck
    void draw3rdPlayerCardCallback() {
        // Get the game object holding the 3rd player card
        GameObject playerCard3 = dealtCards ["PlayerCard3"] as GameObject;

        if ((GameState.Instance.getCurrentBetType () == GameState.BetType.Player) && !skipSqueezing) {
            // User's current bet is on player so let them squeeze the player card
            GameState.Instance.camerasManager.ToggleSqueezeCamera(true);
            GameState.Instance.camerasManager.moveSqueezeCamera("center");

            // Show return card button
            GameState.Instance.guiControls.dealButtonState = GUIControls.DealButtonState.ReturnCard;

            // For the drag recognizer to register squeeze events from finger gestures
            cardParentGO.GetComponent<DragRecognizer>().enabled = true;
            squeezing = true;

            // COMMENTED OUT to try handling in GameState instead
            //GUIControls.SetAutoRotate(false);

            playerCard3.GetComponent<Card> ().setGestureRecognizerStates(true);
            //vplayer_customer.Play();
            //blowDetector.start(playerCard3);
            Debug.Log ("Pushing user the 3rd player card to squeeze");
            playerCard3.GetComponent<Card> ().slideSelf(iTween.Hash("time", dealSpeed, "path",
                iTweenPath.GetPath ("Dealer2Player3Player3"),
                "onCompleteTarget", gameObject,
                "onComplete", "end3rdCardAnimator"));

            // Scale size up a bit to avoid jitter/shakiness when squeezing
            ((GameObject) dealtCards["PlayerCard3"]).GetComponent<Card>().ToggleEnlarge();

            if (need3rdBankerCardOn3rdPlayerCardCallback) {
                // Show the button to reveal the banker's 3rd card
                GameState.Instance.guiControls.ToggleRevealOtherButton(true);
                
                // Draw the banker's 3rd card
                draw3rdBankerCard();
            }
        } else {
            // User doesn't have any bet on the player at the moment so reveal the 3rd player card where it is (in front of the dealer)
            Debug.Log ("Revealing the 3rd player card with no squeezing");

            if (need3rdBankerCardOn3rdPlayerCardCallback) {
                if (GameState.Instance.getCurrentBetType() == GameState.BetType.Tie) {
                    // Specify callback to draw 3rd banker card after the 3rd player card has been revealed
                    reveal3rdCardWithCallback(playerCard3, "draw3rdBankerCard");
                } else {
                    // Show the button to reveal the player's 3rd card
                    GameState.Instance.guiControls.ToggleRevealOtherButton(true);
    
                    // Move on to draw the banker's 3rd card
                    draw3rdBankerCard();
                }
            } else {
                // We don't need a 3rd banker card, go and finish up
                reveal3rdCardWithCallback(playerCard3, "calculate3CardsTotals");
            }
        }
    }

    void draw3rdBankerCardOnDelay() {
       StartCoroutine(draw3rdBankerCardOnDelayCoroutine());
    }

    IEnumerator draw3rdBankerCardOnDelayCoroutine() {
        yield return new WaitForSeconds(dealSpeed);
        draw3rdBankerCard();
    }

    public void draw3rdBankerCard() {
        Debug.Log ("Drawing 3rd banker card");

        // Dealer draws and gives us the 3rd banker card
        if (dealerAnimator != null && dealerCharacter.activeSelf) {
            //GameState.Instance.camerasManager.ToggleDealtCardsCamera(false, null);
        }

        nextBankerPos = 3;
        // Dealer draws and gives us the 3rd banker card
        if (dealerAnimator != null && dealerCharacter.activeSelf) {
            //GameState.Instance.camerasManager.ToggleDealtCardsCamera(false, null);
            StartCoroutine(CallStartDealBankerCoroutine ());
        } else {
            dealBanker("draw3rdBankerCardCallback");
        }
    }

    IEnumerator CallStartDealBankerCoroutine() {
        yield return new WaitForSeconds(dealSpeed*3f);
        StartCoroutine(dealBankerCoroutine("draw3rdBankerCardCallback", true));
    }

    void draw3rdBankerCardCallback() {
        // Get the game object holding the 3rd banker card
        GameObject bankerCard3 = dealtCards ["BankerCard3"] as GameObject;

        if (GameState.Instance.getCurrentBetType () == GameState.BetType.Banker && !skipSqueezing) {
            // User's current bet is on banker so let them squeeze the banker card
            GameState.Instance.camerasManager.ToggleSqueezeCamera(true);
            GameState.Instance.camerasManager.moveSqueezeCamera("center");

            // Show return card button
            GameState.Instance.guiControls.dealButtonState = GUIControls.DealButtonState.ReturnCard;

            // For the drag recognizer to register squeeze events from finger gestures
            cardParentGO.GetComponent<DragRecognizer>().enabled = true;
            squeezing = true;

            // COMMENTED OUT to try handling in GameState instead
            //GUIControls.SetAutoRotate(false);

            bankerCard3.GetComponent<Card> ().setGestureRecognizerStates(true);
            //vbanker_customer.Play ();
            //blowDetector.start(bankerCard3);
            Debug.Log ("Pushing user the 3rd banker card to squeeze");
            bankerCard3.GetComponent<Card> ().slideSelf(iTween.Hash("time", dealSpeed, "path",
                iTweenPath.GetPath ("Dealer2Player3Banker3"),
                "onCompleteTarget", gameObject,
                "onComplete", "end3rdCardAnimator"));

            // Scale size up a bit to avoid jitter/shakiness when squeezing
            ((GameObject) dealtCards["BankerCard3"]).GetComponent<Card>().ToggleEnlarge();
        } else if (GameState.Instance.getCurrentBetType () == GameState.BetType.Tie) {
            Debug.Log ("Revealing banker 3rd card automatically due to bets only on tie");
            bankerCard3.GetComponent<Card> ().revealSelf(this.gameObject, "calculate3CardsTotals");
        } else if (dealtCards["PlayerCard3"] != null &&
            ((GameObject) dealtCards["PlayerCard3"]).GetComponent<Card>().revealed) {
            Debug.Log ("Revealing banker 3rd card automatically due to player 3rd card being already revealed");
            bankerCard3.GetComponent<Card> ().revealSelf(this.gameObject, "calculate3CardsTotals");
        } else if (dealtCards["PlayerCard3"] == null &&
            GameState.Instance.getCurrentBetType () == GameState.BetType.Player) {
            Debug.Log ("Revealing banker 3rd card automatically due to bet on player and no player 3rd card dealt");
            bankerCard3.GetComponent<Card> ().revealSelf(this.gameObject, "calculate3CardsTotals");
        }
    }

    // Fully reveal a card with callback logic
    void reveal3rdCardWithCallback (GameObject cardGameObject, string methodName)
    {
        if (cardGameObject == null) {
            return;
        }

        cardGameObject.GetComponent<Card> ().setGestureRecognizerStates(false);

        Card card = cardGameObject.GetComponent<Card> ();
        if (card != null) {
           card.revealSelf (this.gameObject, methodName);
        }
    }

    // Fully reveal a 3rd card, and then following up with a return to dealer callback
    void reveal3rdCardWithReturnCallback(GameObject cardGameObject, Hashtable paramz) {
        reveal3rdCardWithReturnCallback(cardGameObject, paramz, false);
    }

    // Fully reveal a 3rd card, and then following up with a return to dealer callback
    void reveal3rdCardWithReturnCallback (GameObject cardGameObject, Hashtable paramz, bool draw3rdBankerCard)
    {
        paramz["delay"] = dealSpeed; // wait after flipping card before sending back to dealer
        paramz["time"] = dealSpeed;
        paramz["path"] = iTweenPath.GetPathReversed((string)paramz["returnPath"]);
        if (paramz["dealerCallback"] == null || paramz["dealerCallback"] == "") { // don't override any pre-set dealerCallback, for example calculate3CardTotals
            paramz["dealerCallback"] = draw3rdBankerCard ? "draw3rdBankerCard" : "";
        }
        cardGameObject.GetComponent<Card>().revealSelf(cardGameObject, "returnSelf", paramz, true);
    }


    // Callback method to call when squeezing A 3rd card is done
    public void end3rdCardSqueezing ()
    {
        // COMMENTED OUT to try handling in GameState instead
        //GUIControls.SetAutoRotate(true);

        GameObject playerCard3 = dealtCards ["PlayerCard3"] as GameObject;
       
        // Choose to return the squeezed player or banker 3rd card
        if (GameState.Instance.getCurrentBetType () == GameState.BetType.Player) {

            playerCard3.GetComponent<Card> ().setGestureRecognizerStates(false);

            if (need3rdBankerCardOn3rdPlayerCardCallback && !skipSqueezing) {
                // Specify callback to draw 3rd banker card after the 3rd player card has been revealed
                Debug.Log ("Revealing the 3rd player card");
                reveal3rdCardWithReturnCallback(playerCard3, iTween.Hash ("returnPath", "Dealer2Player3Player3"), false);

                // When betting on player and there are both player and banker 3rd cards,
                // we automatically reveal the banker 3rd card here at the same time as revealing the player 3rd card above
                GameObject bankerCard3 = dealtCards ["BankerCard3"] as GameObject;
                Debug.Log ("Revealing the 3rd banker card on account of the 3rd player one being revealed");
                reveal3rdCardWithCallback(bankerCard3, "calculate3CardsTotals");

                return;
            } else {
                // We don't need a 3rd banker card, just reveal/return the 3rd player card go and finish up
                //returnCard (playerCard3, "Dealer2Player3Player3");
                //revealCard (playerCard3);
                reveal3rdCardWithReturnCallback(playerCard3, iTween.Hash ("returnPath", "Dealer2Player3Player3", "dealerCallback", "calculate3CardsTotals"));
            }
        } else if (GameState.Instance.getCurrentBetType () == GameState.BetType.Banker) {
            GameObject bankerCard3 = dealtCards ["BankerCard3"] as GameObject;

            // Reveal the 3rd player card if it isn't already
            if (playerCard3 != null) {
                playerCard3.GetComponent<Card> ().revealSelf();
            }
            GameState.Instance.guiControls.ToggleRevealOtherButton(false);

            // Return 3rd banker card
            bankerCard3.GetComponent<Card> ().setGestureRecognizerStates(false);
            reveal3rdCardWithReturnCallback(bankerCard3, iTween.Hash ("returnPath", "Dealer2Player3Banker3", "dealerCallback", "calculate3CardsTotals"));
        }

        // For the drag recognizer to register squeeze events from finger gestures
        cardParentGO.GetComponent<DragRecognizer>().enabled = false;
    }

    public void end3rdCardAnimator() {
        if (dealerAnimator != null && dealerCharacter.activeSelf) {
            dealerAnimator.SetBool("Deal3rdCard", false);
        }
    }

    // Calculate the totals of the 3rd cards
    public void calculate3CardsTotals() {
        if (isCalculating3rdCardTotals) {
            Debug.LogWarning ("Skipping calculate3CardsTotals because isCalculating3rdCardTotals is true");
            return; // avoid having double simulateneous invokes of this method
        }
        StartCoroutine (calculate3CardsTotalsCoroutine());
    }

    // Calculate the totals of the 3rd cards
    IEnumerator calculate3CardsTotalsCoroutine() {
        isCalculating3rdCardTotals = true;

        if (dealtCards["PlayerCard3"] != null && dealtCards["BankerCard3"] != null) {
            yield return new WaitForSeconds(dealSpeed*5f); // it felt like it was switching too fast when 6 cards dealt transitioning to the results announcement screen
        } else
            yield return new WaitForSeconds(dealSpeed*3f);

        Debug.Log ("Calculating 3 card totals");

        // Switch to the camera where we can see the dealt cards up close
        //GameState.Instance.camerasManager.ToggleSqueezeCamera(false);

        if (GUIControls.isPortrait && GameState.Instance.getCurrentBetType() != GameState.BetType.Tie) {
            // If in portrait move the camera to the player or banker position
            yield return new WaitForSeconds(dealSpeed*2);
            GameState.Instance.camerasManager.ToggleDealtCardsCamera(true,
                (GameState.Instance.getCurrentBetType() == GameState.BetType.Player) ? "player" : "banker");
        } else if (GameState.Instance.getCurrentBetType() != GameState.BetType.Tie) {
            // If in landscape just center the camera
            GameState.Instance.camerasManager.ToggleDealtCardsCamera(true, "center");
        }

        GameObject playerCard3Obj = (GameObject) dealtCards ["PlayerCard3"];
        GameObject bankerCard3Obj = (GameObject) dealtCards ["BankerCard3"];
        Card playerCard3 = null;
        Card bankerCard3 = null;
        if (playerCard3Obj != null)
            playerCard3 = playerCard3Obj.GetComponent<Card> () as Card;
        if (bankerCard3Obj != null)
            bankerCard3 = bankerCard3Obj.GetComponent<Card> () as Card;

        if (playerCard3 != null) {
            // Player 3rd card
            playerCard3Value = playerCard3.Definition.getValue();
            string playerCard3Text = playerCard3.Definition.Text;
            Debug.Log ("Player 3rd card ("+playerCard3Text+") is worth " + playerCard3Value);
        }

        if (bankerCard3 != null) {
            // Banker 3rd card
            bankerCard3Value = bankerCard3.Definition.getValue();
            string bankerCard3Text = bankerCard3.Definition.Text;
            Debug.Log ("Banker 3rd card ("+bankerCard3Text+") is worth " + bankerCard3Value);
        }

        bool playerNatural = false;
        bool bankerNatural = false;
        bool tie = false;

        // Calculate final punto sums
        playerTotal += playerCard3Value;
        if (playerTotal > 9) {
            playerTotal %= 10;
        }
        Debug.Log ("PLAYER CARD TOTAL IS " + playerTotal);
        GUIControls.playerCardsValueText = ""+playerTotal;


        // Calculate final banco sums
        bankerTotal += bankerCard3Value;
        if (bankerTotal > 9) {
            bankerTotal %= 10;
        }
        Debug.Log ("BANKER CARD TOTAL IS " + bankerTotal);
        GUIControls.bankerCardsValueText = ""+bankerTotal;


        if (hasPlayerNatural && (playerTotal == 8 || playerTotal == 9)) {
            // Player natural
            Debug.Log ("Player has natural " + playerTotal);
            if (playerTotal != bankerTotal)
                GameState.Instance.guiControls.displayPlayerTotal(
                    LanguageManager.GetText("label_player_has_natural") + " " + playerTotal,
                    (GameState.Instance.getCurrentBetType() == GameState.BetType.Player ? true : false));
            playerNatural = true;
        } else {
            Debug.Log ("Player has " + playerTotal);
            if (playerTotal != bankerTotal)
                GameState.Instance.guiControls.displayPlayerTotal(
                    LanguageManager.GetText("label_player_has") + " " + playerTotal,
                    (GameState.Instance.getCurrentBetType() == GameState.BetType.Player ? true : false));
            playerNatural = false;
        }

        if (hasBankerNatural && (bankerTotal == 8 || bankerTotal == 9)) {
            // Banker natural
            Debug.Log ("Banker has natural " + bankerTotal);
            if (playerTotal != bankerTotal)
                GameState.Instance.guiControls.displayBankerTotal(
                    LanguageManager.GetText("label_banker_has_natural") + " " + bankerTotal,
                    (GameState.Instance.getCurrentBetType() == GameState.BetType.Banker ? true : false));
            bankerNatural = true;
        } else {
            Debug.Log ("Banker has " + bankerTotal);
            if (playerTotal != bankerTotal)
                GameState.Instance.guiControls.displayBankerTotal(
                    LanguageManager.GetText("label_banker_has") + " " + bankerTotal,
                    (GameState.Instance.getCurrentBetType() == GameState.BetType.Banker ? true : false));
            bankerNatural = false;
        }

        // See if we have a tie here
        if (playerTotal == bankerTotal) {
            Debug.Log ("Tie on " + playerTotal);
            //yield return new WaitForSeconds(0.1f); // coz TIE TOTAL was displaying over top of PLAYER and BANKER TOTALS in the middle-screen messages
            GameState.Instance.guiControls.displayTieTotal(
                LanguageManager.GetText("label_tie_on") + " " + playerTotal,
                (GameState.Instance.getCurrentBetType() == GameState.BetType.Tie ? true : false));
            tie = true;
        }

        // Allow the user to touch the screen to hurry up onto the next round
        GameState.Instance.dealer.nextTapObject = this.gameObject;
        GameState.Instance.dealer.nextTapMethodName = "skipScoreAnnoucements";
        GameState.Instance.dealer.nextTapResetOnNext = true;

        if (!skipScoreAnnoucementsFlag) {
            if (!tie && playerNatural) {
                // Announce player natural total
                if (skipScoreAnnoucementsFlag) {
                    skipScoreAnnoucementsFlag = false;
                    isCalculating3rdCardTotals = false;
                    yield break;
                }
                vplayer_natural.Play();
                yield return new WaitForSeconds(vplayer_natural.clip.length);
                if (skipScoreAnnoucementsFlag) {
                    skipScoreAnnoucementsFlag = false;
                    isCalculating3rdCardTotals = false;
                    yield break;
                }
                getNumWav(playerTotal).Play();
                yield return new WaitForSeconds(dealSpeed * 3f);
                if (skipScoreAnnoucementsFlag) {
                    skipScoreAnnoucementsFlag = false;
                    isCalculating3rdCardTotals = false;
                    yield break;
                }
            } else if (!tie) {
                // Announce player total
                if (skipScoreAnnoucementsFlag) {
                    skipScoreAnnoucementsFlag = false;
                    isCalculating3rdCardTotals = false;
                    yield break;
                }
                vplayer.Play();
                yield return new WaitForSeconds(vplayer.clip.length);
                if (skipScoreAnnoucementsFlag) {
                    skipScoreAnnoucementsFlag = false;
                    isCalculating3rdCardTotals = false;
                    yield break;
                }
                getNumWav(playerTotal).Play();
                yield return new WaitForSeconds(dealSpeed * 3f);
                if (skipScoreAnnoucementsFlag) {
                    skipScoreAnnoucementsFlag = false;
                    isCalculating3rdCardTotals = false;
                    yield break;
                }
            }
            if (bankerNatural && !tie) {
                // Announce banker natural total
                if (skipScoreAnnoucementsFlag) {
                    skipScoreAnnoucementsFlag = false;
                    isCalculating3rdCardTotals = false;
                    yield break;
                }
                vbanker_natural.Play();
                yield return new WaitForSeconds(vbanker_natural.clip.length);
                if (skipScoreAnnoucementsFlag) {
                    skipScoreAnnoucementsFlag = false;
                    isCalculating3rdCardTotals = false;
                    yield break;
                }
                getNumWav(bankerTotal).Play();
                yield return new WaitForSeconds(dealSpeed * 3f);
                if (skipScoreAnnoucementsFlag) {
                    skipScoreAnnoucementsFlag = false;
                    isCalculating3rdCardTotals = false;
                    yield break;
                }
            } else if (!tie) {
                // Announce banker total
                if (skipScoreAnnoucementsFlag) {
                    skipScoreAnnoucementsFlag = false;
                    isCalculating3rdCardTotals = false;
                    yield break;
                }
                vbanker.Play();
                yield return new WaitForSeconds(vbanker.clip.length);
                if (skipScoreAnnoucementsFlag) {
                    skipScoreAnnoucementsFlag = false;
                    isCalculating3rdCardTotals = false;
                    yield break;
                }
                getNumWav(bankerTotal).Play();
                yield return new WaitForSeconds(dealSpeed * 3f);
                if (skipScoreAnnoucementsFlag) {
                    skipScoreAnnoucementsFlag = false;
                    isCalculating3rdCardTotals = false;
                    yield break;
                }
            }
            if (tie) {
                // Announce tie win
                if (skipScoreAnnoucementsFlag) {
                    skipScoreAnnoucementsFlag = false;
                    isCalculating3rdCardTotals = false;
                    yield break;
                }
                vtie_wins.Play();
                vtie_tap.Play();
                yield return new WaitForSeconds(vtie.clip.length);
                if (skipScoreAnnoucementsFlag) {
                    skipScoreAnnoucementsFlag = false;
                    isCalculating3rdCardTotals = false;
                    yield break;
                }
            }
        }
        isCalculating3rdCardTotals = false;
        skipScoreAnnoucementsFlag = false;
        GameState.Instance.dealer.nextTapObject = null;
        GameState.Instance.dealer.nextTapMethodName = null;
        GameState.Instance.dealer.nextTapResetOnNext = false;

        // Can announce winner!!
        GameState.Instance.currentState = GameState.State.DecideWinner;
    }

    bool skipScoreAnnoucementsFlag = false;
    void skipScoreAnnoucements() {
        Debug.Log ("Skipping round results announcements");
        skipScoreAnnoucementsFlag = true;

        // Stop any annoucement of results playback
        vplayer_natural.Stop();
        getNumWav(playerTotal).Stop();
        vplayer.Stop();
        vbanker_natural.Stop();
        getNumWav(bankerTotal).Stop();
        vbanker.Stop();
        vtie_wins.Stop();
        vtie_tap.Stop();

        // Stop display messages as well
        GameState.Instance.guiControls.displayPlayerTotal("", false);
        GameState.Instance.guiControls.displayBankerTotal("", false);

        // Can announce winner!!
        GameState.Instance.currentState = GameState.State.DecideWinner;
    }

    // Reset parameters afer a coup
    void reset() {
        dealtCards.Clear();
        dealtCards = null;
        dealtCards = new Hashtable();
        squeezing = false;
        playerCard1Value = 0;
        playerCard2Value = 0;
        playerCard3Value = 0;
        bankerCard1Value = 0;
        bankerCard2Value = 0;
        bankerCard3Value = 0;
        playerTotal = 0;
        bankerTotal = 0;
        playerDrew3rdCard = false;
        players3rdCardValue = 0;
        isDrawing3rdCards = false;
        skipSqueezing = false;
        need3rdBankerCardOn3rdPlayerCardCallback = false;
//        isCut = false;
        nextBankerPos = nextBankerPos = 0;
        cardDisplaying = null;
        otherCardsRevealed = false;
        hasNatural = false;
        dealtPlayerPair = false;
        dealtBankerPair = false;
        hasPlayerNatural = false;
        hasBankerNatural = false;
        skipScoreAnnoucementsFlag = false;
        GameState.Instance.dealer.nextTapObject = null;
        GameState.Instance.dealer.nextTapMethodName = null;
        GameState.Instance.dealer.nextTapResetOnNext = false;
    }

    // Decide and announce the winner
    public void decideWinner() {
        StartCoroutine (decideWinnerCoroutine());
    }

    IEnumerator decideWinnerCoroutine() {
        int wonAmount = 0;
        List<GameState.BetType> winners = new List<GameState.BetType>();
        Dictionary<string, string> winnings = new Dictionary<string, string>(); // for Flurry

        // Check side bets first
        int wonOnPlayerPair = 0;
        int wonOnBankerPair = 0;
        int myWin;
        int pairCameraWaitFactor = 2;
        if (dealtPlayerPair && GameState.Instance.betsOnTableHash.Contains(GameState.BetType.PlayerPair)) {
            myWin = GameState.Instance.dishOutWinnings(GameState.BetType.PlayerPair, playerPairWinPayback);
            // Player pair win
            wonAmount += myWin;
            winners.Add (GameState.BetType.PlayerPair);
            winnings.Add (Consts.FEP_WINLOSE_PLAYER_PAIR, myWin+"");
            Debug.Log ("There was a player pair side-bet that won $" + myWin);
            wonOnPlayerPair = myWin;
            yield return new WaitForSeconds(pairCameraWaitFactor * dealSpeed); // allow the playerpair doyesyes flash to show a little before the next one
        } else {
            Debug.Log ("Nothing won on player pair side-bet");
        }
        if (dealtBankerPair && GameState.Instance.betsOnTableHash.Contains(GameState.BetType.BankerPair)) {
            myWin = GameState.Instance.dishOutWinnings(GameState.BetType.BankerPair, bankerPairWinPayback);
            // Banker pair win
            wonAmount += myWin;
            winners.Add (GameState.BetType.BankerPair);
            winnings.Add (Consts.FEP_WINLOSE_BANKER_PAIR, myWin+"");
            Debug.Log ("There was a banker pair side-bet that won $" + myWin);
            wonOnBankerPair = myWin;
            yield return new WaitForSeconds(pairCameraWaitFactor * dealSpeed); // allow the banker doyesyes flash to show a little before the next one
        } else {
            Debug.Log ("Nothing won on banker pair side-bet");
        }

        bool arBonusAri = false;

        // Check main 'players'
        if (playerTotal > bankerTotal) {
            Debug.Log("PLAYER WINS!");
            LogUtils.LogEvent(Consts.FE_WIN_EVENT, new string[] { Consts.FEP_PLAYER_WIN }, false);
            GameState.Instance.roadmapManager.setLatestResult(GameState.BetType.Player, hasPlayerNatural, dealtPlayerPair, dealtBankerPair);
            displayMessage(LanguageManager.GetText("label_player_wins"), vplayer_wins.clip.length);
            vplayer_wins.Play ();

            // AR hack: show the chips in time for winning animations before they disappear (the problem was they were too short (disappearing) in AR
            if (GameState.Instance.camerasManager.isAR())
                GameState.Instance.chipsManager.clearChipsForSqueezing(false);

            // Shake cards if we bet on the winning hand
            if (GameState.Instance.getCurrentBetType() == GameState.BetType.Player)
                StartCoroutine(ShunkanShrinkWinningCards(GameState.BetType.Player));

            // Update TV display
            GameState.Instance.roadmapManager.setPlayerWins();

            // Also display and player/banker pair bet winnings together
            string displayExtraWinningsAmountStr = "";
            if (wonOnPlayerPair > 0)
                displayExtraWinningsAmountStr += LanguageManager.GetText("label_player_has_pair") + "   +$" + wonOnPlayerPair.ToString("n0") + "\n";
            if (wonOnBankerPair > 0)
                displayExtraWinningsAmountStr += LanguageManager.GetText("label_banker_has_pair") + "   +$" + wonOnBankerPair.ToString("n0") + "\n";

            if (GameState.Instance.getCurrentBetType() == GameState.BetType.Player) {

                // Give AR bonus if in AR
#if !UNITY_IPHONE // Apple rejected having an AR Bonus because it conflicts with their idea of in-app purchasing to buy chips.
                if (GameState.Instance.camerasManager.arCamera.activeSelf) {
                    Debug.Log ("AR bonus +$" + AR_BONUS);
                    wonAmount += AR_BONUS;
                    GameState.Instance.currentBalance += AR_BONUS;
                    arBonusAri = true;
                    winnings.Add (Consts.FEP_WINLOSE_BONUS_AR, AR_BONUS+"");
                    displayExtraWinningsAmountStr += LanguageManager.GetText("label_ar_bonus") + " +$" + AR_BONUS + "\n";
                }
#endif

                // Dish out winnings for player bets
                myWin = GameState.Instance.dishOutWinnings(GameState.BetType.Player, playerWinPayback);
                wonAmount += myWin;
                winnings.Add (Consts.FEP_WINLOSE_PLAYER, myWin+"");
                GameState.Instance.guiControls.displayPlayerWins(LanguageManager.GetText("label_you_win") + "\n" + displayExtraWinningsAmountStr
                    + LanguageManager.GetText("label_player_wins") + "   +$" + (wonAmount - wonOnPlayerPair - wonOnBankerPair - (arBonusAri ? AR_BONUS : 0)).ToString("n0"));
            } else if (GameState.Instance.getCurrentBetType() == GameState.BetType.Banker || GameState.Instance.getCurrentBetType() == GameState.BetType.Tie) {
                // Damn, we didn't bet on the winning hand!
                GameState.Instance.guiControls.displayPlayerWins(displayExtraWinningsAmountStr
                    + ((wonOnPlayerPair <= 0 && wonOnBankerPair <= 0) ? LanguageManager.GetText("label_loser") + " ": "")
                    + LanguageManager.GetText("label_player_wins"));
            }

            winners.Add (GameState.BetType.Player);

            // Determine the timing to switch back to the main camera on whether we had pairs or not
            if (wonOnPlayerPair >= 1 && wonOnBankerPair >= 1)
                yield return new WaitForSeconds((8f-(2f*pairCameraWaitFactor))*dealSpeed);
            else if (wonOnPlayerPair >= 1 || wonOnBankerPair >= 1)
                yield return new WaitForSeconds((8f-pairCameraWaitFactor)*dealSpeed);
            else
                yield return new WaitForSeconds(8f*dealSpeed);

            // Reset back to main camera
            GameState.Instance.camerasManager.resetToMainCamera();
        } else if (bankerTotal > playerTotal) {
            Debug.Log ("BANKER WINS!");
            LogUtils.LogEvent(Consts.FE_WIN_EVENT, new string[] { Consts.FEP_BANKER_WIN }, false);
            GameState.Instance.roadmapManager.setLatestResult(GameState.BetType.Banker, hasBankerNatural, dealtPlayerPair, dealtBankerPair);
            displayMessage(LanguageManager.GetText("label_banker_wins"), vbanker_wins.clip.length);
            vbanker_wins.Play ();

            // AR hack: show the chips in time for winning animations before they disappear (the problem was they were too short (disappearing) in AR
            if (GameState.Instance.camerasManager.isAR())
                GameState.Instance.chipsManager.clearChipsForSqueezing(false);

            // Shake cards if we bet on the winning hand
            if (GameState.Instance.getCurrentBetType() == GameState.BetType.Banker)
                StartCoroutine(ShunkanShrinkWinningCards(GameState.BetType.Banker));

            // Update TV display
            GameState.Instance.roadmapManager.setBankerWins();

            // Also display and player/banker pair bet winnings together
            string displayExtraWinningsAmountStr = "";
            if (wonOnPlayerPair > 0)
                displayExtraWinningsAmountStr += LanguageManager.GetText("label_player_has_pair") + "   +$" + wonOnPlayerPair.ToString("n0") + "\n";
            if (wonOnBankerPair > 0)
                displayExtraWinningsAmountStr += LanguageManager.GetText("label_banker_has_pair") + "   +$" + wonOnBankerPair.ToString("n0") + "\n";

            if (GameState.Instance.getCurrentBetType() == GameState.BetType.Banker) {

                // Give AR bonus if in AR
#if !UNITY_IPHONE // Apple rejected having an AR Bonus because it conflicts with their idea of in-app purchasing to buy chips.
                if (GameState.Instance.camerasManager.arCamera.activeSelf) {
                    Debug.Log ("AR bonus +$" + AR_BONUS);
                    wonAmount += AR_BONUS;
                    GameState.Instance.currentBalance += AR_BONUS;
                    arBonusAri = true;
                    winnings.Add (Consts.FEP_WINLOSE_BONUS_AR, AR_BONUS+"");
                    displayExtraWinningsAmountStr += LanguageManager.GetText("label_ar_bonus") + " +$" + AR_BONUS + "\n";
                }
#endif

                // Dish out winnings for banker bets
                myWin = GameState.Instance.dishOutWinnings(GameState.BetType.Banker, bankerWinPayback);
                wonAmount += myWin;
                winnings.Add (Consts.FEP_WINLOSE_BANKER, myWin+"");
                GameState.Instance.guiControls.displayBankerWins(LanguageManager.GetText("label_you_win") + "\n" + displayExtraWinningsAmountStr +
                     LanguageManager.GetText("label_banker_wins") + "   +$" + (wonAmount - wonOnPlayerPair - wonOnBankerPair - (arBonusAri ? AR_BONUS : 0)).ToString("n0"));
            } else if (GameState.Instance.getCurrentBetType() == GameState.BetType.Player || GameState.Instance.getCurrentBetType() == GameState.BetType.Tie) {
                // Damn, we didn't bet on the winning hand!
                GameState.Instance.guiControls.displayBankerWins(displayExtraWinningsAmountStr +
                    ((wonOnPlayerPair <= 0 && wonOnBankerPair <= 0) ? LanguageManager.GetText("label_loser") + " ": "")
                    + LanguageManager.GetText("label_banker_wins"));
            }

            winners.Add (GameState.BetType.Banker);

            // Determine the timing to switch back to the main camera on whether we had pairs or not
            if (wonOnPlayerPair >= 1 && wonOnBankerPair >= 1)
                yield return new WaitForSeconds((8f-(2f*pairCameraWaitFactor))*dealSpeed);
            else if (wonOnPlayerPair >= 1 || wonOnBankerPair >= 1)
                yield return new WaitForSeconds((8f-pairCameraWaitFactor)*dealSpeed);
            else
                yield return new WaitForSeconds(8f*dealSpeed);

            // Reset back to main camera
            GameState.Instance.camerasManager.resetToMainCamera();
        } else if (playerTotal == bankerTotal) {
            Debug.Log ("TIE WINS!");
            LogUtils.LogEvent(Consts.FE_WIN_EVENT, new string[] { Consts.FEP_TIE_WIN }, false);

            // AR hack: show the chips in time for winning animations before they disappear (the problem was they were too short (disappearing) in AR
            if (GameState.Instance.camerasManager.isAR())
                GameState.Instance.chipsManager.clearChipsForSqueezing(false);

            // Shake cards if we bet on the winning hand
            if (GameState.Instance.getCurrentBetType() == GameState.BetType.Tie) {
                StartCoroutine(ShunkanShrinkWinningCards(GameState.BetType.Player));
                StartCoroutine(ShunkanShrinkWinningCards(GameState.BetType.Banker));
            }

            // Update TV display
            GameState.Instance.roadmapManager.setTieWins();

            GameState.Instance.roadmapManager.setLatestResult(GameState.BetType.Tie, hasNatural, dealtPlayerPair, dealtBankerPair);
            //vtie_wins.Play (); // Say it elsewhere now

            // Also display and player/banker pair bet winnings together
            string displayExtraWinningsAmountStr = "";
            if (wonOnPlayerPair > 0)
                displayExtraWinningsAmountStr += LanguageManager.GetText("label_player_has_pair") + "   +$" + wonOnPlayerPair.ToString("n0") + "\n";
            if (wonOnBankerPair > 0)
                displayExtraWinningsAmountStr += LanguageManager.GetText("label_banker_has_pair") + "   +$" + wonOnBankerPair.ToString("n0") + "\n";

            // Give the winners their earnings and take in the losses
            if (GameState.Instance.betsOnTableHash.Contains(GameState.BetType.Tie)) {

                // Give AR bonus if in AR
#if !UNITY_IPHONE // Apple rejected having an AR Bonus because it conflicts with their idea of in-app purchasing to buy chips.
                if (GameState.Instance.camerasManager.arCamera.activeSelf) {
                    Debug.Log ("AR bonus +$" + AR_BONUS+"");
                    wonAmount += AR_BONUS;
                    GameState.Instance.currentBalance += AR_BONUS;
                    arBonusAri = true;
                    winnings.Add (Consts.FEP_WINLOSE_BONUS_AR, AR_BONUS+"");
                    displayExtraWinningsAmountStr += LanguageManager.GetText("label_ar_bonus") + " +$" + AR_BONUS + "\n";
                }
#endif

                // Dish out winnings for tie bets
                displayMessage(LanguageManager.GetText("label_tie_wins"), vtie_wins.clip.length);
                myWin = GameState.Instance.dishOutWinnings(GameState.BetType.Tie, tieWinPayback);
                wonAmount += myWin;
                winnings.Add (Consts.FEP_WINLOSE_TIE, myWin+"");
                GameState.Instance.guiControls.displayTieWins(LanguageManager.GetText("label_you_win") + "\n" + displayExtraWinningsAmountStr +
                      LanguageManager.GetText("label_tie_wins") + "   +$" + (wonAmount - wonOnPlayerPair - wonOnBankerPair - (arBonusAri ? AR_BONUS : 0)).ToString("n0"));
            } else if (GameState.Instance.getCurrentBetType() == GameState.BetType.Banker || GameState.Instance.getCurrentBetType() == GameState.BetType.Player) {
                // Damn, we didn't bet on the winning hand!
                GameState.Instance.guiControls.displayTieWins(displayExtraWinningsAmountStr +
                    ((wonOnPlayerPair <= 0 && wonOnBankerPair <= 0) ? LanguageManager.GetText("label_loser") + " ": "")
                    + LanguageManager.GetText("label_tie_wins"));

                // Return the user's betted amount because they're not supposed to lose their money if they don't win on a tie win
                Debug.Log ("Returning $" + GameState.Instance.getCurrentBetValue() + " because tie win but no bets on tie");
                GameState.Instance.currentBalance += GameState.Instance.getCurrentBetValue();
            }

            winners.Add (GameState.BetType.Tie);
            LogUtils.LogEvent (Consts.FE_WINLOSE_BET_TYPE_AMOUNT_EVENT, winnings, false); // winnings per bet type / bonus
            Dictionary<string, string> winTotals = new Dictionary<string, string>(); // for Flurry
            winTotals.Add (Consts.FEP_WIN_AMOUNT, wonAmount+"");
            LogUtils.LogEvent (Consts.FE_WINLOSE_TOTALS_EVENT, winTotals, false); // total won

            // Determine the timing to switch back to the main camera on whether we had pairs or not
            float tt=8f;
            if (wonOnPlayerPair >= 1 && wonOnBankerPair >= 1)
                yield return new WaitForSeconds((tt-(2f*pairCameraWaitFactor))*dealSpeed);
            else if (wonOnPlayerPair >= 1 || wonOnBankerPair >= 1)
                yield return new WaitForSeconds((tt-(2f*pairCameraWaitFactor))*dealSpeed);
            else
                yield return new WaitForSeconds(tt*dealSpeed);

            // Reset back to main camera
            GameState.Instance.camerasManager.resetToMainCamera();
        } else {
            Debug.Log ("ERROR WINS! :(");
            displayMessage("ERROR WINS! :(");

            // Reset back to main camera
            GameState.Instance.camerasManager.resetToMainCamera();
        }

        if (wonAmount > 0)
            vnice_bet.Play ();

        // Deduct losses from losing hands
        //yield return new WaitForSeconds(2f*Dealer.dealSpeed);
        GameState.Instance.deductLosses(winners);

        // For first time installs, first offer the user to join Google Play Games at the end
        // of their second round. My guess is it's better psychology to have let the user play
        // at least one round before popping up screens which can be annoying for a lot of people.
        if (!PlayerPrefs.HasKey("gpg") && !Social.localUser.authenticated && GameState.Instance.roundNumber == 3 && GameState.Instance.tutorialEndedThisSession) {
            Debug.Log ("Opening screen to connect to Google Play Games for the first time");
            GameState.Instance.guiControls.openScore();
        } else if (Social.localUser.authenticated) {
            GameState.Instance.gpgCloudManager.postScore();
        }

        // Replace the shoe if necessary
        if (lastRound) {
            Debug.Log ("Last round. Resetting shoe...");
            LogUtils.LogEvent(Consts.FE_SHOE_NUMBER, new string[] { ""+shoeNumber }, false);
            shoeNumber++;
            reset ();
            cutCardGameObject = null;
            lastRoundNext = false;
            lastRound = false;
            currentDeckNum = 0;
            isCut = false;
            initializeDecks();
            GameState.Instance.currentState = GameState.State.CutCards;
        }
    }

    /**
     * Do a little animation on the winning hand of cards so it's easier to know which hand won.
     */
    IEnumerator ShunkanShrinkWinningCards(GameState.BetType winningHand) {
        Debug.Log("Animating winning hand of cards: " + winningHand);
        Vector3 sVec = new Vector3(0.1f, 0.1f, 0.1f);
        float tim = 2f;
        //try {
            if (winningHand == GameState.BetType.Player) {
                iTween.ShakeScale((GameObject) dealtCards ["PlayerCard1"], iTween.Hash ("amount", sVec, "time", tim));
                iTween.ShakeScale((GameObject) dealtCards ["PlayerCard2"], iTween.Hash ("amount", sVec, "time", tim));
                if (dealtCards ["PlayerCard3"] != null) {
                    iTween.ShakeScale((GameObject) dealtCards ["PlayerCard3"], iTween.Hash ("amount", sVec, "time", tim));
                } else {
                    //Debug.LogError ("ShunkanShrinkWinningCards: PlayerCard 3 is null");
                }
            } else if (winningHand == GameState.BetType.Banker) {
                iTween.ShakeScale((GameObject) dealtCards ["BankerCard1"], iTween.Hash ("amount", sVec, "time", tim));
                iTween.ShakeScale((GameObject) dealtCards ["BankerCard2"], iTween.Hash ("amount", sVec, "time", tim));
                if (dealtCards ["BankerCard3"] != null) {
                    iTween.ShakeScale((GameObject) dealtCards ["BankerCard3"], iTween.Hash ("amount", sVec, "time", tim));
                } else {
                    //Debug.LogError ("ShunkanShrinkWinningCards: BankerCard 3 is null");
                }
            }
        /*
        } catch (System.NullReferenceException e) {
            // Sometimes there's no 3rd card so ignore the NPE
            if (true) // hack
                Debug.LogError("ShunkanShrinkWinningCards NullReferenceException: " + e.Message);
        } catch (UnassignedReferenceException e2) {
            // Sometimes there's no 3rd card so ignore the NPE
            if (true) // hack
                Debug.LogError("ShunkanShrinkWinningCards UnassignedReferenceException: " + e2.Message);
        } catch (System.Exception e3) {
            // Sometimes there's no 3rd card so ignore the NPE
            if (true) // hack
                Debug.LogError("ShunkanShrinkWinningCards Exception: " + e3.Message);
        }
        */

        yield break;
    }

    // User gets any chips they won
    public void onWin(GameState.BetType winner, int paysout, int wonAmount, int commission) {
        StartCoroutine(onWinCoroutine(winner, paysout, wonAmount, commission));
    }

    IEnumerator onWinCoroutine(GameState.BetType winner, int paysout, int wonAmount, int commission) {
        vgame_win.Play ();

        yield return new WaitForSeconds(3f*Dealer.dealSpeed);

        //yield return new WaitForSeconds(vnice_bet.clip.length);
        GameState.Instance.chipsManager.chipsMadness(winner, paysout, wonAmount, commission);

        // Continue to change clothes colors as long as we're playing winning sounds
        //Color orgBikiniTopColor = bikiniTop.renderer.material.GetColor("_Color");
        /* Disable color changes for now
        Color orgDressColor1 = dress.renderer.materials[0].GetColor("_Color");
        Color orgDressColor2 = dress.renderer.materials[1].GetColor("_Color");
        Color orgSexyTopColor = sexyTop.renderer.materials[0].GetColor("_Color");;
        //for (int i = 0; i < 2; i++) { // Just one loop seems enough
           float r = 160, g = 87;
            for (; r < 255; r+=2, g+=4) {
                //bikiniTop.renderer.material.SetColor ("_Color", new Color (r/255f, g/255f, 0.4f, 1.0f));
                dress.renderer.materials[0].SetColor ("_Color", new Color (r/255f, g/255f, 0.4f, 1.0f));
                dress.renderer.materials[1].SetColor ("_Color", new Color (r/255f, g/255f, 0.4f, 1.0f));
                sexyTop.renderer.materials[0].SetColor ("_Color", new Color (r/255f, g/255f, 0.4f, 1.0f));
                yield return new WaitForSeconds(0.001f); // psuedo-flashing up
            }
            for (r = g = 255; r >= 160; r-=2, g-=4) {
                //bikiniTop.renderer.material.SetColor ("_Color", new Color (r/255f, g/255f, 0.4f, 1.0f));
                dress.renderer.materials[0].SetColor ("_Color", new Color (r/255f, g/255f, 0.4f, 1.0f));
                dress.renderer.materials[1].SetColor ("_Color", new Color (r/255f, g/255f, 0.4f, 1.0f));
                sexyTop.renderer.materials[0].SetColor ("_Color", new Color (r/255f, g/255f, 0.4f, 1.0f));
                yield return new WaitForSeconds(0.001f); // psuedo-flashing down
            }
        //}
        //bikiniTop.renderer.material.SetColor("_Color", orgBikiniTopColor);
        dress.renderer.materials[0].SetColor("_Color", orgDressColor1);
        dress.renderer.materials[1].SetColor("_Color", orgDressColor2);
        sexyTop.renderer.materials[0].SetColor("_Color", orgSexyTopColor);
        changeClothes();
        */
        yield break;
    }

    // Dealer gets any chips that were lost
    public void onLoss(List<GameState.BetType> winners) {
        StartCoroutine(onLossCoroutine(winners));
    }

    IEnumerator onLossCoroutine(List<GameState.BetType> winners) {
        vexcuse_me.Play ();
        vgame_lose.Play ();
        yield return new WaitForSeconds(vexcuse_me.clip.length);
        GameState.Instance.chipsManager.clearLoses(winners);
        dealerAnimator.SetBool ("Collect", true);
        yield return new WaitForSeconds(2);
        dealerAnimator.SetBool ("Collect", false);
        changeClothes();
    }

    void playWithDelay(AudioSource clip, float delay) {
        StartCoroutine(playWithDelayCoro(clip, delay));
    }

    IEnumerator playWithDelayCoro(AudioSource clip, float delay) {
        yield return new WaitForSeconds(delay);
        clip.Play();
    }    

    // Initialize the dealer's voice wav files
    private AudioSource v0;
    private AudioSource v1;
    private AudioSource v2;
    private AudioSource v3;
    private AudioSource v4;
    private AudioSource v5;
    private AudioSource v6;
    private AudioSource v7;
    private AudioSource v8;
    private AudioSource v9;
    private AudioSource vbanker;
    private AudioSource vbanker_card;
    private AudioSource vbanker_card2;
    private AudioSource vbanker_card3;
    private AudioSource vbanker_customer;
    private AudioSource vbanker_natural;
    private AudioSource vbanker_wins;
    public AudioSource vbye_bye;
    private AudioSource vcut_card_appears;
    private AudioSource vexcuse_me;
    private AudioSource vlast_round;
    //private AudioSource vlast_round_next;
    private AudioSource vnice_bet;
    private AudioSource vnice_cut;
    private AudioSource vpair;
    private AudioSource vplace_bets;
    private AudioSource vplayer;
    private AudioSource vplayer_card;
    private AudioSource vplayer_card2;
    private AudioSource vplayer_card3;
    private AudioSource vplayer_customer;
    private AudioSource vplayer_natural;
    private AudioSource vplayer_wins;
    private AudioSource vplease_cut;
    private AudioSource vtie;
    private AudioSource vtie_wins;
    private AudioSource vwelcome;
    public AudioSource vgame_win;
    private AudioSource vgame_lose;
    private AudioSource vinsufficient_funds;
    private AudioSource vtie_tap;
    //private AudioSource vstand_on;

    void loadDealerVoiceAudio() {
        Debug.Log("Loading dealer voice files");

        string path;
        if (Utils.isJapanese()) {
            Debug.Log ("Dealer voice is in Japanese");
            path = "Sounds/ja/nozomi/"; // TODO: add logic for AI-J Sumire when add extra dealer
        } else {
            // Default to English
            Debug.Log ("Dealer voice is defaulting to English");
            path = "Sounds/en/google_lady/";
        }


        // Attach audio sources to the game object
        v0 = gameObject.AddComponent<AudioSource>();
        v1 = gameObject.AddComponent<AudioSource>();
        v2 = gameObject.AddComponent<AudioSource>();
        v3 = gameObject.AddComponent<AudioSource>();
        v4 = gameObject.AddComponent<AudioSource>();
        v5 = gameObject.AddComponent<AudioSource>();
        v6 = gameObject.AddComponent<AudioSource>();
        v7 = gameObject.AddComponent<AudioSource>();
        v8 = gameObject.AddComponent<AudioSource>();
        v9 = gameObject.AddComponent<AudioSource>();
        vbanker = gameObject.AddComponent<AudioSource>();
        vbanker_card = gameObject.AddComponent<AudioSource>();
        vbanker_card2 = gameObject.AddComponent<AudioSource>();
        vbanker_card3 = gameObject.AddComponent<AudioSource>();
        vbanker_customer = gameObject.AddComponent<AudioSource>();
        vbanker_natural = gameObject.AddComponent<AudioSource>();
        vbanker_wins = gameObject.AddComponent<AudioSource>();
        vbye_bye = gameObject.AddComponent<AudioSource>();
        vcut_card_appears = gameObject.AddComponent<AudioSource>();
        vexcuse_me = gameObject.AddComponent<AudioSource>();
        vlast_round = gameObject.AddComponent<AudioSource>();
        //vlast_round_next = gameObject.AddComponent<AudioSource>();
        vnice_bet = gameObject.AddComponent<AudioSource>();
        vnice_cut = gameObject.AddComponent<AudioSource>();
        vpair = gameObject.AddComponent<AudioSource>();
        vplace_bets = gameObject.AddComponent<AudioSource>();
        vplayer = gameObject.AddComponent<AudioSource>();
        vplayer_card = gameObject.AddComponent<AudioSource>();
        vplayer_card2 = gameObject.AddComponent<AudioSource>();
        vplayer_card3 = gameObject.AddComponent<AudioSource>();
        vplayer_customer = gameObject.AddComponent<AudioSource>();
        vplayer_natural = gameObject.AddComponent<AudioSource>();
        vplayer_wins = gameObject.AddComponent<AudioSource>();
        vplease_cut = gameObject.AddComponent<AudioSource>();
        vtie = gameObject.AddComponent<AudioSource>();
        vtie_wins = gameObject.AddComponent<AudioSource>();
        vwelcome = gameObject.AddComponent<AudioSource>();
        vgame_win = gameObject.AddComponent<AudioSource>();
        vgame_lose = gameObject.AddComponent<AudioSource>();
        vinsufficient_funds = gameObject.AddComponent<AudioSource>();
        vtie_tap = gameObject.AddComponent<AudioSource>();
        //vstand_on = gameObject.AddComponent<AudioSource>();

        // Load the wav files
        v0.clip = (AudioClip) Resources.Load(path + "0");
        v1.clip = (AudioClip) Resources.Load(path + "1");
        v2.clip = (AudioClip) Resources.Load(path + "2");
        v3.clip = (AudioClip) Resources.Load(path + "3");
        v4.clip = (AudioClip) Resources.Load(path + "4");
        v5.clip = (AudioClip) Resources.Load(path + "5");
        v6.clip = (AudioClip) Resources.Load(path + "6");
        v7.clip = (AudioClip) Resources.Load(path + "7");
        v8.clip = (AudioClip) Resources.Load(path + "8");
        v9.clip = (AudioClip) Resources.Load(path + "9");
        vbanker.clip = (AudioClip) Resources.Load(path + "banker");
        vbanker_card.clip = (AudioClip) Resources.Load(path + "banker_card");
        vbanker_card2.clip = (AudioClip) Resources.Load(path + "banker_card2");
        vbanker_card3.clip = (AudioClip) Resources.Load(path + "banker_card3");
        vbanker_customer.clip = (AudioClip) Resources.Load(path + "banker_customer");
        vbanker_natural.clip = (AudioClip) Resources.Load(path + "banker_natural");
        vbanker_wins.clip = (AudioClip) Resources.Load(path + "banker_wins");
        vbye_bye.clip = (AudioClip) Resources.Load(path + "bye_bye");
        vcut_card_appears.clip = (AudioClip) Resources.Load(path + "cut_card_appears");
        vexcuse_me.clip = (AudioClip) Resources.Load(path + "excuse_me");
        vlast_round.clip = (AudioClip) Resources.Load(path + "last_round");
        //vlast_round_next.clip = (AudioClip) Resources.Load(path + "last_round_next");
        vnice_bet.clip = (AudioClip) Resources.Load(path + "nice_bet");
        vnice_cut.clip = (AudioClip) Resources.Load(path + "nice_cut");
        vpair.clip = (AudioClip) Resources.Load(path + "pair");
        vplace_bets.clip = (AudioClip) Resources.Load(path + "place_bets");
        vplayer.clip = (AudioClip) Resources.Load(path + "player");
        vplayer_card.clip = (AudioClip) Resources.Load(path + "player_card");
        vplayer_card2.clip = (AudioClip) Resources.Load(path + "player_card2");
        vplayer_card3.clip = (AudioClip) Resources.Load(path + "player_card3");
        vplayer_customer.clip = (AudioClip) Resources.Load(path + "player_customer");
        vplayer_natural.clip = (AudioClip) Resources.Load(path + "player_natural");
        vplayer_wins.clip = (AudioClip) Resources.Load(path + "player_wins");
        vplease_cut.clip = (AudioClip) Resources.Load(path + "please_cut");
        vtie.clip = (AudioClip) Resources.Load(path + "tie");
        vtie_wins.clip = (AudioClip) Resources.Load(path + "tie_wins");
        vwelcome.clip = (AudioClip) Resources.Load(path + "welcome");
        vgame_win.clip = (AudioClip) Resources.Load("Sounds/GameWin");
        vgame_lose.clip = (AudioClip) Resources.Load("Sounds/GameLose");
        vinsufficient_funds.clip = (AudioClip) Resources.Load("Sounds/InsufficientFunds");
        vtie_tap.clip = (AudioClip) Resources.Load("Sounds/TieWin");
        //vstand_on.clip = (AudioClip) Resources.Load(path + "stand_on");

        // Disable auto play
        v0.playOnAwake = false;
        v1.playOnAwake = false;
        v2.playOnAwake = false;
        v3.playOnAwake = false;
        v4.playOnAwake = false;
        v5.playOnAwake = false;
        v6.playOnAwake = false;
        v7.playOnAwake = false;
        v8.playOnAwake = false;
        v9.playOnAwake = false;
        vbanker.playOnAwake = false;
        vbanker_card.playOnAwake = false;
        vbanker_card2.playOnAwake = false;
        vbanker_card3.playOnAwake = false;
        vbanker_customer.playOnAwake = false;
        vbanker_natural.playOnAwake = false;
        vbanker_wins.playOnAwake = false;
        vbye_bye.playOnAwake = false;
        vcut_card_appears.playOnAwake = false;
        vexcuse_me.playOnAwake = false;
        vlast_round.playOnAwake = false;
        //vlast_round_next.playOnAwake = false;
        vnice_bet.playOnAwake = false;
        vnice_cut.playOnAwake = false;
        vpair.playOnAwake = false;
        vplace_bets.playOnAwake = false;
        vplayer.playOnAwake = false;
        vplayer_card.playOnAwake = false;
        vplayer_card2.playOnAwake = false;
        vplayer_card3.playOnAwake = false;
        vplayer_customer.playOnAwake = false;
        vplayer_natural.playOnAwake = false;
        vplayer_wins.playOnAwake = false;
        vplease_cut.playOnAwake = false;
        vtie.playOnAwake = false;
        vtie_wins.playOnAwake = false;
        vwelcome.playOnAwake = false;
        vgame_win.playOnAwake = false;
        vgame_lose.playOnAwake = false;
        vinsufficient_funds.playOnAwake = false;
        vtie_tap.playOnAwake = false;
        //vstand_on.playOnAwake = false;

        return;
    }

    public void playInsuffcientFundsSound ()
    {
        vinsufficient_funds.Play();
    }


    // COMMENTED OUT callers to this function because it didn't sound very nice
    /*
    public void playStandOn(int num) {
        StartCoroutine(playStandOnCoroutine(num));
    }

    IEnumerator playStandOnCoroutine(int num) {
        vstand_on.Play();
        yield return new WaitForSeconds(vstand_on.clip.length);
        getNumWav(num, true).Play();
        yield return new WaitForSeconds(getNumWav(num, true).clip.length);
    }
     */

    // Return the audio source object that matches the specified integer
    AudioSource getNumWav(int val) {
        switch (val) {
            case 0: return v0;
            case 1: return v1;
            case 2: return v2;
            case 3: return v3;
            case 4: return v4;
            case 5: return v5;
            case 6: return v6;
            case 7: return v7;
            case 8: return v8;
            case 9: return v9;
        }
        return v0;
    }

    // Destroy all the objects on the card GameObject before destroying the GO itself to avoid big memory leaks
    private void destroyCard(GameObject cardGO) {
        if (cardGO == null)
            return;
        foreach (MeshFilter mf in cardGO.GetComponentsInChildren<MeshFilter>()) {
            DestroyImmediate (mf.mesh);
        }
        Destroy (cardGO);
    }

    // Clear the cards on the table
    public void clearCards() {
        Debug.Log ("Clearing cards off table");

        destroyCard((GameObject)dealtCards["PlayerCard1"]);
        destroyCard((GameObject)dealtCards["PlayerCard2"]);
        destroyCard((GameObject)dealtCards["PlayerCard3"]);
        destroyCard((GameObject)dealtCards["BankerCard1"]);
        destroyCard((GameObject)dealtCards["BankerCard2"]);
        destroyCard((GameObject)dealtCards["BankerCard3"]);

        // Memory leak cleanup hack. Each time we created cards we had stale meshes left in memory with a reference count of zero.
        // TODO: investigate Card.cs and the combining meshes to find out where the stale meshes are being left hanging there
        Object[] meshes = Object.FindObjectsOfType(typeof(Mesh));
        foreach (Mesh item in meshes)
        {
            if (item != null
                && !item.name.Contains("Box")
                && !item.name.Contains("Plane"))
                    DestroyImmediate (item);
        }

        reset();
//        SayPlaceBets();
    }

    public void bowOn() {
        if (dealerAnimator != null && dealerCharacter.activeSelf) {
            dealerAnimator.SetBool("Bow", true);
        }
    }

    public void bowOff() {
        if (dealerAnimator != null && dealerCharacter.activeSelf) {
            dealerAnimator.SetBool("Bow", false);
        }
    }

    // Change the dealer's clothes based on the bank balance
    public void changeClothes() {
        // TODO: for now, the dealer (Airi) has a rough edge of polygons on the front of her upper right arm
        // which looks like she's had an operation so she'll just wear her sexy top that covers that part of her
        // arm. When we get some money to pay a 3D artist to fix her up then we can revisit changing her clothes.
        return;

        // TODO: review balances
        int balance = GameState.Instance.currentBalance;
        if (balance < 20000) {
            // Normal dress - dark red
            bikiniTop.SetActive(false);
            dress.SetActive(true);
            dress.GetComponent<Renderer>().materials[0].SetColor ("_Color", new Color (175f/255f, 175f/255f, 175f/255f, 1f));
            dress.GetComponent<Renderer>().materials[1].SetColor ("_Color", new Color (175f/255f, 175f/255f, 175f/255f, 1f));
        } else if (balance >= 20000 && balance < 35000) {
            // Dress color 1 - black
            bikiniTop.SetActive(false);
            //sexyTop.SetActive(false); // Too tight so skin polygons showing so can't use yet
            //miniSkirt.SetActive(false); // Pairs with sexy top
            dress.SetActive(true);
            dress.GetComponent<Renderer>().materials[0].SetColor ("_Color", new Color (30f/255f, 30f/255f, 30f/255f, 1.0f));
            dress.GetComponent<Renderer>().materials[1].SetColor ("_Color", new Color (30f/255f, 30f/255f, 30f/255f, 1.0f));
        } else if (balance >= 35000 && balance < 50000) {
            // Dress color 2 - gold
            bikiniTop.SetActive(false);
            dress.SetActive(true);
            dress.GetComponent<Renderer>().materials[0].SetColor ("_Color", new Color (1, 126f/255f, 40f/255f, 1.0f));
            dress.GetComponent<Renderer>().materials[1].SetColor ("_Color", new Color (1, 126f/255f, 40f/255f, 1.0f));
        } else if (balance >= 50000 && balance < 75000) {
            // Bikini color 1 - purple
            bikiniTop.SetActive(true);
            dress.SetActive(false);
            bikiniTop.GetComponent<Renderer>().material.SetColor ("_Color", new Color (156f/255f, 0, 108f/255f, 1.0f));
        } else if (balance >= 75000 && balance < 100000) {
            // Bikini color 2 - lime green
            bikiniTop.SetActive(true);
            dress.SetActive(false);
            bikiniTop.GetComponent<Renderer>().material.SetColor ("_Color", new Color (130f/255f, 236f/255f, 0, 1.0f));
        } else if (balance >= 100000 && balance < 500000) {
            // Bikini color 3 - gold
            bikiniTop.SetActive(true);
            dress.SetActive(false);
            bikiniTop.GetComponent<Renderer>().material.SetColor ("_Color", new Color (1, 126f/255f, 40f/255f, 1.0f));
        } else if (balance >= 500000) {
            // Topless!
            bikiniTop.SetActive(false);
            dress.SetActive(false);
        }
    }


    // Below is a mechanism to skip certain parts of the game by tapping the screen.
    // Any code can register the next gameobject and the method in which to be invoked
    // on the next tap.
    public GameObject nextTapObject = null;
    public string nextTapMethodName = null;
    public GameObject nextTapStopSpeechBubble = null;
    public bool nextTapResetOnNext = false;
    void OnDealtCardsCameraTap() { // bad method name as we receive tap gestures from alllllllllllllllll over the place. Damn. It's 7am 2nd Feb 2014. Done a 21 hour baccarat marathon. zzzz.
        // Stop all speech bubbles currently being displayed
        if (nextTapStopSpeechBubble != null) {
            Debug.Log ("Tagging " + nextTapStopSpeechBubble.name + " with d");
            nextTapStopSpeechBubble.tag = "d"; // will be picked up elsewhere for deletion

            //if (nextTapResetOnNext)
                nextTapStopSpeechBubble = null;
        }

        // Invoke the next method
        if (nextTapObject == null || nextTapMethodName == null)
            return;
        Debug.Log ("Screen tap detected - invoking " + nextTapMethodName + " on " + nextTapObject);
        nextTapObject.SendMessage(nextTapMethodName);

        if (nextTapResetOnNext) {
            nextTapObject = null;
            nextTapMethodName = null;
            nextTapResetOnNext = false;
        }
    }

    // Show and animate the single finger tutorial moving a chip onto the table
    Color moveChipHandOriginalColor;
    Vector3 moveChipHandOriginalPos;
    public void showTutorialChipMove() {
        Debug.Log ("Showing and moving single finger move chip tutorial");
        GameState.Instance.dealer.moveChipHand.SetActive (true);
        GameState.Instance.dealer.moveChipHand.GetComponentInChildren<SkinnedMeshRenderer> ().enabled = true;
        moveChipHandOriginalColor = GameState.Instance.dealer.moveChipHand.GetComponentInChildren<SkinnedMeshRenderer> ().material.color;
        GameState.Instance.dealer.moveChipHand.GetComponentInChildren<SkinnedMeshRenderer> ().material.SetColor ("_Color", Color.white);
        moveChipHandOriginalPos = moveChipHand.transform.position;
        iTween.MoveBy (GameState.Instance.dealer.moveChipHand, iTween.Hash ("amount", new Vector3(0f, 0f, 0.15f), "time", 2.2f, "delay", 0.8f,
            "oncomplete", "showTutorialChipMoveComplete", "oncompletetarget", this.gameObject));
    }
    public void showTutorialChipMoveComplete() {
        GameState.Instance.dealer.moveChipHand.GetComponentInChildren<SkinnedMeshRenderer> ().material.color = moveChipHandOriginalColor;
        GameState.Instance.dealer.moveChipHand.GetComponentInChildren<SkinnedMeshRenderer> ().enabled = false;
        iTween.MoveTo (GameState.Instance.dealer.moveChipHand.gameObject, moveChipHandOriginalPos, 0f);
        Chip.isShowingTutorials = false;
        GameState.Instance.ToggleFingerGestures(true);
        this.Invoke("disableMoveChipHand", 0.2f); // need this in order for the above position-reset command time to work
    }
    public void disableMoveChipHand() {
        GameState.Instance.dealer.moveChipHand.SetActive (false);
    }
}