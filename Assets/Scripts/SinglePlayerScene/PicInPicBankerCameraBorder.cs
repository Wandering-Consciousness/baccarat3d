using UnityEngine;
using System.Collections;

// A pic in pic of the banker hand to display in the top right hand corner of the screen
public class PicInPicBankerCameraBorder : MonoBehaviour
{
    public float alpha = 0.37f;
    private Color bankerColor = new Color(225f/255f, 176f/255f, 6f/255f, 1f); // banker yellowish

    void Start ()
    {
#if UNITY_WEBPLAYER
        Rect newRect = camera.rect;
        newRect.width = 0.4f;
        camera.rect = newRect;
#endif
    }

    void OnGUI ()
    {
        if (!gameObject.GetComponent<Camera>().enabled)
            return;

        // Setup GUI skin
        GUISkin origSkin = GUI.skin;
        GUI.skin = CustomizedColoredGuiStatsPanelSkin.Skin;
        GUILayout.BeginArea (new Rect (0.50f * Screen.width,
                    0.0f,
#if UNITY_WEBPLAYER
                    0.4f * Screen.width,
#else
                    0.5f * Screen.width,
#endif
                    0.20f * Screen.height));
        GUILayout.BeginVertical ();
        int origFontSize = GUI.skin.label.fontSize;
        Color origFontColor = GUI.skin.label.normal.textColor;

        // Camera box
//        Color myCol = Color.black;
//        myCol.a = alpha;
//        GUI.color = myCol;
        GUI.Box (new Rect (0,
            0,
#if UNITY_WEBPLAYER
            0.4f * Screen.width,
#else
            0.5f * Screen.width,
#endif
            0.204f * Screen.height),
            "");

        // Banker card value
        GUI.skin.label.fontStyle = FontStyle.Bold;
        GUI.skin.label.fontSize = GUIControls.fontScaler / 4;
        GUI.skin.label.normal.textColor = bankerColor;
        GUILayout.BeginHorizontal();
        GUILayout.Label (LanguageManager.GetText ("label_banker") + " ");
        GUILayout.Label (GUIControls.bankerCardsValueText);
        GUILayout.EndHorizontal();

        // Current bets on banker and banker pair
        GUI.skin.label.fontSize = GUIControls.fontScaler / 5;
        if (GameState.Instance.getCurrentBetType().Equals(GameState.BetType.Banker)) {
            GUI.skin.label.normal.textColor = Color.white;
            //GUI.skin.label.fontStyle = FontStyle.BoldAndItalic;
            GUILayout.BeginHorizontal();
            GUILayout.Label ("" + LanguageManager.GetText ("label_bet") );
            GUILayout.Label (" $" + GameState.Instance.getCurrentBetValueForBetHand ().ToString ("n0"));
            GUILayout.EndHorizontal();
        }

        // Restore original GUI skin properties
        GUILayout.EndVertical ();
        GUILayout.EndArea ();
        GUI.skin.label.fontSize = origFontSize;
        GUI.skin.label.normal.textColor = origFontColor;
        GUI.skin.label.fontStyle = FontStyle.Normal;
        GUI.skin = origSkin;
    }
}