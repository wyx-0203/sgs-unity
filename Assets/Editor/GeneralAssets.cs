using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

public enum Gender
{
    男, 女
}

[Serializable]
public class GeneralAsset
{
    public string 武将名;
    public int id;
    public string 势力;
    public int 体力上限;
    public int 初始体力;
    public Gender 性别;
    public List<string> 技能;
}

public class GeneralAssets : ScriptableObject
{
    public List<GeneralAsset> 武将列表;
}
