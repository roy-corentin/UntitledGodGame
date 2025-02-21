using UnityEditor;

public class CreateAssetBundles
{
    [MenuItem("UntitledGodGame/Exporter les animaux")]
    static void BuildAssetBundlesForAndroid()
    {
        BuildPipeline.BuildAssetBundles("Assets/AssetBundles", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
    }
}