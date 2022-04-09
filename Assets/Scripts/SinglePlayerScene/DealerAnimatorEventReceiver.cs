using UnityEngine;
using System.Collections;

// The Mecanim Event library will fire our defined events when the animation reaches
// that time on the timeline and fire the correspondingly named event method here.
// This script has to be attached to the dealer character.
public class DealerAnimatorEventReceiver : MonoBehaviour
{
    public void DealPlayer1Card ()
    {
        Debug.Log ("[Animation Event] Dealing player 1 card");
        GameState.Instance.dealer.dealPlayer (1);
    }

    public void DealPlayer2Card ()
    {
        Debug.Log ("[Animation Event] Dealing player 2 card");
        GameState.Instance.dealer.dealPlayer (2);

    }

    public void DealPlayer3Card ()
    {
        Debug.Log ("[Animation Event] Dealing player 3 card");
        GameState.Instance.dealer.dealPlayer (3);

    }

    public void DealBanker1Card ()
    {
        Debug.Log ("[Animation Event] Dealing banker 1 card");
        GameState.Instance.dealer.dealBanker (1);

    }

    public void DealBanker2Card ()
    {
        Debug.Log ("[Animation Event] Dealing banker 2 card");
        GameState.Instance.dealer.dealBanker (2);
    }
}
