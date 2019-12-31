using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ULog : MonoBehaviour
{

    private IEnumerator Start()
    {
        yield return null;
        //yield return new WaitUntil(() => CopyDirectory(ResourceConfig.SA_AB, ResourceConfig.PD_AB, true));
        
        UnityEngine.Object.Instantiate(LoadBundle<GameObject>("cube"));

        //UnityEngine.Object.Instantiate(LoadBundle<AudioClip>("chinese"));

    }
    /// <summary>
    /// 加载bundle
    /// </summary>*
    /// <param name="bundleName">bundle名称</param>
    /// <returns></returns>
    private T LoadBundle<T>(string bundleName) where T : UnityEngine.Object
    {
        List<AssetBundle> ABList = new List<AssetBundle>();
        AssetBundle bundle = AssetBundle.LoadFromFile(ResourceConfig.PD_AB + "AssetBundle");
        print("AssetBundle:" + bundle);
        ABList.Add(bundle);
        AssetBundleManifest manifest = (AssetBundleManifest)bundle.LoadAsset("AssetBundleManifest");
        print("AssetBundleManifest:" + manifest);

        string[] strArr = manifest.GetAllDependencies($"{bundleName}.assetbundle");
        Debug.Log(strArr.Length);
        foreach (var item in strArr)
        {
            AssetBundle manifestBundle = AssetBundle.LoadFromFile(ResourceConfig.PD_AB + item);
            ABList.Add(manifestBundle);
            print("AssetBundle1:" + manifestBundle);
        }

        AssetBundle cubeBundle = AssetBundle.LoadFromFile(ResourceConfig.PD_AB + $"{bundleName}.assetbundle");
        ABList.Add(cubeBundle);
        T obj = cubeBundle.LoadAsset<T>(bundleName);
        //UnityEngine.Object.Instantiate(obj);
        print(obj);
        for (int i = 0; i < ABList.Count; i++)
        {
            ABList[i].Unload(false);
        }
        return obj;
    }
 
    /// <summary>
    /// 拷贝文件
    /// </summary>
    /// <param name="SourcePath">原目录</param>
    /// <param name="DestinationPath">目标目录</param>
    /// <param name="overwriteexisting">是否覆盖</param>
    /// <returns></returns>
    private bool CopyDirectory(string SourcePath, string DestinationPath, bool overwriteexisting)
    {
        bool ret = false;
        try
        {
            SourcePath = SourcePath.EndsWith(@"\") ? SourcePath : SourcePath + @"\";
            DestinationPath = DestinationPath.EndsWith(@"\") ? DestinationPath : DestinationPath + @"\";

            if (Directory.Exists(SourcePath))
            {
                if (Directory.Exists(DestinationPath) == false)
                    Directory.CreateDirectory(DestinationPath);

                foreach (string fls in Directory.GetFiles(SourcePath))
                {
                    FileInfo flinfo = new FileInfo(fls);
                    if (!flinfo.Name.EndsWith(".meta"))
                    {
                        flinfo.CopyTo(DestinationPath + flinfo.Name, overwriteexisting);
                    }
                }
                foreach (string drs in Directory.GetDirectories(SourcePath))
                {
                    DirectoryInfo drinfo = new DirectoryInfo(drs);
                    if (CopyDirectory(drs, DestinationPath + drinfo.Name, overwriteexisting) == false)
                        ret = false;
                }
            }
            ret = true;
        }
        catch (Exception ex)
        {
            print(ex.Message);
            ret = false;
        }
        Shader.WarmupAllShaders();
        print("拷贝完成");
        return ret;
    }



}
