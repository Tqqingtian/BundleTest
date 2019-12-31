using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleMgr 
{
    public static T LoadBundle<T>(string bundleName) where T : UnityEngine.Object
    {
        List<AssetBundle> ABList = new List<AssetBundle>();
        Debug.Log(ResourceConfig.PD_AB + ResourceConfig.Platform + "PC");
        AssetBundle bundle = AssetBundle.LoadFromFile(ResourceConfig.PD_AB +ResourceConfig.Platform + "PC");
        Debug.Log("AssetBundle:" + bundle);
        ABList.Add(bundle);
        AssetBundleManifest manifest = (AssetBundleManifest)bundle.LoadAsset("AssetBundleManifest");
        Debug.Log("AssetBundleManifest:" + manifest);

        string[] strArr = manifest.GetAllDependencies($"{bundleName}.assetbundle");
        Debug.Log(strArr.Length);
        foreach (var item in strArr)
        {
            AssetBundle manifestBundle = AssetBundle.LoadFromFile(ResourceConfig.PD_AB + ResourceConfig.Platform + item);
            ABList.Add(manifestBundle);
            Debug.Log("AssetBundle1:" + manifestBundle);
        }

        AssetBundle cubeBundle = AssetBundle.LoadFromFile(ResourceConfig.PD_AB + ResourceConfig.Platform + $"{bundleName}.assetbundle");
        ABList.Add(cubeBundle);
        T obj = cubeBundle.LoadAsset<T>(bundleName);
        //UnityEngine.Object.Instantiate(obj);
        Debug.Log(obj);
        for (int i = 0; i < ABList.Count; i++)
        {
            ABList[i].Unload(false);
        }
        return obj;
    }
}
