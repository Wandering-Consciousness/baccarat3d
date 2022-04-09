using UnityEngine;
using UnityEditor;

public class WindowCreateSphere : EditorWindow
{
    [MenuItem ("GameObject/Create Geometry/Sphere...")]	
    static void InitConeMenu() 
	{
		EditorWindow.GetWindowWithRect(typeof(WindowCreateSphere), new Rect(0, 0, 236, 102), true, "Create Sphere");		
    }
	
	
	private int   miSubdivision = 2;
	private float mfRadius = 1.0f;
	
	
	public void OnGUI()
	{  
		miSubdivision = Mathf.Clamp(EditorGUILayout.IntField("Subdivision", miSubdivision), 0, 5);
		mfRadius = Mathf.Max(EditorGUILayout.FloatField("Radius", mfRadius), 0.0001f);
		
		GUILayout.Space(8);		
		if (GUILayout.Button("Create Sphere"))
			GameObjectUtils.CreateSphere("NewSphere", mfRadius, miSubdivision);		
	 
	}
}