using UnityEngine;
using System.Collections;

public class PlayerClick : MonoBehaviour 
{
    public CircularMenu menu;
    public GameObject clickOnMe;

    void OnEnable()
    {
        menu.DesactivatedCallBack += OnMenuHide;
    }

    void OnDesable()
    {
        menu.DesactivatedCallBack -= OnMenuHide;
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE || UNITY_WEBPLAYER
        if (Input.GetButtonDown("Fire1") && menu.GetActuelMenuState() == CircularMenu.EtatMenu.Inactive)
        {
            Ray viewportRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            int layer = 1 << LayerMask.NameToLayer("Player");

            if (Physics.Raycast(viewportRay, 100, layer))
            {
                clickOnMe.active = false;
                menu.ShowMenu();
            }
        }
#elif UNITY_IPHONE || UNITY_ANDROID
        if(menu.GetActuelMenuState() == CircularMenu.EtatMenu.Inactive)
        {
            for (int i = 0; i < Input.touches.Length; i++)
            {
                Touch current = Input.touches[i];

                if (current.phase == TouchPhase.Began)
                {
                    Ray viewportRay = Camera.main.ScreenPointToRay(current.position);
                    int layer = 1 << LayerMask.NameToLayer("Player");

                    if (Physics.Raycast(viewportRay, 100, layer))
                    {
                        clickOnMe.active = false;
                        menu.ShowMenu();
                    }
                }
            }
        }
#endif
    }

    private void OnMenuHide()
    {
        clickOnMe.active = true;
    }
}
