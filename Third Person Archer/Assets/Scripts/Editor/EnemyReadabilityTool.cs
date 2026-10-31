using System.Linq;
using Actor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sets up the enemy prefabs for readable combat:
/// a wind-up warning (glint + cue + off-screen flash) on every attack, a short extra wind-up for attacks that were too quick to read,
/// a procedural flinch on every humanoid, and for enemies without a hit state a stagger that cuts the attack short when they are hit.
/// Per type it also sets sight, reaction time, whether the enemy waits its turn with the AttackDirector, and melee reach;
/// and it adds the group reaction (startle, ally deaths).
/// Safe to run again: existing objects are reused and re-tuned.
/// </summary>
public static class EnemyReadabilityTool
{
    private const string Dir = "Assets/Prefabs/Actors/";
    private const string GlintMaterial = "Assets/Materials/Enemy Attack Glint.mat";
    private const string BowCue = "Assets/Audio/Sounds/Bow Pull.wav";
    private const string GunCue = "Assets/Plugins/Feel/MMFeedbacks/Demos/SequencingDemo/Sounds/Kit/MMSequencingTick.wav";

    private const string TelegraphName = "Attack Telegraph [Substate]";
    private const string StaggerName = "Stagger";
    private const string OnHitName = "On Hit Stagger [Transition]";
    private const float StaggerTime = 0.6f;

    private struct Setup
    {
        public string Prefab;
        public float Windup;
        public float Ramp;
        public string Cue;
        public float CuePitch;
        public bool Repeats;
        // Animator state the stagger blends into; null = the enemy already has its own hit state.
        public string StaggerState;
        public string[] AttackTriggers;
        public float SightDistance;
        public float SightAngle;
        public Vector2 Reaction;
        // False: attacks on its own clock (the rifle aims for seconds and keeps firing).
        public bool NoDirector;
        public float MeleeReach;
    }

    private static readonly Vector2 SlowReaction = new Vector2(0.6f, 1f);
    private static readonly Vector2 QuickReaction = new Vector2(0.2f, 0.4f);
    private static readonly Vector2 MeleeReaction = new Vector2(0.4f, 0.7f);

    private static readonly Setup[] Setups =
    {
        // Archers see far down a narrow cone and are slow to react.
        new Setup { Prefab = "Enemy Archer Variant", Ramp = 1.05f, Cue = BowCue, CuePitch = 1f, StaggerState = "Idle", AttackTriggers = new[] { "Shoot" },
            SightDistance = 20f, SightAngle = 50f, Reaction = SlowReaction },
        new Setup { Prefab = "Enemy Viking Archer Variant", Ramp = 0.95f, Cue = BowCue, CuePitch = 0.9f, StaggerState = "Movement", AttackTriggers = new[] { "Attack" },
            SightDistance = 15f, SightAngle = 50f, Reaction = SlowReaction },
        // Ninjas see close but all around and react at once.
        new Setup { Prefab = "Enemy Ninja Variant", Windup = 0.35f, Ramp = 0.8f, CuePitch = 1f, StaggerState = "Movement", AttackTriggers = new[] { "Attack" },
            SightDistance = 12f, SightAngle = 110f, Reaction = QuickReaction },
        new Setup { Prefab = "Enemy Berserker Variant", Ramp = 1.3f, CuePitch = 1f, StaggerState = "Movement", AttackTriggers = new[] { "Attack" },
            SightDistance = 12f, SightAngle = 90f, Reaction = new Vector2(0.3f, 0.5f) },
        new Setup { Prefab = "Enemy Jap Firework", Ramp = 1.6f, CuePitch = 1f, StaggerState = "Movement", AttackTriggers = new[] { "Attack" },
            SightDistance = 15f, SightAngle = 60f, Reaction = new Vector2(0.7f, 1.1f) },
        new Setup { Prefab = "Enemy Cowboy Revolver 1 Variant", Ramp = 0.95f, Cue = GunCue, CuePitch = 0.8f, StaggerState = "Movement", AttackTriggers = new[] { "Attack" },
            SightDistance = 14f, SightAngle = 80f, Reaction = new Vector2(0.3f, 0.5f) },
        new Setup { Prefab = "Enemy Cowboy Revolver 2 Variant", Windup = 0.25f, Ramp = 0.85f, Cue = GunCue, CuePitch = 0.9f, StaggerState = "Movement", AttackTriggers = new[] { "Attack" },
            SightDistance = 14f, SightAngle = 80f, Reaction = new Vector2(0.3f, 0.5f) },
        // Crouches and aims for seconds, then fires again and again: the glint builds over the whole aim. Stays crouched when hit.
        new Setup { Prefab = "Enemy Cowboy Rifle Variant", Ramp = 5.5f, Repeats = true, Cue = GunCue, CuePitch = 0.6f, StaggerState = "Idle Crouching", AttackTriggers = new string[0],
            SightDistance = 22f, SightAngle = 40f, Reaction = new Vector2(0.5f, 0.8f), NoDirector = true },
        new Setup { Prefab = "Enemy Samurai Variant", Windup = 0.3f, Ramp = 0.65f,
            SightDistance = 12f, SightAngle = 100f, Reaction = MeleeReaction, MeleeReach = 4.5f },
        new Setup { Prefab = "Enemy Samurai (Runner) Variant", Windup = 0.3f, Ramp = 0.65f,
            SightDistance = 12f, SightAngle = 100f, Reaction = MeleeReaction, MeleeReach = 4.5f },
        new Setup { Prefab = "Enemy Warrior Variant", Windup = 0.3f, Ramp = 0.65f,
            SightDistance = 12f, SightAngle = 100f, Reaction = MeleeReaction, MeleeReach = 4.5f },
    };

    [MenuItem("Tools/Enemies/Apply Combat Readability")]
    public static void ApplyAll()
    {
        EnsureDirectorSettings();

        var material = AssetDatabase.LoadAssetAtPath<Material>(GlintMaterial);

        foreach (var setup in Setups)
        {
            string path = Dir + setup.Prefab + ".prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Apply(root, setup, material);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Combat readability applied to {Setups.Length} enemy prefabs.");
    }

    private static void Apply(GameObject root, Setup setup, Material material)
    {
        Transform body = root.transform.Find("State Mashine");
        var attack = body.GetComponentInChildren<AnimationAttackState>(true);

        Set(attack, "_windup", setup.Windup);

        var telegraph = GetOrAdd<AttackTelegraphSubstate>(attack.transform, TelegraphName);
        Set(telegraph, "_attack", attack);
        Set(telegraph, "_glintMaterial", material);
        Set(telegraph, "_sizePerMetre", 0.3f);
        Set(telegraph, "_minSize", 1.5f);
        Set(telegraph, "_towardsCamera", 1f);
        Set(telegraph, "_startColor", new Color(1f, 0.95f, 0.85f, 0.55f));
        Set(telegraph, "_endColor", new Color(1f, 0.8f, 0.55f, 1f));
        Set(telegraph, "_rampTime", setup.Ramp);
        Set(telegraph, "_cue", setup.Cue != null ? AssetDatabase.LoadAssetAtPath<AudioClip>(setup.Cue) : null);
        Set(telegraph, "_cuePitch", setup.CuePitch);
        Set(telegraph, "_repeatAfterShot", setup.Repeats);

        foreach (var ragdoll in root.GetComponentsInChildren<RagdollControll>(true))
            if (ragdoll.GetComponent<HitFlinch>() == null)
                ragdoll.gameObject.AddComponent<HitFlinch>();

        if (setup.StaggerState != null)
            AddStagger(body, attack, setup);

        var perception = root.GetComponentInChildren<PerceptionInput>(true);
        Set(perception, "_fieldOfView._maxDistance", setup.SightDistance);
        Set(perception, "_fieldOfView._viewAngle", setup.SightAngle);
        Set(perception, "_reactionTime", setup.Reaction);

        foreach (var aiAttack in root.GetComponentsInChildren<EnemyAttackState>(true))
            Set(aiAttack, "_useDirector", !setup.NoDirector);

        foreach (var damager in root.GetComponentsInChildren<TargetDamager>(true))
            Set(damager, "_maxDistance", setup.MeleeReach);

        if (root.GetComponent<EnemyGroupReaction>() == null)
            root.AddComponent<EnemyGroupReaction>();
    }

    private static void EnsureDirectorSettings()
    {
        string path = "Assets/Resources/" + AttackDirectorSettings.ResourceName + ".asset";
        if (AssetDatabase.LoadAssetAtPath<AttackDirectorSettings>(path) == null)
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AttackDirectorSettings>(), path);
    }

    private static void AddStagger(Transform body, AnimationAttackState attack, Setup setup)
    {
        MainState idle = body.Find("Idle").GetComponent<MainState>();
        MainState death = body.Find("Dealth").GetComponent<MainState>();

        var stagger = GetOrAdd<MainState>(body, StaggerName);

        var crossFade = GetOrAdd<AnimationCrossFadeSubstate>(stagger.transform, "Cancel Attack Animation [Substate]");
        Set(crossFade, "_stateName", setup.StaggerState);
        Set(crossFade, "_duration", 0.15f);
        SetStrings(crossFade, "_resetTriggers", setup.AttackTriggers);

        var delay = GetOrAdd<DelayTransition>(stagger.transform, "Delay [Transition]");
        Set(delay, "_nextState", idle);
        Set(delay, "_delay", StaggerTime);

        var onDeath = GetOrAdd<OnDeathTransition>(stagger.transform, "OnDeath [Transition]");
        Set(onDeath, "_nextState", death);

        // Subscribes to Health in OnEnable, which must wait for the state to enter.
        var onHit = GetOrAdd<OnHitTransition>(attack.transform, OnHitName);
        Set(onHit, "_nextState", stagger);
        onHit.enabled = false;
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

    private static void SetStrings(Object target, string field, string[] values)
    {
        var so = new SerializedObject(target);
        var property = so.FindProperty(field);
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            property.GetArrayElementAtIndex(i).stringValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
