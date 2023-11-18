using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class GeneralManager : EditorWindow
{
    private List<General> generals;
    private General current;
    private ListView listView;

    private IntegerField id;
    private TextField _name;
    private Toggle gender;
    private DropdownField nation;
    private IntegerField hp_limit;

    private VisualElement skills;

    private Button save;


    [MenuItem("Window/武将")]
    public static void ShowExample()
    {
        GeneralManager wnd = GetWindow<GeneralManager>();
        wnd.titleContent = new GUIContent("武将");
    }

    public async void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/GeneralManager.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);

        // 获取元素
        listView = root.Q<ListView>("ListView");
        id = root.Q<IntegerField>("id");
        _name = root.Q<TextField>("name");
        gender = root.Q<Toggle>("gender");
        nation = root.Q<DropdownField>("nation");
        nation.choices = new List<string> { "蜀", "魏", "吴", "群" };
        hp_limit = root.Q<IntegerField>("hp_limit");
        skills = root.Q<VisualElement>("skills").Q<VisualElement>("unity-content");
        save = root.Q<Button>("save");
        save.clicked += Save;

        // 初始化武将列表
        generals = (await GameCore.General.GetList()).Select(x => ScriptableObject.CreateInstance<General>().Init(x)).ToList();
        listView.itemsSource = generals;
        listView.makeItem = MakeListItem;
        listView.bindItem = BindListItem;
        listView.onSelectionChange += OnSelectItem;
        listView.SetSelection(0);

        // 修改id、name时更新武将列表
        id.RegisterValueChangedCallback(x =>
        {
            generals.Sort((x, y) => x.id.CompareTo(y.id));
            listView.Rebuild();
        });
        _name.RegisterValueChangedCallback(x => listView.Rebuild());
    }

    private void Save()
    {
        // 将列表转化为json字符串
        var list = new JsonList<GameCore.General>
        {
            list = generals.Select(x => new GameCore.General
            {
                id = x.id,
                name = x._name,
                gender = x.gender,
                nation = x.nation,
                hp_limit = x.hp_limit,
                skill = x.skills.Select(skill => skill._name).ToList(),
                describe = x.skills.Select(skill => skill.describe).ToList()
            }).OrderBy(x => x.id).ToList()
        };
        string json = JsonUtility.ToJson(list);

        // 写入文件
        string filePath = "Assets/StreamingAssets/Json/general.json";
        using (var writer = new StreamWriter(filePath)) writer.Write(json);
        Debug.Log("字符串已成功写入文件：" + filePath);
    }

    // 选中列表元素时
    private void OnSelectItem(IEnumerable<object> enumerable)
    {
        // 设置当前元素
        if (enumerable.FirstOrDefault() is not General g) return;
        current = g;

        // 绑定数据
        var serializedObject = new SerializedObject(current);
        id.BindProperty(serializedObject);
        _name.BindProperty(serializedObject);
        gender.BindProperty(serializedObject);
        nation.BindProperty(serializedObject);
        hp_limit.BindProperty(serializedObject);

        // 初始化技能
        skills.hierarchy.Clear();
        foreach (var i in current.skills)
        {
            i.src = current;
            skills.hierarchy.Add(i.NewElement());
        }
    }

    private void BindListItem(VisualElement element, int index)
    {
        if (element is not Label label) return;
        label.text = $"{generals[index].id.ToString().PadLeft(3, '0')} {generals[index]._name}";

        label.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
        {
            evt.menu.AppendAction("新建武将", x =>
            {
                generals.Add(ScriptableObject.CreateInstance<General>());
                generals[generals.Count - 1].skills.Add(ScriptableObject.CreateInstance<Skill>());
                generals.Sort((x, y) => x.id.CompareTo(y.id));
                listView.SetSelection(0);
                listView.Rebuild();
            });
            evt.menu.AppendAction("移除武将", x =>
            {
                var g = generals[index];
                generals.RemoveAt(index);
                listView.Rebuild();
                if (current == g) listView.SetSelection(Mathf.Min(index, generals.Count - 1));
            });
        }));
    }

    private VisualElement MakeListItem()
    {
        var label = new Label();
        label.style.unityTextAlign = TextAnchor.MiddleLeft;
        label.style.marginLeft = 5;
        return label;
    }
}