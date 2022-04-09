using UnityEngine;
using System.Collections;

public class CardDefinition : MonoBehaviour
{
	public CardDef Data;
}

[System.Serializable]
public class CardDef
{
	public CardAtlas Atlas;
	public CardStock Stock;
	public string Text;
	public string Symbol; // Atlas shape name
	public int Pattern;
	public string Image;
	public bool FullImage = true;
    public bool markedAsCutCard = false;
	
	public CardDef(CardAtlas atlas, CardStock stock, string text, string symbol, int pattern)
	{
		Atlas = atlas;
		Stock = stock;
		Text = text;
		Symbol = symbol;
		Pattern = pattern;
	}

    // Added by Simon for BaccARat 3D
    // Return value of this card in terms of baccarat
    public int getValue() {
        if (Text == "10" || Text == "K" || Text == "Q" || Text == "J") {
            return 0;
        } else if (Text == "A") {
            return 1;
        } else {
            return int.Parse(Text);
        }
    }
}