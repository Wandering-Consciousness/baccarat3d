// Gyroscope-controlled camera for iPhone & Android revised 2.26.12
// Perry Hoberman <hoberman@bway.net>
//
// Usage:
// Attach this script to main camera.
// Note: Unity Remote does not currently support gyroscope.
//
// This script uses three techniques to get the correct orientation out of the gyroscope attitude:
// 1. creates a parent transform (camParent) and rotates it with eulerAngles
// 2. for Android (Samsung Galaxy Nexus) only: remaps gyro.Attitude quaternion values from xyzw to wxyz (quatMap)
// 3. multiplies attitude quaternion by quaternion quatMult
// Also creates a grandparent (camGrandparent) which can be rotated with localEulerAngles.y
// This node allows an arbitrary heading to be added to the gyroscope reading
// so that the virtual camera can be facing any direction in the scene, no matter what the phone's heading
//
// Ported to C# by Simon McCorkindale <simon <at> aroha.mobi>
using UnityEngine;

public class GyroCam : MonoBehaviour
{
    private bool gyroBool;
    private Gyroscope gyro;
    private Quaternion rotFix;
    public GUISkin scrollbarSkin;
    Vector3 originalRot;

    public void Start ()
    {
        Transform currentParent = transform.parent;
        GameObject camParent = new GameObject ("GyroCamParent");
        camParent.transform.position = transform.position;
        transform.parent = camParent.transform;
        GameObject camGrandparent = new GameObject ("GyroCamGrandParent");
        camGrandparent.transform.position = transform.position;
        camParent.transform.parent = camGrandparent.transform;
        camGrandparent.transform.parent = currentParent;
        originalRot = transform.rotation.eulerAngles;

        if (scrollbarSkin == null) {
            // For some reason the attached scrollbarSkin on this object was null on iOS so OnGUI invokes threw an exception.
            // Hack here to get the reference to an instance from another object.
            scrollbarSkin = GameState.Instance.guiControls.gyroScrollbarSkin;
            Debug.LogWarning ("GyroCam scrollbarSkin was null... got from GUIControls: " + scrollbarSkin);
        }

        #if UNITY_3_4
        gyroBool = Input.isGyroAvailable;
        #else
        gyroBool = SystemInfo.supportsGyroscope;
        #endif
    
        if (gyroBool) {
         
            gyro = Input.gyro;
            gyro.enabled = true;
         
            if (Screen.orientation == ScreenOrientation.LandscapeLeft) {
                camParent.transform.eulerAngles = new Vector3 (90, 180, 0);
            } else if (Screen.orientation == ScreenOrientation.Portrait) {
                camParent.transform.eulerAngles = new Vector3 (90, 180, 0);
            } else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown) {
                camParent.transform.eulerAngles = new Vector3 (90, 180, 0);
            } else if (Screen.orientation == ScreenOrientation.LandscapeRight) {
                camParent.transform.eulerAngles = new Vector3 (90, 180, 0);
            } else {
                camParent.transform.eulerAngles = new Vector3 (90, 180, 0);
            }

            if (Screen.orientation == ScreenOrientation.LandscapeLeft) {
                rotFix = new Quaternion (0, 0, 1, 0);
            } else if (Screen.orientation == ScreenOrientation.Portrait) {
                rotFix = new Quaternion (0, 0, 1, 0);
            } else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown) {
                rotFix = new Quaternion (0, 0, 1, 0);
            } else if (Screen.orientation == ScreenOrientation.LandscapeRight) {
                rotFix = new Quaternion (0, 0, 1, 0);
            } else {
                rotFix = new Quaternion (0, 0, 1, 0);
            }

            //Screen.sleepTimeout = 0;
        } else {
            #if UNITY_EDITOR
            print("NO GYRO");
            #endif
        }
    }
    
    public void Update ()
    {
        if (gyroBool && this.gameObject.GetComponent<Camera>().enabled) {
            Quaternion quatMap;
            #if UNITY_IPHONE
                quatMap = gyro.attitude;

                // only rotate y-axis so we can use the phone at various angles and still see the necessary parts of the room (not just the roof/floor etc.)
                quatMap.x = quatMap.y = 0;
                Quaternion quatMap2 = new Quaternion();
                quatMap2.eulerAngles = new Vector3(originalRot.x, -quatMap.eulerAngles.z+scrollPosition-180, originalRot.z);
                transform.rotation = quatMap2;

                 //Debug.Log ("x: " + transform.localRotation.eulerAngles.x + ", "
                 //       + "y: " + transform.localRotation.eulerAngles.y + ", "
                 //       + "z: " + transform.localRotation.eulerAngles.z);

                //Debug.Log("gyro : quatMap * rotFix: " + transform.localRotation);
                //Debug.Log ("gryo attitude: " + gyro.attitude);
            #elif UNITY_ANDROID
                quatMap = new Quaternion(gyro.attitude.x,gyro.attitude.y,gyro.attitude.z,gyro.attitude.w);

                // only rotate y-axis so we can use the phone at various angles and still see the necessary parts of the room (not just the roof/floor etc.)
                quatMap.x = quatMap.y = 0;
                Quaternion quatMap2 = new Quaternion();
                quatMap2.eulerAngles = new Vector3(originalRot.x, -quatMap.eulerAngles.z+scrollPosition, originalRot.z);
                transform.rotation = quatMap2;

            #endif
        }
    }

    float scrollPosition_ = 180f;
    bool scrollFlurrified = false;
    float scrollPosition {
        get {
            return scrollPosition_;
        }
        set {
            if (scrollPosition_ != 180f && !scrollFlurrified) {
                LogUtils.LogEvent(Consts.FE_GYROCAM_SCROLL);
                scrollFlurrified = true;
            }
            
            scrollPosition_ = value;
        }
    }

    public void OnGUI ()
    {
        // Draw horizontal scroll bar to rotate the gyro cam around the room... can be used to manually adjust for phones with not-so-accurate gyrometers/accelerometers
        GUISkin origSkin = GUI.skin;
        float scrollBarThickness = Screen.width * 0.10f;
        if (!GUIControls.isPortrait)
            scrollBarThickness /= 2;
        float barWidth = Screen.width - 2 * scrollBarThickness;

        if (scrollbarSkin == null) {
            Debug.LogWarning ("GyroCam: setting scrollbarSkin to default GUI.skin");
            scrollbarSkin = GUI.skin;
        } else {
            GUI.skin = scrollbarSkin;
        }

        scrollbarSkin.horizontalScrollbarThumb.fixedWidth = 2*scrollBarThickness;
        scrollbarSkin.horizontalScrollbar.fixedWidth = barWidth;
        scrollbarSkin.horizontalScrollbar.fixedHeight = scrollBarThickness;
        scrollbarSkin.horizontalScrollbarThumb.fixedHeight = scrollBarThickness;

        GUI.skin = scrollbarSkin;
        scrollPosition = GUI.HorizontalScrollbar (new Rect (scrollBarThickness, Screen.height - Screen.height * 0.14f, barWidth, Screen.height * 0.05f), scrollPosition, 1.0f, 0.0f, 360.0f);
        //if (GUI.changed) {
            //Debug.LogWarning("GyroCam scroll change is " + scroll);
        //}

        GUI.skin = origSkin;
    }
}
