using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public static class MenuItems
{
    [MenuItem("Game/更新武将")]
    public static async void SaveGenerals()
    {
        var asset = AssetDatabase.LoadAssetAtPath<GeneralAssets>("Assets/Assets/Generals.asset");

        await SkinAsset.Init();
        var skins = SkinAsset.GetList();
        var generals = asset.武将列表.Select(x => new Model.General
        {
            id = x.id,
            name = x.武将名,
            kindom = x.势力,
            hpLimit = x.体力上限,
            hp = x.初始体力,
            gender = x.性别 == Gender.男 ? Model.Gender.Male : Model.Gender.Female,
            skills = x.技能,
            skins = skins.Where(s => s.generalId == x.id).Select(s => s.id).ToList()
        }).OrderBy(x => x.id);

        // 写入文件
        string json = JsonConvert.SerializeObject(generals);
        string filePath = "Assets/StreamingAssets/Json/general.json";
        using (var writer = new StreamWriter(filePath)) writer.Write(json);
        Debug.Log("武将已修改：" + filePath);
    }

    [MenuItem("Game/更新技能")]
    public static void SaveSkills()
    {
        var asset = AssetDatabase.LoadAssetAtPath<SkillAssets>("Assets/Assets/Skills.asset");

        // 写入文件
        string json = JsonConvert.SerializeObject(asset.skills);
        string filePath = "Assets/StreamingAssets/Json/skill.json";
        using (var writer = new StreamWriter(filePath)) writer.Write(json);
        Debug.Log("技能已修改：" + filePath);
    }

    [MenuItem("Game/上传代码到服务器")]
    public static void CopyToServer()
    {
        CopyDirectory("Assets/Scripts/Model", "../sgs-server/service-room/Model");
        CopyDirectory("Assets/Scripts/GameCore", "../sgs-server/service-room/GameCore");
        CopyDirectory("Assets/Scripts/Skills", "../sgs-server/service-room/Skills");
        CopyDirectory("Assets/StreamingAssets/Json", "../sgs-server/service-room/Room/Static");
    }

    private static void CopyDirectory(string sourceDirectory, string targetDirectory)
    {
        // 确保目标文件夹存在，如果不存在则创建
        if (Directory.Exists(targetDirectory))
        {
            var directory = new DirectoryInfo(targetDirectory);
            // 删除文件夹中的所有文件
            foreach (var file in directory.GetFiles().Where(x => !x.Name.EndsWith(".csproj")))
            {
                file.Delete();
            }

            // 删除文件夹中的所有子文件夹及其内容
            foreach (var subDirectory in directory.GetDirectories())
            {
                subDirectory.Delete(true);
            }
        }
        else Directory.CreateDirectory(targetDirectory);

        // 获取源文件夹中的所有文件和子文件夹
        string[] files = Directory.GetFiles(sourceDirectory);
        string[] subDirectories = Directory.GetDirectories(sourceDirectory);

        // 复制文件
        foreach (string filePath in files.Where(x => x.EndsWith(".cs") || x.EndsWith(".json")))
        {
            string fileName = Path.GetFileName(filePath);
            string targetFilePath = Path.Combine(targetDirectory, fileName);

            File.Copy(filePath, targetFilePath, true); // 如果目标文件存在，覆盖
            // Debug.Log($"file:{filePath}, target:{targetFilePath}");
            Debug.Log($"复制文件: {fileName}");
        }

        // 递归复制子文件夹
        foreach (string subDirectory in subDirectories)
        {
            string subDirectoryName = Path.GetFileName(subDirectory);
            string targetSubDirectory = Path.Combine(targetDirectory, subDirectoryName);

            CopyDirectory(subDirectory, targetSubDirectory);
        }
    }

    private const string abPath = "Assets/StreamingAssets/AssetBundles";

    [MenuItem("Game/Build AssetBundle")]
    private static void BuildAssetBundle()
    {
        Debug.Log("开始清空文件夹");
        // 遍历文件夹中的每个文件
        foreach (string filePath in Directory.GetFiles(abPath))
        {
            // 获取文件名
            string fileName = Path.GetFileName(filePath);

            // 删除文件
            if (!fileName.StartsWith("dynamic") && !fileName.StartsWith("voice")) File.Delete(filePath);
        }

        Debug.Log("开始构建AssetBundle");
        var abbuilds = AssetDatabase.GetAllAssetBundleNames()
            .Where(x => !x.StartsWith("dynamic/") && !x.StartsWith("voice/"))
            .Select(ab => new AssetBundleBuild
            {
                assetBundleName = ab,
                assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(ab)
            })
            .ToArray();

        BuildPipeline.BuildAssetBundles
        (
            abPath,
            abbuilds,
            BuildAssetBundleOptions.ChunkBasedCompression,
            EditorUserBuildSettings.activeBuildTarget
        );

        AssetDatabase.Refresh();
        Debug.Log("构建完成！");
    }

    private const string exportPath = "sgs.unitypackage";

    [MenuItem("Game/Export Package")]
    private static void ExportPackage()
    {
        File.Delete(exportPath);
        string[] assetPaths =
        {
            "Assets/StreamingAssets/AssetBundles",
            "Assets/StreamingAssets/Image",
            "Assets/Assets/Skin"
        };
        AssetDatabase.ExportPackage(assetPaths, exportPath, ExportPackageOptions.Recurse);
        Debug.Log("导出完成: " + exportPath);
    }
}
