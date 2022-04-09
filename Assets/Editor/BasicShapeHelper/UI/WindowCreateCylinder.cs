using UnityEngine;
using UnityEditor;

public class WindowCreateCylinder : EditorWindow
{		
	[MenuItem ("GameObject/Create Geometry/Cylinder...")]	
    static void InitCylinderMenu() 
	{     
		EditorWindow.GetWindowWithRect(typeof(WindowCreateCylinder), new Rect(0, 0, 236, 256), true, "Create Cylinder");		
    }
		
	
	static private int   siRenderSubdivision = 16;
	static private int   siCollisionSubdivision = 8;
	static private float sfTopRadius = 0.5f;
	static private float sfBottomRadius = 0.5f;
	static private float sfHeight = 1.0f;
	static private bool  sbSmoothNormal = true;
	
	static private GUITexCoord sGUITexCoord = new GUITexCoord();		

	
	public void OnGUI()
	{
		siRenderSubdivision = EditorGUILayout.IntField("Render Subdiv.", siRenderSubdivision);	    
		siCollisionSubdivision = EditorGUILayout.IntField("Collision Subdiv.", siCollisionSubdivision);	    
		sfTopRadius = EditorGUILayout.FloatField("Top Radius", sfTopRadius);	    	    	    	
		sfBottomRadius = EditorGUILayout.FloatField("Bottom Radius", sfBottomRadius);	    	    
		sfHeight = EditorGUILayout.FloatField("Height", sfHeight);	    	    
		sbSmoothNormal = EditorGUILayout.Toggle("Smooth Normal", sbSmoothNormal);	    	    
		
		sGUITexCoord.OnGUI();

		GUILayout.Space(8);		
		if (GUILayout.Button("Create Cylinder"))
			GameObjectUtils.CreateCylinder("NewCylinder", siRenderSubdivision, siCollisionSubdivision, sfTopRadius, sfBottomRadius, sfHeight, sbSmoothNormal, sGUITexCoord.UVTiling, sGUITexCoord.UVOffset);
	}	
}
