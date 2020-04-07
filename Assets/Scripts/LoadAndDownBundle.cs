using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/// <summary>
/// 加载清包 sa只有一个小版本号
/// </summary>
public class LoadAndDownBundle : MonoBehaviour
{
    public Queue<string> bundles;

    private double TotalSize;

    public HashSet<string> downloadedBundles;

    private string downloadingBundle;//当前下载的bundle

    private Action OnLoadAssetComplete;//加载资源完成回调
    
    void Start()
    {
        bundles = new Queue<string>();
        downloadedBundles = new HashSet<string>();
        urlVersionConfigData = new byte[] { };
        downloadingBundle = "";

        StartCoroutine(InitLoadAsset());
        //OnLoadAssetComplete = LoadAssetComplete;
    }
    private void LoadAssetComplete()
    {
        AssetBundle.LoadFromFile(PDPath + $"game.assetbundle");
        SceneManager.LoadScene("Game");
        GameObject obj = AssetBundleMgr.LoadBundle<GameObject>("cube");
        
        Instantiate(obj);
    }

    /// <summary>CDN</summary>
    private string URLPath;
    
    /// <summary>可写区</summary>
    private string PDPath;

    /// <summary>只读区</summary>
    private string SAPath;

    /// <summary>
    /// 加载初始化资源
    /// </summary>
    /// <returns></returns>
    private IEnumerator InitLoadAsset()
    {
        URLPath = ResourceConfig.URL_AB + ResourceConfig.Platform;
        PDPath = ResourceConfig.PD_AB + ResourceConfig.Platform;
        SAPath = ResourceConfig.SA_AB + ResourceConfig.Platform;
        
        if (!File.Exists(PDPath + "Version.txt"))//刚下载的时候通常是没有的
        {
            print("可写区没有 就将只读区加载到可写区");
            StartCoroutine(CheckVersionConfig(PDPath, SAPath));
            
        }
        yield return null;
        print("可写区有 就将只CDN加载到可写区");
        StartCoroutine(CheckVersionConfig(PDPath, URLPath));

        OnLoadAssetComplete?.Invoke();
    }

    private byte[] urlVersionConfigData;
    /// <summary>
    /// 获取版本文件信息
    /// </summary>
    /// <param name="path">要加载到的路径</param>
    /// <param name="checkPath">检测的文件路径</param>
    /// <returns></returns>
    private IEnumerator CheckVersionConfig(string path,string checkPath)
    {
        string versionConfig = path + "Version.txt";
        VersionConfig checkVersionConfig = new VersionConfig();
        using (UnityWebRequest webRequest = new UnityWebRequest(checkPath + "Version.txt"))
        {

            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.timeout = 30;
            yield return webRequest.SendWebRequest();
            if (!webRequest.isHttpError && !webRequest.isNetworkError)
            {
                urlVersionConfigData = webRequest.downloadHandler.data;
                checkVersionConfig = JsonHelper.FromJson<VersionConfig>(webRequest.downloadHandler.text);

            }
        }
        if (File.Exists(versionConfig))
        {
            print("有" + versionConfig);
            //都是本地的配置
            VersionConfig localVersionConfig = new VersionConfig();
            using (UnityWebRequest webRequest = new UnityWebRequest(versionConfig))
            {
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.timeout = 30;
                yield return webRequest.SendWebRequest();

                if (!webRequest.isHttpError && !webRequest.isNetworkError)
                {
                    localVersionConfig = JsonHelper.FromJson<VersionConfig>(webRequest.downloadHandler.text);
                }
            }
            Debug.Log("检查文件");
            //删掉远程 不存在的文件
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (directoryInfo.Exists)
            {
                FileInfo[] fileInfos = directoryInfo.GetFiles();
                
                foreach (FileInfo fileInfo in fileInfos)
                {
                    if (checkVersionConfig.FileInfoDict.ContainsKey(fileInfo.Name))
                    {
                        continue;
                    }
                    //if (fileInfo.Name == "Version.txt")
                    //{
                    //    continue;
                    //}
                    Debug.Log("删除不远端存在文件 = " + fileInfo.Name);
                    fileInfo.Delete();//不存在删除
                }
            }
            else
            {
                directoryInfo.Create();
            }
            // 对比MD5
            foreach (FileVersionInfo fileVersionInfo in checkVersionConfig.FileInfoDict.Values)
            {
                // 对比md5
                string localFileMD5 = GetBundleMD5(localVersionConfig, fileVersionInfo.File);
                Debug.Log(fileVersionInfo.File + ":" + localFileMD5 + "\n"+ fileVersionInfo.MD5);
                if (fileVersionInfo.MD5 == localFileMD5)//本地已经有了 退出
                {
                    continue;
                }

                this.bundles.Enqueue(fileVersionInfo.File);
                this.TotalSize += long.Parse(fileVersionInfo.Size);
            }
        }
        else
        {
            print("没有" + versionConfig);
            foreach (FileVersionInfo fileVersionInfo in checkVersionConfig.FileInfoDict.Values)
            {
                this.bundles.Enqueue(fileVersionInfo.File);
                this.TotalSize += long.Parse(fileVersionInfo.Size);
            }
        }
        StartCoroutine(DownloadAsync(path,checkVersionConfig));
    }
    /// <summary>
    /// 获取本地的bundleMD5值
    /// </summary>
    /// <param name="localVersionConfig">本地文件</param>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    public static string GetBundleMD5(VersionConfig localVersionConfig, string bundleName)
    {
        //string path = Path.Combine(ResourceConfig.URL_AB + ResourceConfig.Platform, bundleName);
        //if (File.Exists(path)) //远端有就直接返回？？？
        //{
        //    return MD5Helper.FileMD5(path);
        //}
        if (localVersionConfig.FileInfoDict.ContainsKey(bundleName))
        {
            return localVersionConfig.FileInfoDict[bundleName].MD5;
        }

        return "";
    }


    private double downloadSize;
    private double onDownSize;
    /// <summary>
    /// 加载资源
    /// </summary>
    /// <param name="assetPath">资源根路径 到平台资源</param>
    /// <returns></returns>
    private IEnumerator DownloadAsync(string assetPath, VersionConfig checkfig)
    {
        if (this.bundles.Count == 0 && this.downloadingBundle == "")
        {
            Debug.Log("无更新");
            OnLoadAssetComplete?.Invoke();
            yield break;
        }
        Debug.Log("开始下载");
        while (true)
        {
            if (this.bundles.Count == 0)
            {
                break;
            }
            this.downloadingBundle = this.bundles.Dequeue();
            while (true)
            {
                string url = assetPath + downloadingBundle;
                using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
                {
                    webRequest.downloadHandler = new DownloadHandlerBuffer();
                    webRequest.SendWebRequest();
                    if (webRequest.isHttpError || webRequest.isNetworkError)
                    {
                        print(webRequest.error);
                        continue;
                    }
                    else
                    {
                        while (!webRequest.isDone)
                        {
                            downloadSize = onDownSize + (double)webRequest.downloadedBytes;
                            print("TotalSize:"+ TotalSize + "downloadSize:" + downloadSize + "downloadProgress:" + (float)(downloadSize / TotalSize));
                            yield return null;
                        }
                        onDownSize += long.Parse(checkfig.FileInfoDict[downloadingBundle].Size);
                        print("DownloadAsset:" + onDownSize);
                        byte[] data = webRequest.downloadHandler.data;
                        
                        if (!Directory.Exists(PDPath)) Directory.CreateDirectory(PDPath);

                        string bundlePath = Path.Combine(PDPath, this.downloadingBundle);
                        using (FileStream fs = new FileStream(bundlePath, FileMode.Create))
                        {
                            fs.Write(data, 0, data.Length);
                        }

                    }
                    yield return null;
                }
                break;
            }
            this.downloadedBundles.Add(this.downloadingBundle);
            this.downloadingBundle = "";
        }
        using (FileStream fs = new FileStream(PDPath + "Version.txt", FileMode.Create))
        {
            fs.Write(urlVersionConfigData, 0, urlVersionConfigData.Length);
            urlVersionConfigData = null;
        }
    }

    private void OnDestroy()
    {
        this.TotalSize = 0;
        this.bundles = null;
        this.downloadedBundles = null;
        this.downloadingBundle = null;
    }
}
