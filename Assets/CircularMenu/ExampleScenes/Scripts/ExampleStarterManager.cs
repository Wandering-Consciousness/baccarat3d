using UnityEngine;
using System.Collections;

public class ExampleStarterManager : MonoBehaviour 
{

    public void LoadMap(string _mapName)
    {
        Application.LoadLevel(_mapName);
    }

}
