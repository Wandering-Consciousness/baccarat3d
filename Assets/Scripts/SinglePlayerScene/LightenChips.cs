using UnityEngine;
using System.Collections;

public class LightenChips : MonoBehaviour {
    public Renderer[] chipRenderers;

	// Use this for initialization
	void Start () {
	    // Lighten the chips' tint color on the loading screen
        foreach (Renderer renderer in chipRenderers) {
            if (renderer != null)
                renderer.material.color = Color.white;
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
