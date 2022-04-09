using UnityEngine;
using System.Collections;

// A pic in pic of the player hand to display in the top left hand corner of the screen
public class PicInPicPlayerCameraBorder : MonoBehaviour
{
    public float alpha = 0.37f;
    private Color playerColor = new Color (213f / 255f, 127f / 255f, 81f / 255f, 1f); // player pinkish

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
        GUILayout.BeginArea (new Rect (0.0f,
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

        // Player card value
        GUI.skin.label.fontStyle = FontStyle.Bold;
        GUI.skin.label.fontSize = GUIControls.fontScaler / 4;
        GUI.skin.label.normal.textColor = playerColor;
        GUILayout.BeginHorizontal();
        GUILayout.Label (LanguageManager.GetText ("label_player") + " ");
        GUILayout.Label (GUIControls.playerCardsValueText);
        GUILayout.EndHorizontal();

        // Current bets on player and player pair
        GUI.skin.label.fontSize = GUIControls.fontScaler / 5;
        if (GameState.Instance.getCurrentBetType().Equals(GameState.BetType.Player) || GameState.Instance.getCurrentBetType().Equals(GameState.BetType.PlayerPair)) {
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