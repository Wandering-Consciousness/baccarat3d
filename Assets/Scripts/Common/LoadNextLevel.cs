using UnityEngine;
using System.Collections;

public class LoadNextLevel : MonoBehaviour {
    public static string nextLevelName = "SinglePlayerScene";
    public GameObject[] destroyObjectsOnLoadLevel;
    public GameObject chips;
    public GameObject loadingText;

	// Use this for initialization
    IEnumerator Start() {
        Debug.Log ("Doing async scene load on " + nextLevelName);
        Invoke("loadTextDelay", 1.8f);
        AsyncOperation async = Application.LoadLevelAdditiveAsync(nextLevelName);
        yield return async;
        Debug.Log("Loading scene complete. Destroying objects from load level scene.");
        foreach (GameObject go in destroyObjectsOnLoadLevel) {
            MeshFilter[] mfs = go.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter mf in mfs) {
                if (mf != null)
                    DestroyImmediate(mf.mesh, true);
            }
            if (go.GetComponent<Renderer>() != null && go.GetComponent<Renderer>().material != null && go.GetComponent<Renderer>().material.mainTexture != null)
                DestroyImmediate(go.GetComponent<Renderer>().material.mainTexture);
            Destroy(go);
        }
        Destroy(this.gameObject);
    }
	
	void loadTextDelay () {
        Invoke("disableChips", 3f);
        loadingText.SetActive(true);
	}

    void disableChips () {
        if (chips != null)
            chips.SetActive(false);
    }


    static void SetNextLevelName(string name) {
        Debug.Log ("Setting next level name to " + name);
        nextLevelName = name;
    }
}
