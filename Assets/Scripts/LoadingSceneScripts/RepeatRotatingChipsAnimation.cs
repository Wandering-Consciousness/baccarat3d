using UnityEngine;
using System.Collections;

public class RepeatRotatingChipsAnimation : MonoBehaviour {
    public Animation animation;

	// Use this for initialization
	void Start () {
        animation = GetComponent<Animation>();
        // Loop the chip animation 5 times
        for(int count = 1; count <= 5; count++)
        {
            animation.PlayQueued("SpinngChipsLoadingScene");
        }
	}
}
