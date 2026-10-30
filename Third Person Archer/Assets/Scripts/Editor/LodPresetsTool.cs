using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Re-tunes the LOD groups made with PolyFew. Its middle levels saved few vertices and looked broken up close,
/// so each prefab keeps its original mesh until it is small on screen and at most one far level after it.
/// Dropped levels stay in the prefab, only switched off, so the change can be undone by hand.
/// Groups that still switch meshes or cull fade with the pipeline's dithered cross-fade instead of popping.
/// </summary>
public static class LodPresetsTool
{
    private enum Category
    {
        // Skyline: original mesh until small, then the coarsest level, never culled.
        Building,
        // Original mesh until small, then the coarsest level, culled near the horizon.
        Tree,
        // Hand-made levels (bushes): every level kept, switched further away.
        Vegetation,
        // Few vertices: original mesh only, culled when tiny.
        Prop,
        // Enemies are aimed at: original mesh only, never culled.
        Character,
        // FBX split into fake levels by "_LOD0"/"_LOD1" mesh names: all parts in one level, never culled.
        MergeParts,
    }

    private struct Preset
    {
        public bool KeepFarLevel;
        public bool KeepAllLevels;
        public bool MergeLevels;
        public float OriginalUntil;
        public float CullBelow;
        // Transition heights for KeepAllLevels, the last one being the cull height.
        public float[] Heights;
        public bool CrossFade;
    }

    private static readonly Dictionary<Category, Preset> Presets = new Dictionary<Category, Preset>
    {
        { Category.Building, new Preset { KeepFarLevel = true, OriginalUntil = 0.12f, CullBelow = 0f, CrossFade = true } },
        { Category.Tree, new Preset { KeepFarLevel = true, OriginalUntil = 0.08f, CullBelow = 0.01f, CrossFade = true } },
        { Category.Vegetation, new Preset { KeepAllLevels = true, Heights = new[] { 0.2f, 0.1f, 0.04f, 0.008f }, CrossFade = true } },
        { Category.Prop, new Preset { CullBelow = 0.012f, CrossFade = true } },
        { Category.Character, new Preset { CullBelow = 0f } },
        { Category.MergeParts, new Preset { MergeLevels = true, CullBelow = 0f } },
    };

    private static readonly (string Path, Category Category)[] Prefabs =
    {
        ("Assets/Models/Enviroment/Japanese/japan-castle/japan_castle Variant.prefab", Category.Building),
        ("Assets/Models/Enviroment/Japanese/dae-final-assignment-milestone-house/JapaneseMainHouse.prefab", Category.Building),
        ("Assets/Models/Enviroment/Japanese/Japanese Bridge0.prefab", Category.Building),
        ("Assets/Models/Enviroment/Japanese/japanese-lowpoly-temple/source/JapaneseTemple (2).prefab", Category.Building),
        ("Assets/Models/Enviroment/Viking/Viking Village/Prefabs/Buildings/pf_build_small_house_straw_roof_01_low.prefab", Category.Building),
        ("Assets/Models/Enviroment/Viking/Viking Village/Prefabs/Buildings/pf_build_storage_01_low.prefab", Category.Building),
        ("Assets/Models/Enviroment/Viking/wooden-walkway-lowpoly/Pierce.prefab", Category.Building),
        ("Assets/Models/Enviroment/Japanese/sakura/source/sakura2.prefab", Category.Tree),
        ("Assets/Models/Hipernt/Pine Pack/Prefabs/Pine1.prefab", Category.Tree),
        ("Assets/Models/Hipernt/Pine Pack/Prefabs/Pine2.prefab", Category.Tree),
        ("Assets/Models/Hipernt/Pine Pack/Prefabs/Pine3.prefab", Category.Tree),
        ("Assets/Models/Hipernt/Pine Pack/Prefabs/Pine4.prefab", Category.Tree),
        ("Assets/Models/Hipernt/Pine Pack/Prefabs/Pine5.prefab", Category.Tree),
        ("Assets/Models/Hipernt/Pine Pack/Prefabs/Pine6.prefab", Category.Tree),
        ("Assets/Models/Enviroment/Viking/Viking Village/Prefabs/Props/pf_fence_02.prefab", Category.Prop),
        ("Assets/Models/Enviroment/Viking/Viking Village/Prefabs/Props/pf_plank_01.prefab", Category.Prop),
        ("Assets/Models/Enviroment/Viking/Viking Village/Prefabs/Props/pf_torch_stick_01.prefab", Category.Prop),
        ("Assets/Models/Characters/Vikings/viking/Viking Model Variant.prefab", Category.Character),
        ("Assets/YughuesFreeBushes2018/Prefabs/P_Bush01.prefab", Category.Vegetation),
        ("Assets/YughuesFreeBushes2018/Prefabs/P_Bush02.prefab", Category.Vegetation),
        ("Assets/YughuesFreeBushes2018/Prefabs/P_Bush03.prefab", Category.Vegetation),
        ("Assets/YughuesFreeBushes2018/Prefabs/P_Bush04.prefab", Category.Vegetation),
        ("Assets/YughuesFreeBushes2018/Prefabs/P_Bush05.prefab", Category.Vegetation),
        ("Assets/Prefabs/Weapons/Crossbow/Crossbow Third Person.prefab", Category.MergeParts),
    };

    [MenuItem("Tools/LOD/Apply LOD Presets")]
    public static void ApplyAll()
    {
        foreach (var (path, category) in Prefabs)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (var group in root.GetComponentsInChildren<LODGroup>(true))
                    Apply(group, Presets[category]);

                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"LOD presets applied to {Prefabs.Length} prefabs.");
    }

    private static void Apply(LODGroup group, Preset preset)
    {
        LOD[] lods = group.GetLODs();
        if (lods.Length == 0)
            return;

        if (preset.KeepAllLevels || preset.MergeLevels)
        {
            if (preset.MergeLevels)
            {
                var parts = lods.SelectMany(l => l.renderers).Where(r => r != null).Distinct().ToArray();
                lods = new[] { new LOD(preset.CullBelow, parts) };
            }
            else
            {
                for (int i = 0; i < lods.Length; i++)
                    lods[i].screenRelativeTransitionHeight = preset.Heights[Mathf.Min(i, preset.Heights.Length - 1)];
            }

            group.SetLODs(lods);
            SetFade(group, preset.CrossFade);
            return;
        }

        Renderer[] original = lods[0].renderers.Where(r => r != null).ToArray();
        Renderer[] far = preset.KeepFarLevel && lods.Length > 1 ? lods[lods.Length - 1].renderers.Where(r => r != null).ToArray() : null;

        // Switch off every level that is not used any more; a renderer outside the group would draw all the time.
        for (int i = 1; i < lods.Length; i++)
        {
            bool keep = far != null && i == lods.Length - 1;
            foreach (var renderer in lods[i].renderers)
                if (renderer != null)
                    renderer.gameObject.SetActive(keep);
        }

        var result = new List<LOD>();
        if (far != null)
        {
            result.Add(new LOD(preset.OriginalUntil, original));
            result.Add(new LOD(preset.CullBelow, far));
        }
        else
        {
            result.Add(new LOD(preset.CullBelow, original));
        }

        group.SetLODs(result.ToArray());
        SetFade(group, preset.CrossFade);
    }

    private static void SetFade(LODGroup group, bool crossFade)
    {
        // Animated: the fade runs over LODGroup.crossFadeAnimationDuration once the level changes,
        // so it never sits half-faded while the camera holds still.
        group.fadeMode = crossFade ? LODFadeMode.CrossFade : LODFadeMode.None;
        group.animateCrossFading = crossFade;
        EditorUtility.SetDirty(group);
    }
}
