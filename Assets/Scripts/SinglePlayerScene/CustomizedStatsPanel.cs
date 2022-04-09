using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Customized stats panel for showing bank balance, current bets etc.
public class CustomizedStatsPanel : MonoBehaviour
{
    public List<Color> primaryColors;
    public List<Color> secondaryColors;
    public float normalizedWidth {
        get {
            return GUIControls.isPortrait ? 0.49f : 0.40f;
        }
        set {
        }
    }
    public float normalizedHeight {
        get {
            // Return a height relative the number of bets the user has placed on the table as to avoid
            // the stats panel being higher than it needs to be with unused space below the last line of
            // text
            if (GUIControls.isPortrait) {
                switch (GUIControls.currentBetTexts.Count) {
                case 4:
                    return 0.21f;
                case 3:
                    return 0.19f;
                case 2:
                    return 0.17f;
                case 1:
                    return 0.17f;
                case 0:
                default:
                    return 0.17f;
                }
            } else {
                switch (GUIControls.currentBetTexts.Count) {
                case 4:
                    return 0.43f;
                case 3:
                    return 0.39f;
                case 2:
                    return 0.34f;
                case 1:
                    return 0.31f;
                    break;
                case 0:
                default:
                    return 0.20f;
                    break;
                }
            }
        }
        set {
        }
    }

    private bool hasUpdatedGui = false;
    private int currentColor;
    private Rect windowRect;
    public static bool showPanelb = false;
    private static bool lastPanelb = showPanelb;
    private bool completelyHide = false;
    private Color playerColor = new Color(213f/255f, 127f/255f, 81f/255f, 1f); // player pinkish
    private Color bankerColor = new Color(225f/255f, 176f/255f, 6f/255f, 1f); // banker yellowish
    private Color betsColor = new Color(1f, 1f, 1f, 1f); // bets white
    private string betsSpaceFiller = "     ";

    void OnGUI ()
    {
        if (completelyHide) {
            return;
        }

        if (!hasUpdatedGui) {
            CustomizedColoredGuiStatsPanelSkin.Instance.UpdateGuiColors (primaryColors [0], secondaryColors [0]);
            hasUpdatedGui = true;
        }
        GUI.skin = CustomizedColoredGuiStatsPanelSkin.Skin;
        GUI.skin.window.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / 5f);

        // Window
//        if (showPanelb) {
//            // Title bar with frame
//            CustomizedColoredGuiStatsPanelSkin.Skin.window.normal.background = CustomizedColoredGuiStatsPanelSkin.Skin.customStyles [4].normal.background;
//        } else {
//            // Just title bar
//            CustomizedColoredGuiStatsPanelSkin.Skin.window.normal.background = CustomizedColoredGuiStatsPanelSkin.Skin.customStyles [3].normal.background;
//        }
        if (Event.current.type == EventType.Layout) {
            windowRect = new Rect (Screen.width * (GUIControls.isPortrait ?  0.01f : 0.005f),
                                    Screen.height * (GUIControls.isPortrait ?  0.005f : 0.01f),
                                    Screen.width * normalizedWidth,
                                    Screen.height * normalizedHeight);
            windowRect = GUI.Window (0, windowRect, DoWindow, GUIControls.balanceText);
        }
    }
 
    void DoWindow (int windowID)
    {
        // Left dragon in window bar
        //GUI.DrawTexture (new Rect (20, 4, 31, 40), CustomizedColoredGuiStatsPanelSkin.Skin.customStyles [0].normal.background);

        int origFontSize = GUI.skin.label.fontSize;
        GUI.skin.label.fontStyle = FontStyle.Bold;
        Color origFontColor = GUI.skin.label.normal.textColor;

        GUILayout.BeginArea (new Rect (0,
            45,
            windowRect.width,
            windowRect.height));
        GUILayout.BeginVertical ();

        if (showPanelb) { // Only show banker and player card values and current bets if showPanelb is true
            GUILayout.BeginHorizontal ();

            // Player cards
            GUI.skin.label.fontSize = GUIControls.fontScaler / 4;
            GUI.skin.label.normal.textColor = playerColor;
            GUILayout.Label (LanguageManager.GetText ("label_player") + "  ");
            GUILayout.Label (GUIControls.playerCardsValueText);
    
            // Banker cards
            GUI.skin.label.normal.textColor = bankerColor;
            GUILayout.Label ("  " + LanguageManager.GetText ("label_banker") + "  ");
            GUILayout.Label (GUIControls.bankerCardsValueText);
            GUILayout.EndHorizontal ();
    
            // Current bets
            GUI.skin.label.fontSize = GUIControls.fontScaler / 5;
            GUI.skin.label.normal.textColor = betsColor;
            GUI.skin.label.fontStyle = FontStyle.Normal;
            if (GUIControls.currentBetTexts.Count > 0)
                GUILayout.Label (LanguageManager.GetText ("label_bet"));
            GUI.skin.label.fontStyle = FontStyle.BoldAndItalic;
            foreach (string[] bet in GUIControls.currentBetTexts) {
                GUILayout.BeginHorizontal();
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                GUILayout.Label (betsSpaceFiller + bet[0], GUILayout.Width(windowRect.width * 0.6f)); // bet type (label)
                GUI.skin.label.alignment = TextAnchor.MiddleRight;
                GUILayout.Label (bet[1]); // $ amount
                GUILayout.EndHorizontal();
            }
        }

        // Message of what dealer is speaking
        GUI.skin.label.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / 4.5f);
        GUI.skin.label.normal.textColor = new Color(0x04/255f,0xe0/255f, 0x1d/255f); // light green
        GUI.skin.label.fontStyle = FontStyle.Bold;
        if (GUIControls.message != null && GUIControls.message != "") {
            GUILayout.Label (GUIControls.message);
        }

        GUI.skin.label.fontSize = origFontSize;
        GUI.skin.label.normal.textColor = origFontColor;
        GUI.skin.label.fontStyle = FontStyle.Normal;
        GUILayout.EndVertical ();
        GUILayout.EndArea ();
    }

    // Show the title bar + current bets/card values
    public static void showPanel() {
        Debug.Log ("Showing panel (current bets/card values)");
        lastPanelb = showPanelb;
        showPanelb = true;
    }


    // Hide the current bets/card values and just leave the title bar
    public static void hidePanel() {
        Debug.Log ("Hiding panel (current bets/card values)");
        lastPanelb = showPanelb;
        showPanelb = false;
    }

    // Completely hide the stats panel (title bar + current bets/card values)
    public void hidePanelOnMenuOpen() {
        Debug.Log ("Completely hiding stats panel on menu open");
        completelyHide = true;
    }

    // Completely show the title bar + current bets/card values in its last state
    public void showPanelOnMenuClose() {
        if (CustomizedCenteredGuiWindow.isACustomCenteredGuiDisplaying)
            // Don't show panel if a window was opened from the menu, it'll open when that window closes
            return;

        Debug.Log ("Uncompletely hiding stats panel on menu close");
        completelyHide = false;
        if (lastPanelb) {
            Debug.Log ("\tcurrent bets/card values last state was open");
            showPanelb = true;
        }
    }
}
