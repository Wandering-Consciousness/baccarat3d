using UnityEngine;
using System.Collections;

public class CamerasManager : MonoBehaviour
{
    private Camera activeMainCamera;
    public GameObject arCamera;
    public Camera mainCamera;
    public Camera dealtCardsCamera;
    public Camera dealtCardsOtherPlayerCamera;
    public Camera dealtCardsOtherBankerCamera;
    //public Camera squeezeCamera; // COMMENTING OUT coz sticking with birds eye squeeze camera only
    public Camera birdsEyeSqueezeCamera;
    public Camera gyroCamera;
    public GameObject gyroRealCameraTexture;
    public GameObject lastActiveCamera;
    public Camera picInPicPlayerOtherCamera;
    public Camera picInPicBankerOtherCamera;
    public Camera picInPicPlayerBetOnCamera;
    public Camera picInPicBankerBetOnCamera;
    public Camera speechBubbleCamera;
    public GameObject tiePlayerCamera;
    public GameObject tieBankerCamera;
    public GameObject imageTargets;
    public GameObject roof;
    public GameObject casinoRoom;
    public GameObject[] tableOnOffObjects;
    public GUISkin guiSkin;
    public Texture cameraHorizontalSeparatorTexture;
    public GameObject mainMenuOfCircleButtons;
    public ScreenRaycaster chipsScreenRaycaster;
    public ScreenRaycaster cardsScreenRaycaster;
    Vector3 mainCameraOriginalPosition = new Vector3 (0f, 0f, 0f);
    Vector3 mainCameraOriginalRotation = new Vector3 (0f, 0f, 0f);
    //Vector3 squeezeCameraOriginalPosition = new Vector3 (0f, 0f, 0f);
    Vector3 birdsEyeSqueezeCameraOriginalPosition = new Vector3 (0f, 0f, 0f);
    Vector3 dealtCardsCameraOriginalPosition = new Vector3 (0f, 0f, 0f);
    Vector3 picInPicPlayerOtherCameraOriginalPosition = new Vector3 (0f, 0f, 0f);
    Vector3 picInPicBankerOtherCameraOriginalPosition = new Vector3 (0f, 0f, 0f);
    Vector3 picInPicPlayerBetOnCameraOriginalPosition = new Vector3 (0f, 0f, 0f);
    Vector3 picInPicBankerBetOnCameraOriginalPosition = new Vector3 (0f, 0f, 0f);

    string currentSqueezeCameraPos = "center";
    string oldSqueezeCameraPos = "";
    string currentDealtCardsCameraPos = "center";

    // Turn objects display on/off button dimensions
    float toggleObjectsBtnWidth {
        get {
            return Screen.width * 0.24f;
        }
    }
    float toggleObjectsBtnHeight {
        get {
            return Screen.height * 0.07f;
        }
    }
    float toggleObjectsBtnHeightFactor = 2.2f;
    bool cameraPositionHasChanged = false;
    //bool squeezeCameraOn = false;

    // Return the camera to use for raycasting depending on what mode we're in
    public Camera raycastCamera {
        get {
            if (gyroCamera.enabled)
                return gyroCamera;
            else if (arCamera.GetComponent<Camera>().enabled)
                return arCamera.GetComponent<Camera>();
            else
                return Camera.main;
        }
        set {
        }
    }

    // Use this for initialization
    IEnumerator Start ()
    {
        if (Consts.AR_DEBUG_ON) {
            // Force AR on with no casino room
            GameObject.Find ("CasinoRoomUnity").SetActive (false);
            ToggleMainCamera (false);
            ToggleAR (true);
        } else {
            // Enable/disable AR
            ToggleAR (Utils.isPreferenceEnabled ("pref_key_graphics_settings_ar_enable", false));
        }

        // Register ourselves with the game state manager
        GameState.Instance.camerasManager = this;

        // Keep reference to original position of cameras
        mainCameraOriginalPosition = mainCamera.transform.position;
        mainCameraOriginalRotation = mainCamera.transform.rotation.eulerAngles;
        //squeezeCameraOriginalPosition = squeezeCamera.transform.position;
        birdsEyeSqueezeCameraOriginalPosition = birdsEyeSqueezeCamera.transform.position;
        dealtCardsCameraOriginalPosition = dealtCardsCamera.transform.position;
        picInPicPlayerOtherCameraOriginalPosition = picInPicPlayerOtherCamera.transform.position;
        picInPicBankerOtherCameraOriginalPosition = picInPicBankerOtherCamera.transform.position;
        picInPicPlayerBetOnCameraOriginalPosition = picInPicPlayerBetOnCamera.transform.position;
        picInPicBankerBetOnCameraOriginalPosition = picInPicBankerBetOnCamera.transform.position;

        // By default the active main camera is the main camera
        activeMainCamera = mainCamera;

        yield return new WaitForSeconds(0.3f);
        if (!GameState.Instance.isProEdition) {
#if UNITY_IPHONE
            // When showing banner ads we tilt the camera down a little to give more room for the banner
            iTween.RotateBy (mainCamera.gameObject, iTween.Hash ("amount", new Vector3 (5.5f / 360f, 0f, 0f), "time", 4f));
#else
            // When showing banner ads we tilt the camera down a little to give more room for the banner
            iTween.RotateBy (mainCamera.gameObject, iTween.Hash ("amount", new Vector3 (4.3f / 360f, 0f, 0f), "time", 4f));
#endif
        }
    }
 
    public void RotateMainCameraToOriginalRotation() {
        Debug.Log ("Resetting main camera to original rotation");
        iTween.RotateTo (mainCamera.gameObject, mainCameraOriginalRotation, 4f);
    }

    // Update is called once per frame
    void Update ()
    {
        if (gyroCamera.enabled) {
            // Constantly position the real camera feed texture relative to the moving gyro camera
            //gyroRealCameraTexture.transform.position = gyroCamera.ScreenToWorldPoint(new Vector3(Screen.width/2, Screen.height/2, gyroCamera.nearClipPlane + 3f)); // Commentd out coz jittery
        }

        // COMMENTING OUT because we're giving up the facedown effect and will have it on all the time instead
        /*
        // Show the birds eye view for squeezing if phone is faceup/facedown
        if (squeezeCameraOn &&
            GUIControls.isFaceDown

            // FaceDown/Up doesn't work if iPhone (and Android too maybe) orientation lock is on
            //(Input.deviceOrientation == DeviceOrientation.FaceDown
            //|| Input.deviceOrientation == DeviceOrientation.FaceUp)

            ) {
            //if (!birdsEyeSqueezeCamera.enabled && squeezeCamera.enabled) {
            //Debug.Log ("Switching to birds eye squeeze camera");
            birdsEyeSqueezeCamera.enabled = true;
            squeezeCamera.enabled = false;
            //}
        } else if (squeezeCameraOn) {
            //if (birdsEyeSqueezeCamera.enabled && !squeezeCamera.enabled) {
            //Debug.Log ("Switching to normal squeeze camera");
            birdsEyeSqueezeCamera.enabled = false;
            squeezeCamera.enabled = true;
            //}
        }
         */

        // Hack to reposition the top pic-in-pic camera for player cards when current bet is on the player
        if (picInPicPlayerBetOnCamera.enabled && !GUIControls.isPortrait) {
            if (picInPicPlayerBetOnCamera.gameObject.transform.position == picInPicPlayerBetOnCameraOriginalPosition) {
                // Shift the camera to the left so we don't also see the first banker card in the bottom right
                iTween.MoveTo (picInPicPlayerBetOnCamera.gameObject, picInPicPlayerBetOnCameraOriginalPosition + new Vector3(-0.16f, 0, 0f), 0.0f);
            }
        } else if (picInPicPlayerBetOnCamera.enabled && GUIControls.isPortrait) {
            if (picInPicPlayerBetOnCamera.gameObject.transform.position != picInPicPlayerBetOnCameraOriginalPosition) {
                // Reset position
                iTween.MoveTo (picInPicPlayerBetOnCamera.gameObject, picInPicPlayerBetOnCameraOriginalPosition, 0.0f);
            }
        }

        // Hack to reposition the top pic-in-pic camera for banker cards when current bet is on the player
        if (picInPicBankerOtherCamera.enabled && !GUIControls.isPortrait) {
            if (picInPicBankerOtherCamera.gameObject.transform.position == picInPicBankerOtherCameraOriginalPosition) {
                // Shift the camera to the left so we don't also see the second player card on the left
                iTween.MoveTo (picInPicBankerOtherCamera.gameObject, picInPicBankerOtherCameraOriginalPosition + new Vector3(0.094f, 0, 0f), 0.0f);
            }
        } else if (picInPicBankerOtherCamera.enabled && GUIControls.isPortrait) {
            if (picInPicBankerOtherCamera.gameObject.transform.position != picInPicBankerOtherCameraOriginalPosition) {
                // Reset position
                iTween.MoveTo (picInPicBankerOtherCamera.gameObject, picInPicBankerOtherCameraOriginalPosition, 0.0f);
            }
        }
    }

    public void OnGUI ()
    {
        // Draw the button to turn on/off the casino room display if we're in AR or gyro cam mode
        if (isAR ()
            && !gyroCamera.enabled) { // TODO: disable show/hide casino room btn till we sort out gyro real camera background texture view issues
            GUI.skin = CustomizedColoredGuiWindowSkin.Instance.actualSkin;
            int origFontSize = GUI.skin.label.fontSize;

            GUI.skin.button.fontSize = GUIControls.fontScaler / 4;

            // Table+dealer display on/off buton
            if (tableOnOffObjects[0].activeSelf) {
                // Draw turn off button
                if (GUI.Button (new Rect (Screen.width / 3 - toggleObjectsBtnWidth / 2,
                                            Screen.height - toggleObjectsBtnHeight*toggleObjectsBtnHeightFactor,
                                            toggleObjectsBtnWidth,
                                            toggleObjectsBtnHeight),
                               LanguageManager.GetText ("btn_hide_table"))) {
                    ToggleTableAndDealer (false);
                }
            } else {
                // Draw turn on button
                if (GUI.Button (new Rect (Screen.width / 3 - toggleObjectsBtnWidth / 2,
                                            Screen.height - toggleObjectsBtnHeight*toggleObjectsBtnHeightFactor,
                                            toggleObjectsBtnWidth,
                                            toggleObjectsBtnHeight),
                                LanguageManager.GetText ("btn_show_table"))) {
                    ToggleTableAndDealer (true);
                }
            }

            // Casino room display on/off buton
            if (casinoRoom.activeSelf) {
                // Draw turn off button
                if (GUI.Button (new Rect ((Screen.width / 3 * 2) - toggleObjectsBtnWidth / 2,
                                            Screen.height - toggleObjectsBtnHeight*toggleObjectsBtnHeightFactor,
                                            toggleObjectsBtnWidth,
                                            toggleObjectsBtnHeight),
                               LanguageManager.GetText ("btn_hide_casino"))) {
                    ToggleCasinoRoom (false);
                }
            } else {
                // Draw turn on button
                if (GUI.Button (new Rect ((Screen.width / 3 * 2) - toggleObjectsBtnWidth / 2,
                                            Screen.height - toggleObjectsBtnHeight*toggleObjectsBtnHeightFactor,
                                            toggleObjectsBtnWidth,
                                            toggleObjectsBtnHeight),
                                LanguageManager.GetText ("btn_show_casino"))) {
                    ToggleCasinoRoom (true);
                }
            }

            GUI.skin.button.fontSize = origFontSize;
        }

        // Draw a thickish horizontal line splitting the screen when showing dealt card cameras to easily distinguish between the fact that it's two different cameras
        // and not a skewed/splitup top-down view of the table
        if (GUIControls.isPortrait && !isAR () && (dealtCardsCamera.enabled || tiePlayerCamera.activeSelf)) {
            float height = Screen.height * 0.01f;
            GUI.DrawTexture (new Rect (0,
                        Screen.height / 2 - height / 2,
                        Screen.width,
                        height),
                        cameraHorizontalSeparatorTexture);
        }
    }

    public void setCanZoomAndPan (bool enable)
    {
        if (enable) {
            // Enable zooming and panning
            Debug.Log ("Toggling active main camera's zoom and panning on");
            if (activeMainCamera.GetComponent<GesturesCamera> () != null)
                activeMainCamera.GetComponent<GesturesCamera> ().enabled = true;
        } else {
            // No zooming and panning allowed
            Debug.Log ("Toggling active main camera's zoom and panning on");
            if (activeMainCamera.GetComponent<GesturesCamera> () != null)
                activeMainCamera.GetComponent<GesturesCamera> ().enabled = false;
        }
    }

    public void ResetMainCameraPos ()
    {
        Debug.Log ("Resetting main camera's position");
        iTween.MoveTo (mainCamera.gameObject, iTween.Hash ("position", mainCameraOriginalPosition, "time", 0.3f));

        if (!GameState.Instance.isProEdition) {
#if UNITY_IPHONE
            // When showing banner ads we tilt the camera down a little to give more room for the banner
            iTween.RotateTo (mainCamera.gameObject, iTween.Hash ("amount", (mainCameraOriginalRotation + new Vector3 (5.5f / 360f, 0f, 0f)), "time", 2f));
#else
            // When showing banner ads we tilt the camera down a little to give more room for the banner
            iTween.RotateTo (mainCamera.gameObject, iTween.Hash ("amount", (mainCameraOriginalRotation + new Vector3 (4.3f / 360f, 0f, 0f)), "time", 2f));
#endif
        }
    }

    public void ToggleMainCamera (bool enable)
    {
        ToggleMainCamera (enable, false);
    }

    public void ToggleMainCamera (bool enable, bool resetToOriginalPosition)
    {
        if (arCamera.activeSelf || gyroCamera.enabled)
            return;

        // Enable/disable main camera
        if (enable) {
            Debug.Log ("Toggling main camera on");
            mainCamera.enabled = true;
            if (chipsScreenRaycaster.Cameras.Length != 0)
                chipsScreenRaycaster.Cameras[0] = mainCamera;
            else
                chipsScreenRaycaster.Cameras = Camera.allCameras;
            if (cardsScreenRaycaster.Cameras.Length != 0)
                cardsScreenRaycaster.Cameras[0] = mainCamera;
            else
                cardsScreenRaycaster.Cameras = Camera.allCameras;
            mainCamera.GetComponent<PinchRecognizer> ().enabled = true;
            mainCamera.GetComponent<DragRecognizer> ().enabled = true;
            mainCamera.GetComponent<GesturesCamera> ().enabled = true;
            if (resetToOriginalPosition) {
                //iTween.MoveTo (mainCamera.gameObject, iTween.Hash ("position", mainCameraOriginalPosition, "time", 0.3f));
                ResetMainCameraPos();
            }
        } else {
            Debug.Log ("Toggling main camera off");
            mainCamera.enabled = false;
            mainCamera.GetComponent<PinchRecognizer> ().enabled = false;
            mainCamera.GetComponent<DragRecognizer> ().enabled = false;
            mainCamera.GetComponent<GesturesCamera> ().enabled = false;
        }
    }

    public void ToggleLastActiveCamera (bool enable)
    {
        // Enable/disable last active main camera
        if (lastActiveCamera != null && enable) {
            Debug.Log ("Toggling last active camera on");
            if (lastActiveCamera.GetComponent<Camera>() == birdsEyeSqueezeCamera && GameState.Instance.currentState == GameState.State.SqueezeCards) {
                ToggleSqueezeCamera(true);
            } else {
                lastActiveCamera = activeMainCamera.gameObject;
            }
            lastActiveCamera.GetComponent<Camera>().gameObject.SetActive(true);
            lastActiveCamera.GetComponent<Camera>().enabled = true;
            chipsScreenRaycaster.Cameras[0] = lastActiveCamera.GetComponent<Camera>();
            cardsScreenRaycaster.Cameras[0] = lastActiveCamera.GetComponent<Camera>();
            mainCamera.GetComponent<PinchRecognizer> ().enabled = true;
            mainCamera.GetComponent<DragRecognizer> ().enabled = true;
            mainCamera.GetComponent<GesturesCamera> ().enabled = true;
        } else if (lastActiveCamera != null) {
            Debug.Log ("Toggling last active camera off");
            if (lastActiveCamera.GetComponent<Camera>() == birdsEyeSqueezeCamera) {
                ToggleSqueezeCamera(false);
            } else {
                lastActiveCamera = activeMainCamera.gameObject;
            }
            lastActiveCamera.gameObject.SetActive(false);
            mainCamera.GetComponent<PinchRecognizer> ().enabled = false;
            mainCamera.GetComponent<DragRecognizer> ().enabled = false;
            mainCamera.GetComponent<GesturesCamera> ().enabled = false;
        } else {
            ToggleMainCamera(true);
        }
    }

    public void ToggleTableAndDealer (bool enable)
    {
        Debug.Log ("Toggling dealer + table enable: " + enable);
        if (!enable)
            ToggleCasinoRoom(false); // don't allow casino room to be turned on without table and dealer showing too
        foreach (GameObject go in tableOnOffObjects)
            go.SetActive(enable);
    }

    public void ToggleCasinoRoom (bool enable)
    {
        Debug.Log ("Toggling casino room enable: " + enable);
        casinoRoom.SetActive (enable);
        if (enable)
            ToggleTableAndDealer(true); // don't allow casino room to be turned on without table and dealer showing too
        if (enable && gyroRealCameraTexture.activeSelf) {
            // Turn off the real camera feed texture if we're in Gyro Cam AR mode and turning on the casino room display
            gyroRealCameraTexture.SetActive (false);
        }
    }

    public void ToggleAR (bool enable)
    {
        ToggleAR (enable, false, false);
    }

    public void ToggleAR (bool enable, bool casinoOrNot, bool customTarget)
    {
        // Toggle Gyro
        if (gyroCamera.enabled)
            ToggleGyroCamera (false);

        // Enable/disable AR
        if (enable) {
            Debug.Log ("Toggling AR on");
            if (!customTarget) {
               // Use money bills ($, yen etc) as AR targets
               imageTargets.SetActive (true);
            } else {
            }
            roof.SetActive (false); // show no roof if in AR mode so we can see down on things
            ToggleCasinoRoom (casinoOrNot);
            if (birdsEyeSqueezeCamera.enabled) {
                lastActiveCamera = birdsEyeSqueezeCamera.GetComponent<Camera>().gameObject;
            }
            else
                lastActiveCamera = activeMainCamera.gameObject;
            ToggleLastActiveCamera(false);
            GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.Normal);
            arCamera.SetActive (true);
            chipsScreenRaycaster.Cameras[0] = arCamera.GetComponent<Camera>();
            cardsScreenRaycaster.Cameras[0] = arCamera.GetComponent<Camera>();
            mainCamera.enabled = false;

            bool focusModeSet = true ; // Simon temp disable: CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
            if (!focusModeSet) {
                Debug.Log("Failed to set focus mode (unsupported mode).");
            } else {
                Debug.Log("QCAR FOCUS_MODE_CONTINUOUSAUTO set");
            }
        } else {
            //if (!casinoOrNot || Utils.isPreferenceEnabled ("pref_key_graphics_settings_show_casino_room", true)) {
            Debug.Log ("Toggling AR off");
            imageTargets.SetActive (false);
            roof.SetActive (true); // show no roof if in AR mode so we can see down on things
            ToggleCasinoRoom (true);
            arCamera.SetActive (false);
            ToggleLastActiveCamera(true);
        }
    }

    public void ToggleGyroCamera (bool enable)
    {
        ToggleGyroCamera (enable, false);
    }

    public void ToggleGyroCamera (bool enable, bool casinoOrNot)
    {
        // Disable AR
        if (arCamera.activeSelf)
            ToggleAR (false);

        // Enable/disable gyro camera
        if (enable) {
            Debug.Log ("Toggling gyro camera on");
            if (birdsEyeSqueezeCamera.enabled) {
                lastActiveCamera = birdsEyeSqueezeCamera.GetComponent<Camera>().gameObject;
            }
            ToggleLastActiveCamera(false);
            gyroCamera.enabled = true;
            if (!casinoOrNot)
                gyroRealCameraTexture.SetActive (true);
            activeMainCamera = gyroCamera;
            chipsScreenRaycaster.Cameras[0] = gyroCamera;
            cardsScreenRaycaster.Cameras[0] = gyroCamera;
            gyroCamera.GetComponent<GyroCam> ().enabled = true;
            gyroCamera.GetComponent<PinchRecognizer> ().enabled = true;
            //gyroCamera.GetComponent<DragRecognizer>().enabled = true;
            gyroCamera.GetComponent<GesturesCamera> ().enabled = true;
            roof.SetActive (casinoOrNot);
            casinoRoom.SetActive (casinoOrNot);
            GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.Normal);
        } else {
            Debug.Log ("Toggling gyro camera off");
            gyroCamera.enabled = false;
            gyroCamera.GetComponent<GyroCam> ().enabled = false;
            gyroRealCameraTexture.SetActive (false);
            ToggleLastActiveCamera(true);
            activeMainCamera = mainCamera;
            gyroCamera.GetComponent<PinchRecognizer> ().enabled = false;
            //gyroCamera.GetComponent<DragRecognizer>().enabled = false;
            gyroCamera.GetComponent<GesturesCamera> ().enabled = false;
            roof.SetActive (true);
            casinoRoom.SetActive (true);
        }
    }

    public void ToggleSqueezeCamera (bool enable)
    {
        //return;//TODO:remove

        // Do nothing if in AR or Gyro mode
        if (arCamera.activeSelf || gyroCamera.enabled)
            return;

        // Enable/disable squeeze camera
        if (enable) {
            Debug.Log ("Toggling squeeze cameras on");
            ToggleMainCamera (false);
            activeMainCamera.enabled = false;
            //dealtCardsCamera.enabled = false;
            //squeezeCamera.enabled = true;
            birdsEyeSqueezeCamera.gameObject.SetActive(true);
            birdsEyeSqueezeCamera.enabled = true;
            chipsScreenRaycaster.Cameras[0] = birdsEyeSqueezeCamera;
            cardsScreenRaycaster.Cameras[0] = birdsEyeSqueezeCamera;
            mainMenuOfCircleButtons.SetActive (false); // circle open/close grey button overlaps the pic in pic cameras and closing it would raise the stats panel over the player one
            GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.NoText);

            // Turn on pic-in-pic cameras so we can see all cards while zoomed up on the one we're squeezing
            if (GameState.Instance.getCurrentBetType () == GameState.BetType.Banker) {
                TogglePicInPicPlayerOtherCamera (true);
                TogglePicInPicBankerBetOnCamera (true);
            } else if (GameState.Instance.getCurrentBetType () == GameState.BetType.Player) {
                TogglePicInPicBankerOtherCamera (true);
                TogglePicInPicPlayerBetOnCamera (true);
            }
            GameState.Instance.guiControls.ShowHideStatsPanelCompletely (false);
            //squeezeCameraOn = true;
        } else {
            Debug.Log ("Toggling squeeze cameras off");
            TogglePicInPicPlayerOtherCamera (false);
            TogglePicInPicBankerOtherCamera (false);
            TogglePicInPicPlayerBetOnCamera (false);
            TogglePicInPicBankerBetOnCamera (false);
            GameState.Instance.guiControls.ShowHideStatsPanelCompletely (true);
            //squeezeCamera.enabled = false;
            GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.Normal);
            birdsEyeSqueezeCamera.enabled = false;
            mainMenuOfCircleButtons.SetActive (true);
            ToggleMainCamera (true);
            mainCamera.GetComponent<Camera>().enabled = false;
            activeMainCamera.enabled = true;
            //squeezeCameraOn = false;
        }
    }

    /** NOT USED
    // Raise the squeeze camera a little to see the hands better while doing the squeeze tutorials
    public void setTutorialSqueezePos(bool enable) {
        Debug.Log("Setting the squeeze camera into position for tutorials: " + enable);
        if (enable)
            // Raise
            iTween.MoveBy(squeezeCamera.gameObject, iTween.Hash ("amount", new Vector3(0, 0.5f, 0f), "time", 0.3f));
        else
            // Reset position
            iTween.MoveTo(squeezeCamera.gameObject, squeezeCameraOriginalPosition, 0.3f);
    }
    */

    public void ToggleDealtCardsCamera (bool enable, string pos)
    {
        //return;//TODO:remove

        // Do nothing if in AR or Gyro mode
        if (arCamera.activeSelf || gyroCamera.enabled)
            return;

        if (enable && pos != null) {
            switch (pos) {
            case "player":
                Debug.Log ("Moving dealt cards camera player (left)");
                iTween.MoveTo (dealtCardsCamera.gameObject, dealtCardsCameraOriginalPosition + new Vector3 (0.225f, -0.02f, 0.1299023f), 0.4f);
                break;

            case "banker":
                Debug.Log ("Moving dealt cards camera banker (right)");
                iTween.MoveTo (dealtCardsCamera.gameObject, dealtCardsCameraOriginalPosition + new Vector3 (-0.252f, -0.02f, 0.05f), 0.4f);
                break;

            case "center":
            default:
                Debug.Log ("Moving dealt cards camera to center");
                if (!GUIControls.isPortrait) {
                    // Center the camera better when in landscape
                    if (GameState.Instance.getCurrentBetType() == GameState.BetType.Banker)
                        iTween.MoveTo (dealtCardsCamera.gameObject, dealtCardsCameraOriginalPosition + new Vector3(-0.02f, 0.04f, 0.17f), 0.3f);
                    else if (GameState.Instance.getCurrentBetType() == GameState.BetType.Player)
                        iTween.MoveTo (dealtCardsCamera.gameObject, dealtCardsCameraOriginalPosition + new Vector3(0.0468457f, 0.02f, 0), 0.3f);
                } else
                    iTween.MoveTo (dealtCardsCamera.gameObject, dealtCardsCameraOriginalPosition, 0.3f);
                break;
            }

            currentDealtCardsCameraPos = pos;
        } else if (!enabled) {
            Debug.Log ("Default moving dealt cards camera to center");
            iTween.MoveTo (dealtCardsCamera.gameObject, dealtCardsCameraOriginalPosition, 0.4f);
            currentDealtCardsCameraPos = "center";
        }


        // Show other cards
        if ("player" == pos) {
            dealtCardsOtherPlayerCamera.enabled = true;
        } else if ("banker" == pos) {
            dealtCardsOtherBankerCamera.enabled = true;
        }

        // Enable/disable dealt cards camera
        if (enable) {
            Debug.Log ("Toggling dealt cards camera on");
            activeMainCamera.enabled = false;
            //squeezeCamera.enabled = false;
            birdsEyeSqueezeCamera.enabled = false;
            dealtCardsCamera.enabled = true;
            GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.NoText);
            TogglePicInPicBankerBetOnCamera(false);
            TogglePicInPicBankerOtherCamera(false);
            TogglePicInPicPlayerBetOnCamera(false);
            TogglePicInPicPlayerOtherCamera(false);
        } else {
            Debug.Log ("Toggling dealt cards camera off");
            dealtCardsCamera.enabled = false;
            GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.Normal);
            // Redisplay the chips
            if (GameState.Instance.chipsManager != null) {
                GameState.Instance.chipsManager.clearChipsForSqueezing (false);
            }
            dealtCardsOtherBankerCamera.enabled = false;
            dealtCardsOtherPlayerCamera.enabled = false;
            activeMainCamera.enabled = true;
        }
    }

    // Get current position of squeeze camera
    public string getSqueezeCameraCurrentPosition ()
    {
        // This method should only be called by logic interested in knowing whether the camera is left or right,
        // And if the current one isn't one of those two return it's old position (assumption is that the old
        // one would be left or right - check out my ugly logic in moveSqueezeCamera! :-)
        if (currentSqueezeCameraPos.Equals ("left") || currentSqueezeCameraPos.Equals ("right")) {
            Debug.Log ("getSqueezeCameraCurrentPosition: returning currentSqueezeCameraPos==" + currentSqueezeCameraPos);
            return currentSqueezeCameraPos;
        } else {
            Debug.Log ("getSqueezeCameraCurrentPosition: returning oldSqueezeCameraPos==" + oldSqueezeCameraPos);
            return oldSqueezeCameraPos;
        }
    }

    // Return if the camera position has changed
    public bool hasCameraPositionChanged ()
    {
        bool retVal = cameraPositionHasChanged;
        cameraPositionHasChanged = false; // reset it on first query
        return retVal;
    }

    // Move the squeeze camera slightly to the left, center or right
    // Player/banker 1st cards - left
    // Player/banker 3rd cards - center
    // Player/banker 2nd cards - right
    // There are two types of squeeze camera: normal squeeze camera and birds eye. Will show birds eye
    // when phone sensor is facedown or faceup
    public void moveSqueezeCamera (string pos) {
        moveSqueezeCamera(pos, false);
    }
    public void moveSqueezeCamera (string pos, bool flurryOrNot)
    {
        if (pos == currentSqueezeCameraPos)
            return;

        switch (pos) {
        case "left":
            if (currentSqueezeCameraPos != "up") {
                Debug.Log ("Moving squeeze cameras left. Old position were " + currentSqueezeCameraPos);
                //iTween.MoveTo (squeezeCamera.gameObject, squeezeCameraOriginalPosition + new Vector3 (-0.05f, -0.01f, 0f), 0.3f);
                iTween.MoveTo (birdsEyeSqueezeCamera.gameObject, birdsEyeSqueezeCameraOriginalPosition + new Vector3 (-0.05f, -0.01f, -0.0025f), 0.3f);
            }
            break;
        case "right":
            if (currentSqueezeCameraPos != "up") {
                Debug.Log ("Moving squeeze cameras right. Old position were " + currentSqueezeCameraPos);
                //iTween.MoveTo (squeezeCamera.gameObject, squeezeCameraOriginalPosition + new Vector3 (0.055f, 0.0f, 0f), 0.3f);
                iTween.MoveTo (birdsEyeSqueezeCamera.gameObject, birdsEyeSqueezeCameraOriginalPosition + new Vector3 (0.306f, 0.0f, 0.0025f), 0.3f);
            }
            break;
        case "up":
            //Debug.Log ("Moving squeeze cameras up. Old position were " + currentSqueezeCameraPos);
            //iTween.MoveTo (squeezeCamera.gameObject, squeezeCamera.gameObject.transform.position + new Vector3 (0f, 0.02f, 0.0f), 0.9f);
            //iTween.MoveTo (birdsEyeSqueezeCamera.gameObject, birdsEyeSqueezeCameraOriginalPosition + new Vector3 (0f, 0.02f, 0.0f), 0.9f);
            break;
        case "backDown":
            //Debug.Log ("Moving squeeze cameras back down. Old position were " + currentSqueezeCameraPos);
            //iTween.MoveTo (squeezeCamera.gameObject, squeezeCamera.gameObject.transform.position + new Vector3 (0f, -0.02f, 0.0f), 0.9f);
            //iTween.MoveTo (birdsEyeSqueezeCamera.gameObject, birdsEyeSqueezeCameraOriginalPosition + new Vector3 (0f, -0.02f, 0.0f), 0.9f);
            break;
        case "otherside":
            Debug.Log ("Moving squeeze cameras to other side");
            if (currentSqueezeCameraPos == "left") {

                if (flurryOrNot)
                    LogUtils.LogEvent(Consts.FE_BTN_OTHER_CARD_EVENT, new string[] { Consts.FEP_BTN_LEFT_CARD }, false);

                moveSqueezeCamera ("right");
                pos = "right";
            } else if (currentSqueezeCameraPos == "right") {
                if (flurryOrNot)
                    LogUtils.LogEvent(Consts.FE_BTN_OTHER_CARD_EVENT, new string[] { Consts.FEP_BTN_RIGHT_CARD }, false);
                 
                moveSqueezeCamera ("left");
                pos = "left";
            }
            break;
        case "center":
        default:
            Debug.Log ("Moving squeeze cameras center. Old position were " + currentSqueezeCameraPos);
            //iTween.MoveTo (squeezeCamera.gameObject, squeezeCameraOriginalPosition, 0.3f);
            iTween.MoveTo (birdsEyeSqueezeCamera.gameObject, birdsEyeSqueezeCameraOriginalPosition, 0.3f);
            break;
        }

        oldSqueezeCameraPos = currentSqueezeCameraPos;
        currentSqueezeCameraPos = pos;
        cameraPositionHasChanged = true;
    }

    public void TogglePicInPicPlayerOtherCamera (bool enable)
    {
        // Do nothing if in AR or Gyro mode
        if (arCamera.activeSelf || gyroCamera.enabled)
            return;

        // Don't turn them on if we're in tutorial mode
        if (GameState.Instance.tutorialCounter <= 1 && enable)
            return;

        // Enable/disable pic-in-pic showing player cards when the player is the other hand (i.e. you bet on the banker)
        if (enable) {
            Debug.Log ("Toggling pic-in-pic player-is-the-other-hand camera on");
            picInPicPlayerOtherCamera.enabled = true;
        } else {
            Debug.Log ("Toggling pic-in-pic player-is-the-other-hand camera off");
            picInPicPlayerOtherCamera.enabled = false;
        }
    }

    public void TogglePicInPicBankerOtherCamera (bool enable)
    {
        // Do nothing if in AR or Gyro mode
        if (arCamera.activeSelf || gyroCamera.enabled)
            return;

        // Don't turn them on if we're in tutorial mode
        if (GameState.Instance.tutorialCounter <= 1 && enable)
            return;

        // Enable/disable pic-in-pic showing banker cards when the banker is the other hand (i.e. you bet on the player)
        if (enable) {
            Debug.Log ("Toggling pic-in-pic banker-is-the-other-hand camera on");
            picInPicBankerOtherCamera.enabled = true;
        } else {
            Debug.Log ("Toggling pic-in-pic banker-is-the-other-hand camera off");
            picInPicBankerOtherCamera.enabled = false;
        }
    }

    public void TogglePicInPicPlayerBetOnCamera (bool enable)
    {
        // Do nothing if in AR or Gyro mode
        if (arCamera.activeSelf || gyroCamera.enabled)
            return;

        // Don't turn them on if we're in tutorial mode
        if (GameState.Instance.tutorialCounter <= 1 && enable)
            return;

        // Enable/disable pic-in-pic showing player cards when the player is the hand you bet on
        if (enable) {
            Debug.Log ("Toggling pic-in-pic player-is-the-hand-bet-on camera on");
            picInPicPlayerBetOnCamera.enabled = true;
        } else {
            Debug.Log ("Toggling pic-in-pic player-is-the-hand-bet-on camera off");
            picInPicPlayerBetOnCamera.enabled = false;
        }
    }

    public void TogglePicInPicBankerBetOnCamera (bool enable)
    {
        // Do nothing if in AR or Gyro mode
        if (arCamera.activeSelf || gyroCamera.enabled)
            return;

        // Don't turn them on if we're in tutorial mode
        if (GameState.Instance.tutorialCounter <= 1 && enable)
            return;

        // Enable/disable pic-in-pic showing banker cards when the banker is the hand you bet on
        if (enable) {
            Debug.Log ("Toggling pic-in-pic banker-is-the-hand-bet-on camera on");
            picInPicBankerBetOnCamera.enabled = true;
        } else {
            Debug.Log ("Toggling pic-in-pic banker-is-the-hand-bet-on camera off");
            picInPicBankerBetOnCamera.enabled = false;
        }
    }

    // Cameras for showing player and banker cards together.
    // Used when user bets only on a tie and therefore no player or banker card squeezing happens.
    public void ToggleTieCameras (bool enable)
    {
        // Do nothing if in AR or Gyro mode
        if (arCamera.activeSelf || gyroCamera.enabled)
            return;

        // Enable/disable tie cameras
        if (enable) {
            Debug.Log ("Toggling tie cameras on");
            tiePlayerCamera.SetActive (true);
            tieBankerCamera.SetActive (true);
            GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.NoText);
            GameState.Instance.guiControls.ShowHideStatsPanelCompletely (false);
            mainMenuOfCircleButtons.SetActive (false);
        } else {
            Debug.Log ("Toggling tie cameras off");
            tiePlayerCamera.SetActive (false);
            tieBankerCamera.SetActive (false);
            GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.Normal);
            GameState.Instance.guiControls.ShowHideStatsPanelCompletely (true);
            mainMenuOfCircleButtons.SetActive (true);
        }
    }

    // Toggle the special extra main camera used just for speech bubbles in tutorials
    public void ToggleSpeechBubbleCamera(bool enable) {
        // Do nothing if in AR or Gyro mode
        if (arCamera.activeSelf || gyroCamera.enabled)
            return;

        // Enable/disable speech bubble camera
        if (enable) {
            Debug.Log ("Toggling speech bubble camera on");
            speechBubbleCamera.enabled = true;

            // Disable the pic-in-pic cameras while we show the tutorials because the speech bubbles appear there too
            TogglePicInPicBankerBetOnCamera(false);
            TogglePicInPicBankerOtherCamera(false);
            TogglePicInPicPlayerBetOnCamera(false);
            TogglePicInPicPlayerOtherCamera(false);
            GameState.Instance.guiControls.ToggleRevealOtherButton(false);
        } else {
            Debug.Log ("Toggling speech bubble camera off");
            speechBubbleCamera.enabled = false;
            if (GameState.Instance.getCurrentBetType() == GameState.BetType.Banker) {
                StartCoroutine(turnOnPicInPicCamerasForBankerBet());
            } else if (GameState.Instance.getCurrentBetType() == GameState.BetType.Player) {
                StartCoroutine(turnOnPicInPicCamerasForPlayerBet());
            }
        }
    }

    IEnumerator turnOnPicInPicCamerasForBankerBet() {
        yield return new WaitForSeconds(3f);
        TogglePicInPicBankerBetOnCamera(true);
        TogglePicInPicPlayerOtherCamera(true);
        GameState.Instance.tutorialHelpManager.speechBubbleStartSqueezing.SetActive(false);
        GameState.Instance.guiControls.ToggleRevealOtherButton(true);
    }

    IEnumerator turnOnPicInPicCamerasForPlayerBet() {
        yield return new WaitForSeconds(3f);
        TogglePicInPicPlayerBetOnCamera(true);
        TogglePicInPicBankerOtherCamera(true);
        GameState.Instance.tutorialHelpManager.speechBubbleStartSqueezing.SetActive(false);
        GameState.Instance.guiControls.ToggleRevealOtherButton(true);
    }
    
    // Reset view back to main camera
    public void resetToMainCamera ()
    {
        // Do nothing if in AR/Gyro mode
        if (arCamera.activeSelf || gyroCamera.enabled)
            return;

        Debug.Log ("Resetting to main camera");
        ToggleAR (false);
        ToggleSqueezeCamera (false);
        ToggleDealtCardsCamera (false, null);
        ToggleTieCameras (false);
        ToggleMainCamera (true);
        activeMainCamera = mainCamera;
    }

    public bool isAR ()
    {
        return arCamera.activeSelf || gyroCamera.enabled;
    }
}
