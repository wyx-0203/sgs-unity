using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Newtonsoft.Json;

namespace Editor
{
    public class Voice : ScriptableObject
    {
        public string skill_name;
        public string url1;
        public string url2;

        private static VisualTreeAsset template;

        public VisualElement NewElement(SkinManager skinManager)
        {
            template ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/voice.uxml");
            var element = template.CloneTree();

            var skillName = element.Q<TextField>("skill-name");
            var voice_url1 = element.Q<TextField>("voice-url1");
            var voice_url2 = element.Q<TextField>("voice-url2");

            var serializedObject = new SerializedObject(this);
            skillName.BindProperty(serializedObject);
            voice_url1.BindProperty(serializedObject);
            voice_url2.BindProperty(serializedObject);

            element.RegisterCallback((ContextualMenuPopulateEvent evt) =>
            {
                evt.menu.AppendAction("新建技能", x =>
                {
                    var newVoice = ScriptableObject.CreateInstance<Voice>();
                    skinManager.voices.Add(newVoice);
                    element.parent.hierarchy.Add(newVoice.NewElement(skinManager));
                });
                evt.menu.AppendAction("移除技能", x =>
                {
                    skinManager.voices.Remove(this);
                    element.parent.hierarchy.Remove(element);
                });
            });

            return element;
        }
    }


    public class SkinManager : EditorWindow
    {
        [MenuItem("Window/新建皮肤")]
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
            dynamic = root.Q<Toggle>("dynamic");
            voiceParent = root.Q<VisualElement>("voices").Q<VisualElement>("unity-content");
            save = root.Q<Button>("save");
            save.clicked += Save;

            AddSkill("技能1");
            AddSkill("技能2");
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
        private Toggle dynamic;

        private VisualElement voiceParent;
        private Button save;

        public List<Voice> voices = new();

        private const string seatUrl = "https://web.sanguosha.com/10/pc/res/assets/runtime/general/seat/static/";
        private const string windowUrl = "https://web.sanguosha.com/10/pc/res/assets/runtime/general/window/";
        private const string bigUrl = "https://web.sanguosha.com/10/pc/res/assets/runtime/general/big/static/";
        private const string dynamicUrl = "https://web.sanguosha.com/10/pc/res/assets/runtime/general/big/dynamic/";
        // https://web.sanguosha.com/10/pc/res/assets/runtime/general/big/dynamic/706001/daiji.png
        private const string seatPath = "Assets/StreamingAssets/Image/General/Seat/";
        private const string windowPath = "Assets/StreamingAssets/Image/General/Window/";
        private const string bigPath = "Assets/StreamingAssets/Image/General/Big/";

        private const string voicePath = "Assets/Assets/Skin/Voice/";


        // 下载图片
        private async Task DownloadImage()
        {
            using (WebClient client = new WebClient())
            {
                string fileName = id.value + ".png";

                await client.DownloadFileTaskAsync(seatUrl + fileName, seatPath + fileName);
                Debug.Log("图片下载成功并保存为 " + seatPath + fileName);

                await client.DownloadFileTaskAsync(windowUrl + fileName, windowPath + fileName);
                Debug.Log("图片下载成功并保存为 " + windowPath + fileName);

                await client.DownloadFileTaskAsync(bigUrl + fileName, bigPath + fileName);
                Debug.Log("图片下载成功并保存为 " + bigPath + fileName);
            }
        }

        private async Task<AudioClip> DownloadOneVoice(string url)
        {
            var s = url.Split('/');
            string folderName = voicePath + s[s.Length - 2];
            string fileName = folderName + "/" + s[s.Length - 1];

            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            using (WebClient client = new WebClient()) await client.DownloadFileTaskAsync(url, fileName);
            AssetDatabase.Refresh();
            Debug.Log("音频下载成功并保存为 " + fileName);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(fileName);
        }

        // 下载语音
        private async Task DownloadVoice()
        {
            var voiceAsset = ScriptableObject.CreateInstance<VoiceAsset>();
            voiceAsset.voices = new();
            foreach (var i in voices)
            {
                var voice = new KeyValue<List<AudioClip>>
                {
                    key = i.skill_name,
                    value = new List<AudioClip>()
                };
                if (i.url1 != null && i.url1 != "") voice.value.Add(await DownloadOneVoice(i.url1));
                if (i.url2 != null && i.url2 != "") voice.value.Add(await DownloadOneVoice(i.url2));
                voiceAsset.voices.Add(voice);
                // Debug.Log(voice.value.All(x => x != null));
            }

            string fileName = $"{voicePath}{id.value}.asset";
            string abName = $"voice/{id.value}";
            // AssetDatabase.Refresh();
            AssetDatabase.CreateAsset(voiceAsset, fileName);
            AssetImporter.GetAtPath(fileName).assetBundleName = abName;
            var abbuild = new AssetBundleBuild
            {
                assetBundleName = abName,
                assetNames = new string[] { fileName }
            };
            BuildPipeline.BuildAssetBundles
            (
                "Assets/StreamingAssets/AssetBundles",
                new AssetBundleBuild[] { abbuild },
                BuildAssetBundleOptions.ChunkBasedCompression,
                EditorUserBuildSettings.activeBuildTarget
            );
            // BuildPipeline.BuildAssetBundles()
            // AssetDatabase.GetAssetPathsFromAssetBundle();
            // foreach (var ab in AssetDatabase.GetAllAssetBundleNames().Where(x => !x.StartsWith("dynamic/") && !x.StartsWith("voice/")))
            // {
            //     BuildPipeline.BuildAssetBundle(
            //         "Assets/StreamingAssets/AssetBundles1",
            //         AssetDatabase.GetAssetPathsFromAssetBundle(ab).Select(x=>new AssetBundleBuild{assetBundleName=ab,
            //         assetNames=x})
            //     )
            // }
            Debug.Log($"打包成功：{abName}");
        }

        private async Task DownloadOneDynamic(int id, string name, string folderName)
        {
            id += 200000;
            Debug.Log($"{dynamicUrl}{id}/{name}");
            var client = new WebClient();
            try
            {
                await client.DownloadFileTaskAsync($"{dynamicUrl}{id}/{name}.json", $"{folderName}/{name}.json");
                await client.DownloadFileTaskAsync($"{dynamicUrl}{id}/{name}.png", $"{folderName}/{name}.png");
                await client.DownloadFileTaskAsync($"{dynamicUrl}{id}/{name}.atlas", $"{folderName}/{name}.atlas.txt");
                Debug.Log($"下载成功：{dynamicUrl}{id}/{name}");
            }
            catch (System.Exception e) { Debug.Log(e); }
            finally { client.Dispose(); }
        }

        private async Task DownloadDynamic()
        {
            string folderName = "Assets/Assets/Skin/Dynamic/" + id.value;
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            string[] names = { "daiji", "daiji2", "beijing" };

            foreach (var i in names) await DownloadOneDynamic(id.value, i, folderName);
            AssetDatabase.Refresh();

            string abname = $"dynamic/{id.value}";
            var abbuild = new AssetBundleBuild
            {
                assetBundleName = abname,
                assetNames = names.Select(x => $"{folderName}/{x}_SkeletonData.asset").Where(x => File.Exists(x)).ToArray()
            };
            var dependency = new AssetBundleBuild
            {
                assetBundleName = "spine-base",
                assetNames = new string[]
                {
                    "Assets/3rd/Spine/spine-unity/Editor/GUI/SkeletonDataAsset Icon.png",
                    "Assets/3rd/Spine/spine-unity/Editor/GUI/AtlasAsset Icon.png",
                    "Assets/3rd/Spine/spine-unity/Shaders/Spine-Skeleton.shader"
                }
            };
            foreach (var i in abbuild.assetNames) AssetImporter.GetAtPath(i).assetBundleName = abname;
            BuildPipeline.BuildAssetBundles
            (
                "Assets/StreamingAssets/AssetBundles",
                new AssetBundleBuild[] { dependency, abbuild },
                BuildAssetBundleOptions.ChunkBasedCompression,
                EditorUserBuildSettings.activeBuildTarget
            );
        }

        private void Save1()
        {

        }

        private async void Save()
        {
            // 下载皮肤图片
            await DownloadImage();

            // 下载语音
            await DownloadVoice();

            // 下载动态皮肤
            if (dynamic.value) await DownloadDynamic();

            // 创建skin对象
            var skin = new SkinAsset
            {
                generalId = general_id.value,
                id = id.value,
                name = _name.value,
                dynamic = dynamic.value
            };

            // 更新皮肤列表
            await SkinAsset.Init();
            var skins = SkinAsset.GetList();
            skins.Add(skin);
            skins = skins.OrderBy(x => x.generalId).ThenBy(x => x.name == "界限突破" ? 0 : x.id).ToList();

            // 写入文件
            string json = JsonConvert.SerializeObject(skins);
            string filePath = "Assets/StreamingAssets/Json/skin.json";
            using (var writer = new StreamWriter(filePath)) writer.Write(json);
            Debug.Log("字符串已成功写入文件：" + filePath);

            // 更新武将的皮肤信息
            Model.General.Init(await WebRequest.Get(Url.JSON + "general.json"));
            var generals = Model.General.GetList();
            generals.Find(x => x.id == general_id.value).skins = skins
                .Where(x => x.generalId == general_id.value)
                .Select(x => x.id)
                .ToList();

            // 写入文件
            json = JsonConvert.SerializeObject(generals);
            filePath = "Assets/StreamingAssets/Json/general.json";
            using (var writer = new StreamWriter(filePath)) writer.Write(json);
            Debug.Log("字符串已成功写入文件：" + filePath);

            AssetDatabase.Refresh();

        }
    }
}