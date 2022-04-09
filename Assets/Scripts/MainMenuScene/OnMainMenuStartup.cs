using UnityEngine;
using System.Collections;
	
public class OnMainMenuStartup : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		Debug.Log("Main Menu scene starting");

#if (!UNITY_EDITOR && UNITY_ANDROID)
		// Get Android activity context
		AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

		// Check if preference to display the casino room is enabled. This setting applies to the main menu
		// and play scenes. It defines whether or not the background 3D room with animated fly-through camera
		// is rendered.
		Camera casinoCamera = Camera.main.camera;
        GameObject casinoRoom = GameObject.Find("CasinoRoomStatic");
        if (!Utils.isPreferenceEnabled("pref_key_graphics_settings_show_casino_room", false)) {
			// Instruct the activity to set the background image to the preset static one
			Debug.Log("Requesting main menu background be a static image");
            casinoCamera.camera.enabled = false;
            casinoRoom.SetActive(false);
			activity.Call("showStaticBackgroundImage");
		} else {
			// Set the background of the main menu to the 3D casino room
			Debug.Log("Starting 3D casino room scene for the main menu background");
            casinoRoom.SetActive(true);
            casinoCamera.camera.enabled = true;
		}
		
		// Tell the activity to now render its main menu buttons
		Debug.Log("Calling main menu buttons to be rendered");
		activity.Call("postUnityInit");
#endif
	}
}