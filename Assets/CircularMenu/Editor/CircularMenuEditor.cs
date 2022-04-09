using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;

[CustomEditor(typeof(CircularMenu))]
public class CircularMenuEditor : Editor
{
    private CircularMenu _target;

    void OnEnable()
    {
        // Init
        _target = (CircularMenu)target;
    }

    /// <summary>
    /// Called when [inspector GUI].
    /// </summary>
    public override void OnInspectorGUI()
    {
        // GUI
        //serializedObject.Update();
        EditorGUILayout.LabelField("v1.1", EditorStyles.boldLabel);

        // Categories
        EditorGUILayout.LabelField("Categories", EditorStyles.boldLabel);
        for (int i = 0; i < _target.menuCategories.Count; i++)
            ShowMenuCategory(_target.menuCategories[i]);
        if (GUILayout.Button("Add categorie"))
            _target.AddCategory();

        EditorGUILayout.Separator();
        
        // Global params
        EditorGUILayout.LabelField("Global parameters", EditorStyles.boldLabel);
        _target.startActive = EditorGUILayout.Toggle("Start active", _target.startActive);
        _target.usedPositionType = (CircularMenu.PositionType)EditorGUILayout.EnumPopup("Menu position type", _target.usedPositionType);
        EditorGUILayout.BeginHorizontal();
        _target.manuallySetRadius = EditorGUILayout.Toggle("Manually set radius", _target.manuallySetRadius);
        if (GUILayout.Button("Refresh radius", GUILayout.Width(100)))
            _target.UpdateButtonsNumber();
        EditorGUILayout.EndHorizontal();
        if(_target.manuallySetRadius)
            _target.circleRadius = EditorGUILayout.Slider("Radius", _target.circleRadius, 0.01f, 0.5f);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Button size (% of screen width [0-1])");
        _target.buttonSize = EditorGUILayout.FloatField("", _target.buttonSize);
        EditorGUILayout.EndHorizontal();

        _target.startMenuAngle = EditorGUILayout.Slider("Menu initial rotation", _target.startMenuAngle, 0, 360);
        _target.menuMaxOpenAngle = EditorGUILayout.Slider("Menu open angle", _target.menuMaxOpenAngle, -360, 360);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Category name radius");
        _target.categoryNameRadius = EditorGUILayout.Slider(_target.categoryNameRadius, 0, 2);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("No upsidedown category name");
        _target.categoryNameNoUpsidedown = EditorGUILayout.Toggle("", _target.categoryNameNoUpsidedown);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();
        
        // Activation button
        _target.showActivationButton = EditorGUILayout.BeginToggleGroup("Use activation button", _target.showActivationButton);
        if (_target.showActivationButton)
        {
            ShowCircularButtonGUI(_target.activationButton);
        }
        EditorGUILayout.EndToggleGroup();

        EditorGUILayout.Separator();
        
        // Central button
        _target.showCentralButton = EditorGUILayout.BeginToggleGroup("Use central button", _target.showCentralButton);
        if (_target.showCentralButton)
        {
            _target.centralButtonHideMenu = EditorGUILayout.Toggle("Central button hide menu", _target.centralButtonHideMenu);
            ShowCircularButtonGUI(_target.centralButton);
        }
        EditorGUILayout.EndToggleGroup();
        
        EditorGUILayout.Separator();
        
        // Transitions
        _target.useTransitions = EditorGUILayout.BeginToggleGroup("Use transitions", _target.useTransitions);
        if (_target.useTransitions)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Active buttons click during transitions");
            _target.useButtonsDuringTransition = EditorGUILayout.Toggle("", _target.useButtonsDuringTransition);
            EditorGUILayout.EndHorizontal();

            _target.transitionDuration = EditorGUILayout.FloatField("Transition duration", _target.transitionDuration);
            // Radius
            _target.radiusTransition = EditorGUILayout.Toggle("Radius transition", _target.radiusTransition);
            // Size
            _target.sizeTransition = EditorGUILayout.Toggle("Size transition", _target.sizeTransition);
            // Fan
            _target.fanTransition = EditorGUILayout.Toggle("Fan transition", _target.fanTransition);

            // Alpha
            _target.fadeTransition = EditorGUILayout.Toggle("Fade transition", _target.fadeTransition);
            if (_target.fadeTransition)
            {
                _target.minAlpha = EditorGUILayout.FloatField("Alpha min", _target.minAlpha);
                _target.maxAlpha = EditorGUILayout.FloatField("Alpha max", _target.maxAlpha);
            }
            // Rotation
            _target.rotationTransition = EditorGUILayout.Toggle("Rotation transition", _target.rotationTransition);
            if (_target.rotationTransition)
                _target.maxRotation = EditorGUILayout.FloatField("Angle rotation", _target.maxRotation);
        }
        EditorGUILayout.EndToggleGroup();
        
        
        EditorGUILayout.Separator();

        _target.showSeparators = EditorGUILayout.BeginToggleGroup("Show separators", _target.showSeparators);
        if (_target.showSeparators)
        {
            EditorGUILayout.BeginHorizontal();
            _target.separatorSize = EditorGUILayout.Vector2Field("Separator size (% menu radius)", _target.separatorSize, GUILayout.Width(200));
            EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(GUILayout.Width(100), GUILayout.Height(80));
            _target.separatorStyle.normal.background = (Texture2D)EditorGUILayout.ObjectField(_target.separatorStyle.normal.background, typeof(Texture2D), false, GUILayout.Height(80));
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndToggleGroup();

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        if (GUILayout.Button("Show / Hide the circular menu (Only on play)"))
        {
            if (_target.GetActuelMenuState() == CircularMenu.EtatMenu.Active)
                _target.HideMenu();
            else if (_target.GetActuelMenuState() == CircularMenu.EtatMenu.Inactive)
                _target.ShowMenu();
        }
        
        //serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Function called to draw a category gui
    /// </summary>
    /// <param name="_part"></param>
    private void ShowMenuCategory(CircularMenuCategory _part)
    {
        EditorGUILayout.BeginHorizontal();
        _part.showInEditor = EditorGUILayout.Foldout(_part.showInEditor, _part.name);
        if (GUILayout.Button("Remove '" + _part.name + "'", GUILayout.Width(150f)))
            _target.RemoveCategory(_part);
        EditorGUILayout.EndHorizontal();

        if (_part.showInEditor)
        {
            EditorGUI.indentLevel = 2;

            EditorGUILayout.BeginVertical();

            _part.name = EditorGUILayout.TextField("Name", _part.name);

            _part.showCategoryName = EditorGUILayout.BeginToggleGroup("Show category name", _part.showCategoryName);
            EditorGUI.indentLevel = 3;
            if (_part.showCategoryName)
            {
                _part.textLabelSize = EditorGUILayout.Vector2Field("Label size", _part.textLabelSize);
                _part.textStyle.font = (Font)EditorGUILayout.ObjectField("Title font", _part.textStyle.font, typeof(Font), false);
                _part.textStyle.alignment = (TextAnchor)EditorGUILayout.EnumPopup("Text alignement", _part.textStyle.alignment);
                _part.textStyle.fontSize = EditorGUILayout.IntField("Text size", _part.textStyle.fontSize);
                _part.textStyle.fontStyle = (FontStyle)EditorGUILayout.EnumPopup("Text style", _part.textStyle.fontStyle);
                _part.textStyle.normal.textColor = EditorGUILayout.ColorField("Text color", _part.textStyle.normal.textColor);
            }
            EditorGUI.indentLevel = 2;
            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.LabelField("Buttons", EditorStyles.boldLabel);
            EditorGUI.indentLevel = 3;
            for (int i = 0; i < _part.buttonList.Count; i++)
            {
                ShowCircularButtonGUI(_part.buttonList[i]);
                if (_part.buttonList[i].showInEditor)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("", EditorStyles.boldLabel);
                    if (GUILayout.Button("Clear", GUILayout.Width(150f)))
                        _part.buttonList[i].Clear();
                    if (GUILayout.Button("Remove button '" + _part.buttonList[i].name + "'", GUILayout.Width(150f)))
                        _part.RemoveButton(_part.buttonList[i]);
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add button to " + _part.name))
                _part.AddButton();
            EditorGUILayout.LabelField("");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Separator();
        
        EditorGUI.indentLevel = 0;
        EditorGUILayout.Separator();

    }

    /// <summary>
    /// Function used to draw a cicular button gui
    /// </summary>
    /// <param name="_btn"></param>
    private void ShowCircularButtonGUI(CircularMenuButton _btn, bool showElementTocall = true, bool showTextures = true)
    {
        _btn.showInEditor = EditorGUILayout.Foldout(_btn.showInEditor, _btn.name);
        if (_btn.showInEditor)
        {
            EditorGUI.indentLevel += 1;
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            _btn.name = EditorGUILayout.TextField("Name", _btn.name);

            if (showElementTocall)
            {
                EditorGUILayout.LabelField("On click", EditorStyles.boldLabel);
                ShowCircularButtonSendMessageItem(_btn.OnClickMessage);
                EditorGUILayout.LabelField("On release", EditorStyles.boldLabel);
                ShowCircularButtonSendMessageItem(_btn.OnReleaseMessage );
            }

            EditorGUILayout.EndVertical();

            if(showTextures)
                ShowCicularButtonTextures(_btn);

            EditorGUI.indentLevel -= 1;
        }
    }

    private void ShowCicularButtonTextures(CircularMenuButton _btn)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(GUILayout.Width(100), GUILayout.Height(80));
        EditorGUILayout.LabelField("Normal");
        _btn.buttonStyle.normal.background = (Texture2D)EditorGUILayout.ObjectField(_btn.buttonStyle.normal.background, typeof(Texture2D), false, GUILayout.Height(80));
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical(GUILayout.Width(100), GUILayout.Height(80));
        EditorGUILayout.LabelField("Hover");
        _btn.buttonStyle.hover.background = (Texture2D)EditorGUILayout.ObjectField(_btn.buttonStyle.hover.background, typeof(Texture2D), false, GUILayout.Height(80));
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical(GUILayout.Width(100), GUILayout.Height(80));
        EditorGUILayout.LabelField("Click");
        _btn.buttonStyle.active.background = (Texture2D)EditorGUILayout.ObjectField(_btn.buttonStyle.active.background, typeof(Texture2D), false, GUILayout.Height(80));
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    private void ShowCircularButtonSendMessageItem(SendMessageParameters _sendMessage)
    {
        bool allowSceneObjects = !EditorUtility.IsPersistent(target);
        _sendMessage.elementToCall = (GameObject)EditorGUILayout.ObjectField("Element to call", _sendMessage.elementToCall, typeof(GameObject), allowSceneObjects);
        if (_sendMessage.elementToCall != null)
        {
            _sendMessage.functionToCall = EditorGUILayout.TextField("Function to call", _sendMessage.functionToCall);
            _sendMessage.paramType = (SendMessageParameters.UsableParameterType)EditorGUILayout.EnumPopup("Parameter Type", _sendMessage.paramType);
            switch (_sendMessage.paramType)
            {
                case SendMessageParameters.UsableParameterType.Bool:
                    _sendMessage.paramBool = EditorGUILayout.Toggle("Parameter", (bool)_sendMessage.paramBool);
                    break;
                case SendMessageParameters.UsableParameterType.Float:
                    _sendMessage.paramFloat = EditorGUILayout.FloatField("Parameter", (float)_sendMessage.paramFloat);
                    break;
                case SendMessageParameters.UsableParameterType.Int:
                    _sendMessage.paramInt = EditorGUILayout.IntField("Parameter", (int)_sendMessage.paramInt);
                    break;
                case SendMessageParameters.UsableParameterType.String:
                    _sendMessage.paramString = EditorGUILayout.TextField("Parameter", (string)_sendMessage.paramString);
                    break;
                default:
                    _sendMessage.paramString = EditorGUILayout.TextField("Parameter", (string)_sendMessage.paramString);
                    break;
            }
        }
    }

}

