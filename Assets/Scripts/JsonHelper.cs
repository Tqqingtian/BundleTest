using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public static class JsonHelper
{
    public static string ToJson(object obj)
    {
        return LitJson.JsonMapper.ToJson(obj);
    }

    public static T FromJson<T>(string str)
    {
        T t = LitJson.JsonMapper.ToObject<T>(str);
        ISupportInitialize iSupportInitialize = t as ISupportInitialize;
        if (iSupportInitialize == null)
        {
            return t;
        }
        iSupportInitialize.EndInit();
        return t;
    }

    public static T Clone<T>(T t)
    {
        return FromJson<T>(ToJson(t));
    }
}
