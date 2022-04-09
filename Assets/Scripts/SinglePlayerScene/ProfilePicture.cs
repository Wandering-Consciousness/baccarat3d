using UnityEngine;
using System.Collections;

// Download an profile picture and display it on a texture
public class ProfilePicture : MonoBehaviour {
    public string url = "";
    public Texture texture;
    public Rect rect;

    public ProfilePicture(string url) {
        this.url = url;
        StartCoroutine(Start ());
    }

    IEnumerator Start() {
        Debug.Log("Downloading profile picture: " + url);
        WWW www = new WWW(url);
        yield return www;
        Debug.Log("Downloaded profile picture: " + url);
        texture = www.texture;
    }

    public void DrawGUI() {
        if (!texture) {
            return;
        }
        GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, true, 1.0F);
    }
}