using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 加载清包 sa只有一个小版本号
/// </summary>
public class LoadURLBundle : MonoBehaviour
{
    public Queue<string> bundles;

    private double TotalSize;

    public HashSet<string> downloadedBundles;

    private string downloadingBundle;

    private System.Action action;
    void Start()
    {
        bundles = new Queue<string>();
        downloadedBundles = new HashSet<string>();
        downloadingBundle = "";
        action = LoadAssetComplete;
        StartCoroutine(LoadToBundle());
    }
    private void LoadAssetComplete()
    {
        GameObject obj = AssetBundleMgr.LoadBundle<GameObject>("cube");
        Instantiate(obj);

    }
    private void ThreadLoad()
    {
        StartCoroutine(LoadToBundle());
    }

    private VersionConfig remoteVersionConfig;
    private IEnumerator LoadToBundle()
    {
        string versionUrl = ResourceConfig.URL_AB + ResourceConfig.Platform + "Version.txt";
        remoteVersionConfig = new VersionConfig();

        //Debug.Log(versionUrl);
        using (UnityWebRequest webRequest = new UnityWebRequest(versionUrl))
        {
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.timeout = 30;

            yield return webRequest.SendWebRequest();
            if (webRequest.isHttpError || webRequest.isNetworkError)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                remoteVersionConfig = JsonHelper.FromJson<VersionConfig>(webRequest.downloadHandler.text);
            }
        }
       
        VersionConfig streamingVersionConfig = new VersionConfig ();
        string versionPath = ResourceConfig.SA_AB + ResourceConfig.Platform + "Version.txt";
        //Debug.Log(versionPath);

        using (UnityWebRequest webRequest = new UnityWebRequest(versionPath))
        {
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.timeout = 30;
            yield return webRequest.SendWebRequest();
            if (webRequest.isHttpError || webRequest.isNetworkError)
            {
                Debug.Log(webRequest.error);
            }

            else
            {
                streamingVersionConfig = JsonHelper.FromJson<VersionConfig>(webRequest.downloadHandler.text);
            }
        }
        
        // 删掉远程不存在的文件
        DirectoryInfo directoryInfo = new DirectoryInfo(ResourceConfig.PD_AB + ResourceConfig.Platform);
        if (directoryInfo.Exists)
        {
            FileInfo[] fileInfos = directoryInfo.GetFiles();
            foreach (FileInfo fileInfo in fileInfos)
            {
                if (remoteVersionConfig.FileInfoDict.ContainsKey(fileInfo.Name))
                {
                    continue;
                }

                if (fileInfo.Name == "Version.txt")
                {
                    continue;
                }

                fileInfo.Delete();
            }
        }
        else
        {
            directoryInfo.Create();
        }

        // 对比MD5
        foreach (FileVersionInfo fileVersionInfo in remoteVersionConfig.FileInfoDict.Values)
        {
            // 对比md5
            string localFileMD5 = GetBundleMD5(streamingVersionConfig, fileVersionInfo.File);
            if (fileVersionInfo.MD5 == localFileMD5)
            {
                continue;
            }
            
            this.bundles.Enqueue(fileVersionInfo.File);
            this.TotalSize += long.Parse(fileVersionInfo.Size);
        }
        StartCoroutine(DownloadAsync());
    }
    private double downloadSize;
    private double onDownSize;
    private IEnumerator DownloadAsync()
    {
        if (this.bundles.Count == 0 && this.downloadingBundle == "")
        {
            action?.Invoke();
            yield break;
        }
        while (true)
        {
            if (this.bundles.Count == 0)
            {
                break;
            }
            this.downloadingBundle = this.bundles.Dequeue();
            while (true)
            {
                string url = ResourceConfig.URL_AB + ResourceConfig.Platform + downloadingBundle;
             
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
                            print(webRequest.downloadedBytes);
                            downloadSize = onDownSize + (double)webRequest.downloadedBytes;
                            print("TotalSize:"+ TotalSize + "downloadSize:" + downloadSize + "downloadProgress:" + (float)(downloadSize / TotalSize));
                            yield return null;
                        }
                        onDownSize += long.Parse(remoteVersionConfig.FileInfoDict[downloadingBundle].Size);
                        print("DownloadAsset:" + onDownSize);
                        byte[] data = webRequest.downloadHandler.data;
                        string path = Path.Combine(ResourceConfig.PD_AB + ResourceConfig.Platform, this.downloadingBundle);
                        using (FileStream fs = new FileStream(path, FileMode.Create))
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
        action?.Invoke();
        print("下载完成！");
    }

    public static string GetBundleMD5(VersionConfig streamingVersionConfig, string bundleName)
    {
        string path = Path.Combine(ResourceConfig.PD_AB + ResourceConfig.Platform, bundleName);
        if (File.Exists(path))
        {
            return MD5Helper.FileMD5(path);
        }

        if (streamingVersionConfig.FileInfoDict.ContainsKey(bundleName))
        {
            return streamingVersionConfig.FileInfoDict[bundleName].MD5;
        }

        return "";
    }

    private void OnDestroy()
    {
        this.TotalSize = 0;
        this.bundles = null;
        this.downloadedBundles = null;
        this.downloadingBundle = null;
    }
}
