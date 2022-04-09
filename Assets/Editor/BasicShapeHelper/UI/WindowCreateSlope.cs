using UnityEngine;
using UnityEditor;

public class WindowCreateSlope : EditorWindow
{
    [MenuItem ("GameObject/Create Geometry/Slope...")]
    static void Init () 
	{
		EditorWindow.GetWindowWithRect(typeof(WindowCreateSlope), new Rect(0, 0, 236, 222), true, "Create Slope");				
    }
	
	
	static private Vector2 sPlaneDims = new Vector2(1.0f, 1.0f);
	static private Vector2 sHeights = new Vector2(0.5f, 1.0f);
	
	private GUITexCoord mGUITexCoord = new GUITexCoord();
	
	public void OnGUI()
	{
		sPlaneDims.x = EditorGUILayout.FloatField("Width", sPlaneDims.x);	    
		sPlaneDims.y= EditorGUILayout.FloatField("Depth", sPlaneDims.y);	    	    
		sHeights.x = EditorGUILayout.FloatField("Height 1", sHeights.x);	    	    
		sHeights.y = EditorGUILayout.FloatField("Height 2", sHeights.y);	    	    
				
		mGUITexCoord.OnGUI();

		GUILayout.Space(8);
		if (GUILayout.Button("Create Slope"))
			GameObjectUtils.CreateSlope("NewSlope", sPlaneDims, sHeights, mGUITexCoord.UVTiling, mGUITexCoord.UVOffset);
	}
}