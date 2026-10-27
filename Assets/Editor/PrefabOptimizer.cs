using UnityEditor;
using UnityEngine;

/// <summary>
/// 预制体优化工具：把屏幕外滞留时间长的预制体（敌人、子弹）的 Animator
/// 设为 CullCompletely，屏幕外完全停止动画求值。幂等，可重复执行。
/// </summary>
public static class PrefabOptimizer
{
    private static readonly string[] CullPrefabs =
    {
        "Assets/Prefabs/Enemy/Bird.prefab",
        "Assets/Prefabs/Enemy/Goblin.prefab",
        "Assets/Prefabs/Enemy/Slime.prefab",
        "Assets/Prefabs/Weapon/Projectile/BulletCommon.prefab",
        "Assets/Prefabs/Weapon/Projectile/BulletLighting.prefab",
    };

    [MenuItem("Tools/Optimize Prefabs (Animator Culling)")]
    public static void SetAnimatorCulling()
    {
        foreach (var path in CullPrefabs)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning("[PrefabOptimizer] 找不到预制体，跳过: " + path);
                continue;
            }

            var animators = prefab.GetComponentsInChildren<Animator>(true);
            if (animators.Length == 0)
            {
                Debug.LogWarning("[PrefabOptimizer] 预制体上没有 Animator，跳过: " + path);
                continue;
            }

            var changed = false;
            foreach (var animator in animators)
            {
                if (animator.cullingMode == AnimatorCullingMode.CullCompletely)
                {
                    continue;
                }

                animator.cullingMode = AnimatorCullingMode.CullCompletely;
                changed = true;
            }

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefab, path);
                Debug.Log("[PrefabOptimizer] 已设置 CullCompletely: " + path);
            }
            else
            {
                Debug.Log("[PrefabOptimizer] 无需修改: " + path);
            }
        }
    }

    /// <summary>
    /// 供命令行使用：刷新图集 + 设置 Animator Culling + 打包 一条龙
    /// </summary>
    public static void OptimizeAndBuild()
    {
        AtlasCreator.CreateAtlases();
        SetAnimatorCulling();
        CommandLineBuild.BuildWin64();
    }
}
