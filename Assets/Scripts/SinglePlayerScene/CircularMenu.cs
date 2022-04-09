using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SendMessageParameters
{
    public enum UsableParameterType { Int, Float, String, Bool };

    public GameObject elementToCall;
    public string functionToCall;
    public UsableParameterType paramType;
    public int paramInt;
    public float paramFloat;
    public bool paramBool;
    public string paramString;

    public SendMessageParameters()
    {
        elementToCall = null;
        functionToCall = "";
        paramString = "";
    }

    /// <summary>
    /// Function called when the message have to be send
    /// </summary>
    /// <param name="debug"> Does the error message is log ?</param>
    public void Use(string _callingElementName)
    {
        if (elementToCall != null)
        {
            if (functionToCall != "")
            {
                object sentParam;
                switch (paramType)
                {
                    case SendMessageParameters.UsableParameterType.Bool:
                        sentParam = paramBool;
                        break;
                    case SendMessageParameters.UsableParameterType.Float:
                        sentParam = paramFloat;
                        break;
                    case SendMessageParameters.UsableParameterType.Int:
                        sentParam = paramInt;
                        break;
                    case SendMessageParameters.UsableParameterType.String:
                        sentParam = paramString;
                        break;
                    default:
                        sentParam = paramString;
                        break;
                }
                elementToCall.SendMessage(functionToCall, sentParam);
            }
            else
                Debug.LogError("Button '" + _callingElementName + "' doesn't have a function to call");

        }
        else
            Debug.LogError("Button '" + _callingElementName + "' doesn't have a GameObject to call");
    }

}

[System.Serializable]
public class CircularMenuButton
{
    [SerializeField]
    private CircularMenuCategory parentCategory;
    public string name;
    public GUIStyle buttonStyle;
    public SendMessageParameters OnClickMessage;
    public SendMessageParameters OnReleaseMessage;
    public Rect viewRectangle;
    public bool isClicked = false;
    public int fingerId= -1;
    public bool showInEditor = false;


    public CircularMenuButton(CircularMenuCategory _parentCategory)
    {
        parentCategory = _parentCategory;
        Clear();
    }

    /// <summary>
    /// Function called to clear all the button's parameters
    /// </summary>
    public void Clear()
    {
        name = "Button";
        buttonStyle = new GUIStyle();
        OnClickMessage = new SendMessageParameters();
        OnReleaseMessage = new SendMessageParameters();
    }

    /// <summary>
    /// Function called when the button is pressed to call the linked function
    /// </summary>
    /// <param name="debug"> Does the error message is log ?</param>
    public void OnClick(int _fingerId)
    {
        fingerId = _fingerId;
        isClicked = true;

        if(OnClickMessage.elementToCall != null)
            OnClickMessage.Use(name);
    }

    /// <summary>
    /// Function called when the button is released to call the linked function
    /// </summary>
    /// <param name="debug"> Does the error message is log ?</param>
    public void OnRelease(int _fingerId)
    {
        if (isClicked && fingerId == _fingerId && OnReleaseMessage.elementToCall != null)
            OnReleaseMessage.Use(name);

        isClicked = false;
    }

}

[System.Serializable]
public class CircularMenuCategory
{
    [SerializeField]
    private CircularMenu parentMenu;
    public string name;
    public bool showCategoryName = false;
    public GUIStyle textStyle;
    public Vector2 textLabelSize;
    public List<CircularMenuButton> buttonList;
    public bool showInEditor = false;

    public CircularMenuCategory(CircularMenu _parentMenu)
    {
        name = "Category";
        textStyle = new GUIStyle();
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.fontStyle = FontStyle.Bold;
        textLabelSize = new Vector2(0.2f, 0.1f);
        buttonList = new List<CircularMenuButton>();
        parentMenu = _parentMenu;
    }

    /// <summary>
    /// Function use to add a button to the current category
    /// </summary>
    /// <returns> The created button </returns>
    public CircularMenuButton AddButton()
    {
        CircularMenuButton newButton = new CircularMenuButton(this);
        buttonList.Add(newButton);
        Debug.Log("Add");
        parentMenu.UpdateButtonsNumber();
        return newButton;
    }

    /// <summary>
    /// Function use to remove a specific button to the current category
    /// </summary>
    public void RemoveButton(CircularMenuButton _button)
    {
        buttonList.Remove(_button);
        if(parentMenu != null)
            parentMenu.UpdateButtonsNumber();
        _button = null;
    }

    /// <summary>
    /// Get a button from its name on the list or null
    /// </summary>
    /// <param name="_name">Button's name</param>
    /// <returns></returns>
    public CircularMenuButton GetButtonFromName(string _name)
    {
        return buttonList.Where(b => b.name == _name).FirstOrDefault();
    }

}

[System.Serializable]
public class CircularMenu : MonoBehaviour 
{
    private List<string> errorsList = new List<string>();

    //------- Menu position ------
    public enum PositionType { ScreenPercentage, ScreenRaycast };
    public PositionType usedPositionType;
    private Vector3 actualMenuPosition;

    //------- Basics -------
    public List<CircularMenuCategory> menuCategories = new List<CircularMenuCategory>();
    public bool startActive = false;
    public bool manuallySetRadius = false;
    public float circleRadius = 0.1f;
    public float buttonSize {
        get { return ( isPortrait ? 0.13f : 0.06f); }
        set {  }
    }
    private bool isPortrait {
        get {
            return GUIControls.isPortrait;
        }
        set { }
    }
    public float categoryNameRadius = 1.6f;
    public bool categoryNameNoUpsidedown = false;
    public float startMenuAngle = 180;
    public float menuMaxOpenAngle = 360;

    //------- Central button -------
    public bool showCentralButton = false;
    public bool centralButtonHideMenu = false;
    public CircularMenuButton centralButton = new CircularMenuButton(null);

    //------- Activation button -------
    public bool showActivationButton = false;
    public CircularMenuButton activationButton = new CircularMenuButton(null);

    //------- Transition -------
    public bool useTransitions = false;
    public bool useButtonsDuringTransition = false;
    public float transitionDuration = 1;
    public bool radiusTransition = false;
    public bool sizeTransition = false;
    public bool fanTransition = false;
    public bool fadeTransition = false;
    public float minAlpha = 0;
    public float maxAlpha = 1;
    public bool rotationTransition = false;
    public float maxRotation = 360;

    //------- Separator -------
    public bool showSeparators = false;
    public GUIStyle separatorStyle;
    public Vector2 separatorSize = new Vector2(0.8f, 0.05f);

    //------- Runtime -------
    public enum EtatMenu { Show, Hide, Active, Inactive };
    private EtatMenu actualState;
    private bool menuIsActive = false;
    private int totalElementNumber;
    private float actualAlpha;
    private float actualCircleRadius;
    private float startTime;
    private float actualRotAngle;
    private float actualBtnSize;
    private float actualOpenAngle;

    private float lockedAlpha;
    private float lockedRotAngle;
    private float lockedBtnSize;
    private float lockedRadius;
    private float lockedOpenAngle;
    
    public delegate void DefaultDelegate();
    // Callback called when the menu is activated
    public DefaultDelegate ActivatedCallBack;
    // Callback called when the menu is desactivated
    public DefaultDelegate DesactivatedCallBack;


    void Start()
    {
        UpdateButtonsNumber();

        actualState = EtatMenu.Inactive;

        // Auto active
        if (startActive)
            ShowMenu();
    }

    /// <summary>
    /// Function called to show the circular menu. It launch the transition if needed.
    /// </summary>
    public void ShowMenu(bool _instantly = false)
    {
        if (radiusTransition && useTransitions && !_instantly)
            actualCircleRadius = 0;
        else
            actualCircleRadius = circleRadius;

        if (fadeTransition && useTransitions && !_instantly)
            actualAlpha = 0;
        else
            actualAlpha = 1;

        if (sizeTransition && useTransitions && !_instantly)
            actualBtnSize = 0;
        else
            actualBtnSize = buttonSize;

        if (fanTransition && useTransitions && !_instantly)
            actualOpenAngle = 0;
        else
            actualOpenAngle = menuMaxOpenAngle;

        actualRotAngle = 0;

        menuIsActive = true;

        if (useTransitions && !_instantly)
        {
            startTime = Time.fixedTime;
            actualState = EtatMenu.Show;
        }
        else
            actualState = EtatMenu.Active;
    }

    /// <summary>
    /// Function called to hide the circular menu. It launch the transition if needed.
    /// </summary>
    public void HideMenu(bool _instantly = false)
    {
        if (actualState == EtatMenu.Active || actualState == EtatMenu.Show)
        {
            if (useTransitions && !_instantly)
            {
                lockedAlpha = actualAlpha;
                lockedBtnSize = actualBtnSize;
                lockedRotAngle = actualRotAngle;
                lockedRadius = actualCircleRadius;
                lockedOpenAngle = actualOpenAngle;

                startTime = Time.fixedTime;
                actualState = EtatMenu.Hide;
            }
            else
            {
                actualState = EtatMenu.Inactive;
                DesactivateMenu();
            }
        }
    }

    /// <summary>
    /// Function called to set the menu state to actif.
    /// It is the state of the menu when he is shown and all the transitions are done.
    /// </summary>
    private void StabiliseMenu()
    {
        if (ActivatedCallBack != null)
            ActivatedCallBack();

        actualCircleRadius = circleRadius;
        actualState = EtatMenu.Active;
    }

    /// <summary>
    /// Function called to desappear the circular menu.
    /// It is the state of the menu when he desappear and all the transitions are done.
    /// </summary>
    private void DesactivateMenu()
    {
        if (DesactivatedCallBack != null)
            DesactivatedCallBack();

        actualState = EtatMenu.Inactive;
        actualCircleRadius = 0;
        actualAlpha = 0;
        actualRotAngle = 0;
        menuIsActive = false;
    }

    void Update()
    {
        UpdateMenuPosition();
        UpdateAction();
        UpdateTransitions();

        // Baccarat 3D customizations:
        // Nicely adjust the menu to suit either landscape or portrait sizes and dimensions
        if (isPortrait) {
            actualMenuPosition.x = 0.57f;
            actualMenuPosition.y = 0.04f;
            circleRadius = 0.271f;
        } else {
            actualMenuPosition.x = 0.525f;
            actualMenuPosition.y = 0.055f;
            circleRadius = 0.141f;
        }
    }

    /// <summary>
    /// Update the menu position depending of the usedPositionType
    /// </summary>
    private void UpdateMenuPosition()
    {
        switch (usedPositionType)
        {
            case PositionType.ScreenPercentage:
                actualMenuPosition = transform.localPosition;
                break;
            case PositionType.ScreenRaycast:
                actualMenuPosition = Camera.main.WorldToViewportPoint(transform.position);
                actualMenuPosition.y = 1 - actualMenuPosition.y;
                break;
        }
    }

    /// <summary>
    /// Look if the user click on a button
    /// </summary>
    private void UpdateAction()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE || UNITY_WEBPLAYER
        if (Input.GetButtonDown("Fire1"))
        {
            Vector2 cursorPosition = Input.mousePosition;
            cursorPosition.y = Screen.height - cursorPosition.y;

            if (menuIsActive && ((actualState != EtatMenu.Show && actualState != EtatMenu.Hide) || useButtonsDuringTransition))
            {
                for (int i = 0; i < menuCategories.Count; i++)
                {
                    for (int y = 0; y < menuCategories[i].buttonList.Count; y++)
                    {
                        if (menuCategories[i].buttonList[y].viewRectangle.Contains(cursorPosition))
                            menuCategories[i].buttonList[y].OnClick(0);
                    }
                }

                if (showCentralButton)
                {
                    if (centralButton.viewRectangle.Contains(cursorPosition))
                    {
                        centralButton.OnClick(0);
                        if (centralButtonHideMenu)
                            HideMenu();
                    }
                }
            }
            else
            {
                if (showActivationButton && actualState == EtatMenu.Inactive)
                {
                    if (activationButton.viewRectangle.Contains(cursorPosition))
                    {
                        activationButton.OnClick(0);
                        ShowMenu();
                    }
                }
            }
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            centralButton.OnRelease(0);
            activationButton.OnRelease(0);
            for (int i = 0; i < menuCategories.Count; i++)
            {
                for (int y = 0; y < menuCategories[i].buttonList.Count; y++)
                    menuCategories[i].buttonList[y].OnRelease(0);
            }
        }
#elif UNITY_IPHONE || UNITY_ANDROID
        foreach (Touch item in Input.touches)
	    {
            if (item.phase == TouchPhase.Began)
            {
                Vector2 cursorPosition = item.position;
                cursorPosition.y = Screen.height - cursorPosition.y;

                if (menuIsActive && ((actualState != EtatMenu.Show && actualState != EtatMenu.Hide) || useButtonsDuringTransition))
                {
                    for (int i = 0; i < menuCategories.Count; i++)
                    {
                        for (int y = 0; y < menuCategories[i].buttonList.Count; y++)
                        {
                            if (menuCategories[i].buttonList[y].viewRectangle.Contains(cursorPosition))
                                menuCategories[i].buttonList[y].OnClick(item.fingerId);
                        }
                    }

                    if (showCentralButton)
                    {
                        if (centralButton.viewRectangle.Contains(cursorPosition))
                        {
                            centralButton.OnClick(item.fingerId);
                            if (centralButtonHideMenu)
                                HideMenu();
                        }
                    }
                }
                else
                {
                    if (showActivationButton)
                    {                    
                        if (activationButton.viewRectangle.Contains(cursorPosition))
                        {
                            activationButton.OnClick(item.fingerId);
                            ShowMenu();
                        }
                    }
                }
            }
            else if (item.phase == TouchPhase.Ended)
            {
                centralButton.OnRelease(item.fingerId);
                activationButton.OnRelease(item.fingerId);
                for (int i = 0; i < menuCategories.Count; i++)
                {
                    for (int y = 0; y < menuCategories[i].buttonList.Count; y++)
                        menuCategories[i].buttonList[y].OnRelease(item.fingerId);
                }
            }
        }
#endif
    }

    /// <summary>
    /// Change buttons values for transitions
    /// </summary>
    private void UpdateTransitions()
    {
        if (menuIsActive)
        {
            if (actualState == EtatMenu.Show)
            {
                float t = Mathf.Clamp01((Time.fixedTime - startTime) / transitionDuration);
                if (radiusTransition)
                    actualCircleRadius = Mathf.Lerp(0, circleRadius, t);
                if (fadeTransition)
                    actualAlpha = Mathf.Lerp(minAlpha, maxAlpha, t);
                if (rotationTransition)
                    actualRotAngle = Mathf.Lerp(0, maxRotation, t);
                if (sizeTransition)
                    actualBtnSize = Mathf.Lerp(0, buttonSize, t);
                if (fanTransition)
                    actualOpenAngle = Mathf.Lerp(0, menuMaxOpenAngle, t);

                if (t == 1)
                    StabiliseMenu();
            }
            else if (actualState == EtatMenu.Hide)
            {
                float t = Mathf.Clamp01((Time.fixedTime - startTime) / transitionDuration);
                if (radiusTransition)
                    actualCircleRadius = Mathf.Lerp(lockedRadius, 0, t);
                if (fadeTransition)
                    actualAlpha = Mathf.Lerp(lockedAlpha, minAlpha, t);
                if (rotationTransition)
                    actualRotAngle = Mathf.Lerp(lockedRotAngle, 0, t);
                if (sizeTransition)
                    actualBtnSize = Mathf.Lerp(lockedBtnSize, 0, t);
                if (fanTransition)
                    actualOpenAngle = Mathf.Lerp(lockedOpenAngle, 0, t);

                if (t == 1)
                    DesactivateMenu();
            }
            else if (actualState == EtatMenu.Active)
            {
                actualCircleRadius = circleRadius;
                actualOpenAngle = menuMaxOpenAngle;
                actualBtnSize = buttonSize;
            }
        }
    }

    void OnGUI()
    {
        if (menuIsActive)
        {
            if (totalElementNumber > 0)
            {
                float angleActuel = startMenuAngle + actualRotAngle;
                float ecartAngle = Mathf.Min(actualOpenAngle / (float)(totalElementNumber - 1), 360f / (float)totalElementNumber);
                float angleDebutCategorie = angleActuel;

                GUI.color = new Color(1, 1, 1, actualAlpha);

                // Central button
                if (showCentralButton)
                {
                    centralButton.viewRectangle = new Rect(Screen.width * (actualMenuPosition.x - actualBtnSize / 2), (Screen.height * actualMenuPosition.y) - (Screen.width * actualBtnSize / 2), Screen.width * actualBtnSize, Screen.width * actualBtnSize);
                    GUI.Button(centralButton.viewRectangle, "", centralButton.buttonStyle);
                    /*if (GUI.Button(new Rect(Screen.width * (actualMenuPosition.x - actualBtnSize / 2), (Screen.height * actualMenuPosition.y) - (Screen.width * actualBtnSize / 2), Screen.width * actualBtnSize, Screen.width * actualBtnSize), "", centralButton.buttonStyle))
                    {
                        centralButton.Use(false);
                        if (centralButtonHideMenu)
                            HideMenu();
                    }*/
                }

                for (int i = 0; i < menuCategories.Count; i++)
                {
                    if (menuCategories[i].buttonList.Count > 0)
                    {
                        // Start category
                        angleDebutCategorie = angleActuel;

                        for (int y = 0; y < menuCategories[i].buttonList.Count; y++)
                        {
                            
                            DrawButton(menuCategories[i].buttonList[y], angleActuel);

                            angleActuel += ecartAngle;
                        }

                        // End category
                        DrawCategoryName(menuCategories[i], angleDebutCategorie, angleActuel - ecartAngle);

                        // If not a full circle, dont draw last separator
                        if (i != menuCategories.Count - 1 || actualOpenAngle >= 360 - ecartAngle)
                            DrawSeparator(angleActuel - ecartAngle / 2);
                    }
                }
            }
            else
                Debug.LogError("Add categories and buttons to your circular menu.");
        }
        else
        {
            // Central activation button
            if (showActivationButton)
            {
                 activationButton.viewRectangle = new Rect(Screen.width * (actualMenuPosition.x - buttonSize / 2), (Screen.height * actualMenuPosition.y) - (Screen.width * buttonSize / 2), Screen.width * buttonSize, Screen.width * buttonSize);
                GUI.Button(activationButton.viewRectangle, "", activationButton.buttonStyle);
                /*if (GUI.Button(new Rect(Screen.width * (actualMenuPosition.x - buttonSize / 2), (Screen.height * actualMenuPosition.y) - (Screen.width * buttonSize / 2), Screen.width * buttonSize, Screen.width * buttonSize), "", activationButton.buttonStyle))
                {
                    activationButton.Use(false);
                    ShowMenu();
                }*/
            }
        }
    }

    /// <summary>
    /// Function used to draw a button
    /// </summary>
    /// <param name="actualBtn">CircularMenu button you want to show</param>
    /// <param name="angleActuel">Angle position in the menu</param>
    private void DrawButton(CircularMenuButton actualBtn, float angleActuel)
    {
        Vector2 positionBtn = new Vector2(Mathf.Cos(angleActuel * Mathf.PI / 180) * actualCircleRadius, Mathf.Sin(angleActuel * Mathf.PI / 180) * actualCircleRadius);

        positionBtn.x = actualMenuPosition.x * Screen.width + (positionBtn.x - actualBtnSize / 2) * Screen.width;
        positionBtn.y = actualMenuPosition.y * Screen.height + (positionBtn.y - actualBtnSize / 2) * Screen.width;

        // On enregistre la position du bouton
        actualBtn.viewRectangle = new Rect(positionBtn.x, positionBtn.y, Screen.width * actualBtnSize, Screen.width * actualBtnSize);

        // On dessine le bouton
        /*if (GUI.Button(actualBtn.viewRectangle, "", actualBtn.buttonStyle))
            actualBtn.Use();*/
        GUI.Button(actualBtn.viewRectangle, "", actualBtn.buttonStyle);
    }

    /// <summary>
    /// Function used to draw the separator texture
    /// </summary>
    /// <param name="angle">Angle position it has to be drawn</param>
    private void DrawSeparator(float angle)
    {
        if (showSeparators && actualState == EtatMenu.Active)
        {
            Vector2 positionBtn = new Vector2(Mathf.Cos(angle * Mathf.PI / 180) * actualCircleRadius, Mathf.Sin(angle * Mathf.PI / 180) * actualCircleRadius);
            float widthSeparateur = actualCircleRadius * separatorSize.x * Screen.width;
            float heightSeparateur = actualCircleRadius * separatorSize.y * Screen.width;

            Vector2 pivot = positionBtn;
            pivot.x = actualMenuPosition.x * Screen.width + positionBtn.x * Screen.width;
            pivot.y = actualMenuPosition.y * Screen.height + positionBtn.y * Screen.width;

            positionBtn.x = pivot.x - widthSeparateur / 2;
            positionBtn.y = pivot.y - heightSeparateur / 2;

            Matrix4x4 tempMatrix = GUI.matrix;

            GUIUtility.RotateAroundPivot((float)ClampRotation(angle), pivot);

            GUI.Box(new Rect(positionBtn.x, positionBtn.y, widthSeparateur, heightSeparateur), "", separatorStyle);

            GUI.matrix = tempMatrix;
        }
    }

    /// <summary>
    /// Function used to draw the category name
    /// </summary>
    /// <param name="part">CircularMenuCategory you want to draw</param>
    /// <param name="angleDebut">Start angle of the category elements</param>
    /// <param name="angleFin">End angle of the category elements</param>
    private void DrawCategoryName(CircularMenuCategory part, float angleDebut, float angleFin)
    {
        if (part.showCategoryName && actualState == EtatMenu.Active)
        {
            float angleMedian = (angleFin - angleDebut) / 2 + angleDebut;
            float tempRayonCercle = actualCircleRadius * categoryNameRadius;

            Vector2 positionBtn = new Vector2(Mathf.Cos(angleMedian * Mathf.PI / 180) * tempRayonCercle, Mathf.Sin(angleMedian * Mathf.PI / 180) * tempRayonCercle);

            positionBtn.x = actualMenuPosition.x * Screen.width + (positionBtn.x - part.textLabelSize.x / 2) * Screen.width;
            positionBtn.y = actualMenuPosition.y * Screen.height + (positionBtn.y - part.textLabelSize.y / 2) * Screen.width;

            Vector2 pivot = positionBtn;
            pivot.x += part.textLabelSize.x / 2 * Screen.width;
            pivot.y += part.textLabelSize.y / 2 * Screen.width;

            Matrix4x4 tempMatrix = GUI.matrix;

            float clampAngle = (float)ClampRotation(angleMedian);

            float matrixRotationAngle = clampAngle + 90;

            if (categoryNameNoUpsidedown && clampAngle > 0 && clampAngle < 180)
            {
                if (clampAngle > 90)
                    matrixRotationAngle += 180;
                else
                    matrixRotationAngle -= 180;
            }

            GUIUtility.RotateAroundPivot(matrixRotationAngle, pivot);

            GUI.Box(new Rect(positionBtn.x, positionBtn.y, Screen.width * part.textLabelSize.x, Screen.width * part.textLabelSize.y), part.name, part.textStyle);

            GUI.matrix = tempMatrix;
        }
    }

    /// <summary>
    /// Getter of the total number of buttons in the menu
    /// </summary>
    /// <returns></returns>
    public int GetTotalElementsNumber()
    {
        int total = 0;
        for (int i = 0; i < menuCategories.Count; i++)
            total += menuCategories[i].buttonList.Count;
        return total;
    }

    /// <summary>
    /// Getter for the menu state
    /// </summary>
    /// <returns></returns>
    public EtatMenu GetActuelMenuState()
    {
        return actualState;
    }

    /// <summary>
    /// Function used to clamp rotation angles between -360 and 360
    /// </summary>
    /// <param name="rotation"></param>
    /// <returns></returns>
    private float ClampRotation(float rotation)
    {
        if (rotation < 0)
            rotation += 360;
        else if (rotation > 360)
            rotation -= 360;

        return rotation;
    }

    /// <summary>
    /// Function use to add a category to the menu
    /// </summary>
    /// <returns> The created category </returns>
    public CircularMenuCategory AddCategory()
    {
        CircularMenuCategory newCategory = new CircularMenuCategory(this);
        menuCategories.Add(newCategory);
        return newCategory;
    }

    /// <summary>
    /// Function use to remove a specific category to the menu
    /// </summary>
    public void RemoveCategory(CircularMenuCategory _category)
    {
        for (int i = 0; i < _category.buttonList.Count; i++)
            _category.RemoveButton(_category.buttonList[i]);

        menuCategories.Remove(_category);
        UpdateButtonsNumber();
        _category = null;

        System.GC.Collect();
    }

    /// <summary>
    /// Set in memory the total number of buttons
    /// </summary>
    public void UpdateButtonsNumber()
    {
        // Calculate the number of elements
        totalElementNumber = GetTotalElementsNumber();

        // Auto calculate the radius
        if (!manuallySetRadius)
            circleRadius = totalElementNumber * 0.2f * buttonSize * (360 / menuMaxOpenAngle) + buttonSize / 2;
    }

    public CircularMenuButton GetButtonFromName(string _name)
    {
        CircularMenuButton foundButton = null;

        for (int i = 0; i < menuCategories.Count; i++)
        {
            CircularMenuButton tempAnswer = menuCategories[i].GetButtonFromName(_name);
            if (tempAnswer != null)
            {
                foundButton = tempAnswer;
                break;
            }
        }

        return foundButton;
    }

    /// <summary>
    /// Get a category from its name or null
    /// </summary>
    /// <param name="_name">Name of the category</param>
    /// <returns></returns>
    public CircularMenuCategory GetCategoryFromName(string _name)
    {
        return menuCategories.Where(c => c.name == _name).FirstOrDefault();
    }

    private void LogError(string error)
    {
        errorsList.Add(error);
    }

    private void ShowErrors()
    {
        for (int i = 0; i < errorsList.Count; i++)
            GUI.Label(new Rect(10, 10 + i * 20, 500, 20), errorsList[i]);
    }

}
