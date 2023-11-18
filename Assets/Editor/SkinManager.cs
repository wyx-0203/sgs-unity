using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class SkinManager : EditorWindow
{
    [MenuItem("Window/皮肤")]
    public static void ShowExample()
    {
        SkinManager wnd = GetWindow<SkinManager>();
        wnd.titleContent = new GUIContent("新建皮肤");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/SkinManager.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);

        // 获取元素
        general_id = root.Q<IntegerField>("general_id");
        id = root.Q<IntegerField>("id");
        _name = root.Q<TextField>("name");
        voiceParent = root.Q<VisualElement>("voices").Q<VisualElement>("unity-content");
        save = root.Q<Button>("save");
        save.clicked += Save;

        // 绑定数据
        var skin = ScriptableObject.CreateInstance<Skin>();
        var serializedObject = new SerializedObject(skin);
        general_id.BindProperty(serializedObject);
        id.BindProperty(serializedObject);
        _name.BindProperty(serializedObject);

        AddSkill("技能1");
        AddSkill("阵亡");
    }

    private void AddSkill(string name)
    {
        var voice = ScriptableObject.CreateInstance<Voice>();
        voice.skill_name = name;
        voices.Add(voice);
        var element = voice.NewElement(this);
        voiceParent.hierarchy.Add(element);
    }

    private IntegerField general_id;
    private IntegerField id;
    private TextField _name;

    private VisualElement voiceParent;
    private Button save;

    public List<Voice> voices = new();

    private const string seatUrl = "https://web.sanguosha.com/10/pc/res/assets/runtime/general/seat/static/";
    private const string windowUrl = "https://web.sanguosha.com/10/pc/res/assets/runtime/general/window/";
    private const string bigUrl = "https://web.sanguosha.com/10/pc/res/assets/runtime/general/big/static/";

    private const string seatPath = "Assets/StreamingAssets/Image/General/Seat/";
    private const string windowPath = "Assets/StreamingAssets/Image/General/Window/";
    private const string bigPath = "Assets/StreamingAssets/Image/General/Big/";

    private const string voicePath = "Assets/StreamingAssets/Audio/skin/";

    // 下载图片
    private void DownloadImage(int skin_id)
    {
        using (WebClient client = new WebClient())
        {
            string fileName = skin_id + ".png";

            client.DownloadFile(seatUrl + fileName, seatPath + fileName);
            Debug.Log("图片下载成功并保存为 " + seatPath + fileName);

            client.DownloadFile(windowUrl + fileName, windowPath + fileName);
            Debug.Log("图片下载成功并保存为 " + windowPath + fileName);

            client.DownloadFile(bigUrl + fileName, bigPath + fileName);
            Debug.Log("图片下载成功并保存为 " + bigPath + fileName);
        }
    }

    // 下载语音
    private string DownloadVoice(string url)
    {
        var strings = url.Split('/');
        string folderName = strings[strings.Length - 2];
        string fileName = folderName + "/" + strings[strings.Length - 1];

        if (!Directory.Exists(voicePath + folderName))
        {
            Directory.CreateDirectory(voicePath + folderName);
        }

        using (WebClient client = new WebClient()) client.DownloadFile(url, voicePath + fileName);
        Debug.Log("音频下载成功并保存为 " + voicePath + fileName);
        return fileName;
    }

    private async void Save()
    {
        // 下载皮肤图片
        DownloadImage(id.value);

        // 下载语音
        Dictionary<Voice, List<string>> dict = new();
        foreach (var i in voices)
        {
            dict.Add(i, new List<string>());
            if (i.url1 != null && i.url1 != "") dict[i].Add(DownloadVoice(i.url1));
            if (i.url2 != null && i.url2 != "") dict[i].Add(DownloadVoice(i.url2));
        }

        // 创建skin对象
        var skin = new GameCore.Skin
        {
            general_id = general_id.value,
            id = id.value,
            name = _name.value,
            voice = voices.Select(x => new GameCore.Voice
            {
                name = x.skill_name,
                url = dict[x]
            }).ToList()
        };
        Debug.Log(JsonUtility.ToJson(skin));

        // 获得并更新皮肤列表
        var skins = await GameCore.Skin.GetList();
        skins.Add(skin);
        var list = new JsonList<GameCore.Skin>
        {
            list = skins.OrderBy(x => x.general_id).ThenBy(x => x.id).ToList()
        };
        string json = JsonUtility.ToJson(list);

        // 写入文件
        string filePath = "Assets/StreamingAssets/Json/skin.json";
        using (var writer = new StreamWriter(filePath)) writer.Write(json);
        Debug.Log("字符串已成功写入文件：" + filePath);
    }
}
