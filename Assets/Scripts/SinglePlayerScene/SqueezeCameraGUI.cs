using UnityEngine;
using System.Collections;

public class SqueezeCameraGUI : MonoBehaviour {
    public GUIStyle buttonStyle;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI() {
        // RETURN button for returning cards to dealer
        if (GUI.Button (new Rect (Screen.width - GUIControls.BUTTON_WIDTH - GUIControls.PADDING_HORIZTONAL,
                              GUIControls.PADDING_VERTICAL,
                              GUIControls.BUTTON_WIDTH,
                              GUIControls.BUTTON_HEIGHT),
                    LanguageManager.GetText ("btn_return"),
                    buttonStyle)) {

            // No more squeezing, return the cards to dealer
            GameState.Instance.dealer.endSqueezing();
        }
    }
}
