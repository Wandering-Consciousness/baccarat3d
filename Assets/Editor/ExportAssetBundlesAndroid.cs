using UnityEngine;
using UnityEditor;

public class ExportAssetBundlesAndroid {
	[MenuItem("AssetBundles/Build AssetBundle Android")]
	static void ExportResource () {
		string path = "Assets/AssetBundles/B3DBGMMusic/b3dbgmandroid.unity";
		Object[] selection =  Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
		BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.Android);
	}
}