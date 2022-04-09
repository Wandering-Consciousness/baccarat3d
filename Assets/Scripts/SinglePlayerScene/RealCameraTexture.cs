using UnityEngine;
using System.Collections;

public class RealCameraTexture : MonoBehaviour
{
    string deviceName;
    WebCamTexture webCamTexture;

    // Use this for initialization
    void Start ()
    {
        // Play the real camera feed on a background texture (for Gyro AR)
        Debug.Log ("Starting real camera feed for Gyro background texture");
        deviceName = WebCamTexture.devices [0].name;
        webCamTexture = new WebCamTexture (deviceName, Screen.width, Screen.height, 30);
        webCamTexture.Play ();
        GetComponent<Renderer>().material.mainTexture = webCamTexture;

        // Rotate as necessary
        // TODO: needs to be shifted elsewhere to update to orientation changes in-game perhaps..?
        switch (Input.deviceOrientation) {
            case DeviceOrientation.LandscapeLeft:
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z-90);
                break;
            case DeviceOrientation.LandscapeRight:
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z+90);
                break;
            case DeviceOrientation.Unknown:
            case DeviceOrientation.Portrait:
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z-180);
                break;
            case DeviceOrientation.PortraitUpsideDown:
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z+180);
                break;
            }
    }
}
