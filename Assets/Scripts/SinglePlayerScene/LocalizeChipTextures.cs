using UnityEngine;
using System.Collections;

public class LocalizeChipTextures : MonoBehaviour {
    public int chipValue = 0;

    // Reference for chip localized chip textures (ones that aren't just numbers)
    public Texture hundredThousandKTexture;
    public Texture hundredThousandCJKTexture;
    public Texture tenThousandKTexture;
    public Texture tenThousandCJKTexture;

	// Use this for initialization
	void Start () {
        if (gameObject.GetComponent<Chip>() == null) {
            // Call localize chip textures here only if our associated game object is a static chip.
            // Otherwise moveable chip objects will have the associated Chip script and their only
            // real-used chip value in there. This call is primarily for the $1000 and $10,000 chips
            // that are static in the troff and on the 2nd to top row. They're briefly exposed as
            // a chip is picked up to be dropped on the table.
            Debug.Log ("Localizing static chip " + gameObject.name + " texture");
	        localizeChipTextures();
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}


    // Show localized textures for some chips when avaiable
    public void localizeChipTextures ()
    {
        // $100,000
        if (Utils.isJapanese () && chipValue == 100000) { // TODO: add chinese
            gameObject.GetComponent<Renderer>().material.mainTexture = hundredThousandCJKTexture;
        } else if (chipValue == 100000) {
            gameObject.GetComponent<Renderer>().material.mainTexture = hundredThousandKTexture;
        }

        // $10,000
        if (Utils.isJapanese () && chipValue == 10000) { // TODO: add chinese
            gameObject.GetComponent<Renderer>().material.mainTexture = tenThousandCJKTexture;
        } else if (chipValue == 10000) {
            gameObject.GetComponent<Renderer>().material.mainTexture = tenThousandKTexture;
        }

    }
}
