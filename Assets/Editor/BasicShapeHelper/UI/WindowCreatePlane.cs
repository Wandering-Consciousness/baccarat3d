using UnityEngine;
using UnityEditor;

public class WindowCreatePlane : EditorWindow
{		 
    [MenuItem ("GameObject/Create Geometry/Plane...")]
    static void Init () 
	{
		EditorWindow.GetWindowWithRect(typeof(WindowCreatePlane), new Rect(0, 0, 236, 186), true, "Create Plane");		
    }
	
	
	static private Vector2     sPlaneDims = new Vector2(1.0f, 1.0f);	
	static private GUITexCoord sGUITexCoord = new GUITexCoord();
	

	public void OnGUI()
	{
		sPlaneDims.x = EditorGUILayout.FloatField("Width", sPlaneDims.x);	    
		sPlaneDims.y= EditorGUILayout.FloatField("Depth", sPlaneDims.y);	    	    
				
		sGUITexCoord.OnGUI();
	
		GUILayout.Space(8);
		if (GUILayout.Button("Create Plane"))
			GameObjectUtils.CreatePlane("NewPlane", sPlaneDims, sGUITexCoord.UVTiling, sGUITexCoord.UVOffset);
	}
}