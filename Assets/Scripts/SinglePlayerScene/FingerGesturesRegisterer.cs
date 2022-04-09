using UnityEngine;
using System.Collections;

public class FingerGesturesRegisterer : MonoBehaviour {

	// Use this for initialization
	void Start () {
        // Register ourselves with the GameState
        Debug.Log("Registering FingerGestures with Game State");
        GameState.Instance.fingerGestures = this.gameObject.GetComponent<FingerGestures>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
