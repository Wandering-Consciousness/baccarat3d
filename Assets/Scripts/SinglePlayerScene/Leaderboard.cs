using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// using GooglePlayGames;// TODO2022: refactor
using UnityEngine.SocialPlatforms;

// Dialog for listing Facebook leaderboard and posting score
public class Leaderboard : MonoBehaviour
{
 #region variables

    public FingerGestures fingerGestures;

    // Hack callbacks so when Facebook or Google Play Games sign on we can show the right screen
    public static bool gpgAuthenticated = false;
    public static bool fbAuthenticated = false;

    // Dimensions
    static float BTN_WIDTH = 0.30f;
    static float BTN_HEIGHT {
        get {
            if (GUIControls.isPortrait) 
                return 0.075f;
            else
                return 0.09f;
        }
        set {}
    }

    static float CELL_WIDTH { // facebook user name
        get {
            if (GUIControls.isPortrait) {
                return 0.85f;
            } else {
                return 0.70f;
            }
        }
        set {}
    }

    static float CELL_HEIGHT = 0.08f;
    static float BOX_WIDTH = 0.75f;
    static float BOX_HEIGHT = 0.6f;

    static float PIC_SIZE { // profile pic size
        get {
            if (GUIControls.isPortrait) {
                return 0.10f;
            } else {
                return 0.05f;
            }
        }
        set {}
    }

    #endregion

    void Start() {
        // Open Leaderboard each time they open the app, if tutorial has already been done
        if (GameState.Instance.tutorialCounter > 1) {
            Debug.Log ("Automatically opening Leaderboard upon app start");
            fingerGestures.enabled = false; // usually we'd do GameState.Instance.ToggleFingerGestures(false) but Instance was null (script execution order problem)
            ToggleOn();
        }
    }

    static int MAX_RANKS = 8; // facebook only
    bool hasDebuggedOutputOnce = false;

    enum DisplayMode {
        NotConnectedFacebook,
        NotConnectedGoogleGames,
        FacebookLeaderboard
    }
    DisplayMode displayMode = DisplayMode.NotConnectedGoogleGames;

    public void ToggleOn ()
    {
        if (!GetComponent<CustomizedCenteredGuiWindow> ().isDrawGui) {
            // Reset triggers
            fbAuthenticated = false;
            gpgAuthenticated = false;

            if (!Social.localUser.authenticated) {
                Debug.Log ("Setting Leaderboard display mode to NotConnectedGoogleGames");
                displayMode = DisplayMode.NotConnectedGoogleGames; // Toggle Logon to Google Play Games screen to ON
            } else {
                Debug.Log ("Setting Leaderboard display mode to NotConnectedFacebook");
                displayMode = DisplayMode.NotConnectedFacebook;
            }
        }

        Debug.Log ("Toggling Leaderboard isDrawGui from " + GetComponent<CustomizedCenteredGuiWindow> ().isDrawGui + " to " + !GetComponent<CustomizedCenteredGuiWindow> ().isDrawGui);
        if (!GetComponent<CustomizedCenteredGuiWindow>().isDrawGui) {
            GetComponent<CustomizedCenteredGuiWindow>().normalizedHeight = 0.70f;
        }
        GetComponent<CustomizedCenteredGuiWindow> ().isDrawGui = !GetComponent<CustomizedCenteredGuiWindow> ().isDrawGui;

        if (!GetComponent<CustomizedCenteredGuiWindow> ().isDrawGui) {
            // Make sure we can move chips after closing this window
            if (GameState.Instance != null)
                GameState.Instance.ToggleFingerGestures(true);
        }

        if (!GetComponent<CustomizedCenteredGuiWindow> ().isDrawGui) {
            hasDebuggedOutputOnce = false;
        }

        // Hide the stats panel when showing any centered GUI window, and show on vice versa
        if (GetComponent<CustomizedCenteredGuiWindow>().isDrawGui) {
            // This doesn't actually close it (haha I'll let you try and recall why, just know that it closes elsewhere. Hint: order!)
            GameState.Instance.guiControls.statsPanel.GetComponent<CustomizedStatsPanel> ().hidePanelOnMenuOpen ();
        } else {
            GameState.Instance.guiControls.statsPanel.GetComponent<CustomizedStatsPanel> ().showPanelOnMenuClose ();
        }
    }

    void Update() {
//        if (displayMode != DisplayMode.NotConnectedFacebook && gpgAuthenticated && !GameState.Instance.isFacebookConnectedInterally) { // Connected to Google Play Games, but not Facebook
//            gpgAuthenticated = false;
//            Debug.Log ("Leaderboard#Update(): Setting Leaderboard display mode to NotConnectedFacebook");
//
//            if (GameState.Instance != null)
//                GameState.Instance.ToggleFingerGestures(false);
//
//            displayMode = DisplayMode.NotConnectedFacebook;
//        }
    }

    public void OnGUICallback (string[] args)
    {
        switch (displayMode) {
        case DisplayMode.NotConnectedGoogleGames:
            ConnectGoogleGamesOnGUI(args);
            break;
        case DisplayMode.NotConnectedFacebook:
            ConnectFacebookOnGUI(args);
            break;
        }
    }

    public void ConnectFacebookOnGUI (string[] args)
    {
        // Extract string args and convert to ints (passed from SendMessage call)
        float width = float.Parse (args [0]);
        float height = float.Parse (args [1]);
        
        float buttonWidth = width * BTN_WIDTH;
        float buttonHeight = height * BTN_HEIGHT;

        // Global rankings on Google Play Games button
        if (GUI.Button (new Rect (width*0.125f,
                                  buttonHeight*1.5f,
                                  width * 0.75f,
                                  buttonHeight),
                        LanguageManager.GetText ("btn_global_ranking_long"))) {
            
            Debug.Log ("Google Play Games Global Leaderboard button clicked");

            if (!Social.localUser.authenticated) {
                Debug.Log ("Not logged into GPG games so will do login instead");
                
                // Sign into Google
                GameState.Instance.gpgCloudManager.login();
            } else {
                Debug.Log ("User is authenticated with GPG, showing leaderboard UI");
//                ((PlayGamesPlatform) Social.Active).ShowLeaderboardUI(Consts.GPG_LEADERBOARD_ID);
                LogUtils.LogEvent(Consts.FE_BTN_LEADERBOARD_GPG_LEADERBOARD);

                GameState.Instance.ToggleFingerGestures (true);
                ToggleOn();
            }
        }
        
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
        GUI.skin.label.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / (GUIControls.isPortrait ? 4 : 6));
        GUI.skin.label.normal.background = null;
        GUI.skin.label.margin.top = GUI.skin.label.margin.bottom = 0;
        GUI.skin.label.padding.top = GUI.skin.label.padding.bottom = 0;
        if (Utils.isJapanese()) {
            GUILayout.Label(" ");
            GUILayout.Label(LanguageManager.GetText("label_link_facebook_01"));
            GUILayout.Label(" ");
            GUILayout.Label(LanguageManager.GetText("label_link_facebook_03"));
            GUILayout.Label(LanguageManager.GetText("label_link_facebook_04"));
            GUILayout.Label(" ");
            GUILayout.Label(LanguageManager.GetText("label_link_facebook_06"));
            GUILayout.Label(" ");
            GUILayout.Label(LanguageManager.GetText("label_link_facebook_08"));
            GUILayout.Label(LanguageManager.GetText("label_link_facebook_09"));
            GUILayout.Label(" ");
            GUILayout.Label(LanguageManager.GetText("label_link_facebook_11"));
            GUILayout.Label(LanguageManager.GetText("label_link_facebook_12"));
            GUILayout.Label(" ");
        } else {
            GUILayout.Label(" ");
            GUILayout.Label(LanguageManager.GetText("label_link_facebook_01"));
            GUILayout.Label(" ");
            GUILayout.Label(LanguageManager.GetText("label_link_facebook_03"));
            GUILayout.Label(LanguageManager.GetText("label_link_facebook_04"));
            GUILayout.Label(" ");
            GUILayout.Label(LanguageManager.GetText("label_link_facebook_06"));
            GUILayout.Label(LanguageManager.GetText("label_link_facebook_07"));
            GUILayout.Label(" ");
            GUILayout.Label(LanguageManager.GetText("label_link_facebook_09"));
            GUILayout.Label(LanguageManager.GetText("label_link_facebook_10"));
            GUILayout.Label(LanguageManager.GetText("label_link_facebook_11"));
            GUILayout.Label(LanguageManager.GetText("label_link_facebook_12"));
            GUILayout.Label(" ");
        }

        GUI.skin.button.active.textColor = GUI.skin.button.normal.textColor = origTextColor;
        
        GUILayout.EndVertical ();
        GUILayout.FlexibleSpace ();
        GUILayout.EndHorizontal ();
        GUILayout.FlexibleSpace ();
        GUILayout.EndArea ();

        // Connect Facebook button
        buttonWidth = width * BTN_WIDTH;
        buttonHeight = height * BTN_HEIGHT;
        GUI.skin.button.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / 4.5f);
        if (GUI.Button (new Rect (width/2 - buttonWidth,
                                  height - buttonHeight*1.5f,
                                  buttonWidth,
                                  buttonHeight),
                        LanguageManager.GetText ("btn_connect_facebook"))) {
            Debug.Log ("Pressed Connect to Facebook button");
            LogUtils.LogEvent(Consts.FE_BTN_LEADERBOARD_FACEBOOK_CONNECT);
//            FacebookSNS.Instance().Login();
            GameState.Instance.ToggleFingerGestures (true);
            
            displayMode = DisplayMode.FacebookLeaderboard;
        }

        // Close button
        if (GUI.Button (new Rect (width/2 /*+ buttonWidth*/,
                                  height - buttonHeight*1.5f,
                                  buttonWidth,
                                  buttonHeight),
                        LanguageManager.GetText ("btn_close"))) {
            Debug.Log ("Close Facebook Connect screen pressed");
            LogUtils.LogEvent(Consts.FE_BTN_LEADERBOARD_FACEBOOK_CONNECT_CLOSE);
            GameState.Instance.ToggleFingerGestures (true);
            ToggleOn();
        }
    }

    public void ConnectGoogleGamesOnGUI (string[] args)
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
        GUI.skin.label.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / (GUIControls.isPortrait ? 4 : 6));
        GUI.skin.label.normal.background = null;
        GUI.skin.label.margin.top = GUI.skin.label.margin.bottom = 0;
        GUI.skin.label.padding.top = GUI.skin.label.padding.bottom = 0;
        if (Utils.isJapanese()) {
            GUILayout.Label(" ");
            GUILayout.Label(LanguageManager.GetText("label_link_googlegames_01"));
            GUILayout.Label(LanguageManager.GetText("label_link_googlegames_02"));
            GUILayout.Label(LanguageManager.GetText("label_link_googlegames_03"));
            GUILayout.Label(" ");
            GUILayout.Label(LanguageManager.GetText("label_link_googlegames_05"));
            GUILayout.Label(" ");
            GUILayout.Label(LanguageManager.GetText("label_link_googlegames_07"));
            GUILayout.Label(LanguageManager.GetText("label_link_googlegames_08"));
            GUILayout.Label(LanguageManager.GetText("label_link_googlegames_09"));
            GUILayout.Label(" ");
        } else {
            GUILayout.Label(" ");
            GUILayout.Label(LanguageManager.GetText("label_link_googlegames_01"));
            GUILayout.Label(LanguageManager.GetText("label_link_googlegames_02"));
            GUILayout.Label(LanguageManager.GetText("label_link_googlegames_03"));
            GUILayout.Label(" ");
            GUILayout.Label(LanguageManager.GetText("label_link_googlegames_05"));
            GUILayout.Label(LanguageManager.GetText("label_link_googlegames_06"));
            GUILayout.Label(" ");
            GUILayout.Label(LanguageManager.GetText("label_link_googlegames_08"));
            GUILayout.Label(LanguageManager.GetText("label_link_googlegames_09"));
            GUILayout.Label(LanguageManager.GetText("label_link_googlegames_10"));
            GUILayout.Label(LanguageManager.GetText("label_link_googlegames_11"));
            GUILayout.Label(" ");
        }
        
        GUI.skin.button.active.textColor = GUI.skin.button.normal.textColor = origTextColor;
        
        GUILayout.EndVertical ();
        GUILayout.FlexibleSpace ();
        GUILayout.EndHorizontal ();
        GUILayout.FlexibleSpace ();
        GUILayout.EndArea ();
        
        // Connect Google Play Games button
        buttonWidth = width * BTN_WIDTH;
        buttonHeight = height * BTN_HEIGHT;
        GUI.skin.button.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / 4.5f);
        if (GUI.Button (new Rect (width / 2 - buttonWidth,
                                  height - buttonHeight*1.5f,
                                  buttonWidth,
                                  buttonHeight),
                        LanguageManager.GetText ("btn_gpg_logon"))) {
            Debug.Log ("Pressed Logon to Google Play Games button");
            LogUtils.LogEvent(Consts.FE_BTN_LEADERBOARD_GPG_LOGON);

            // Sign into Google
            GameState.Instance.gpgCloudManager.login();

            displayMode = DisplayMode.NotConnectedFacebook;
            GameState.Instance.ToggleFingerGestures (true);
            
            ToggleOn();
        }
        // Close button
        if (GUI.Button (new Rect (width / 2,
                                  height - buttonHeight*1.5f,
                                  buttonWidth,
                                  buttonHeight),
                        LanguageManager.GetText ("btn_close"))) {
            Debug.Log ("Close Google Play Games Logon screen pressed");
            LogUtils.LogEvent(Consts.FE_BTN_LEADERBOARD_GPG_LOGON_CLOSE);
            GameState.Instance.ToggleFingerGestures (true);
            ToggleOn();
        }
    }
}