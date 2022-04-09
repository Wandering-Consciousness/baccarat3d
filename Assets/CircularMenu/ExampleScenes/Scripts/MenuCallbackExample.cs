using UnityEngine;
using System.Collections;

public class MenuCallbackExample : MonoBehaviour 
{
    // Menu you want to listen
    public CircularMenu listenMenu;

    public GUIStyle TextStyle;
    public Rect TextPosition;
    private string messageToShow;

    void Start()
    {
        listenMenu.ActivatedCallBack += MenuJustOpened;
        listenMenu.DesactivatedCallBack += MenuJustClosed;
    }

    /// <summary>
    /// Function called when the spotted menu is opened
    /// </summary>
    private void MenuJustOpened()
    {
        messageToShow = listenMenu.name + " have just been opened.";
    }

    /// <summary>
    /// Function called when the spotted menu is closed
    /// </summary>
    private void MenuJustClosed()
    {
        messageToShow = listenMenu.name + " have just been closed.";
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width * TextPosition.x, Screen.height * TextPosition.y, Screen.width * TextPosition.width, Screen.height * TextPosition.height), messageToShow, TextStyle);
    }

}
