using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(PlayingCard))] 
public class PlayingCardInspector : Editor 
{
	
	public override void OnInspectorGUI()
	{
		
		PlayingCard script = (PlayingCard) target;
		GUILayoutOption[] emptyOptions;
		
		emptyOptions = new GUILayoutOption[0];
		
		script.Back = (Enums.Back) EditorGUILayout.EnumPopup("Back", script.Back, emptyOptions);
		
		script.Number = (Enums.Number) EditorGUILayout.EnumPopup("Number", script.Number, emptyOptions);
		
		if (script.Number != Enums.Number._JOKER)
		{
			script.Suit = (Enums.Suit) EditorGUILayout.EnumPopup("Suit", script.Suit, emptyOptions);
		}
		

		if (GUI.changed)
		{
			
			script.Refresh();	
			
		}
		
		
	}
	
	
}