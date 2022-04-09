using UnityEngine;
using System;
using System.Collections;

public static class Enums
{
	
	public enum Suit { H=0,D,C,S, Count }
	public enum Number { _JOKER=0,_2,_3,_4,_5,_6,_7,_8,_9,_10,_J,_Q,_K,_A,Count }
	public enum Back { Red=0,Blue }
	public enum Face { Simple=0,Traditional }
	public enum Chip { BlackChip=0,BlueChip,GreenChip,OrangeChip,RedChip,WhiteBlackChip,WhiteBlueChip,WhiteGreenChip,WhiteRedChip } 
	
}

public class PlayingCard : MonoBehaviour {

	
	Enums.Suit suit = Enums.Suit.S;
	Enums.Number number = Enums.Number._A;
	Enums.Back back = Enums.Back.Red;
	
	
	GameObject card;
	
	public void Awake()
	{
	
		Refresh();
		
	}
	
	public Enums.Back Back
	{
		get { return back; }
		set { back = value; }
	}
	
	public Enums.Suit Suit
	{
		get { return suit; }
		set { suit = value; }
	}
	
	public Enums.Number Number
	{
		get { return number; }
		set { number = value; }
	}
	
	public void ChangeToRandom(bool ignoreJoker)
	{

		Enums.Suit changeSuit = (Enums.Suit) UnityEngine.Random.Range(0, ((int) Enums.Suit.Count) - 1);
		Enums.Number changeNumber;
		
		if (ignoreJoker)
		{
			changeNumber = (Enums.Number) UnityEngine.Random.Range(1, ((int) Enums.Suit.Count) - 2);
		}
		else
		{
			changeNumber = (Enums.Number) UnityEngine.Random.Range(0, ((int) Enums.Suit.Count) - 1);
		}
	
		Change(changeSuit,changeNumber);
	
	}
	
	public void Change(Enums.Suit cardSuit, Enums.Number cardNumber)
	{
	
	   suit = cardSuit;
	   number = cardNumber;
	
	   Refresh();
	
	}
	
	public void Style(Enums.Back styleBack)
	{
	
		back = styleBack;
		
		Refresh(); 
		
	}
	
	public void Refresh()
	{
	
		string findCard = Enum.GetName(typeof(Enums.Back), back) + Enum.GetName(typeof(Enums.Number), number) + Enum.GetName(typeof(Enums.Suit), suit);

		if (card)
		{
			
			card.GetComponent<Renderer>().enabled = false;
		
		}

				
		Transform foundCard = this.transform.Find(findCard);
		
		if (foundCard)
		{
			
			card = foundCard.gameObject; 
			card.GetComponent<Renderer>().enabled = true;
				
		}


	}

}
