using UnityEngine;
using System.Collections;

public class Utils : MonoBehaviour
{

    // Use this for initialization
    void Start ()
    {
 
    }
 
    // Update is called once per frame
    void Update ()
    {

    }

    // Find out if a particular preference is enabled or not in native-side code
    public static bool isPreferenceEnabled (string prefName, bool defaultValue)
    {
#if (!UNITY_EDITOR && UNITY_ANDROID)
        if (Consts.DEBUG) {
            // Calling new AndroidJavaClass("mobi.aroha.cariana.util.Utils") caused Unity to crash when
            // deployed to Android as a standalone Unity project (and not merged with the BaccARat3D
            // Eclipse project). There were no helpful messages, just a hex stacktrace.
            Debug.Log ("Returning defaultValue " + defaultValue + " for " + prefName + " because Consts.DEBUG is true");
            return defaultValue;
        }

        // Get Android context
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass utils = new AndroidJavaClass("mobi.aroha.cariana.util.Utils");

        // Invoke the native code to check Android preferences
        if (utils != null) {
           bool enabled = utils.CallStatic<bool>("isPreferenceEnabled", activity, prefName);
           Debug.Log(prefName + " is enabled? " + enabled);
           return enabled;
        } else {
           // Return defaultValue by default
           Debug.Log("Could not find native setting for " + prefName + ". Defaulting to enabled? " + defaultValue);
           return defaultValue;
        }
#endif
        // Fallback; default to defaultValue if we trickle down this far
        return defaultValue;
    }

    // Find users preferred Language
    public static string getLanguage() {
        if (Consts.JAP_DEBUG_ON) {
            return "Japanese";
        } else {
            string sysLang = Application.systemLanguage.ToString ();
            return sysLang;
        }
    }

    public static bool isEnglish() {
        bool res = (getLanguage() != null && getLanguage().Contains("English"));
        //Debug.Log ("Is Japanese? " + res);
        return res;
    }

    public static bool isJapanese() {
        bool res = (getLanguage() != null && getLanguage().Contains("Japanese"));
        //Debug.Log ("Is Japanese? " + res);
        return res;
    }
}
