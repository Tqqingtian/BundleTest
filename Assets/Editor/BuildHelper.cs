using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildHelper
{
    public static void Build(PlatformType type,bool isBuildInSA)
    {
        BuildTarget buildTarget = BuildTarget.StandaloneWindows;
        switch (type)
        {
            case PlatformType.PC:
                buildTarget = BuildTarget.StandaloneWindows64;
                break;
            case PlatformType.Android:
                buildTarget = BuildTarget.Android;
                break;
            case PlatformType.IOS:
                buildTarget = BuildTarget.iOS;
                break;
            case PlatformType.MacOS:
                buildTarget = BuildTarget.StandaloneOSX;
                break;
        }

        string URL_AB_Type = ResourceConfig.URL_AB + type;
        FileHelper.CleanDirectory(URL_AB_Type);
        if (!Directory.Exists(URL_AB_Type))
        {
            Directory.CreateDirectory(URL_AB_Type);
        }
        Debug.Log("开始打包 ");
        BuildPipeline.BuildAssetBundles(URL_AB_Type, BuildAssetBundleOptions.None, buildTarget);
        Debug.Log("完成资源打包:" + URL_AB_Type);

        GenerateVersionInfo(URL_AB_Type);
        if (isBuildInSA)
        {
            FileHelper.CleanDirectory($"{ResourceConfig.SA_AB}{type}");
            FileHelper.CopyDirectory(URL_AB_Type, $"{ResourceConfig.SA_AB}{type}");
        }
    }

    /// <summary>
    /// 打包初始包
    /// </summary>
    public static void BuildInitial(string saPath)
    {
        //string saPath = ResourceConfig.SA_AB + ResourceConfig.Platform;
        FileHelper.CleanDirectory(saPath);
        if (!Directory.Exists(saPath))
        {
            Directory.CreateDirectory(saPath);
        }
        VersionConfig versionProto = new VersionConfig();
        using (FileStream fileStream = new FileStream($"{saPath}/Version.txt", FileMode.Create))
        {
            byte[] bytes = JsonHelper.ToJson(versionProto).ToByteArray();
            fileStream.Write(bytes, 0, bytes.Length);
        }

    }

    private static VersionConfig versionConfig;
    private static void GenerateVersionInfo(string dir)
    {
        versionConfig = new VersionConfig();

        GenerateVersionProto(dir, "");

        using (FileStream fileStream = new FileStream($"{dir}/Version.txt", FileMode.Create))
        {
            byte[] bytes = JsonHelper.ToJson(versionConfig).ToByteArray();
            fileStream.Write(bytes, 0, bytes.Length);
        }
    }

    private static void GenerateVersionProto(string dir, string relativePath)
    {
        foreach (string file in Directory.GetFiles(dir))
        {
            string md5 = MD5Helper.FileMD5(file);
            FileInfo fi = new FileInfo(file);
            string size = fi.Length.ToString();
            string filePath = relativePath == "" ? fi.Name : $"{relativePath}/{fi.Name}";

            versionConfig.FileInfoDict.Add(filePath, new FileVersionInfo
            {
                File = filePath,
                MD5 = md5,
                Size = size,
            });
        }

        foreach (string directory in Directory.GetDirectories(dir))
        {
            DirectoryInfo dinfo = new DirectoryInfo(directory);
            string rel = relativePath == "" ? dinfo.Name : $"{relativePath}/{dinfo.Name}";
            GenerateVersionProto($"{dir}/{dinfo.Name}", rel);
        }
    }
}
