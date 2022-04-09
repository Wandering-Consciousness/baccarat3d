using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultipleMenuExample : MonoBehaviour
{
    // All menus
    public CircularMenu MainMenu;
    public List<CircularMenu> menus = new List<CircularMenu>();

    private int nextMenuIndexToShow = 0;

    void Start()
    {
        // Callbacks
        MainMenu.DesactivatedCallBack += ShowSelectedMenu;

        for (int i = 0; i < menus.Count; i++)
            menus[i].DesactivatedCallBack += ShowMainMenu;
    }

    private void ShowSelectedMenu()
    {
        menus[nextMenuIndexToShow].ShowMenu();
    }

    /// <summary>
    /// Hide the main menu and show the selected menu
    /// </summary>
    public void ShowMenuAt(int index)
    {
        MainMenu.HideMenu();
        nextMenuIndexToShow = index;
    }

    /// <summary>
    /// Show the main menu
    /// </summary>
    public void ShowMainMenu()
    {
        MainMenu.ShowMenu();
    }

}
