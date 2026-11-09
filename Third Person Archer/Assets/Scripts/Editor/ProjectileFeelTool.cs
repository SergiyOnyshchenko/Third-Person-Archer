using System.Collections.Generic;
using System.Linq;
using Actor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sets up projectile visuals without touching how anything flies:
/// enemy projectiles that glow and whiz past, surface impacts, a release puff, trails that follow the draw,
/// and lighter projectiles (fire/frost effects loaded only when used, stuck ones capped).
/// Safe to run again: existing objects are reused and re-tuned.
/// </summary>
public static class ProjectileFeelTool
{
    private const string Weapons = "Assets/Prefabs/Weapons/";
    private const string Materials = "Assets/Materials/Impact/";
    private const string SettingsPath = "Assets/Resources/" + ImpactEffectsSettings.ResourcePath + ".asset";
    private const string GlintMaterial = "Assets/Materials/Enemy Attack Glint.mat";
    private const string AlphaMaterial = "Assets/Materials/White Unlit.mat";

    private const string NiceVibrations = "Assets/Plugins/Feel/NiceVibrations/HapticSamples/Impacts/";
    private const string Sounds = "Assets/Audio/Sounds/";

    private struct Shot
    {
        public string Prefab;
        public bool Enemy;
        public float GlowSize;
    }

    private static readonly Shot[] Shots =
    {
        new Shot { Prefab = "Arrow/Arrow Projectile Player" },
        new Shot { Prefab = "Spear/Spear Projectile" },
        new Shot { Prefab = "Shuriken/Shuriken Projectile Player" },
        new Shot { Prefab = "Boomerang/Boomerang Projectile Player" },
        new Shot { Prefab = "Enemy/Arrow Projectile Enemy", Enemy = true, GlowSize = 0.45f },
        new Shot { Prefab = "Enemy/Axe Enemy Projectile New Variant", Enemy = true, GlowSize = 0.6f },
        new Shot { Prefab = "Enemy/Shuriken Enemy Projectile New Variant", Enemy = true, GlowSize = 0.45f },
    };

    [MenuItem("Tools/Projectiles/Apply Projectile Feel")]
    public static void ApplyAll()
    {
        var settings = EnsureImpactSettings();
        var glint = AssetDatabase.LoadAssetAtPath<Material>(GlintMaterial);
        var whiz = AssetDatabase.LoadAssetAtPath<AudioClip>(Sounds + "Arrow Fly.mp3");

        // Fire/frost effects out of the projectiles first: the variants below inherit the change.
        MoveElementalFx(Weapons + "Projectile.prefab",
            new[] { "View/VFX/Elemrntal", "View/VFX/Parent Hit/Fire Hit", "View/VFX/Parent Hit/Frost Hit" },
            Weapons + "Elemental/Projectile Elemental FX.prefab", "Elemental FX Loader");
        // The arrows' fire/frost mesh glows stay: they sit inside the nested arrow model, which can't give them up.

        foreach (var shot in Shots)
            Edit(Weapons + shot.Prefab + ".prefab", root => SetupShot(root, shot, glint, whiz));

        Edit("Assets/Prefabs/Actors/Player Actor.prefab", root =>
        {
            Transform systems = root.transform.Find("Systems") ?? root.transform;
            GetOrAdd<ShotReleaseEffect>(systems, "Shot Release Effect");
        });

        AssetDatabase.SaveAssets();
        Debug.Log($"Projectile feel applied ({settings.Presets.Length} impact surfaces).");
    }

    // ── Projectile prefabs ──────────────────────────────────────────────

    private static void SetupShot(GameObject root, Shot shot, Material glint, AudioClip whiz)
    {
        var projectile = root.GetComponent<Projectile>();
        Transform systems = root.transform.Find("Systems");
        Transform view = root.transform.Find("View");

        var impact = GetOrAdd<ProjectileImpact>(systems, "Impact");
        Set(impact, "_onActors", !shot.Enemy);

        if (!shot.Enemy)
            GetOrAdd<StuckProjectileLimit>(systems, "Stuck Limit");

        var style = view.GetComponent<ProjectileTrailStyle>();
        if (style == null)
            style = view.gameObject.AddComponent<ProjectileTrailStyle>();
        Set(style, "_projectile", projectile);
        Set(style, "_trail", view.GetComponentInChildren<TrailRenderer>(true));
        if (shot.Enemy)
        {
            // Warm and glowing: a threat that reads from across the level.
            Set(style, "_width", 0.1f);
            Set(style, "_time", 0.3f);
            Set(style, "_fullDrawWidth", 0.1f);
            Set(style, "_fullDrawTime", 0.3f);
            Set(style, "_head", new Color(1f, 0.62f, 0.25f, 0.95f));
            Set(style, "_tail", new Color(1f, 0.3f, 0.1f, 0f));
            Set(style, "_fullDrawHead", new Color(1f, 0.62f, 0.25f, 0.95f));
            Set(style, "_glowMaterial", glint);
            Set(style, "_glowColor", new Color(1f, 0.45f, 0.15f, 0.9f));
            Set(style, "_glowSize", shot.GlowSize);

            var nearMiss = view.GetComponent<ProjectileNearMiss>();
            if (nearMiss == null)
                nearMiss = view.gameObject.AddComponent<ProjectileNearMiss>();
            Set(nearMiss, "_projectile", projectile);
            Set(nearMiss, "_clip", whiz);
        }
        else
        {
            Set(style, "_glowMaterial", null);
        }

        // The old one-size-fits-all hit puff gives way to the surface impacts (fire/frost hits keep their own).
        foreach (var path in new[] { "View/VFX/Parent Hit", "View/VFX/Parent Hit/Regular Hit" })
        {
            Transform hit = root.transform.Find(path);
            if (hit != null && hit.TryGetComponent(out ParticleSystemRenderer renderer))
                renderer.enabled = false;
        }
    }

    // ── Lazy fire/frost effects ─────────────────────────────────────────

    private static void MoveElementalFx(string prefabPath, string[] paths, string fxPath, string loaderName)
    {
        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            var parts = paths.Select(p => (path: p, transform: root.transform.Find(p))).Where(p => p.transform != null).ToList();
            if (parts.Count == 0)
                return;

            EnsureFolder(fxPath.Substring(0, fxPath.LastIndexOf('/')));

            var fxRoot = new GameObject(System.IO.Path.GetFileNameWithoutExtension(fxPath));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(fxRoot, root.scene);
            var fxView = fxRoot.AddComponent<ElementalView>();

            var moved = new HashSet<Transform>();
            foreach (var (path, part) in parts)
                foreach (var t in part.GetComponentsInChildren<Transform>(true))
                    moved.Add(t);

            MoveViewCalls(root, moved, fxView);

            foreach (var (path, part) in parts)
            {
                string parentPath = path.Contains('/') ? path.Substring(0, path.LastIndexOf('/')) : "";
                var mount = part.gameObject.AddComponent<ElementalFxMount>();
                mount.Setup(parentPath, part);
                part.SetParent(fxRoot.transform, false);
            }

            var fx = PrefabUtility.SaveAsPrefabAsset(fxRoot, fxPath);
            Object.DestroyImmediate(fxRoot);

            var loader = GetOrAdd<ElementalFxLoader>(root.transform.Find("Systems"), loaderName);
            Set(loader, "_prefab", fx);

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Debug.Log($"{System.IO.Path.GetFileName(prefabPath)}: {parts.Count} elemental parts now load on demand from {fxPath}.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    /// <summary>Moves every view call aimed at a moved part from the projectile's elemental views to the new view.</summary>
    private static void MoveViewCalls(GameObject root, HashSet<Transform> moved, ElementalView target)
    {
        var targetSo = new SerializedObject(target);
        var targetViews = targetSo.FindProperty("_views");
        targetViews.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            var view = targetViews.GetArrayElementAtIndex(i);
            view.FindPropertyRelative("_type").enumValueIndex = i;
            view.FindPropertyRelative("OnViewSeted.m_PersistentCalls.m_Calls").arraySize = 0;
            view.FindPropertyRelative("OnViewReseted.m_PersistentCalls.m_Calls").arraySize = 0;
        }

        foreach (var source in root.GetComponentsInChildren<ElementalView>(true))
        {
            var so = new SerializedObject(source);
            var views = so.FindProperty("_views");
            for (int v = 0; v < views.arraySize; v++)
            {
                var view = views.GetArrayElementAtIndex(v);
                int type = view.FindPropertyRelative("_type").enumValueIndex;
                if (type >= 3)
                    continue;

                foreach (var evt in new[] { "OnViewSeted", "OnViewReseted" })
                {
                    var calls = view.FindPropertyRelative(evt + ".m_PersistentCalls.m_Calls");
                    var into = targetViews.GetArrayElementAtIndex(type).FindPropertyRelative(evt + ".m_PersistentCalls.m_Calls");
                    for (int c = calls.arraySize - 1; c >= 0; c--)
                    {
                        var call = calls.GetArrayElementAtIndex(c);
                        if (!Targets(call.FindPropertyRelative("m_Target").objectReferenceValue, moved))
                            continue;

                        into.arraySize++;
                        CopyCall(call, into.GetArrayElementAtIndex(into.arraySize - 1));
                        calls.DeleteArrayElementAtIndex(c);
                    }
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        targetSo.ApplyModifiedPropertiesWithoutUndo();
    }

    private static bool Targets(Object target, HashSet<Transform> moved) => target switch
    {
        GameObject go => moved.Contains(go.transform),
        Component component => moved.Contains(component.transform),
        _ => false
    };

    private static void CopyCall(SerializedProperty from, SerializedProperty to)
    {
        to.FindPropertyRelative("m_Target").objectReferenceValue = from.FindPropertyRelative("m_Target").objectReferenceValue;
        foreach (var field in new[] { "m_TargetAssemblyTypeName", "m_MethodName", "m_Arguments.m_ObjectArgumentAssemblyTypeName", "m_Arguments.m_StringArgument" })
            to.FindPropertyRelative(field).stringValue = from.FindPropertyRelative(field).stringValue;
        foreach (var field in new[] { "m_Mode", "m_CallState", "m_Arguments.m_IntArgument" })
            to.FindPropertyRelative(field).intValue = from.FindPropertyRelative(field).intValue;
        to.FindPropertyRelative("m_Arguments.m_FloatArgument").floatValue = from.FindPropertyRelative("m_Arguments.m_FloatArgument").floatValue;
        to.FindPropertyRelative("m_Arguments.m_BoolArgument").boolValue = from.FindPropertyRelative("m_Arguments.m_BoolArgument").boolValue;
        to.FindPropertyRelative("m_Arguments.m_ObjectArgument").objectReferenceValue = from.FindPropertyRelative("m_Arguments.m_ObjectArgument").objectReferenceValue;
    }

    // ── Impact effects ──────────────────────────────────────────────────

    private static ImpactEffectsSettings EnsureImpactSettings()
    {
        EnsureFolder(Materials.TrimEnd('/'));
        EnsureFolder("Assets/Resources");

        var settings = AssetDatabase.LoadAssetAtPath<ImpactEffectsSettings>(SettingsPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<ImpactEffectsSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
        }

        settings.Dust = MaterialFrom(AlphaMaterial, "Impact Dust", "Assets/JMO Assets/Cartoon FX (legacy)/Textures (Mobile)/CFXM_T_Smoke_4Frames_AB.tga");
        settings.Chip = MaterialFrom(AlphaMaterial, "Impact Chip", "Assets/JMO Assets/Cartoon FX (legacy)/Textures (Mobile)/CFXM_T_WhiteCircleSmall.png");
        settings.Glow = MaterialFrom(GlintMaterial, "Impact Glow", null);
        settings.Ring = MaterialFrom(GlintMaterial, "Impact Ring", "Assets/Hovl Studio/Resources/Textures/Circle17.png");

        AudioClip[] Clips(params string[] paths) => paths.Select(AssetDatabase.LoadAssetAtPath<AudioClip>).Where(c => c != null).ToArray();
        var thud = Clips(NiceVibrations + "Body1.wav", NiceVibrations + "Body2.wav", NiceVibrations + "Body3.wav");

        settings.Presets = new[]
        {
            new ImpactEffectsSettings.Preset { Surface = ImpactSurface.Default,
                Dust = new Color(0.5f, 0.46f, 0.42f, 0.6f), DustCount = 3, DustSize = 0.3f,
                Chips = new Color(0.36f, 0.32f, 0.28f, 1f), ChipCount = 5, ChipSize = 0.04f, ChipSpeed = 3f,
                Flash = Clear, Ring = Clear, Sounds = thud, Volume = 0.4f, Pitch = new Vector2(0.85f, 1f) },
            new ImpactEffectsSettings.Preset { Surface = ImpactSurface.Dirt,
                Dust = new Color(0.47f, 0.4f, 0.3f, 0.75f), DustCount = 5, DustSize = 0.4f,
                Chips = new Color(0.3f, 0.24f, 0.17f, 1f), ChipCount = 9, ChipSize = 0.045f, ChipSpeed = 3.5f,
                Flash = Clear, Ring = Clear, Sounds = thud, Volume = 0.45f, Pitch = new Vector2(0.75f, 0.95f) },
            new ImpactEffectsSettings.Preset { Surface = ImpactSurface.Sand,
                Dust = new Color(0.86f, 0.73f, 0.52f, 0.75f), DustCount = 6, DustSize = 0.45f,
                Chips = new Color(0.78f, 0.64f, 0.42f, 1f), ChipCount = 12, ChipSize = 0.035f, ChipSpeed = 3.2f,
                Flash = Clear, Ring = Clear, Sounds = thud, Volume = 0.4f, Pitch = new Vector2(0.9f, 1.1f) },
            new ImpactEffectsSettings.Preset { Surface = ImpactSurface.Snow,
                Dust = new Color(0.95f, 0.97f, 1f, 0.85f), DustCount = 6, DustSize = 0.45f,
                Chips = new Color(0.97f, 0.98f, 1f, 1f), ChipCount = 10, ChipSize = 0.04f, ChipSpeed = 3f,
                Flash = Clear, Ring = Clear, Sounds = thud, Volume = 0.35f, Pitch = new Vector2(0.9f, 1.05f) },
            new ImpactEffectsSettings.Preset { Surface = ImpactSurface.Wood,
                Dust = new Color(0.55f, 0.45f, 0.32f, 0.5f), DustCount = 2, DustSize = 0.25f,
                Chips = new Color(0.62f, 0.47f, 0.3f, 1f), ChipCount = 7, ChipSize = 0.045f, ChipSpeed = 4.5f, Splinters = true,
                Flash = Clear, Ring = Clear,
                Sounds = Clips(NiceVibrations + "Wood1.wav", NiceVibrations + "Wood2.wav", NiceVibrations + "Wood3.wav"), Volume = 0.7f },
            new ImpactEffectsSettings.Preset { Surface = ImpactSurface.Stone,
                Dust = new Color(0.62f, 0.62f, 0.6f, 0.55f), DustCount = 3, DustSize = 0.3f,
                Chips = new Color(0.45f, 0.45f, 0.43f, 1f), ChipCount = 6, ChipSize = 0.04f, ChipSpeed = 4f,
                Sparks = new Color(1f, 0.82f, 0.5f, 1f), SparkCount = 5,
                Flash = Clear, Ring = Clear,
                Sounds = Clips(NiceVibrations + "Concrete1.wav", NiceVibrations + "Concrete2.wav", NiceVibrations + "Concrete3.wav"), Volume = 0.6f },
            new ImpactEffectsSettings.Preset { Surface = ImpactSurface.Metal,
                DustCount = 0, ChipCount = 0,
                Sparks = new Color(1f, 0.8f, 0.45f, 1f), SparkCount = 10,
                Flash = new Color(1f, 0.85f, 0.6f, 0.7f), FlashSize = 0.3f, Ring = Clear,
                Sounds = Clips(NiceVibrations + "ImpactMetal1.wav", NiceVibrations + "BulletMetal1.wav"), Volume = 0.6f },
            // Bodies: a bright punch of light, no blood.
            new ImpactEffectsSettings.Preset { Surface = ImpactSurface.Body,
                Dust = new Color(0.65f, 0.6f, 0.55f, 0.35f), DustCount = 2, DustSize = 0.25f,
                ChipCount = 0,
                Sparks = new Color(1f, 0.9f, 0.7f, 1f), SparkCount = 3,
                Flash = new Color(1f, 0.92f, 0.8f, 0.75f), FlashSize = 0.45f, Ring = Clear,
                Sounds = Clips(Sounds + "Body Impact 1.wav", Sounds + "Body Impact 2.wav"), Volume = 0.8f },
            new ImpactEffectsSettings.Preset { Surface = ImpactSurface.Head,
                DustCount = 0, ChipCount = 0,
                Sparks = new Color(1f, 0.85f, 0.5f, 1f), SparkCount = 8,
                Flash = new Color(1f, 0.95f, 0.75f, 0.95f), FlashSize = 0.7f,
                Ring = new Color(1f, 0.9f, 0.6f, 0.85f), RingSize = 1.6f,
                Sounds = Clips(Sounds + "Bone Crack.wav"), Volume = 0.9f, Pitch = new Vector2(1f, 1.15f) },
            new ImpactEffectsSettings.Preset { Surface = ImpactSurface.Release,
                Dust = new Color(1f, 1f, 1f, 0.22f), DustCount = 3, DustSize = 0.16f,
                ChipCount = 0, SparkCount = 0,
                Flash = new Color(1f, 1f, 1f, 0.18f), FlashSize = 0.18f, Ring = Clear },
        };

        EditorUtility.SetDirty(settings);
        return settings;
    }

    private static readonly Color Clear = new Color(1f, 1f, 1f, 0f);

    private static Material MaterialFrom(string template, string name, string texture)
    {
        string path = Materials + name + ".mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(path) == null)
            AssetDatabase.CopyAsset(template, path);

        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (texture != null)
        {
            var map = AssetDatabase.LoadAssetAtPath<Texture2D>(texture);
            material.SetTexture("_BaseMap", map);
            material.mainTexture = map;
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    // ── Helpers ─────────────────────────────────────────────────────────

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
            return;

        int slash = folder.LastIndexOf('/');
        EnsureFolder(folder.Substring(0, slash));
        AssetDatabase.CreateFolder(folder.Substring(0, slash), folder.Substring(slash + 1));
    }

    private static void Edit(string path, System.Action<GameObject> edit)
    {
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            edit(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static T GetOrAdd<T>(Transform parent, string name) where T : Component
    {
        Transform child = parent.Cast<Transform>().FirstOrDefault(t => t.name == name);
        if (child == null)
        {
            child = new GameObject(name).transform;
            child.SetParent(parent, false);
        }

        var component = child.GetComponent<T>();
        return component != null ? component : child.gameObject.AddComponent<T>();
    }

    private static void Set(Object target, string field, object value)
    {
        var so = new SerializedObject(target);
        var property = so.FindProperty(field);
        switch (value)
        {
            case float f: property.floatValue = f; break;
            case bool b: property.boolValue = b; break;
            case Color c: property.colorValue = c; break;
            case Vector2 v: property.vector2Value = v; break;
            case Object o: property.objectReferenceValue = o; break;
            case string s: property.stringValue = s; break;
            case null: property.objectReferenceValue = null; break;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
