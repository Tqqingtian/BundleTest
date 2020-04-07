using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
public class BundleInfo
{
    public List<string> ParentPaths = new List<string>();
}

public enum PlatformType
{
    None,
    Android,
    IOS,
    PC,
    MacOS,
}

public enum BuildType
{
    Development,
    Release,
}
public class BuildEditor : EditorWindow
{
    private readonly Dictionary<string, BundleInfo> dictionary = new Dictionary<string, BundleInfo>();

    private PlatformType platformType;

    private bool isBuildInSA;
    [MenuItem("BuildAB/BundleSetting")]
    public static void ShowWindow()
    {
        GetWindow(typeof(BuildEditor));
    }

    private void OnGUI()
    {
        if (GUILayout.Button("打开PD"))
        {
            string output = Application.persistentDataPath;
            if (!Directory.Exists(output))
            {
                Directory.CreateDirectory(output);
            }
            output = output.Replace("/", "\\");
            System.Diagnostics.Process.Start("explorer.exe", output);
        }

        this.platformType = (PlatformType)EditorGUILayout.EnumPopup(platformType);

        isBuildInSA = EditorGUILayout.Toggle("是否同将资源打进SA: ", isBuildInSA);


        if (GUILayout.Button("开始打包"))
        {
            if (this.platformType == PlatformType.None)
            {
                Debug.Log("请选择打包平台!");
                return;
            }
            string fold = ResourceConfig.BD_AB + ResourceConfig.Platform;
            if (!Directory.Exists(fold))
            {
                Directory.CreateDirectory(fold);
            }
            //if (!Directory.Exists(ResourceConfig.URL_AB + ResourceConfig.Platform))
            //{
            //    Debug.Log("没有 路径 = " + ResourceConfig.URL_AB + ResourceConfig.Platform);
            //    return;
            //}
            DeleteFileOrFolder(ResourceConfig.BD_AB + ResourceConfig.Platform);
            BuildHelper.Build(this.platformType, isBuildInSA);
        }


        if (GUILayout.Button("设置标签"))
        {
            string path = string.Format("Assets/Download");

            if (Directory.Exists(path))
            {
                DirectoryInfo direction = new DirectoryInfo(path);
                FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
                Debug.Log(files.Length);
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Name.EndsWith(".meta"))
                    {
                        continue;
                    }
                    //if (files[i].Name.EndsWith(".bytes"))
                    //{
                    //    continue;
                    //}
                    string currentFile = files[i].FullName.Replace("\\", "/").Replace(@"\", "/");
                    int startIndex = currentFile.IndexOf("Assets");
                    string assetFile = currentFile.Substring(startIndex, currentFile.Length - startIndex);
                    Debug.Log(assetFile);
                    string sceneName = Path.GetFileNameWithoutExtension(assetFile);
                    AssetImporter ai = AssetImporter.GetAtPath(assetFile);
                    ai.assetBundleName = sceneName + ".assetbundle";
                }
            }
        }


        if (GUILayout.Button("删除资源"))
        {
            if (!Directory.Exists(ResourceConfig.SA_AB + ResourceConfig.Platform))
            {
                return;
            }
            DeleteFileOrFolder(ResourceConfig.SA_AB + ResourceConfig.Platform);
            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("打包轻包"))
        {
            BuildHelper.BuildInitial(ResourceConfig.SA_AB + platformType);
        }
    }

    /// <summary>
    /// 删除文件和文件夹
    /// </summary>
    /// <param name="fileOrFolder"></param>
    private static void DeleteFileOrFolder(string fileOrFolder)
    {
        if (Directory.Exists(fileOrFolder))
        {
            string[] allFiles = Directory.GetFiles(fileOrFolder);
            if (allFiles != null)
            {
                for (int i = 0; i < allFiles.Length; i++)
                {
                    DeleteFileOrFolder(allFiles[i]);
                }
            }

            string[] allFolders = Directory.GetDirectories(fileOrFolder);
            if (allFolders != null)
            {
                for (int i = 0; i < allFolders.Length; i++)
                {
                    DeleteFileOrFolder(allFolders[i]);
                }
            }

            Directory.Delete(fileOrFolder);
        }
        else
        {
            if (File.Exists(fileOrFolder))
            {
                File.Delete(fileOrFolder);
            }
        }
    }
}
