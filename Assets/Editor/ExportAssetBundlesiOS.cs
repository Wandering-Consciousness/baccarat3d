using UnityEngine;
using UnityEditor;

public class ExportAssetBundlesiOS {
	[MenuItem("AssetBundles/Build AssetBundle iOS")]
	static void ExportResource () {
		string path = "Assets/AssetBundles/B3DBGMMusic/b3dbgmios.unity";
		Object[] selection =  Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
		BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.iOS);
	}
}