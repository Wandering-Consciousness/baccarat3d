using UnityEngine;
using System.Collections;

// Class to attach to buttons that are circular and invoke a specific function on a specific gameobject
public class SingleCircularButton : MonoBehaviour
{
    public GameObject callbackObject;
    public string methodToInvoke;
    public Rect btnPositionPortrait;
    public Rect btnPositionLandscape;
    private Rect btnPosition {
        get {
            return isPortrait ? btnPositionPortrait : btnPositionLandscape;
        }
        set {}
    }
    public GUIStyle styleBtn;
    public AudioSource btnClick;
    private float buttonSize {
        get { return ( isPortrait ? 0.13f : 0.06f); }
        set {}
    }
    private bool isPortrait {
        get {
            return GUIControls.isPortrait;
        }
        set { }
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(btnPosition.x * Screen.width,
                                btnPosition.y * Screen.height,
                                buttonSize * Screen.width,
                                buttonSize * Screen.width),
            "", styleBtn)) {
            btnClick.Play();
            callbackObject.SendMessage(methodToInvoke);
        }
    }
}
