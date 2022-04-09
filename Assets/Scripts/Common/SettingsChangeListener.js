
// Function to call from native code to enable/disable augmented reality
function ToggleAR(enable) {
	// Skip if we're on the main menu
	if (Application.loadedLevelName == "MainMenuScene") {
	 	return;
	}
	
	var imageTargets = GameObject.Find("ImageTargets");
	var roof = GameObject.Find("Roof");
	if (enable == "enable") {
		Debug.Log("Toggling AR on from request from native code");
		if (imageTargets != null) {
			imageTargets.SetActive(true);
		}	
		if (roof != null) {
			roof.SetActive(false); // show no roof if in AR mode so we can see down on things
		}
	} else {
		Debug.Log("Toggling AR off from request from native code");
		if (imageTargets != null) {
			imageTargets.SetActive(false);
		}	
		if (roof != null) {
			roof.SetActive(true); // show roof if no AR
		}
	}
}

// Function to call from native code to enable/disable casino room display
function ToggleCasinoRoom(enable) {
	var casinoRoom = GameObject.Find("CasinoRoomUnity");
	if (enable == "enable") {
		Debug.Log("Toggling casino room display on from request from native code");
		if (casinoRoom != null) {
			casinoRoom.SetActive(true);
      	}
	} else {
		Debug.Log("Toggling casino room display off from request from native code");
		if (casinoRoom != null) {
			casinoRoom.SetActive(false);
		}
	}
}

// Function to call from native code to enable/disable dealers display
function ToggleDealers(enable) {
	var dealers = GameObject.Find("Dealers");
	if (enable == "enable") {
		Debug.Log("Toggling dealers display on from request from native code");
		if (dealers != null) {
			dealers.SetActive(true);
		}	
	} else {
		Debug.Log("Toggling dealers display off from request from native code");
		if (dealers != null) {
			dealers.SetActive(false);
		}
	}
}