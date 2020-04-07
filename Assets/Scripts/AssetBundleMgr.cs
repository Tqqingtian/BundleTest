using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleMgr 
{
    public static T LoadBundle<T>(string bundleName) where T : UnityEngine.Object
    {
        //平台包
        string PDP = ResourceConfig.PD_AB + ResourceConfig.Platform;
        List<AssetBundle> ABList = new List<AssetBundle>();
        //Debug.Log(PDP + PDP + ResourceConfig.Platform.Remove(ResourceConfig.Platform.Length - 1, 1));
        //查找依赖关系
        AssetBundle bundle = AssetBundle.LoadFromFile(PDP + ResourceConfig.Platform.Remove(ResourceConfig.Platform.Length - 1, 1));
        ABList.Add(bundle);
        AssetBundleManifest manifest = (AssetBundleManifest)bundle.LoadAsset("AssetBundleManifest");
        string[] strArr = manifest.GetAllDependencies($"{bundleName}.assetbundle");
        foreach (var item in strArr)
        {
            AssetBundle manifestBundle = AssetBundle.LoadFromFile(PDP + item);
            ABList.Add(manifestBundle);
        }

        AssetBundle cubeBundle = AssetBundle.LoadFromFile(PDP + $"{bundleName}.assetbundle");
        ABList.Add(cubeBundle);
        T obj = cubeBundle.LoadAsset<T>(bundleName);
        for (int i = 0; i < ABList.Count; i++)
        {
            ABList[i].Unload(false);
        }
        return obj;
    }
}
