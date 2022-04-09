using UnityEngine;
using System.Collections;

public class BackToMenuScript : MonoBehaviour 
{
    public Rect btnPosition;
    public GUIStyle styleBtn;

    void OnGUI()
    {
        if (GUI.Button(new Rect(btnPosition.x * Screen.width, btnPosition.y * Screen.height, btnPosition.width * Screen.width, btnPosition.height * Screen.width), "", styleBtn))
            Application.LoadLevel("0_ExampleStarter");
    }

}
