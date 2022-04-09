using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardDeck : MonoBehaviour
{
	[System.Serializable]
	public class DeckItem
	{
		public int Count = 1;
		public CardDef Card;
	}
	
	protected DeckItem [] m_itemList;
	
	// List of cards in live deck
	public List<CardDef> m_cards = new List<CardDef>();

    public CardDef getCardAtPos(int pos) {
        return m_cards[pos];
    }

    public void replaceCardAtPos(CardDef newCardDef, int pos) {
        m_cards[pos] = newCardDef;
    }

	public virtual void Initialize()
	{
	}
	
	public void Reset()
	{
		m_cards.Clear();
		
		foreach (DeckItem item in m_itemList)
		{
			for (int i=0; i<item.Count; ++i)
			{
				m_cards.Add(item.Card);
			}
		}
	}
	
	public void Shuffle()
	{
		for (int i=0; i<m_cards.Count; ++i)
		{
			int other = Random.Range(0,m_cards.Count);
			if (other != i)
			{
				CardDef swap = m_cards[i];
				m_cards[i] = m_cards[other];
				m_cards[other] = swap;
			}
		}
	}
	
	public CardDef Pop()
	{
		int last = m_cards.Count-1;
		if (last >= 0)
		{
			CardDef result = m_cards[last];
			m_cards.RemoveAt(last);
			return result;
		}
		return null;
	}

    public int Remaining() {
        return m_cards.Count;
    }
}
