using UnityEngine;
using System.Collections;

public class SimpleMenuExample : MonoBehaviour 
{
    public GUIStyle TextStyle;
    public Rect TextPosition;
    private string messageToShow;
    
    /// <summary>
    /// Function called when the spotted menu is opened
    /// </summary>
    private void LogInfo(string _info)
    {
        messageToShow = _info;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width * TextPosition.x, Screen.height * TextPosition.y, Screen.width * TextPosition.width, Screen.height * TextPosition.height), messageToShow, TextStyle);
    }

}
