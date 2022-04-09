using UnityEngine;
using System.Collections;
 
public class StartGame : MonoBehaviour
{
    // Use this for initialization
    void Start ()
    {
    }

    void OnGUI ()
    {
        if (GUI.Button (new Rect (10, 10, 200, 60), "Start Game (shortcut)")) {
            Debug.Log ("Start Game button pressed");
            StartGameNow ("SinglePlayerScene");
        }
    }

    void StartGameNow (string levelName)
    {
        Debug.Log ("Starting Single Player scene");
        LoadNextLevel.nextLevelName = levelName;
        Application.LoadLevel ("LoadNextLevel");
    }

    void ReloadMainMenuScene ()
    {
        Debug.Log ("Restarting Main Menu scene");
        LoadNextLevel.nextLevelName = "MainMenuScene";
        Application.LoadLevel ("LoadNextLevel");
    }

}