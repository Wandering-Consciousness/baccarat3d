using UnityEngine;
using System;
using System.Collections;


public class PokerChip : MonoBehaviour {

	
	Enums.Chip chip = Enums.Chip.BlackChip;
	
	GameObject pokerChip;
	
	public void Awake()
	{
	
		Refresh();
		
	}
	
	public Enums.Chip Chip
	{
		get { return chip; }
		set { chip = value; }
	}

	public void Change(Enums.Chip changeChip)
	{
	
	   chip = changeChip;
	
	   Refresh();
	
	}
	
	public void Refresh()
	{
	
		string findChip = Enum.GetName(typeof(Enums.Chip), chip);
		
		if (pokerChip)
		{
			
			pokerChip.GetComponent<Renderer>().enabled = false;
		
		}
				
		Transform foundChip = this.transform.Find(findChip);
		
		if (foundChip)
		{
			
			pokerChip = foundChip.gameObject; 
			pokerChip.GetComponent<Renderer>().enabled = true;
				
		}

	}

}
