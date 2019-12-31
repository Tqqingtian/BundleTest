using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileVersionInfo
{
    public string File;
    public string MD5;
    public string Size;
}

public class VersionConfig
{

    public Dictionary<string, FileVersionInfo> FileInfoDict;

    public VersionConfig() {
        
        FileInfoDict = new Dictionary<string, FileVersionInfo>();
    }
}
