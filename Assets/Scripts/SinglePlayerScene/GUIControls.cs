using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GUIControls : MonoBehaviour
{
    // Dimension constants
    public static int BUTTON_WIDTH = (int)Mathf.Round (Screen.width * 0.22f);
    public static int BUTTON_HEIGHT = (int)Mathf.Round (Screen.height * 0.12f);
    public static int PANEL_TOP_HEIGHT = (int)Mathf.Round (Screen.height * 0.14f);
    public static int PANEL_TOP_WIDTH = (int)Mathf.Round (Screen.width * 0.47f);
    public static int PANEL_SIDE_HEIGHT = (int)Mathf.Round (Screen.height * 0.15f);
    public static int PANEL_SIDE_WIDTH = (int)Mathf.Round (Screen.height * 0.195f);
    public static int PADDING_HORIZTONAL = (int)Mathf.Round (Screen.width * 0.005f);
    public static int PADDING_VERTICAL = 0;//(int)Mathf.Round(Screen.height*0.005f);

    // GUIStyles
    public GUIStyle buttonStyle;
    public GUIStyle panelTopStyle;
    public GUIStyle panelSideStyle;
    public GUIStyle messageStyle;
    public GUISkin gyroScrollbarSkin;

    // Updatable text fields for GUI labels
    public static string bankerCardsValueText = "";
    public static string playerCardsValueText = "";
    public static List<string[]> currentBetTexts = new List<string[]> ();
    public static int balanceInt = 0;
    public static int lastBalanceInt = 0;
    public static string balanceText = "";
    public static string message = "";
    bool isDisplayingMessage = false;
    public static bool returnCardPressed = false;

    // Settings for messages to display card totals at end of a hand
    private Color playerColor = new Color (213f / 255f, 127f / 255f, 81f / 255f, 1f); // player pinkish
    private Color bankerColor = new Color (225f / 255f, 176f / 255f, 6f / 255f, 1f); // banker yellowish
    private static float TOTAL_MSG_WIDTH = 1.0f;
    private static float TOTAL_MSG_HEIGHT = 0.12f;

    // Button game objects
    public GameObject clearBtn;
    public GameObject dealBtn;
    public GameObject arBtn;
    public GameObject gyroBtn;
    public GameObject _3dBtn;
    public GameObject rebetBtn;
    bool hideLeftRightCardButtonsBool = false;
    public GameObject leftCardBtn;
    public GameObject rightCardBtn;
    public GameObject revealReturnBtn;
    public GameObject revealBankerOtherBtn;
    public GameObject revealPlayerOtherBtn;
    public GameObject circularMenu;
    public GameObject leaderboard;
    public GameObject shop;
    public GameObject help;
    //public GameObject arMenu;
    public GameObject statsPanel;
    GameObject visibleCardMoveBtn;
    GameObject invisibleCardMoveBtn;

    // Main menu
    public GameObject menu;

    // Rebet button dimensions
    float rebetBtnWidth {
        get {
            return GUIControls.isPortrait ? Screen.width * 0.23f : Screen.width * 0.20f;
        }
    }

    float rebetBtnHeight {
        get {
            return Screen.height * 0.07f;
        }
    }

    // Font scaler
    public static int fontScaler {
        get {
            return Mathf.RoundToInt (GUIControls.isPortrait ? (Screen.width * 0.145f) : (Screen.width * 0.115f));
        }
    }

    public enum DealButtonState
    {
        Hide,
        ReturnCard,
        Deal,
        Rebet
    };
    public DealButtonState dealButtonState = DealButtonState.Hide;
    private DealButtonState lastDealButtonState = DealButtonState.Hide;

    public enum ClearButtonState
    {
        Hide,
        Clear,
        OtherCard
    };
    public ClearButtonState clearButtonState = ClearButtonState.Hide;
    private ClearButtonState lastClearButtonState = ClearButtonState.Hide;

    // Return if we're in portrait mode or not taking into account the other
    // orientations like faceup and facedown, in which case we use the last
    // remembered portrait or landscape
    private static bool lastOrientationWasPortrait = true;

    public static bool isPortrait {
        get {
            //return false; // test to force landscape

            // We force portrait if in tutorial mode or the help screen is open
            if (GameState.Instance != null && GameState.Instance.tutorialHelpManager != null) {
                if (GameState.Instance.tutorialCounter <= 1) {
                    //Debug.LogError ("isPortrait because tutorialCount=="+GameState.Instance.tutorialCounter);
                    return true;
                }
                if (GameState.Instance.tutorialHelpManager.isShowing) {
                    //Debug.LogError ("isPortrait because GameState.Instance.tutorialHelpManager.isShowing==true");
                    return true;
                }
            }

            // Force portrait if auto rotate has been turned off (used while squeezing)
            if (Screen.orientation == ScreenOrientation.Portrait) {
                //Debug.LogError ("isPortrait because Screen.orientation != ScreenOrientation.AutoRotation =>"+Screen.orientation.ToString());
                return true;
            }

            switch (Input.deviceOrientation) {
            case DeviceOrientation.LandscapeLeft:
            case DeviceOrientation.LandscapeRight:
                lastOrientationWasPortrait = false;
                return false;
            case DeviceOrientation.Portrait:
            case DeviceOrientation.PortraitUpsideDown:
                lastOrientationWasPortrait = true;
                return true;
            default:
                return lastOrientationWasPortrait;
            }
        }
        set {}
    }

    public static bool isPortraitNoConditions { // used mainly for Ads
        get {
            // Force portrait if auto rotate has been turned off (used while squeezing)
            if (Screen.orientation == ScreenOrientation.Portrait) {
                //Debug.LogError ("isPortrait because Screen.orientation != ScreenOrientation.AutoRotation =>"+Screen.orientation.ToString());
                return true;
            }

            switch (Input.deviceOrientation) {
            case DeviceOrientation.LandscapeLeft:
            case DeviceOrientation.LandscapeRight:
                lastOrientationWasPortrait = false;
                return false;
            case DeviceOrientation.Portrait:
            case DeviceOrientation.PortraitUpsideDown:
                lastOrientationWasPortrait = true;
                return true;
            default:
                return lastOrientationWasPortrait;
            }
        }
        set {}
    }


    public static bool isFaceDown {
        get {
            return Input.gyro.attitude.x > -0.2f && Input.gyro.attitude.x < 0.2f &&
                Input.gyro.attitude.y > -0.2f && Input.gyro.attitude.y < 0.2f;
        }
        set {}
    }

    void Start ()
    {
        Debug.Log ("Initializing GUI controls");

        // Initialize our localization manager
        if (Utils.isJapanese ()) {
            Debug.Log ("User language is Japanese");
            LogUtils.LogEvent(Consts.FE_ENV_LANGUAGE, new string[] { "Japanese" }, false);
            LanguageManager.LoadLanguageFile (Language.Japanese);
            // TODO: add Chinese
        } else {
            // Default to English
            Debug.Log ("User language is defaulting to English");
            LogUtils.LogEvent(Consts.FE_ENV_LANGUAGE, new string[] { Application.systemLanguage.ToString () }, false);
            LanguageManager.LoadLanguageFile (Language.English);
        }

        // Register ourselves with game stage manager
        GameState.Instance.guiControls = this;

        localizeMenu ();

        // Default button for switching between left and right (1st and 2nd) cards is right
        visibleCardMoveBtn = rightCardBtn;
        invisibleCardMoveBtn = leftCardBtn;
        leftCardBtn.GetComponent<SingleCircularButton> ().callbackObject = this.gameObject;
        leftCardBtn.GetComponent<SingleCircularButton> ().methodToInvoke = "showOtherCardCallback";
        rightCardBtn.GetComponent<SingleCircularButton> ().callbackObject = this.gameObject;
        rightCardBtn.GetComponent<SingleCircularButton> ().methodToInvoke = "showOtherCardCallback";
    }

    // Set the menu category labels to their localized text
    void localizeMenu ()
    {
        if (menu == null)
            return;

        // Invite friends
        CircularMenu menuScr = menu.GetComponent<CircularMenu> ();
        CircularMenuCategory inviteCat = menuScr.GetCategoryFromName ("INVITE");
        inviteCat.name = LanguageManager.GetText ("btn_invite");

        // Shop
        CircularMenuCategory shopCat = menuScr.GetCategoryFromName ("SHOP");
        shopCat.name = LanguageManager.GetText ("btn_shop");

        // Home
        CircularMenuCategory homeCat = menuScr.GetCategoryFromName ("HOME");
        homeCat.name = LanguageManager.GetText ("btn_home");

        // Score
        CircularMenuCategory scoreCat = menuScr.GetCategoryFromName ("SCORE");
        scoreCat.name = LanguageManager.GetText ("btn_score");

        // Music
        CircularMenuCategory gcamCat = menuScr.GetCategoryFromName ("MUSIC");
        gcamCat.name = LanguageManager.GetText ("btn_music");

        // Help
        CircularMenuCategory helpCat = menuScr.GetCategoryFromName ("HELP");
        helpCat.name = LanguageManager.GetText ("label_help");
    }

    float displayPlayerTotalMessageStartTime = 0;
    float displayBankerTotalMessageStartTime = 0;
    float displayTieTotalMessageStartTime = 0;
    float displayWinMessageStartTime = 0;
    public static float DURATION_SHOW_CARD_TOTALS = 8.5f;
    public static float DURATION_SHOW_CARD_RESULT = 6.5f;



    /** To ensure we don't get the follow error, we set booleans that falsify the display-or-not booleans
     * for GUI controls on the next Update() call, not from within OnGUI() calls.
     * ArgumentException: Getting control 1's position in a group with only 1 controls when doing Repaint
     */
    bool stopIsDisplayPlayerTotal = false;
    bool stopIsDisplayBankerTotal = false;
    bool stopIsDisplayTieTotal = false;
    bool stopIsDisplayPlayerWins = false;
    bool stopIsDisplayBankerWins = false;
    bool stopIsDisplayTieWins = false;

    void Update () {
        if (GUIControls.balanceInt != GUIControls.lastBalanceInt)
        {
            int f = 3;
            // Basic animated count-up/count-down when balance increases/decreases
            if (lastBalanceInt < balanceInt) {
                lastBalanceInt+=(balanceInt-lastBalanceInt)/f;
                if ((lastBalanceInt+f) > balanceInt) {
                    lastBalanceInt = balanceInt;
                }
            } else if (lastBalanceInt > balanceInt) {
                lastBalanceInt-=(lastBalanceInt-balanceInt)/f;
                if ((lastBalanceInt-f) < balanceInt) {
                    lastBalanceInt = balanceInt;
                }
            }
        }
        balanceText = "$" + lastBalanceInt.ToString("n0");
        
        // Disable displays if queued
        if (stopIsDisplayPlayerTotal) {
            isDisplayPlayerTotal = false;
            stopIsDisplayPlayerTotal = false;
        }
        if (stopIsDisplayBankerTotal) {
            isDisplayBankerTotal = false;
            stopIsDisplayBankerTotal = false;
        }
        if (stopIsDisplayTieTotal) {
            isDisplayTieTotal = false;
            stopIsDisplayTieTotal = false;
        }
        if (stopIsDisplayPlayerWins) {
            isDisplayPlayerWins = false;
            stopIsDisplayPlayerWins = false;
        }
        if (stopIsDisplayBankerWins) {
            isDisplayBankerWins = false;
            stopIsDisplayBankerWins = false;
        }
        if (stopIsDisplayTieWins) {
            isDisplayTieWins = false;
            stopIsDisplayTieWins = false;
        }
        // COMMENTED OUT because we removed ARMenu
        /*
        if (arBtn.activeSelf && GameState.Instance.camerasManager.isAR()) {
            // Switch AR and 3D button displays if we switched to AR mode from the menu
            arBtn.SetActive(false);
            _3dBtn.SetActive(true);
        } else if (_3dBtn.activeSelf && !GameState.Instance.camerasManager.isAR()) {
            // Switch AR and 3D button displays if we switched to AR mode from the menu
            arBtn.SetActive(true);
            _3dBtn.SetActive(false);
        }
        */
    }

    void OnGUI ()
    {
        setupButtons ();
        setupLabels ();
        drawPlayerTotal ();
        drawBankerTotal ();
        drawTieTotal ();
        drawPlayerWins ();
        drawBankerWins ();
        drawTieWins ();
    }

    void drawPlayerTotal ()
    {
        // When a round is finished show a message in the middle of the player cards showing the total for the player cards
        if (isDisplayPlayerTotal && displayPlayerTotalMessageStartTime != 0 && Time.time - displayPlayerTotalMessageStartTime > (1.6f * DURATION_SHOW_CARD_TOTALS * Dealer.dealSpeed)) {
            stopIsDisplayPlayerTotal = true;
            displayPlayerTotalMessageStartTime = 0;
        }

        if (isDisplayPlayerTotal && !isDisplayPlayerWins
            && isDisplayBankerTotal) {
            if (displayPlayerTotalMessageStartTime == 0 && !stopIsDisplayPlayerTotal) {
                displayPlayerTotalMessageStartTime = Time.time;
            }

            // Setup GUI skin
            GUISkin origSkin = GUI.skin;
            GUI.skin = CustomizedColoredGuiStatsPanelSkin.Skin;
            GUILayout.BeginArea (new Rect (Screen.width / 2f - ((Screen.width * TOTAL_MSG_WIDTH) / 2f),
                                            (Screen.height / 2) - (1.1f * TOTAL_MSG_HEIGHT * Screen.height),
                                            TOTAL_MSG_WIDTH * Screen.width,
                                            2.2f * TOTAL_MSG_HEIGHT * Screen.height));
            GUILayout.BeginHorizontal ();
            int origFontSize = GUI.skin.label.fontSize;
            Color origFontColor = GUI.skin.label.normal.textColor;
            Color origGuiColor = GUI.color;

            // Black background box
            GUI.Box (new Rect (0,
                        0,
                        TOTAL_MSG_WIDTH * Screen.width,
                        2.2f * TOTAL_MSG_HEIGHT * Screen.height),
                        "");
            //Color darkerBg = new Color (origGuiColor.r, origGuiColor.g, origGuiColor.b, origGuiColor.a/2f);
            //GUI.color = darkerBg;

            // Player card value
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / 2f);
            GUI.skin.label.normal.textColor = isDisplayPlayerTotalHighlight ? Color.yellow : Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.Label (displayPlayerTotalStr);
            GUILayout.FlexibleSpace();

            // Restore original GUI skin properties
            GUILayout.EndHorizontal ();
            GUILayout.EndArea ();
            GUI.skin.label.fontSize = origFontSize;
            GUI.skin.label.normal.textColor = origFontColor;
            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUI.skin = origSkin;
        }
    }

    void drawBankerTotal ()
    {
        // When a round is finished show a message in the middle of the banker cards showing the total for the banker cards
        if (isDisplayBankerTotal && displayBankerTotalMessageStartTime != 0 && Time.time - displayBankerTotalMessageStartTime > (1.6f * DURATION_SHOW_CARD_TOTALS * Dealer.dealSpeed)) {
            stopIsDisplayBankerTotal = true;
            displayBankerTotalMessageStartTime = 0;
        }

        if (isDisplayBankerTotal && !isDisplayBankerWins
                && isDisplayPlayerTotal) {
            if (displayBankerTotalMessageStartTime == 0 && !stopIsDisplayBankerTotal) {
                displayBankerTotalMessageStartTime = Time.time;
            }

            // Setup GUI skin
            GUISkin origSkin = GUI.skin;
            GUI.skin = CustomizedColoredGuiStatsPanelSkin.Skin;
            GUILayout.BeginArea (new Rect (Screen.width / 2f - ((Screen.width * TOTAL_MSG_WIDTH) / 2f),
                                            Screen.height / 2,
                                            TOTAL_MSG_WIDTH * Screen.width,
                                            TOTAL_MSG_HEIGHT * Screen.height));
            GUILayout.BeginHorizontal ();
            int origFontSize = GUI.skin.label.fontSize;
            Color origFontColor = GUI.skin.label.normal.textColor;

            // Black background box
            /* COMMENTING OUT because the black box with two lines (one for player total, one for banker total below that) is now drawn in the drawPlayerTotal method)
            GUI.Box (new Rect (0,
                        0,
                        TOTAL_MSG_WIDTH * Screen.width,
                        TOTAL_MSG_HEIGHT * Screen.height),
                        "");
                        */

            // Banker card value
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / 2f);
            GUI.skin.label.normal.textColor = isDisplayBankerTotalHighlight ? Color.yellow : Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.Label (displayBankerTotalStr);
            GUILayout.FlexibleSpace();

            // Restore original GUI skin properties
            GUILayout.EndHorizontal ();
            GUILayout.EndArea ();
            GUI.skin.label.fontSize = origFontSize;
            GUI.skin.label.normal.textColor = origFontColor;
            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUI.skin = origSkin;
        }
    }

    void drawTieTotal ()
    {
        // When a round is finished show a message in the middle of the screen showing the total for the tie
        if (isDisplayTieTotal && displayTieTotalMessageStartTime != 0 && Time.time - displayTieTotalMessageStartTime > (DURATION_SHOW_CARD_TOTALS * Dealer.dealSpeed)) {
            stopIsDisplayTieTotal = true;
            displayTieTotalMessageStartTime = 0;
        }

        if (isDisplayTieTotal) { // && !isDisplayTieWins) {
            if (displayTieTotalMessageStartTime == 0 && !stopIsDisplayTieTotal) {
                displayTieTotalMessageStartTime = Time.time;
            }

            // Setup GUI skin
            GUISkin origSkin = GUI.skin;
            GUI.skin = CustomizedColoredGuiStatsPanelSkin.Skin;
            GUILayout.BeginArea (new Rect (Screen.width / 2f - ((Screen.width * TOTAL_MSG_WIDTH) / 2f),
                                            (Screen.height / 2) - (1.1f * TOTAL_MSG_HEIGHT * Screen.height),
                                            TOTAL_MSG_WIDTH * Screen.width,
                                            2.2f * TOTAL_MSG_HEIGHT * Screen.height));

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal ();
            int origFontSize = GUI.skin.label.fontSize;
            Color origFontColor = GUI.skin.label.normal.textColor;

            // Black background box
            GUI.Box (new Rect (0,
                        0,
                        TOTAL_MSG_WIDTH * Screen.width,
                        2.2f * TOTAL_MSG_HEIGHT * Screen.height),
                        "");

            // Tie value
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / 2f);
            GUI.skin.label.normal.textColor = isDisplayTieTotalHighlight ? Color.yellow : Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.Label (displayTieTotalStr);
            GUILayout.FlexibleSpace();

            // Restore original GUI skin properties
            GUILayout.EndHorizontal ();
            GUILayout.FlexibleSpace();
            GUILayout.EndArea ();
            GUI.skin.label.fontSize = origFontSize;
            GUI.skin.label.normal.textColor = origFontColor;
            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUI.skin = origSkin;
        }
    }

    void drawPlayerWins ()
    {
        if (isDisplayPlayerWins && displayWinMessageStartTime != 0 && Time.time - displayWinMessageStartTime > (DURATION_SHOW_CARD_RESULT * Dealer.dealSpeed)) {
            stopIsDisplayPlayerWins = true;
            displayWinMessageStartTime = 0;
        }

        if (isDisplayPlayerWins && !isDisplayPlayerTotal) {
            if (displayWinMessageStartTime == 0 && !stopIsDisplayPlayerWins) {
                displayWinMessageStartTime = Time.time;
            }

            // Display the winner trophy
            if (displayPlayerWinsStr.Contains("+")) // The customer won some amount
                GUI.DrawTexture (new Rect ((0.5f * Screen.width) - (WinsTrophy.width / WinsTrophyShrinkFactor / 2f),
                                        (Screen.height * 0.50f) - WinsTrophy.height / WinsTrophyShrinkFactor / 2f,
                                        WinsTrophy.width / WinsTrophyShrinkFactor,
                                        WinsTrophy.height / WinsTrophyShrinkFactor),
                                    WinsTrophy);

            // Setup GUI skin
            GUISkin origSkin = GUI.skin;
            GUI.skin = CustomizedColoredGuiStatsPanelSkin.Skin;
            GUILayout.BeginArea (new Rect (Screen.width / 2f - ((Screen.width * TOTAL_MSG_WIDTH) / 2f),
                                            (Screen.height / 2) - (1.1f * TOTAL_MSG_HEIGHT * Screen.height),
                                            TOTAL_MSG_WIDTH * Screen.width,
                                            2.2f * TOTAL_MSG_HEIGHT * Screen.height));
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal ();
            int origFontSize = GUI.skin.label.fontSize;
            Color origFontColor = GUI.skin.label.normal.textColor;
            Color origGuiColor = GUI.color;

            // Black background box
            GUI.Box (new Rect (0,
                        0,
                        TOTAL_MSG_WIDTH * Screen.width,
                        2.2f * TOTAL_MSG_HEIGHT * Screen.height),
                        "");
            //Color darkerBg = new Color (origGuiColor.r, origGuiColor.g, origGuiColor.b, origGuiColor.a/2f);
            //GUI.color = darkerBg;

            // Player card value
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / 2f);
            GUI.skin.label.normal.textColor = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.Label (displayPlayerWinsStr);
            GUILayout.FlexibleSpace();

            // Restore original GUI skin properties
            GUILayout.EndHorizontal ();
            GUILayout.FlexibleSpace();
            GUILayout.EndArea ();
            GUI.skin.label.fontSize = origFontSize;
            GUI.skin.label.normal.textColor = origFontColor;
            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUI.skin = origSkin;
        }
    }

    void drawBankerWins ()
    {
        if (isDisplayBankerWins && displayWinMessageStartTime != 0 && Time.time - displayWinMessageStartTime > (DURATION_SHOW_CARD_RESULT * Dealer.dealSpeed)) {
            stopIsDisplayBankerWins = true;
            displayWinMessageStartTime = 0;
        }

        if (isDisplayBankerWins && !isDisplayBankerTotal) {
            if (displayWinMessageStartTime == 0 && !stopIsDisplayBankerWins) {
                displayWinMessageStartTime = Time.time;
            }

            // Display the winner trophy if this method is also being used to show that the banker won
            if (displayBankerWinsStr.Contains("+")) // The customer won some amount
                GUI.DrawTexture (new Rect ((0.50f * Screen.width) - (WinsTrophy.width / WinsTrophyShrinkFactor / 2f),
                                            (Screen.height * 0.50f) - WinsTrophy.height / WinsTrophyShrinkFactor / 2f,
                                            WinsTrophy.width / WinsTrophyShrinkFactor,
                                            WinsTrophy.height / WinsTrophyShrinkFactor),
                                     WinsTrophy);

            // Setup GUI skin
            GUISkin origSkin = GUI.skin;
            GUI.skin = CustomizedColoredGuiStatsPanelSkin.Skin;
            GUILayout.BeginArea (new Rect (Screen.width / 2f - ((Screen.width * TOTAL_MSG_WIDTH) / 2f),
                                            (Screen.height / 2) - (1.1f * TOTAL_MSG_HEIGHT * Screen.height),
                                            TOTAL_MSG_WIDTH * Screen.width,
                                            2.2f * TOTAL_MSG_HEIGHT * Screen.height));
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal ();
            int origFontSize = GUI.skin.label.fontSize;
            Color origFontColor = GUI.skin.label.normal.textColor;

            // Black background box
            GUI.Box (new Rect (0,
                        0,
                        TOTAL_MSG_WIDTH * Screen.width,
                        2.2f * TOTAL_MSG_HEIGHT * Screen.height),
                        "");

            // Banker card value
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / 2f);
            GUI.skin.label.normal.textColor = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.Label (displayBankerWinsStr);
            GUILayout.FlexibleSpace();

            // Restore original GUI skin properties
            GUILayout.EndHorizontal ();
            GUILayout.FlexibleSpace();
            GUILayout.EndArea ();
            GUI.skin.label.fontSize = origFontSize;
            GUI.skin.label.normal.textColor = origFontColor;
            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUI.skin = origSkin;
        }
    }

    void drawTieWins ()
    {
        if (isDisplayTieWins && displayWinMessageStartTime != 0 && Time.time - displayWinMessageStartTime > (DURATION_SHOW_CARD_RESULT * Dealer.dealSpeed)) {
            stopIsDisplayTieWins = true;
            displayWinMessageStartTime = 0;
        }

        if (isDisplayTieWins && !isDisplayTieTotal) {
            if (displayWinMessageStartTime == 0 && !stopIsDisplayTieWins) {
                displayWinMessageStartTime = Time.time;
            }

            // Display the winner trophy
            if (displayTieWinsStr.Contains("+")) // The customer won some amount
                GUI.DrawTexture (new Rect ((Screen.width * 0.50f) - WinsTrophy.width / WinsTrophyShrinkFactor / 2f,
                                        (Screen.height * 0.50f) - WinsTrophy.height / WinsTrophyShrinkFactor / 2f,
                                        WinsTrophy.width / WinsTrophyShrinkFactor,
                                        WinsTrophy.height / WinsTrophyShrinkFactor),
                                     WinsTrophy);

            // Setup GUI skin
            GUISkin origSkin = GUI.skin;
            GUI.skin = CustomizedColoredGuiStatsPanelSkin.Skin;
            GUILayout.BeginArea (new Rect (Screen.width / 2f - ((Screen.width * TOTAL_MSG_WIDTH) / 2f),
                                            (Screen.height / 2) - (1.1f * TOTAL_MSG_HEIGHT * Screen.height),
                                            TOTAL_MSG_WIDTH * Screen.width,
                                            2.2f * TOTAL_MSG_HEIGHT * Screen.height));
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal ();
            int origFontSize = GUI.skin.label.fontSize;
            Color origFontColor = GUI.skin.label.normal.textColor;

            // Black background box
            GUI.Box (new Rect (0,
                        0,
                        TOTAL_MSG_WIDTH * Screen.width,
                        2.2f * TOTAL_MSG_HEIGHT * Screen.height),
                        "");

            // Tie value
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / 2f);
            GUI.skin.label.normal.textColor = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.Label (displayTieWinsStr);
            GUILayout.FlexibleSpace();

            // Restore original GUI skin properties
            GUILayout.EndHorizontal ();
            GUILayout.FlexibleSpace();
            GUILayout.EndArea ();
            GUI.skin.label.fontSize = origFontSize;
            GUI.skin.label.normal.textColor = origFontColor;
            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUI.skin = origSkin;
        }
    }


    // Function name is deal button but in the same location we also have return and rebet buttons
    public void setupDealButton ()
    {
        // Avoid unnecessary code calls every frame update
        if (dealButtonState != DealButtonState.Rebet && lastDealButtonState == dealButtonState)
            return;

        // DEAL | REBET | RETURN CARD button (bottom right)
        switch (dealButtonState) {
        case DealButtonState.ReturnCard:
//            if (GUI.Button (new Rect (Screen.width - BUTTON_WIDTH - PADDING_HORIZTONAL,
//                                      Screen.height - BUTTON_HEIGHT - PADDING_VERTICAL,
//                                      BUTTON_WIDTH,
//                                      BUTTON_HEIGHT),
//                            LanguageManager.GetText ("btn_return_card"),
//                            buttonStyle)) {
            revealReturnBtn.SetActive (true);
            dealBtn.SetActive (false);
            SetAR3dButtonActive (true);
                //rebetBtn.SetActive (false);
            revealReturnBtn.GetComponent<SingleCircularButton> ().callbackObject = this.gameObject;
            revealReturnBtn.GetComponent<SingleCircularButton> ().methodToInvoke = "returnCardCallback";
//            }
            break;

        case DealButtonState.Rebet:
            // Don't show button if there were no chips from the last betted round
            if (GameState.Instance.chipsManager.clearedChipsList.Count == 0) {
                break;
            }
            if (returnCardPressed) {
                returnCardPressed = false;
            }
//            if (GUI.Button (new Rect (Screen.width - BUTTON_WIDTH - PADDING_HORIZTONAL,
//                                  Screen.height - BUTTON_HEIGHT - PADDING_VERTICAL,
//                                  BUTTON_WIDTH,
//                                  BUTTON_HEIGHT),
//                        LanguageManager.GetText ("btn_rebet"),
//                        buttonStyle)) {
            GUISkin origSkin = GUI.skin;
            GUI.skin = CustomizedColoredGuiWindowSkin.Instance.actualSkin;
            int origFontSize = GUI.skin.label.fontSize;

            // Button to rebet chips from last round
            GUI.skin.button.fontSize = GUIControls.fontScaler / 4;
            if (GUI.Button (new Rect (Screen.width - rebetBtnWidth,
                                        Screen.height - rebetBtnHeight*3.5f,
                                        rebetBtnWidth,
                                        rebetBtnHeight),
                           LanguageManager.GetText ("btn_rebet"))) {
                // Hide the 'place bet tutorial'
                GameState.Instance.tutorialHelpManager.placeBets(false);
                
                // Check for sufficient balance
                if (GameState.Instance.chipsManager.clearedChipsTotalAmount > GameState.Instance.currentBalance) {
                    Debug.LogWarning ("Not enough money left to rebet. Last round was $"
                        + GameState.Instance.chipsManager.clearedChipsTotalAmount.ToString ("n0")
                        + ". Balance is $"
                        + GameState.Instance.currentBalance.ToString ("n0"));
                    GameState.Instance.guiControls.displayMessage (LanguageManager.GetText ("label_insufficient_balance"));
                    GameState.Instance.guiControls.dealButtonState = GUIControls.DealButtonState.Hide;
                    GameState.Instance.dealer.playInsuffcientFundsSound ();
#if !UNITY_WEBPLAYER
                    Handheld.Vibrate ();
#endif
                    break;
                } else {
                    // We have enough money to rebet

                    dealButtonState = DealButtonState.Deal;
    
                    //rebetBtn.SetActive (false);
                    dealBtn.SetActive (true);
                    SetAR3dButtonActive (true);
                    revealReturnBtn.SetActive (false);
                    //rebetBtn.GetComponent<SingleCircularButton> ().callbackObject = this.gameObject;
                    //rebetBtn.GetComponent<SingleCircularButton> ().methodToInvoke = "rebetCallback";
    
                    rebetCallback ();
                }
            }
            GUI.skin.button.fontSize = origFontSize;
            GUI.skin = origSkin;
            break;

        case DealButtonState.Hide:
            if (returnCardPressed) {
                returnCardPressed = false;
            }
            //rebetBtn.SetActive(false);
            dealBtn.SetActive (false);
            SetAR3dButtonActive (false);
            revealReturnBtn.SetActive (false);
            break;

        case DealButtonState.Deal:
            if (returnCardPressed) {
                returnCardPressed = false;
            }
//                if (GUI.Button (new Rect (Screen.width - BUTTON_WIDTH - PADDING_HORIZTONAL,
//                                      Screen.height - BUTTON_HEIGHT - PADDING_VERTICAL,
//                                      BUTTON_WIDTH,
//                                      BUTTON_HEIGHT),
//                            LanguageManager.GetText ("btn_deal"),
//                            buttonStyle)) {
//    
//                    // Tell the dealer to deal the cards
//                    GameState.Instance.currentState = GameState.State.DealCards;
//                }
            dealBtn.SetActive (true);
            SetAR3dButtonActive (true);
            //rebetBtn.SetActive(false);
            revealReturnBtn.SetActive (false);
            dealBtn.GetComponent<SingleCircularButton> ().callbackObject = this.gameObject;
            dealBtn.GetComponent<SingleCircularButton> ().methodToInvoke = "dealCallback";
            break;
        }

        lastDealButtonState = dealButtonState;
    }

    // Turn on the AR or 3D button (and gyrocam button)
    void SetAR3dButtonActive(bool enable) {
#if UNITY_WEBPLAYER
        return;
#endif

        if (GameState.Instance.tutorialCounter <= 1)
            return;

        if (GameState.Instance.camerasManager.isAR())
            _3dBtn.SetActive(enable);
        else
            arBtn.SetActive(enable);

        if (arBtn.activeSelf)
            gyroBtn.SetActive(true);
        else
            gyroBtn.SetActive(false);

    }

    // Toggle AR on
    public void arCallback ()
    {
#if UNITY_WEBPLAYER
        return;
#endif

        if (GameState.Instance.tutorialCounter <= 1)
            return;

        Debug.Log("Pressed AR button");
        LogUtils.LogEvent(Consts.FE_BTN_AR);
        gyroBtn.SetActive(false);
        arBtn.SetActive(false);
        _3dBtn.SetActive(true);

        GameState.Instance.camerasManager.ToggleAR (true, false, false);
        GameState.Instance.ToggleFingerGestures (true);
        GameObject speechBubble = GameState.Instance.tutorialHelpManager.arGuide(true);
        StartCoroutine(setSpeechBubbleResetDelayed(speechBubble));
    }

    // Toggle gyrocam on
    public void gyroCallback ()
    {
#if UNITY_WEBPLAYER
        return;
#endif

        if (GameState.Instance.tutorialCounter <= 1)
            return;

        Debug.Log("Pressed GyroCam button");
        LogUtils.LogEvent(Consts.FE_BTN_GYROCAM);
        gyroBtn.SetActive(false);
        arBtn.SetActive(false);
        _3dBtn.SetActive(true);

        GameState.Instance.camerasManager.ToggleGyroCamera (true, true); // TODO: force showing of casino room for meantime till we sort out real time web cam view
        GameState.Instance.ToggleFingerGestures (true);
        GameObject speechBubble = GameState.Instance.tutorialHelpManager.gryoCamGuide(true);
        StartCoroutine(setSpeechBubbleResetDelayed(speechBubble));
    }

    IEnumerator setSpeechBubbleResetDelayed(GameObject speechBubble) {
        yield return new WaitForSeconds(0.4f); // needed a little delay otherwise sometimes an extra tap would delete the speech bubble display before we even saw it...
        GameState.Instance.dealer.nextTapStopSpeechBubble = speechBubble;
        GameState.Instance.dealer.nextTapResetOnNext = true;
    }

    // Toggle AR off (back to normal 3D)
    public void _3dCallback ()
    {
#if UNITY_WEBPLAYER
        return;
#endif
        Debug.Log("Pressed 3D button");
        LogUtils.LogEvent(Consts.FE_BTN_3D);
        gyroBtn.SetActive(true);
        arBtn.SetActive(true);
        _3dBtn.SetActive(false);
        GameState.Instance.camerasManager.ToggleGyroCamera(false);
        GameState.Instance.camerasManager.ToggleAR(false);
    }

    public void returnCardCallback ()
    {
        // Let them see the tutorial till the end
        if (Card.isShowingTutorials && GameState.Instance.tutorialCounter <= 1)
            return;

        LogUtils.LogEvent(Consts.FE_BTN_RETURN_CARD);

        // Return the current displaying card to the dealer
        GameState.Instance.dealer.returnCurrentCard ();

        returnCardPressed = true;
    }

    public void rebetCallback ()
    {
        LogUtils.LogEvent(Consts.FE_BTN_REBET);
        GameState.Instance.chipsManager.rebet ();
    }

    public void dealCallback ()
    {
        // Let them see the tutorial till the end
        if (GameState.Instance.tutorialCounter <= 1 && (Chip.isShowingTutorials || Card.isShowingTutorials))
            return;

        LogUtils.LogEvent(Consts.FE_BTN_DEAL);

        // Hide tutorials
        GameState.Instance.tutorialHelpManager.fingers2Zoom(false);
        GameState.Instance.tutorialHelpManager.fingers3Swipe(false);
        GameState.Instance.tutorialHelpManager.clearBets(false);
        GameState.Instance.tutorialHelpManager.startDealing(false);

        // Tell the dealer to deal the cards
        GameState.Instance.currentState = GameState.State.DealCards;
    }

    // Function name is for the clear button but we also have the show left/right card buttons there too
    public void setupClearButton ()
    {
        // Avoid unnecessary code calls every frame update
        if (lastClearButtonState == clearButtonState && !GameState.Instance.camerasManager.hasCameraPositionChanged ())
            return;

        // CLEAR | OTHER CARD button (bottom left)
        switch (clearButtonState) {
        case ClearButtonState.OtherCard:
//                if (GUI.Button (new Rect (PADDING_HORIZTONAL,
//                                      Screen.height - BUTTON_HEIGHT - PADDING_VERTICAL,
//                                      BUTTON_WIDTH,
//                                      BUTTON_HEIGHT),
//                            LanguageManager.GetText ("btn_other_card"),
//                            buttonStyle)) {
            clearBtn.SetActive (false);

            // Determine to show left or right card move button
            if (!GameState.Instance.camerasManager.getSqueezeCameraCurrentPosition ().Equals ("right")) {
                // -> right arrow
                visibleCardMoveBtn = rightCardBtn;
                invisibleCardMoveBtn = leftCardBtn;
            } else {
                // <- left arrow
                visibleCardMoveBtn = leftCardBtn;
                invisibleCardMoveBtn = rightCardBtn;
            }

            if (!hideLeftRightCardButtonsBool) visibleCardMoveBtn.SetActive (true);
            invisibleCardMoveBtn.SetActive (false);
            break;

        case ClearButtonState.Hide:
            // Hide buttons
            clearBtn.SetActive (false);
            leftCardBtn.SetActive (false);
            rightCardBtn.SetActive (false);
            break;

        case ClearButtonState.Clear:
//                if (GUI.Button (new Rect (PADDING_HORIZTONAL,
//                                  Screen.height - BUTTON_HEIGHT - PADDING_VERTICAL,
//                                  BUTTON_WIDTH,
//                                  BUTTON_HEIGHT),
//                        LanguageManager.GetText ("btn_clear"),
//                        buttonStyle)) {
            hideLeftRightCardButtonsBool = false;
            clearBtn.SetActive (true);
            leftCardBtn.SetActive (false);
            rightCardBtn.SetActive (false);
            clearBtn.GetComponent<SingleCircularButton> ().callbackObject = this.gameObject;
            clearBtn.GetComponent<SingleCircularButton> ().methodToInvoke = "clearCallback";
//
//                }
            break;
        }

        lastClearButtonState = clearButtonState;
    }

    public void hideLeftRightCardButtons() {
        Debug.Log ("hideLeftRightCardButtons called"); 
        leftCardBtn.SetActive(false);
        rightCardBtn.SetActive(false);
        hideLeftRightCardButtonsBool = true;
    }

    public void showOtherCardCallback ()
    {
        // Let them see the tutorial till the end
        if (Card.isShowingTutorials && GameState.Instance.tutorialCounter <= 1)
            return;

        // Switch the squeeze camera to focus on the other card
        GameState.Instance.camerasManager.moveSqueezeCamera ("otherside", true);
    }

    public void clearCallback ()
    {
        // Let them see the tutorial till the end
        if (GameState.Instance.tutorialCounter <= 1 && (Chip.isShowingTutorials || Card.isShowingTutorials))
            return;

        LogUtils.LogEvent(Consts.FE_BTN_CLEAR_CHIP);

        // Hide tutorials
        GameState.Instance.tutorialHelpManager.fingers2Zoom(false);
        GameState.Instance.tutorialHelpManager.fingers3Swipe(false);
        GameState.Instance.tutorialHelpManager.clearBets(false);
        GameState.Instance.tutorialHelpManager.startDealing(false);

        // Clear all the currently bet chips on the table
        GameState.Instance.clearChips ();
    }

    void setupButtons ()
    {
        setupDealButton ();
        setupClearButton ();

        // Side menu buttons
//        GUILayout.BeginArea (new Rect (PADDING_HORIZTONAL,
//                                       PANEL_TOP_HEIGHT + PADDING_VERTICAL * 2,
//                                       BUTTON_WIDTH,
//                                       (BUTTON_HEIGHT + PADDING_VERTICAL) * 4)); // 4 = num of buttons
//        GUILayout.BeginVertical (buttonStyle);
//        if (GUILayout.Button (LanguageManager.GetText ("btn_menu"), buttonStyle)) {
//            Debug.Log ("Quitting Game");
//            GameState.Instance.dealer.vbye_bye.Play();
//#if (UNITY_ANDROID && !UNITY_EDITOR)
//            if (Consts.DEBUG) {
//                // MENU acts like Quit when its just the BaccARat 3D Unity project deployed to a device
//                Application.Quit();
//                return;
//            }
//
//            // Get Android activity context
//            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
//            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
//            Debug.Log("unityPlayer is " + unityPlayer);
//            Debug.Log("activity is " + activity);
//
//            // Quit the game and return to the main menu
//            Debug.Log("Quitting PlayScene and returning to MainMenu");
//            LoadNextLevel.nextLevelName = "MainMenuScene";
//            Application.LoadLevel("LoadNextLevel");
//            activity.Call("mainMenuOn");
//#else
//            Application.Quit ();
//#endif
//        }
//        if (GUILayout.Button (LanguageManager.GetText("btn_gryocam"), buttonStyle)) {
//            GameState.Instance.camerasManager.ToggleGyroCamera(!GameState.Instance.camerasManager.gyroCamera.enabled);
//        }
////        GUILayout.Button (LanguageManager.GetText ("btn_road"), buttonStyle);
////        GUILayout.Button (LanguageManager.GetText ("btn_invite"), buttonStyle);
//
//        if (GUILayout.Button (LanguageManager.GetText ("btn_leaderboard"), buttonStyle)) {
//            Debug.Log ("Facebook app token: " +  FacebookSNS.Instance().GetAppAccessToken());
//            Debug.Log ("test:Posting score " + GameState.Instance.currentBalance + " to Facebook");
//            //FacebookSNS.Instance().PostMessage("My bank balance is now $"+GameState.Instance.currentBalance, "http://www.aroha.mobi");
//            FacebookSNS.Instance().PostScore(GameState.Instance.currentBalance);
//            //FacebookSNS.Instance().PostScreenShot();
//        }
//        GUILayout.EndVertical ();
//        GUILayout.EndArea ();
    }

    void setupLabels ()
    {
        // Moved to CustomizedStatsPanel **

        /************************************* OLD CODE *************************************/
        // Top stats panel (top center)
//        GUILayout.BeginArea (new Rect (Screen.width / 2 - PANEL_TOP_WIDTH / 2 - PADDING_HORIZTONAL,
//        GUILayout.BeginArea (new Rect (PADDING_HORIZTONAL,
//                                       PADDING_VERTICAL,
//                                       PANEL_TOP_WIDTH,
//                                       PANEL_TOP_HEIGHT));
//        GUILayout.BeginHorizontal (panelTopStyle);
////        GUILayout.Label (LanguageManager.GetText ("label_rank") + " No. 1!!", panelTopStyle);
//        GUILayout.Label (LanguageManager.GetText ("label_player"), panelTopStyle);
//        GUILayout.Label (playerCardsValueText, panelTopStyle);
//        GUILayout.Label (LanguageManager.GetText ("label_banker"), panelTopStyle);
//        GUILayout.Label (bankerCardsValueText, panelTopStyle);
//        GUILayout.EndHorizontal ();
//        GUILayout.BeginHorizontal (messageStyle);
//        GUILayout.Label (message, messageStyle);
//        GUILayout.EndHorizontal ();
//        GUILayout.EndArea ();
//
//        // Current balance and total bet panel (bottom right)
//        GUILayout.BeginArea (new Rect (Screen.width - PADDING_HORIZTONAL - PANEL_SIDE_WIDTH,
//                                       Screen.height - PADDING_VERTICAL * 3 - BUTTON_HEIGHT - PANEL_SIDE_HEIGHT,
//                                       PANEL_SIDE_WIDTH,
//                                       PANEL_SIDE_HEIGHT));
//        GUILayout.BeginVertical (panelSideStyle);
//        GUILayout.Label (LanguageManager.GetText ("label_bet") + " ", panelSideStyle);
//        GUILayout.Label (currentBetText, panelSideStyle);
//        GUILayout.Label (LanguageManager.GetText ("label_balance") + " ", panelSideStyle);
//        GUILayout.Label (balanceText, panelSideStyle);
//        GUILayout.EndVertical ();
//        GUILayout.EndArea ();
        /************************************* OLD CODE *************************************/
    }

    // Clear the message after x seconds
    IEnumerator clearMessageCoroutine (float x)
    {
        yield return new WaitForSeconds(x);
        message = "";
        isDisplayingMessage = false;
    }

    // Display a message for a specified amount of time
    IEnumerator displayMessageCoroutine (string msg, float delay)
    {
        // "Queue" messages if we're currently displaying a message and the message strings aren't the same
        while (isDisplayingMessage && msg != message) {
            yield return new WaitForSeconds(0.2f);
        }

        message = msg;
        isDisplayingMessage = true;
        StartCoroutine (clearMessageCoroutine (delay));
    }

    // Display a message for a default amount of time
    public void displayMessage (string msg)
    {
        displayMessage (msg, 4.0f);
    }

    // Display a message for a specified amount of time
    public void displayMessage (string msg, float delay)
    {
        StartCoroutine (displayMessageCoroutine (msg, delay));
    }

    // Quick method by passing the dealer's logic to sum up hand totals and give real time display of values as cards revealed
    public int internalPlayerTotal = 0;
    public int internalBankerTotal = 0;

    public void AddCardValue (GameState.BetType betType, int amount)
    {
        Debug.Log ("AddCardValue:" + gameObject.name + ", betType:" + betType + ", amount:" + amount);
        if (betType == GameState.BetType.Player) {
            // Update player value display
            //Debug.Log ("   pre-add: internalPlayerTotal:" + internalPlayerTotal);
            internalPlayerTotal += amount;
            //Debug.Log ("   post-add: internalPlayerTotal:" + internalPlayerTotal);
            if (internalPlayerTotal > 9) {
                internalPlayerTotal %= 10;
            }
            playerCardsValueText = "" + internalPlayerTotal;
        } else if (betType == GameState.BetType.Banker) {
            // Update banker value display
            //Debug.Log ("   pre-add: internalBankerTotal:" + internalBankerTotal);
            internalBankerTotal += amount;
            //Debug.Log ("   post-add: internalBankerTotal:" + internalBankerTotal);
            if (internalBankerTotal > 9) {
                internalBankerTotal %= 10;
            }
            bankerCardsValueText = "" + internalBankerTotal;
        }
    }

    // Close the circular menu and show the stats panel on its close
    public void closeCircularMenu ()
    {
        closeCircularMenu (true);
    }

    // Close the circular menu
    public void closeCircularMenu (bool showStatsPanelOnClose)
    {
        CircularMenu cm = circularMenu.GetComponent<CircularMenu> ();
        cm.HideMenu ();
        circularMenu.GetComponentInChildren<MenuSounds> ().menuClose ();
        if (showStatsPanelOnClose)
            statsPanel.GetComponent<CustomizedStatsPanel> ().showPanelOnMenuClose ();
    }

    // Completely show/hide the stats panel
    public void ShowHideStatsPanelCompletely (bool show)
    {
        Debug.Log ("Completely showing stats panel? " + show);
        if (show)
            statsPanel.GetComponent<CustomizedStatsPanel> ().showPanelOnMenuClose ();
        else
            statsPanel.GetComponent<CustomizedStatsPanel> ().hidePanelOnMenuOpen ();
    }

    // Invite friends on Facebook
    public void openInvite ()
    {
        Debug.Log ("Opening invite");
        LogUtils.LogEvent(Consts.FE_BTN_MAIN_MENU_EVENT, new string[] { Consts.FEP_BTN_MENU_INVITE_FRIENDS }, false);

        // Open Facebook list of friends and close the circular menu
        closeCircularMenu ();
    }

    public void openShop ()
    {
        Debug.Log ("Opening Shop Manager");
        LogUtils.LogEvent(Consts.FE_BTN_MAIN_MENU_EVENT, new string[] { Consts.FEP_BTN_MENU_SHOP }, false);

        // Open ShopManger
        shop.GetComponent<ShopManager> ().ToggleOn ();

        if (shop.activeSelf) {
            GameState.Instance.ToggleFingerGestures (false);
        } else {
            GameState.Instance.ToggleFingerGestures (true);
        }

        closeCircularMenu ();
    }

    // Go home (main menu)
    public void openHome ()
    {
        Debug.Log ("Opening home (main menu) (qutting)");
        LogUtils.LogEvent(Consts.FE_BTN_MAIN_MENU_EVENT, new string[] { Consts.FEP_BTN_MENU_EXIT_GAME }, false);
        StartCoroutine (openHomeCoroutine ());
        closeCircularMenu (false);
    }

    IEnumerator openHomeCoroutine ()
    {
        // Say bye bye and bow the dealers header
        GameState.Instance.dealer.vbye_bye.Play ();
        GameState.Instance.dealer.bowOn ();
        yield return new WaitForSeconds(GameState.Instance.dealer.vbye_bye.clip.length);
        GameState.Instance.dealer.bowOff ();
#if (UNITY_ANDROID && !UNITY_EDITOR)
        //if (Consts.DEBUG) {
            // MENU acts like Quit when its just the BaccARat 3D Unity project deployed to a device
            Application.Quit();
        //}
       /*
        // Get Android activity context
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        Debug.Log("unityPlayer is " + unityPlayer);
        Debug.Log("activity is " + activity);

        // Quit the game and return to the main menu
        Debug.Log("Quitting PlayScene and returning to MainMenu");
        LoadNextLevel.nextLevelName = "MainMenuScene";
        Application.LoadLevel("LoadNextLevel");
        activity.Call("mainMenuOn");
        */
#else
        Application.Quit ();
#endif
    }


    // Post score to Facebook/view leaderboard (scoreboard)
    public void openScore ()
    {
        Debug.Log ("Opening scoreboard");
        LogUtils.LogEvent(Consts.FE_BTN_MAIN_MENU_EVENT, new string[] { Consts.FEP_BTN_MENU_LEADERBOARD }, false);

        // Open Facebook list of friends (ranking/scoreboard/leaderboard)
        leaderboard.GetComponent<Leaderboard> ().ToggleOn ();

        if (leaderboard.activeSelf) {
            GameState.Instance.ToggleFingerGestures (false);
        } else {
            GameState.Instance.ToggleFingerGestures (true);
        }

        closeCircularMenu ();
    }

    public void toggleMusic() {
        LogUtils.LogEvent(Consts.FE_BTN_MAIN_MENU_EVENT, new string[] { Consts.FEP_BTN_MENU_MUSIC }, false);

        if (GameState.Instance.bgm == null)
            return;

        AudioSource audioSource = GameState.Instance.bgm.GetComponent<AudioSource>();
        if (audioSource.isPlaying) {
            Debug.Log ("Toggle off BGM");
            audioSource.Stop();
            PlayerPrefs.SetInt("BGM", 0);
        }
        else
        {
            Debug.Log ("Toggle on BGM");
            audioSource.Play();
            PlayerPrefs.SetInt("BGM", 1);
        }
    }

    // COMMENTED OUT because now have AR/eye buttons
    /*
    // Gyro Cam (changed to AR Menu)
    public void openGCam ()
    {
        Debug.Log ("Opening G-Gam (AR Menu)");

        // Open AR Menu
        arMenu.GetComponent<ARMenu> ().ToggleOn ();

        if (arMenu.activeSelf) {
            GameState.Instance.ToggleFingerGestures (false);
        } else {
            GameState.Instance.ToggleFingerGestures (true);
        }

        closeCircularMenu ();
    }
    */

    // Open Help
    public void openHelp ()
    {
        Debug.Log ("Opening help");
        LogUtils.LogEvent(Consts.FE_BTN_MAIN_MENU_EVENT, new string[] { Consts.FEP_BTN_MENU_HELP }, false);

        // Open TutorialHelpManager
        help.GetComponent<TutorialHelpManager> ().ToggleOn ();

        if (help.activeSelf) {
            GameState.Instance.ToggleFingerGestures (false);
        } else {
            GameState.Instance.ToggleFingerGestures (true);
        }

        closeCircularMenu ();
    }

    // Show/hide the button to flip over the other hands cards (i.e. opposite to the hand you bet on)
    public void ToggleRevealOtherButton (bool enable)
    {
        // Don't turn it on if we're in tutorial mode
        if (GameState.Instance.tutorialCounter <= 1 && enable)
            return;

        if (GameState.Instance.getCurrentBetType () == GameState.BetType.Banker) {
            Debug.Log ("Toggling reveal other (player cards) button: " + enable);
            revealPlayerOtherBtn.SetActive (enable);
            revealPlayerOtherBtn.GetComponent<SingleCircularButton> ().callbackObject = this.gameObject;
            revealPlayerOtherBtn.GetComponent<SingleCircularButton> ().methodToInvoke = "revealOtherCards";
        } else if (GameState.Instance.getCurrentBetType () == GameState.BetType.Player) {
            Debug.Log ("Toggling reveal other (banker cards) button: " + enable);
            revealBankerOtherBtn.SetActive (enable);
            revealBankerOtherBtn.GetComponent<SingleCircularButton> ().callbackObject = this.gameObject;
            revealBankerOtherBtn.GetComponent<SingleCircularButton> ().methodToInvoke = "revealOtherCards";
        }
    }

    public void revealOtherCards ()
    {
        Debug.Log ("Revealing other hands cards upon user pressing the flip button");
        GameState.Instance.dealer.revealOtherCards (true);
        revealPlayerOtherBtn.SetActive (false);
        revealBankerOtherBtn.SetActive (false);
    }

    bool isDisplayPlayerTotal = false;
    bool isDisplayPlayerTotalHighlight = false;
    string displayPlayerTotalStr = "";

    public void displayPlayerTotal (string msg, bool highlight)
    {
        if (msg == null || msg == "") {
            displayPlayerTotalMessageStartTime = 0;
            displayPlayerTotalStr = "";
            isDisplayPlayerTotal = false;
            return;
        }
        isDisplayPlayerTotal = true;
        isDisplayPlayerTotalHighlight = highlight;
        displayPlayerTotalStr = msg;
    }

    bool isDisplayBankerTotal = false;
    bool isDisplayBankerTotalHighlight = false;
    string displayBankerTotalStr = "";

    public void displayBankerTotal (string msg, bool highlight)
    {
        if (msg == null || msg == "") {
            displayBankerTotalMessageStartTime = 0;
            displayBankerTotalStr = "";
            isDisplayBankerTotal = false;
            return;
        }
        isDisplayBankerTotal = true;
        isDisplayBankerTotalHighlight = highlight;
        displayBankerTotalStr = msg;
    }

    bool isDisplayTieTotal = false;
    bool isDisplayTieTotalHighlight = false;
    string displayTieTotalStr = "";

    public void displayTieTotal (string msg, bool highlight)
    {
        // Clear player and banker totals
        displayPlayerTotalMessageStartTime = 0;
        displayPlayerTotalStr = "";
        isDisplayPlayerTotal = false;
        displayBankerTotalMessageStartTime = 0;
        displayBankerTotalStr = "";
        isDisplayBankerTotal = false;

        isDisplayTieTotal = true;
        isDisplayTieTotalHighlight = highlight;
        displayTieTotalStr = msg;
    }

    bool isDisplayPlayerWins = false;
    string displayPlayerWinsStr = "";

    public void displayPlayerWins (string msg)
    {
        isDisplayPlayerWins = true;
        displayPlayerWinsStr = msg;
    }

    bool isDisplayBankerWins = false;
    string displayBankerWinsStr = "";

    public void displayBankerWins (string msg)
    {
        isDisplayBankerWins = true;
        displayBankerWinsStr = msg;
    }

    bool isDisplayTieWins = false;
    string displayTieWinsStr = "";

    public void displayTieWins (string msg)
    {
        isDisplayTieWins = true;
        displayTieWinsStr = msg;
    }

    public Texture WinsTrophy;
    private float WinsTrophyShrinkFactor {
        get {
            return isPortrait ? 1f : 2f;
        }
    }

   // Turn on/off screen auto orientaiton rotation changes
   public static void SetAutoRotate(bool val) {
        if (GameState.Instance.camerasManager.arCamera.activeSelf
            || GameState.Instance.camerasManager.gyroCamera.enabled)
            return;

        Debug.Log ("Turning auto screen rotation on/off: " + val);
        if (!val)
            Screen.orientation = ScreenOrientation.Portrait;
        else
            Screen.orientation = ScreenOrientation.AutoRotation;
   }
}
