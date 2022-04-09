using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Help dialog
public class TutorialHelpManager : MonoBehaviour
{
    #region variables

    // Dimensions
    static float BTN_WIDTH = 0.95f;
    static float BTN_HEIGHT = 0.11f;

    public GUISkin scrollBarSkin;
    bool scrollFlurrified = false;
    Vector2 scrollPosition_ = Vector2.zero;
    Vector2 scrollPosition {
        get {
            return scrollPosition_;
        }
        set {
            if (scrollPosition_ != Vector2.zero && !scrollFlurrified) {
                LogUtils.LogEvent(Consts.FE_TUTORIAL_HELP_SCROLL);
                scrollFlurrified = true;
            }

            scrollPosition_ = value;
        }
    }

    // GameObjects which show speech bubbles for the tutorial
    public GameObject speechBubble3FingerSwipe;
    public GameObject speechBubble2FingerZoom;
    public GameObject speechBubbleClearBets;
    public GameObject speechBubblePlaceBets;
    public GameObject speechBubbleStartDealing;
    public GameObject speechBubbleReturnCard;
    public GameObject speechBubbleOtherCard;
    public GameObject speechBubbleStartSqueezing;
    public GameObject speechBubbleSqueezeOneFinger;
    public GameObject speechBubbleSqueezeTwoFingers;
    public GameObject speechBubbleSqueezeRotateTap;
    public GameObject speechBubbleSqueezePinchTwist;
    public GameObject speechBubbleARGuide;
    public GameObject speechBubbleARGuideNoBonus_forIOS;
    public GameObject speechBubbleGyroCamGuide;


    #endregion

    public void Start() {
        // Register ourselves with the GameState instance
        GameState.Instance.tutorialHelpManager = this;

        // Show the Help screen on game startup if the first time
        if (GameState.Instance.tutorialCounter <= 1) {
            GameState.Instance.ToggleFingerGestures(false);
            ToggleOn();
        }
    }

    public void ToggleOn() {
       Debug.Log("Toggling TutorialHelpManager isDrawGui from " + GetComponent<CustomizedCenteredGuiWindow>().isDrawGui + " to " + !GetComponent<CustomizedCenteredGuiWindow>().isDrawGui);
       if (!GetComponent<CustomizedCenteredGuiWindow>().isDrawGui) {
            GetComponent<CustomizedCenteredGuiWindow>().normalizedHeight = 0.75f;
       }
       GetComponent<CustomizedCenteredGuiWindow>().isDrawGui = !GetComponent<CustomizedCenteredGuiWindow>().isDrawGui;

        if (GetComponent<CustomizedCenteredGuiWindow>().isDrawGui)
            LogUtils.LogEvent(Consts.FE_TUTORIAL_SCREEN_TOGGLED_ON);
        else
            LogUtils.LogEvent(Consts.FE_TUTORIAL_SCREEN_TOGGLED_OFF);

        // Disable auto orientation rotations if we're showing the help screen
        if (GetComponent<CustomizedCenteredGuiWindow>().isDrawGui)
            GUIControls.SetAutoRotate(false);
        else
            GUIControls.SetAutoRotate(true);

        // Hide the stats panel when showing any centered GUI window, and show on vice versa
        if (GetComponent<CustomizedCenteredGuiWindow>().isDrawGui) {
            // This doesn't actually close it (haha I'll let you try and recall why, just know that it closes elsewhere. Hint: order!)
            GameState.Instance.guiControls.statsPanel.GetComponent<CustomizedStatsPanel> ().hidePanelOnMenuOpen ();
        } else {
            GameState.Instance.guiControls.statsPanel.GetComponent<CustomizedStatsPanel> ().showPanelOnMenuClose ();
        }
    }

    // Return whether the screen is open or not
    public bool isShowing {
        get {
            return GetComponent<CustomizedCenteredGuiWindow>().isDrawGui;
        }
    }

    public void OnGUICallback (string[] args)
    {
        // Extract string args and convert to ints (passed from SendMessage call)
        float width = float.Parse (args [0]);
        float height = float.Parse (args [1]);

        float buttonWidth = width * BTN_WIDTH;
        float buttonHeight = height * BTN_HEIGHT;

        GUILayout.BeginArea (new Rect (0, 0, width, height));
        GUILayout.FlexibleSpace ();
        GUILayout.BeginHorizontal ();
        GUILayout.FlexibleSpace ();
        GUILayout.BeginVertical ();
        GUI.skin.button.fontStyle = FontStyle.Bold;

        // Remember the original style states
        Color origTextColor = GUI.skin.button.active.textColor;
        int origFontSize = GUI.skin.label.fontSize;

        // Window title font size
        GUI.skin.window.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / 6f);
        GUI.skin.window.fontStyle = FontStyle.Bold;

        // GUI label
        GUI.skin.label.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / 4.5f);
        GUI.skin.label.normal.background = null;
        GUI.skin.label.margin.top = GUI.skin.label.margin.bottom = 0;
        GUI.skin.label.padding.top = GUI.skin.label.padding.bottom = 0;
        GUISkin origSkin = GUI.skin;
        scrollBarSkin.label = GUI.skin.label;
        float scrollBarThickness = Screen.width * 0.06f;
        scrollBarSkin.verticalScrollbarThumb.fixedWidth = scrollBarThickness;
        scrollBarSkin.verticalScrollbar.fixedWidth = scrollBarThickness;
        scrollBarSkin.verticalScrollbar.fixedHeight = height * 0.30f;
        scrollBarSkin.verticalScrollbarThumb.fixedHeight = Screen.height * 0.05f;
        scrollBarSkin.label.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / 3.9f);
        scrollBarSkin.horizontalScrollbarThumb.normal.background = scrollBarSkin.horizontalScrollbarThumb.active.background = null;
        scrollBarSkin.horizontalScrollbar.normal.background = scrollBarSkin.horizontalScrollbarThumb.active.background = null;
        GUI.skin = scrollBarSkin;
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.Height(height*0.5f));
        if (Utils.isJapanese()) {
            GUILayout.Label(LanguageManager.GetText("label_help_1"));
            GUILayout.Label(LanguageManager.GetText("label_help_2"));
            GUILayout.Label(LanguageManager.GetText("label_help_3"));
            GUILayout.Label(" ");

            GUI.skin.label.active.textColor = GUI.skin.label.normal.textColor = Color.white;
            GUILayout.Label(LanguageManager.GetText("label_help_4"));
            GUI.skin.label.active.textColor = GUI.skin.label.normal.textColor = origTextColor;

            GUILayout.Label(LanguageManager.GetText("label_help_5"));
            GUILayout.Label(LanguageManager.GetText("label_help_6"));
            GUILayout.Label(LanguageManager.GetText("label_help_7"));
            GUILayout.Label(LanguageManager.GetText("label_help_8"));
            GUILayout.Label(LanguageManager.GetText("label_help_9"));
            GUILayout.Label(" ");

            GUI.skin.label.active.textColor = GUI.skin.label.normal.textColor = Color.white;
            GUILayout.Label(LanguageManager.GetText("label_help_10"));
            GUI.skin.label.active.textColor = GUI.skin.label.normal.textColor = origTextColor;

            GUILayout.Label(LanguageManager.GetText("label_help_11"));
            GUILayout.Label(LanguageManager.GetText("label_help_12"));
            GUILayout.Label(LanguageManager.GetText("label_help_13"));
            GUILayout.Label(LanguageManager.GetText("label_help_14"));
            GUILayout.Label(LanguageManager.GetText("label_help_15"));
            GUILayout.Label(" ");
            GUILayout.Label(LanguageManager.GetText("label_help_17"));
            GUILayout.Label(LanguageManager.GetText("label_help_18"));
            GUILayout.Label(LanguageManager.GetText("label_help_19"));
            GUILayout.Label(" ");

            GUI.skin.label.active.textColor = GUI.skin.label.normal.textColor = Color.white;
            GUILayout.Label(LanguageManager.GetText("label_help_20"));
            GUI.skin.label.active.textColor = GUI.skin.label.normal.textColor = origTextColor;

            GUILayout.Label(LanguageManager.GetText("label_help_21"));
            GUILayout.Label(LanguageManager.GetText("label_help_22"));
            GUILayout.Label(LanguageManager.GetText("label_help_23"));
            GUILayout.Label(LanguageManager.GetText("label_help_24"));
            GUILayout.Label(" ");

#if !UNITY_WEBPLAYER // AR
            GUI.skin.label.active.textColor = GUI.skin.label.normal.textColor = Color.white;
            GUILayout.Label(LanguageManager.GetText("label_help_25"));
            GUI.skin.label.active.textColor = GUI.skin.label.normal.textColor = origTextColor;

            GUILayout.Label(LanguageManager.GetText("label_help_26"));
            GUILayout.Label(LanguageManager.GetText("label_help_27"));
            GUILayout.Label(" ");
#endif
            GUILayout.Label(" ");
            GUILayout.Label(LanguageManager.GetText("label_help_29"));
            GUILayout.Label(LanguageManager.GetText("label_help_30"));
            GUILayout.Label(LanguageManager.GetText("label_help_31"));
            GUILayout.Label(" ");
        } else {
            GUILayout.Label(" ");

            GUI.skin.label.active.textColor = GUI.skin.label.normal.textColor = Color.white;
            GUILayout.Label(LanguageManager.GetText("label_help_0a"));
            GUI.skin.label.active.textColor = GUI.skin.label.normal.textColor = origTextColor;

            GUILayout.Label(LanguageManager.GetText("label_help_1"));
            GUILayout.Label(LanguageManager.GetText("label_help_2"));
            GUILayout.Label(LanguageManager.GetText("label_help_3"));
            GUILayout.Label(LanguageManager.GetText("label_help_4"));
            GUILayout.Label(LanguageManager.GetText("label_help_5"));
            GUILayout.Label(LanguageManager.GetText("label_help_6"));
            GUILayout.Label(LanguageManager.GetText("label_help_7"));
            GUILayout.Label(LanguageManager.GetText("label_help_8"));
            GUILayout.Label(" ");

            GUI.skin.label.active.textColor = GUI.skin.label.normal.textColor = Color.white;
            GUILayout.Label(LanguageManager.GetText("label_help_9"));
            GUI.skin.label.active.textColor = GUI.skin.label.normal.textColor = origTextColor;

            GUILayout.Label(LanguageManager.GetText("label_help_10"));
            GUILayout.Label(LanguageManager.GetText("label_help_11"));
            GUILayout.Label(LanguageManager.GetText("label_help_12"));
            GUILayout.Label(LanguageManager.GetText("label_help_13"));
            GUILayout.Label(LanguageManager.GetText("label_help_14"));
            GUILayout.Label(LanguageManager.GetText("label_help_15"));
            GUILayout.Label(LanguageManager.GetText("label_help_16"));
            GUILayout.Label(" ");
            GUILayout.Label(LanguageManager.GetText("label_help_18"));
            GUILayout.Label(LanguageManager.GetText("label_help_19"));
            GUILayout.Label(LanguageManager.GetText("label_help_20"));
            GUILayout.Label(LanguageManager.GetText("label_help_21"));
            GUILayout.Label(LanguageManager.GetText("label_help_22"));
            GUILayout.Label(LanguageManager.GetText("label_help_23"));
            GUILayout.Label(" ");

            GUI.skin.label.active.textColor = GUI.skin.label.normal.textColor = Color.white;
            GUILayout.Label(LanguageManager.GetText("label_help_24"));
            GUI.skin.label.active.textColor = GUI.skin.label.normal.textColor = origTextColor;

            GUILayout.Label(LanguageManager.GetText("label_help_25"));
            GUILayout.Label(LanguageManager.GetText("label_help_26"));
            GUILayout.Label(LanguageManager.GetText("label_help_27"));
            GUILayout.Label(LanguageManager.GetText("label_help_28"));
            GUILayout.Label(LanguageManager.GetText("label_help_29"));
            GUILayout.Label(LanguageManager.GetText("label_help_30"));
            GUILayout.Label(" ");

#if !UNITY_WEBPLAYER // AR
            GUI.skin.label.active.textColor = GUI.skin.label.normal.textColor = Color.white;
            GUILayout.Label(LanguageManager.GetText("label_help_31"));
            GUI.skin.label.active.textColor = GUI.skin.label.normal.textColor = origTextColor;

            GUILayout.Label(LanguageManager.GetText("label_help_32"));
            GUILayout.Label(LanguageManager.GetText("label_help_33"));
            GUILayout.Label(LanguageManager.GetText("label_help_34"));
            GUILayout.Label(" ");
#endif
            GUILayout.Label(" ");
            GUILayout.Label(LanguageManager.GetText("label_help_36"));
            GUILayout.Label(LanguageManager.GetText("label_help_37"));
            GUILayout.Label(LanguageManager.GetText("label_help_38"));
            GUILayout.Label(" ");
        }
        GUILayout.EndScrollView();
        GUI.skin = origSkin;

        // More help info button
        GUI.skin.button.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / 3.3f);
        GUI.skin.button.active.textColor = GUI.skin.button.normal.textColor = Color.blue;
        if (GUILayout.Button (LanguageManager.GetText("btn_more_help"), GUILayout.Width (buttonWidth), GUILayout.Height (buttonHeight))) {
            LogUtils.LogEvent(Consts.FE_BTN_HELP_HOW_TO_SQUEEZE);
            if (Utils.isJapanese()) {
                Debug.Log ("Opening Japanese how-to-squeeze page");
                Application.OpenURL ("http://resocasi.com/game/detail?id=24");
            } else {
                Debug.Log ("Opening English how-to-squeeze page");
                Application.OpenURL ("http://www.worldgamingmag.com/en/gaming/baccarat/item/90-baccarat-squeeze-play");
            }
            GameState.Instance.ToggleFingerGestures (true);
        }
        GUI.skin.button.active.textColor = GUI.skin.button.normal.textColor = origTextColor;

        GUILayout.BeginHorizontal ();

        // Beginners mode
        GUI.skin.button.active.textColor = GUI.skin.button.normal.textColor = Color.yellow;
        if (GUILayout.Button (LanguageManager.GetText("btn_reenable_tutorials"), GUILayout.Width (buttonWidth*0.99f/2), GUILayout.Height (buttonHeight))) {
            LogUtils.LogEvent(Consts.FE_BTN_HELP_TUTORIAL);
            GameState.Instance.ToggleFingerGestures (true);
            ToggleOn();

            if (GameState.Instance.guiControls.dealButtonState == GUIControls.DealButtonState.Rebet) {
                // If we open the tutorial again while rebet is displaying then the game gets stuck
                GameState.Instance.guiControls.dealButtonState = GUIControls.DealButtonState.Hide;
            }

            if (GameState.Instance.tutorialCounter > 1) {
                // Set the tutorial counter to 1 so it will show the tutorials once more but the user can press the buttons
                // during squeezing to continue if they wish (which isn't allowed the very first time so we can guarantee
                // they see all the tutorials at least once.
                GameState.Instance.tutorialCounter = 1;
            }

            // Tutorial stuff
            if (GameState.Instance.tutorialCounter <= 1) {
                // Turn off screen auto orientation rotation changes
                GUIControls.SetAutoRotate(false);

                // Turn on the special main camera needed for showing the speech bubbles
                GameState.Instance.camerasManager.ToggleSpeechBubbleCamera(true);

                // Disable AR/3D/GyroCam buttons during tutorials
                GameState.Instance.guiControls.arBtn.SetActive(false);
                GameState.Instance.guiControls._3dBtn.SetActive(false);
                GameState.Instance.guiControls.gyroBtn.SetActive(false);
                GameState.Instance.camerasManager.ToggleAR(false);
                GameState.Instance.camerasManager.ToggleGyroCamera(false);

                // Show the first speech bubble (how to place bets)
                if (GameState.Instance.getCurrentBetValue() <= 0) { // if no bets on table yet
                    Chip.isShowingTutorials = true;
                    GameObject speechBubble = GameState.Instance.tutorialHelpManager.placeBets(true);
                    StartCoroutine(setSpeechBubbleResetDelayed(speechBubble, GameState.Instance.dealer.gameObject, "showTutorialChipMove"));
                } else {
                    Chip.isShowingTutorials = false;
                }
            }
        }

        // Tutorial video
        GUI.skin.button.active.textColor = GUI.skin.button.normal.textColor = Color.blue;
        if (GUILayout.Button (LanguageManager.GetText("btn_tutorial_video"), GUILayout.Width (buttonWidth*0.99f/2), GUILayout.Height (buttonHeight))) {
            LogUtils.LogEvent(Consts.FE_BTN_HELP_VIDEO);
            GameState.Instance.ToggleFingerGestures (true);

            if (Utils.isJapanese()) {
                Debug.Log ("Opening Japanese YouTube tutorial video");
                Application.OpenURL ("http://www.youtube.com/watch?v=0g6vutX849Q");
            } else {
                Debug.Log ("Opening English YouTube tutorial video");
                Application.OpenURL ("https://www.youtube.com/watch?v=Mc3i-VDUVQA");
            }
        }
        GUI.skin.button.active.textColor = GUI.skin.button.normal.textColor = origTextColor;

        GUILayout.EndHorizontal();

        // Play the game now!
        GUI.skin.button.active.textColor = GUI.skin.button.normal.textColor = Color.white;
        GUI.skin.button.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / 2.5f);
        if (GUILayout.Button (LanguageManager.GetText ("btn_play"), GUILayout.Width (buttonWidth), GUILayout.Height (buttonHeight))) {
            LogUtils.LogEvent(Consts.FE_BTN_HELP_PLAY_NOW);
            GameState.Instance.ToggleFingerGestures (true);
            ToggleOn();

            // Tutorial stuff
            if (GameState.Instance.tutorialCounter <= 1) {
                // Turn off screen auto orientation rotation changes
                GUIControls.SetAutoRotate(false);

                // Turn on the special main camera needed for showing the speech bubbles
                GameState.Instance.camerasManager.ToggleSpeechBubbleCamera(true);

                // Disable AR/3D/GyroCam buttons during tutorials
                GameState.Instance.guiControls.arBtn.SetActive(false);
                GameState.Instance.guiControls._3dBtn.SetActive(false);
                GameState.Instance.guiControls.gyroBtn.SetActive(false);
                GameState.Instance.camerasManager.ToggleAR(false);
                GameState.Instance.camerasManager.ToggleGyroCamera(false);

                // Show the first speech bubble (how to place bets)
                if (GameState.Instance.getCurrentBetValue() <= 0) { // if no bets on table yet
                    Chip.isShowingTutorials = true;
                    GameObject speechBubble = GameState.Instance.tutorialHelpManager.placeBets(true);
                    StartCoroutine(setSpeechBubbleResetDelayed(speechBubble, GameState.Instance.dealer.gameObject, "showTutorialChipMove"));
                } else {
                    Chip.isShowingTutorials = false;
                }
            }
        }
        GUI.skin.button.active.textColor = GUI.skin.button.normal.textColor = origTextColor;

        GUILayout.EndVertical ();
        GUILayout.FlexibleSpace ();
        GUILayout.EndHorizontal ();
        GUILayout.FlexibleSpace ();
        GUILayout.EndArea ();
    }

    IEnumerator setSpeechBubbleResetDelayed(GameObject speechBubble, GameObject target, string method) {
        yield return new WaitForSeconds(0.4f); // needed a little delay otherwise sometimes an extra tap would delete the speech bubble display before we even saw it...
        GameState.Instance.dealer.nextTapObject = target;
        GameState.Instance.dealer.nextTapMethodName = method;
        GameState.Instance.dealer.nextTapStopSpeechBubble = speechBubble;
        GameState.Instance.dealer.nextTapResetOnNext = true;
    }

    /**
     * Show/hide the '2 finger zoom' speech bubble
     */
    public GameObject fingers2Zoom(bool enable) {
        if (GameState.Instance.tutorialCounter > 1)
            return null;
        if (speechBubble2FingerZoom != null && enable) {
            speechBubble2FingerZoom.tag = "sa";
            Debug.Log ("Showing '2 finger zoom' speech bubble");
            LogUtils.LogEvent(Consts.FE_TUTORIAL_SHOWED_TWO_FINGER_ZOOM);
            speechBubble2FingerZoom.SetActive(enable);
        } else if (speechBubble2FingerZoom != null) {
            speechBubble2FingerZoom.tag = "d";
        }
        return speechBubble2FingerZoom;
    }

    /**
     * Show/hide the '3 finger swipe' speech bubble
     */
    public GameObject fingers3Swipe(bool enable) {
        if (GameState.Instance.tutorialCounter > 1)
            return null;
        if (speechBubble3FingerSwipe != null && enable) {
            speechBubble3FingerSwipe.tag = "sa";
            Debug.Log ("Showing '3 finger swipe' speech bubble");
            LogUtils.LogEvent(Consts.FE_TUTORIAL_SHOWED_THREE_FINGER_SWIPE);
            speechBubble3FingerSwipe.SetActive(enable);
        } else if (speechBubble3FingerSwipe != null) {
            speechBubble3FingerSwipe.tag = "d";
        }
        return speechBubble3FingerSwipe;
    }

    /**
     * Show/hide the first speech bubble tutorial - place bets (move chips)
     */
    public GameObject placeBets(bool enable) {
        if (GameState.Instance.tutorialCounter > 1
            || GameState.Instance.getCurrentBetValue() > 0) // Skip if bets are already on the table
            return null;
        if (enable) {
            speechBubblePlaceBets.tag = "sa";
            Debug.Log ("Showing 'place your bets' speech bubble");
            LogUtils.LogEvent(Consts.FE_TUTORIAL_SHOWED_PLACE_BETS);
            speechBubblePlaceBets.SetActive(enable);
        } else if (speechBubblePlaceBets != null) {
            speechBubblePlaceBets.tag = "d";
        }
        return speechBubblePlaceBets;
    }

    /**
     * Show/hide the 'clear bets' speech bubble
     */
    public GameObject clearBets(bool enable) {
        if (GameState.Instance.tutorialCounter > 1)
            return null;
        if (speechBubbleClearBets != null && enable) {
            speechBubbleClearBets.tag = "sa";
            Debug.Log ("Showing 'clear bets' speech bubble");
            LogUtils.LogEvent(Consts.FE_TUTORIAL_SHOWED_CLEAR_CHIPS);
            speechBubbleClearBets.SetActive(enable);
        } else if (speechBubbleClearBets != null) {
            speechBubbleClearBets.tag = "d";
        }
        return speechBubbleClearBets;
    }

    /**
     * Show/hide the 'start dealing' speech bubble
     */
    public GameObject startDealing(bool enable) {
        if (GameState.Instance.tutorialCounter > 1)
            return null;
        if (speechBubbleStartDealing != null && enable) {
            speechBubbleStartDealing.tag = "sa";
            Debug.Log ("Showing 'start dealing' speech bubble");
            LogUtils.LogEvent(Consts.FE_TUTORIAL_SHOWED_START_DEALING);
            speechBubbleStartDealing.SetActive(enable);
        } else if (speechBubbleStartDealing != null) {
            speechBubbleStartDealing.tag = "d";
        }
        return speechBubbleStartDealing;
    }

    /**
     * Show/hide the 'return card' speech bubble
     */
    public GameObject returnCard(bool enable) {
        if (GameState.Instance.tutorialCounter > 1)
            return null;
        if (speechBubbleReturnCard != null && enable) {
            speechBubbleReturnCard.tag = "sa";
            Debug.Log ("Showing 'return card' speech bubble");
            LogUtils.LogEvent(Consts.FE_TUTORIAL_SHOWED_RETURN_CARD);
            speechBubbleReturnCard.SetActive(enable);
        } else if (speechBubbleReturnCard != null) {
            speechBubbleReturnCard.tag = "d";
        }
        return speechBubbleReturnCard;
    }

    /**
     * Show/hide the 'other card' speech bubble
     */
    public GameObject otherCard(bool enable) {
        if (GameState.Instance.tutorialCounter > 1)
            return null;
        if (speechBubbleOtherCard != null && enable) {
            speechBubbleOtherCard.tag = "sa";
            Debug.Log ("Showing 'other card' speech bubble");
            LogUtils.LogEvent(Consts.FE_TUTORIAL_SHOWED_OTHER_CARD);
            speechBubbleOtherCard.SetActive(enable);
        } else if (speechBubbleOtherCard != null) {
            speechBubbleOtherCard.tag = "d";
        }
        return speechBubbleOtherCard;
    }

    /**
     * Show/hide the 'start squeezing' speech bubble
     */
    public GameObject startSqueezing(bool enable) {
        if (GameState.Instance.tutorialCounter > 1)
            return null;
        if (speechBubbleStartSqueezing != null && enable) {
            speechBubbleStartSqueezing.tag = "sa";
            Debug.Log ("Showing 'start squeezing' speech bubble");
            LogUtils.LogEvent(Consts.FE_TUTORIAL_SHOWED_START_SQUEEZE);
            speechBubbleStartSqueezing.SetActive(enable);
        } else if (speechBubbleStartSqueezing != null) {
            speechBubbleStartSqueezing.tag = "d";
        }
        return speechBubbleStartSqueezing;
    }

    /**
     * Show/hide the 'single finger squeeze' speech bubble
     */
    public GameObject singleFingerSqueeze(bool enable) {
        if (GameState.Instance.tutorialCounter > 1)
            return null;
        if (speechBubbleSqueezeOneFinger != null && enable) {
            speechBubbleSqueezeOneFinger.tag = "sa";
            Debug.Log ("Showing 'one finger squeeze' speech bubble");
            LogUtils.LogEvent(Consts.FE_TUTORIAL_SHOWED_ONE_FINGER_SQUEEZE);
            speechBubbleSqueezeOneFinger.SetActive(enable);
        } else if (speechBubbleSqueezeOneFinger != null) {
            speechBubbleSqueezeOneFinger.tag = "d";
        }
        return speechBubbleSqueezeOneFinger;
    }

    /**
     * Show/hide the 'double finger squeeze' speech bubble
     */
    public GameObject doubleFingerSqueeze(bool enable) {
        if (GameState.Instance.tutorialCounter > 1)
            return null;
        if (speechBubbleSqueezeTwoFingers != null && enable) {
            speechBubbleSqueezeTwoFingers.tag = "sa";
            Debug.Log ("Showing 'two finger squeeze' speech bubble");
            LogUtils.LogEvent(Consts.FE_TUTORIAL_SHOWED_TWO_FINGER_SQUEEZE);
            speechBubbleSqueezeTwoFingers.SetActive(enable);
        } else if (speechBubbleSqueezeTwoFingers != null) {
            speechBubbleSqueezeTwoFingers.tag = "d";
        }
        return speechBubbleSqueezeTwoFingers;
    }

    /**
     * Show/hide the 'squeeze rotate double tap' speech bubble
     */
    public GameObject doubleTapRotate(bool enable) {
        if (GameState.Instance.tutorialCounter > 1)
            return null;
        if (speechBubbleSqueezeRotateTap != null && enable) {
            speechBubbleSqueezeRotateTap.tag = "sa";
            Debug.Log ("Showing 'squeeze rotate double tap' speech bubble");
            LogUtils.LogEvent(Consts.FE_TUTORIAL_SHOWED_TAP_ROTATE);
            speechBubbleSqueezeRotateTap.SetActive(enable);
        } else if (speechBubbleSqueezeRotateTap != null) {
            speechBubbleSqueezeRotateTap.tag = "d";
        }
        return speechBubbleSqueezeRotateTap;
    }

    /**
     * Show/hide the 'squeeze rotate pinch twist' speech bubble
     */
    /*
    public GameObject pinchTwistRotate(bool enable) {
        if (GameState.Instance.tutorialCounter > 1)
            return null;
        if (speechBubbleSqueezePinchTwist != null && enable) {
            speechBubbleSqueezePinchTwist.tag = "sa";
            Debug.Log ("Showing 'squeeze rotate pinch twist' speech bubble");
            LogUtils.LogEvent(Consts.TWIST...
            speechBubbleSqueezePinchTwist.SetActive(enable);
        } else if (speechBubbleSqueezePinchTwist != null) {
            speechBubbleSqueezePinchTwist.tag = "d";
        }
        return speechBubbleSqueezePinchTwist;
    }
    */

    /**
     * Show/hide the 'AR guide' speech bubble
     */
    public GameObject arGuide(bool enable) {
        //if (GameState.Instance.tutorialCounter > 1)
        //    return;
#if !UNITY_IPHONE // Apple rejected having an AR Bonus because it conflicts with their idea of in-app purchasing to buy chips.
        if (speechBubbleARGuide != null && enable) {
            speechBubbleARGuide.tag = "sa";
            Debug.Log ("Showing 'AR guide' speech bubble");
            LogUtils.LogEvent(Consts.FE_TUTORIAL_SHOWED_AR);
            speechBubbleARGuide.SetActive(enable);
        } else if (speechBubbleARGuide != null) {
            speechBubbleARGuide.tag = "d";
        }
        return speechBubbleARGuide;
#else
        if (speechBubbleARGuideNoBonus_forIOS != null && enable) {
            speechBubbleARGuideNoBonus_forIOS.tag = "sa";
            Debug.Log ("Showing 'AR guide' (no bonus for iOS) speech bubble");
            LogUtils.LogEvent(Consts.FE_TUTORIAL_SHOWED_AR);
            speechBubbleARGuideNoBonus_forIOS.SetActive(enable);
        } else if (speechBubbleARGuideNoBonus_forIOS != null) {
            speechBubbleARGuideNoBonus_forIOS.tag = "d";
        }
        return speechBubbleARGuideNoBonus_forIOS;
#endif
    }

    /**
     * Show/hide the 'Gyro Cam guide' speech bubble
     */
    public GameObject gryoCamGuide(bool enable) {
        if (GameState.Instance.tutorialCounter > 1)
            return speechBubbleGyroCamGuide;
        if (speechBubbleGyroCamGuide != null && enable) {
            speechBubbleGyroCamGuide.tag = "sa";
            Debug.Log ("Showing 'Gyro Cam guide' speech bubble");
            LogUtils.LogEvent(Consts.FE_TUTORIAL_SHOWED_GYROCAM);
            speechBubbleGyroCamGuide.SetActive(enable);
        } else if (speechBubbleGyroCamGuide != null) {
            speechBubbleGyroCamGuide.tag = "d";
        }
        return speechBubbleGyroCamGuide;
    }

    // Word wrap text
    // TODO: not using this now... it doesn't work on Japanese
    // Ref: http://forum.unity3d.com/threads/31351-GUIText-width-and-height
    /*
    public static Rect FormatGuiTextArea(GUIText guiText, float maxAreaWidth)
    {
        string[] words = guiText.text.Split(' ');
        string result = "";
        Rect textArea = new Rect();

        for(int i = 0; i < words.Length; i++)
        {
            // set the gui text to the current string including new word
            guiText.text = (result + words[i] + " ");

            // measure it
            textArea = guiText.GetScreenRect();

            // if it didn't fit, put word onto next line, otherwise keep it
            if(textArea.width > maxAreaWidth)
            {
                result += ("\n" + words[i] + " ");
            }
            else
            {
                result = guiText.text;
            }

        }

        return textArea;
    }
    */
}