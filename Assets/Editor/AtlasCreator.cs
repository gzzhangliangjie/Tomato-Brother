using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// 批量创建 Sprite Atlas 的工具，按 Sprites 子目录分集，幂等可重复执行。
/// </summary>
public static class AtlasCreator
{
    // 每个模块的贴图单独成集，保证同模块同时渲染的贴图命中同一图集
    private static readonly string[] PackFolders =
    {
        "Assets/Sprites/Effects",
        "Assets/Sprites/Enemy",
        "Assets/Sprites/Map",
        "Assets/Sprites/Player",
        "Assets/Sprites/UI",
        "Assets/Sprites/Weapon",
    };

    private const string AtlasOutputDir = "Assets/Atlases";

    [MenuItem("Tools/Create Sprite Atlases")]
    public static void CreateAtlases()
    {
        if (!AssetDatabase.IsValidFolder(AtlasOutputDir))
        {
            AssetDatabase.CreateFolder("Assets", "Atlases");
        }

        foreach (var folder in PackFolders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                Debug.LogWarning("[AtlasCreator] 目录不存在，跳过: " + folder);
                continue;
            }

            var atlasName = folder.Substring(folder.LastIndexOf('/') + 1);
            var atlasPath = AtlasOutputDir + "/" + atlasName + ".spriteatlas";
            if (AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath) != null)
            {
                Debug.Log("[AtlasCreator] 已存在，跳过: " + atlasPath);
                continue;
            }

            var spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { folder });
            if (spriteGuids.Length == 0)
            {
                Debug.LogWarning("[AtlasCreator] 目录下没有 Sprite，跳过: " + folder);
                continue;
            }

            var sprites = new List<Object>(spriteGuids.Length);
            foreach (var guid in spriteGuids)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid));
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
            }

            var atlas = new SpriteAtlas();
            atlas.Add(sprites.ToArray());
            atlas.SetIncludeInBuild(true);
            AssetDatabase.CreateAsset(atlas, atlasPath);
            Debug.Log("[AtlasCreator] 已创建 " + atlasPath + "，包含 " + sprites.Count + " 张贴图");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 供命令行使用：先建图集再打包
    /// </summary>
    public static void CreateAtlasesAndBuild()
    {
        CreateAtlases();
        CommandLineBuild.BuildWin64();
    }
}
