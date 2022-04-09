using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class ToolsMenu : MonoBehaviour
{

	[MenuItem( "Tools/Zero position and rotation" )]
	static void ZeroPositionAndRotation()
	{
		
		foreach(Transform t in Selection.transforms)
		{
				
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			
				
		}

	}
		
	[MenuItem( "Tools/Enable all objects" )]
	static void EnableAllObjects()
	{
		
		foreach(Transform t in Selection.transforms)
		{
				
			t.gameObject.active = true;
				
		}

	}
		
	[MenuItem( "Tools/Disable all renders" )]
	static void DisableAllRenders()
	{
		
		foreach(Transform t in Selection.transforms)
		{
				
			t.renderer.enabled = false;
				
		}

	}
	
	[MenuItem("Tools/Copy all materials")]
	static void CopyAllMaterialsFromFirst()
	{
	
		try
		{
			Material m = Selection.activeTransform.renderer.sharedMaterial;
			
			foreach(Transform t in Selection.transforms)
			{
				
				t.renderer.sharedMaterial = 	m;
				
			}
		
		}
		catch 
		{
			
			Debug.Log("Unable to copy material as the first selected object doesn't have a material");
			
		}
	
	}
}
