using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Dialog for AR Menu
public class ARMenu : MonoBehaviour
{
    #region variables

    // Dimensions
    static float BTN_WIDTH = 0.92f;
    static float BTN_HEIGHT = 0.18f;

    #endregion

    public void ToggleOn() {
       Debug.Log("Toggling AR Menu isDrawGui from " + GetComponent<CustomizedCenteredGuiWindow>().isDrawGui + " to " + !GetComponent<CustomizedCenteredGuiWindow>().isDrawGui);
       if (!GetComponent<CustomizedCenteredGuiWindow>().isDrawGui) {
           GetComponent<CustomizedCenteredGuiWindow>().normalizedHeight = 0.35f;
       }
       GetComponent<CustomizedCenteredGuiWindow>().isDrawGui = !GetComponent<CustomizedCenteredGuiWindow>().isDrawGui;

        // Hide the stats panel when showing any centered GUI window, and show on vice versa
        if (GetComponent<CustomizedCenteredGuiWindow>().isDrawGui) {
            // This doesn't actually close it (haha I'll let you try and recall why, just know that it closes elsewhere. Hint: order!)
            GameState.Instance.guiControls.statsPanel.GetComponent<CustomizedStatsPanel> ().hidePanelOnMenuOpen ();
        } else {
            GameState.Instance.guiControls.statsPanel.GetComponent<CustomizedStatsPanel> ().showPanelOnMenuClose ();
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

        int origFontSize = GUI.skin.label.fontSize;

        // Window title font size
        GUI.skin.window.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / 6f);
        GUI.skin.window.fontStyle = FontStyle.Bold;

        GUI.skin.button.fontStyle = FontStyle.Bold;

        // Cancel AR
        GUI.skin.button.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / 4.5f);
        if (GameState.Instance.camerasManager.isAR()) {
            if (GUILayout.Button (LanguageManager.GetText ("btn_cancel_ar"), GUILayout.Width (buttonWidth), GUILayout.Height (buttonHeight))) {
                Debug.Log ("Cancelling AR");
                GameState.Instance.camerasManager.ToggleAR (false, false, false);
                GameState.Instance.camerasManager.ToggleGyroCamera (false);
                //GameState.Instance.camerasManager.ResetMainCameraPos();
                GameState.Instance.camerasManager.resetToMainCamera();
                GameState.Instance.ToggleFingerGestures (true);
                ToggleOn();
            }
        } else {
            // Close menu button
            if (GUILayout.Button (LanguageManager.GetText ("btn_close"), GUILayout.Width (buttonWidth), GUILayout.Height (buttonHeight))) {
                Debug.Log ("Closing AR menu");
                GameState.Instance.ToggleFingerGestures (true);
                ToggleOn();
            }
        }

        // Gyro Cam
        if (GUILayout.Button (LanguageManager.GetText ("btn_gyro_ar"), GUILayout.Width (buttonWidth), GUILayout.Height (buttonHeight))) {
            Debug.Log ("Opening Gyro Cam");
            GameState.Instance.camerasManager.ToggleGyroCamera (true, true); // TODO: force showing of casino room for meantime till we sort out real time web cam view
            GameState.Instance.ToggleFingerGestures (true);
            ToggleOn();
            GameState.Instance.tutorialHelpManager.gryoCamGuide(true);
        }

        // $ note AR
        if (GUILayout.Button (LanguageManager.GetText ("btn_money_ar"), GUILayout.Width (buttonWidth), GUILayout.Height (buttonHeight))) {
            Debug.Log ("Opening $ note AR");
            GameState.Instance.camerasManager.ToggleAR (true, false, false);
            GameState.Instance.ToggleFingerGestures (true);
            ToggleOn();
            GameState.Instance.tutorialHelpManager.arGuide(true);
        }

        // Custom target AR (no casino room)
//        if (GUILayout.Button (LanguageManager.GetText ("btn_custom_ar"), GUILayout.Width (buttonWidth), GUILayout.Height (buttonHeight))) {
//            Debug.Log ("Opening custom target AR");
//            GameState.Instance.camerasManager.ToggleAR (true, false, true);
//            GameState.Instance.ToggleFingerGestures (true);
//            ToggleOn();
//        }

        GUI.skin.button.fontSize = origFontSize;

        GUILayout.EndVertical ();
        GUILayout.FlexibleSpace ();
        GUILayout.EndHorizontal ();
        GUILayout.FlexibleSpace ();
        GUILayout.EndArea ();
    }
}