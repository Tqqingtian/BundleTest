using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ChinarDownLoadFile : MonoBehaviour
{
    public Slider ProgressBar; //进度条
    public Text SliderValue; //滑动条值

    private Button startBtn;    //开始按钮

    void Start()
    {
        //初始化进度条和文本框
        ProgressBar.value = 0;
        SliderValue.text = "0.0%";
        OnClickStartDownload();
    }


    /// <summary>
    /// 回调函数：开始下载
    /// </summary>
    public void OnClickStartDownload()
    {
        StartCoroutine(DownloadFile());
    }


    /// <summary>
    /// 协程：下载文件
    /// </summary>
    IEnumerator DownloadFile()
    {
        UnityWebRequest uwr = UnityWebRequest.Get("E:/LianXi/UnpackDemo/BundleTest/Mars.bytes"); //创建UnityWebRequest对象，将Url传入
        uwr.SendWebRequest();                                                                                  //开始请求
        if (uwr.isNetworkError || uwr.isHttpError)                                                             //如果出错
        {
            Debug.Log(uwr.error); //输出 错误信息
        }
        else
        {
            while (!uwr.isDone) //只要下载没有完成，一直执行此循环
            {
                ProgressBar.value = uwr.downloadProgress; //展示下载进度
                SliderValue.text = Math.Floor(uwr.downloadProgress * 100) + "%";
                print(Math.Floor(uwr.downloadProgress * 100) + "%");
                yield return 0;
            }

            if (uwr.isDone) //如果下载完成了
            {
                print("完成");
                ProgressBar.value = 1; //改变Slider的值
                SliderValue.text = 100 + "%";
            }

            byte[] results = uwr.downloadHandler.data;
            // 注意真机上要用Application.persistentDataPath
            CreateFile("E:/LianXi/Mars.bytes", results, uwr.downloadHandler.data.Length);
            //AssetDatabase.Refresh(); //刷新一下
        }
    }


    /// <summary>
    /// 这是一个创建文件的方法
    /// </summary>
    /// <param name="path">保存文件的路径</param>
    /// <param name="bytes">文件的字节数组</param>
    /// <param name="length">数据长度</param>
    void CreateFile(string path, byte[] bytes, int length)
    {
        Stream sw;
        FileInfo file = new FileInfo(path);
        if (!file.Exists)
        {
            sw = file.Create();
        }
        else
        {
            return;
        }

        sw.Write(bytes, 0, length);
        sw.Close();
        sw.Dispose();
    }
}
