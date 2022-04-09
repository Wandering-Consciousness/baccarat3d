using UnityEngine;
using UnityEditor;

public class WindowCreateBox : EditorWindow
{
	[MenuItem ("GameObject/Create Geometry/Box...")]
    static void Init () 
	{
		EditorWindow.GetWindowWithRect(typeof(WindowCreateBox), new Rect(0, 0, 236, 202), true, "Create Box");		
    }		
	
	static private Vector3     sBoxDims = new Vector3(1.0f, 1.0f, 1.0f);	
	static private GUITexCoord sGUITexCoord = new GUITexCoord();	

	public void OnGUI()
	{
		sBoxDims.x = EditorGUILayout.FloatField("Width", sBoxDims.x);	    
		sBoxDims.y = EditorGUILayout.FloatField("Height", sBoxDims.y);	    	    
		sBoxDims.z = EditorGUILayout.FloatField("Depth", sBoxDims.z);	    	    
		
		sGUITexCoord.OnGUI();
	
		GUILayout.Space(8);
		if (GUILayout.Button("Create Box"))
			GameObjectUtils.CreateBox("NewBox", sBoxDims, sGUITexCoord.UVTiling, sGUITexCoord.UVOffset);
	}	
}