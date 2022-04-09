using UnityEngine;
using UnityEditor;

public class WindowCreateCone : EditorWindow
{
    [MenuItem ("GameObject/Create Geometry/Cone...")]	
    static void InitConeMenu() 
	{
        EditorWindow.GetWindowWithRect(typeof(WindowCreateCone), new Rect(0, 0, 236, 244), true, "Create Cone");
    }
	
	static private int   siRenderSubdivision = 16;
	static private int   siCollisionSubdivision = 8;
	static private float sfBottomRadius = 0.5f;
	static private float sfHeight = 1.0f;
	static private bool  sbSmoothNormal = true;
	
	private GUITexCoord sGUITexCoord = new GUITexCoord();		 

	
	public void OnGUI()
	{
		siRenderSubdivision = EditorGUILayout.IntField("Render Subdiv.", siRenderSubdivision);	    
		siCollisionSubdivision = EditorGUILayout.IntField("Collision Subdiv.", siCollisionSubdivision);	    

		sfBottomRadius = EditorGUILayout.FloatField("Bottom Radius", sfBottomRadius);	    	    
		sfHeight = EditorGUILayout.FloatField("Height", sfHeight);	    	    
		sbSmoothNormal = EditorGUILayout.Toggle("Smooth Normal", sbSmoothNormal);	    	    
		
		sGUITexCoord.OnGUI();

		GUILayout.Space(8);		
		if (GUILayout.Button("Create Cone"))
			GameObjectUtils.CreateCylinder("NewCone", siRenderSubdivision, siCollisionSubdivision, 0.0f, sfBottomRadius, sfHeight, sbSmoothNormal, sGUITexCoord.UVTiling, sGUITexCoord.UVOffset);
	}	
	
}