using UnityEngine;
using System.Collections;

/**
 * This script is used to automatically rename the children meshes' names to include text we can use to filter them for
 * with when using Object.FindObjectsOfType(typeof(MeshFilter)) which is used to help clean up memory leaks but not remove
 * the meshes we wanna keep.
 */
public class RenameMeshNames : MonoBehaviour {
    public static string MESH_FILTER_TEXT = "FMesh_";

	// Use this for initialization
	void Start () {
	    MeshFilter[] mfs = GetComponentsInChildren<MeshFilter>(true);
        foreach (MeshFilter mf in mfs) {
            if (mf != null && mf.mesh != null && mf.mesh.name != null) {
                Debug.Log ("Renaming mesh name: " + mf.mesh.name + " to " + MESH_FILTER_TEXT + mf.mesh.name);
                mf.mesh.name = MESH_FILTER_TEXT + mf.mesh.name;
            }
        }
	}
}
