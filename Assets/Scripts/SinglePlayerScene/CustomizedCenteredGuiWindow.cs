using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Customized Baccarat 3D GUI window with frame and title bar
public class CustomizedCenteredGuiWindow : MonoBehaviour
{
 
    public List<Color> primaryColors;
    public List<Color> secondaryColors;
    public string titleKey;
    public int uniqueWindowId = 0;
    public float normalizedWidth = 0f;
    public float normalizedHeight = 0f;

    // Callback GO/method for OnGUI calls
    public string callbackMethod;

    private bool hasUpdatedGui = false;
    private int currentColor;
    private Rect windowRect;

    public static bool isACustomCenteredGuiDisplaying = false;
    private bool _isDrawGui = false;
    public bool isDrawGui {
        get {
            return _isDrawGui;
        }
        set {
            _isDrawGui = isACustomCenteredGuiDisplaying = value;
        }
    }

    void OnGUI ()
    {
        if (!hasUpdatedGui) {
            CustomizedColoredGuiWindowSkin.Instance.UpdateGuiColors (primaryColors [0], secondaryColors [0]);
            hasUpdatedGui = true;
        }

        if (!isDrawGui) {
            return;
        }

        GUI.skin = CustomizedColoredGuiWindowSkin.Skin;

        // Window
        if (Event.current.type == EventType.Layout) {
            windowRect = new Rect (Screen.width * ((1f-normalizedWidth)/2), Screen.height * ((1-normalizedHeight)/2), Screen.width * normalizedWidth, Screen.height * normalizedHeight);
            windowRect = GUI.Window (uniqueWindowId, windowRect, DoWindow, LanguageManager.GetText (titleKey));
        }
      }
 
    void DoWindow (int windowID)
    {
        // Left dragon in window bar
        //GUI.DrawTexture (new Rect (20, 4, 31, 40), CustomizedColoredGuiWindowSkin.Skin.customStyles [1].normal.background);

        // Right dragon in window bar
        //GUI.DrawTexture (new Rect (windowRect.width - 51, 4, 31, 40), CustomizedColoredGuiWindowSkin.Skin.customStyles [2].normal.background);

        // Draw the callback contents in this window
        if (gameObject != null) {
            string[] args = new string[2];
            args[0] = ""+windowRect.width;
            args[1] = ""+ windowRect.height;
            gameObject.SendMessage(callbackMethod, args);
        }
    }
}
