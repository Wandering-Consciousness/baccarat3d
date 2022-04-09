using UnityEngine;
using System.Collections;

public class DisableOnStartup : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    this.gameObject.GetComponent<Camera>().enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
