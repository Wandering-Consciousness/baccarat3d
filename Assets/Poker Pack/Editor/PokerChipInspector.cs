using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(PokerChip))] 
public class PokerChipInspector : Editor 
{
	
	public override void OnInspectorGUI()
	{
		
		PokerChip script = (PokerChip) target;
		GUILayoutOption[] emptyOptions;
		
		emptyOptions = new GUILayoutOption[0];
		
		script.Chip = (Enums.Chip) EditorGUILayout.EnumPopup("Chip", script.Chip, emptyOptions);

		if (GUI.changed)
		{
			
			script.Refresh();	
			
		}
		
		
	}
	
	
}