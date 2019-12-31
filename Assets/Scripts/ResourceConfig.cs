using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceConfig 
{

    public static string URL_AB = Application.dataPath + "/../AssetBundle/";

    /// <summary>
    /// 生成包
    /// </summary>
    public static string SA_AB = SA + "/AssetBundle/";

    /// <summary>
    /// 读取包
    /// </summary>
    public static string PD_AB = Application.persistentDataPath + "/AssetBundle/";

    /// <summary>
    /// 应用程序内部资源路径存放路径(www/webrequest专用)
    /// </summary>
    public static string SA
    {
        get
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
                return $"file://{Application.streamingAssetsPath}";
#else
            return Application.streamingAssetsPath;
#endif

        }
    }
    public static string Platform
    {
        get
        {
            string url = "";
#if UNITY_ANDROID
			url += "Android/";
#elif UNITY_IOS
			url += "IOS/";
#elif UNITY_WEBGL
			url += "WebGL/";
#elif UNITY_STANDALONE_OSX
			url += "MacOS/";
#else
            url += "PC/";
# endif
            return url;
        }

        private set { Platform = value; }

            
    }


}
