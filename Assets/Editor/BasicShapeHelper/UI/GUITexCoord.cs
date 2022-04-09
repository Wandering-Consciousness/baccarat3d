using UnityEngine;
using UnityEditor;

public class GUITexCoord
{
	private Vector2 mUVTiling = new Vector2(1.0f, 1.0f);	
	private Vector2 mUVOffset = new Vector2(0.0f, 0.0f);
	
	public Vector2 UVTiling		
	{
		get { return mUVTiling; }			
	}
	
	public Vector2 UVOffset
	{
		get { return mUVOffset; }			
	}	
	
	public void OnGUI()
	{				
		GUILayout.Space(8);
		
		mUVTiling.x = EditorGUILayout.FloatField("U Tiling ", mUVTiling.x);	    	    
		mUVTiling.y = EditorGUILayout.FloatField("V Tiling", mUVTiling.y);	    	    
		mUVOffset.x = EditorGUILayout.FloatField("U Offset", mUVOffset.x);	    	    
		mUVOffset.y = EditorGUILayout.FloatField("V Offset", mUVOffset.y);	    	    
	}
}
