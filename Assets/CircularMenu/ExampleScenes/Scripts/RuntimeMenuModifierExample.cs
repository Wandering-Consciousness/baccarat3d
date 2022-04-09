using UnityEngine;
using System.Collections;

public class RuntimeMenuModifierExample : MonoBehaviour 
{

    public CircularMenu menuToModify;
    public Texture2D defaultButtonTexture;
    public Texture2D hoverButtonTexture;
    public Texture2D clickButtonTexture;

    void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width * 0.2f, Screen.height * 0.86f, Screen.width * 0.15f, Screen.height * 0.05f), "Add category"))
        {
            AddCategory();
            AddButtonToLastCategory();
        }

        if (GUI.Button(new Rect(Screen.width * 0.35f, Screen.height * 0.86f, Screen.width * 0.15f, Screen.height * 0.05f), "Add Button"))
            AddButtonToLastCategory();

        if (GUI.Button(new Rect(Screen.width * 0.5f, Screen.height * 0.86f, Screen.width * 0.15f, Screen.height * 0.05f), "Remove Button"))
            RemoveLastButtonFromLastCategory();

        if (GUI.Button(new Rect(Screen.width * 0.65f, Screen.height * 0.86f, Screen.width * 0.15f, Screen.height * 0.05f), "Remove category"))
            RemoveLastCategory();

    }

    /// <summary>
    /// Add a default category
    /// </summary>
    private void AddCategory()
    {
        CircularMenuCategory category = menuToModify.AddCategory();
        category.name = "New category " + menuToModify.menuCategories.Count;
        category.showCategoryName = true;
        category.textStyle.normal.textColor = Color.white;
        category.textStyle.fontSize = 20;
        category.textLabelSize.x = 0.2f;

    }

    /// <summary>
    /// Remove the last created category
    /// </summary>
    private void RemoveLastCategory()
    {
        if(menuToModify.menuCategories.Count > 0)
            menuToModify.RemoveCategory(menuToModify.menuCategories[menuToModify.menuCategories.Count - 1]);
    }

    /// <summary>
    /// Add a default button to the last category
    /// </summary>
    private void AddButtonToLastCategory()
    {
        CircularMenuButton button = menuToModify.menuCategories[menuToModify.menuCategories.Count - 1].AddButton();
        button.buttonStyle.normal.background = defaultButtonTexture;
        button.buttonStyle.hover.background = hoverButtonTexture;
        button.buttonStyle.active.background = clickButtonTexture;
    }

    /// <summary>
    /// Remove the last button from the last category
    /// </summary>
    private void RemoveLastButtonFromLastCategory()
    {
        CircularMenuCategory tempCategory = menuToModify.menuCategories[menuToModify.menuCategories.Count - 1];
        if(tempCategory.buttonList.Count > 0)
            tempCategory.RemoveButton(tempCategory.buttonList[tempCategory.buttonList.Count - 1]);
    }



}
