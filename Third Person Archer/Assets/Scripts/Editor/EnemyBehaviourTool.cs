using System.Linq;
using Actor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Livelier enemy behaviour from animations already in the project, without changing what each enemy does.
/// Melee (samurai, runner, warrior): sprint for the last metres, three different two-blow attacks,
/// staggering back when hit, sidesteps and taunts between attacks, locomotion matched to the real speed.
/// Every enemy: something to do before it notices the player and a few ways to react (EnemyBehaviourTool.Entrance.cs).
/// Safe to run again: existing objects, states and events are reused and re-tuned.
/// </summary>
public static partial class EnemyBehaviourTool
{
    private const string Prefabs = "Assets/Prefabs/Actors/";
    private const string Controllers = "Assets/Animations/!Animator Controllers/";
    private const string SwordClips = "Assets/Animations/Sword_and_Shield_Anims/Art/Animations/SwordAndShield_";

    private const string Locomotion = "Movement 0";
    private const string AttackEvent = "Attack";
    private const string EventFunction = "InvokeAnimationEvent";

    // Measured from the clips (hips travel per second), at state speed 1.
    private const float WalkSpeed = 1.14f;
    private const float RunSpeed = 1.97f;
    private const float SprintSpeed = 4.2f;
    private const float DashSpeed = 4.94f;

    // Seconds into the clip where the blade is fastest (sampled on the samurai rig).
    private static readonly (string Clip, float Time)[] NewBlows =
    {
        ("Attack_InPlace_Slash_Diagonal", 0.42f),
        ("Attack_InPlace_Slash_Horizontal", 0.48f),
        ("Attack_InPlace_Slash_Vertical", 0.42f),
        ("Attack_InPlace_Stab", 0.33f),
    };

    // Each attack is two blows, like the original slash pair, so the damage dealt stays the same.
    private static readonly (string First, string Second)[] Combos =
    {
        ("Attack_Forward_Slash01", "Attack_Forward_Slash02"),
        ("Attack_InPlace_Slash_Diagonal", "Attack_InPlace_Slash_Horizontal"),
        ("Attack_InPlace_Slash_Vertical", "Attack_InPlace_Stab"),
    };

    private static readonly string[] Hits = { "Stagger_Back01", "Stagger_Back02" };
    private static readonly string[] Taunts = { "Idle_Taunt_ShieldTaps", "Idle_Taunt_ArmsWide", "Idle_Taunt_Frustrated" };

    private struct Melee
    {
        public string Prefab;
        public float ChargeDistance;
        public float ChargeSpeed;
        public float TopSpeed;
    }

    private static readonly Melee[] MeleeSetups =
    {
        new Melee { Prefab = "Enemy Samurai Variant", ChargeDistance = 7f, ChargeSpeed = SprintSpeed, TopSpeed = SprintSpeed },
        new Melee { Prefab = "Enemy Warrior Variant", ChargeDistance = 6f, ChargeSpeed = 3.6f, TopSpeed = SprintSpeed },
        new Melee { Prefab = "Enemy Samurai (Runner) Variant", ChargeDistance = 8f, ChargeSpeed = DashSpeed, TopSpeed = DashSpeed },
    };

    [MenuItem("Tools/Enemies/Apply Behaviour")]
    public static void ApplyAll()
    {
        AddBlowEvents();

        SetupMeleeController(Controllers + "Samurai.controller", Sword("Run_Sprint_Forward"));
        // The runner keeps its charge-dash clip as the top speed.
        SetupMeleeController(Controllers + "Samurai Runner.controller", null);

        foreach (var setup in MeleeSetups)
            EditPrefab(setup.Prefab, root => SetupMeleePrefab(root, setup));

        ApplyEntrance();

        AssetDatabase.SaveAssets();
        Debug.Log("Enemy behaviour applied.");
    }

    #region Clips

    private static void AddBlowEvents()
    {
        foreach (var (clip, time) in NewBlows)
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(SwordClips + clip + ".fbx");
            var clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;

            var events = clips[0].events.Where(e => e.stringParameter != AttackEvent).ToList();
            float length = Sword(clip).length;
            events.Add(new UnityEngine.AnimationEvent { functionName = EventFunction, stringParameter = AttackEvent, time = time / length });
            clips[0].events = events.ToArray();

            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }
    }

    private static AnimationClip Sword(string name) => Load(SwordClips + name, "SwordAndShield_" + name);

    private static AnimationClip Load(string pathWithoutExtension, string clipName)
    {
        return AssetDatabase.LoadAllAssetsAtPath(pathWithoutExtension + ".fbx")
            .OfType<AnimationClip>()
            .First(c => c.name == clipName);
    }

    #endregion

    #region Melee controller

    private static void SetupMeleeController(string path, Motion topClip)
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        var machine = controller.layers[0].stateMachine;

        AddParameter(controller, "AttackType", AnimatorControllerParameterType.Int);
        AddParameter(controller, "HitType", AnimatorControllerParameterType.Int);
        AddParameter(controller, "Taunt", AnimatorControllerParameterType.Trigger);
        AddParameter(controller, "TauntType", AnimatorControllerParameterType.Int);

        AnimatorState locomotion = FindState(machine, Locomotion);
        BuildLocomotion(controller, locomotion, topClip);

        // Attacks: AttackType picks the pair.
        for (int i = 0; i < Combos.Length; i++)
        {
            AnimatorState first = GetOrAddState(machine, Sword(Combos[i].First), new Vector3(320f + i * 260f, 300f));
            AnimatorState second = GetOrAddState(machine, Sword(Combos[i].Second), new Vector3(320f + i * 260f, 390f));
            first.speed = second.speed = 1.25f;

            var toFirst = GetOrAddTransition(locomotion, first);
            SetTiming(toFirst, false, 0.81f, 0.25f);
            SetConditions(toFirst, ("Attack", AnimatorConditionMode.If, 0f), ("AttackType", AnimatorConditionMode.Equals, i));

            var toSecond = GetOrAddTransition(first, second);
            SetTiming(toSecond, true, 0.85f, 0.25f);
            SetConditions(toSecond);

            var back = GetOrAddTransition(second, locomotion);
            SetTiming(back, true, 0.85f, 0.25f);
            SetConditions(back);
        }

        // Hit: stagger back instead of the shield block (the block state stays, unused).
        foreach (var old in machine.anyStateTransitions.Where(t => t.destinationState != null &&
                     t.destinationState.motion == Sword("Block_HitReaction02")).ToArray())
            machine.RemoveAnyStateTransition(old);

        for (int i = 0; i < Hits.Length; i++)
        {
            AnimatorState hit = GetOrAddState(machine, Sword(Hits[i]), new Vector3(150f + i * 200f, -60f));
            hit.speed = 1.1f;

            var fromAny = machine.anyStateTransitions.FirstOrDefault(t => t.destinationState == hit) ?? machine.AddAnyStateTransition(hit);
            SetTiming(fromAny, false, 0.75f, 0.12f);
            fromAny.canTransitionToSelf = true;
            SetConditions(fromAny, ("Hit", AnimatorConditionMode.If, 0f), ("HitType", AnimatorConditionMode.Equals, i));

            var back = GetOrAddTransition(hit, locomotion);
            SetTiming(back, true, 0.8f, 0.3f);
            SetConditions(back);
        }

        // Taunts between attacks.
        for (int i = 0; i < Taunts.Length; i++)
        {
            AnimatorState taunt = GetOrAddState(machine, Sword(Taunts[i]), new Vector3(630f, 260f + i * 80f));
            taunt.speed = 1.1f;

            var to = GetOrAddTransition(locomotion, taunt);
            SetTiming(to, false, 0f, 0.25f);
            SetConditions(to, ("Taunt", AnimatorConditionMode.If, 0f), ("TauntType", AnimatorConditionMode.Equals, i));

            var back = GetOrAddTransition(taunt, locomotion);
            SetTiming(back, true, 0.88f, 0.3f);
            SetConditions(back);
        }

        EditorUtility.SetDirty(controller);
    }

    /// <summary>
    /// Idle, walk, run and a top speed (sprint / dash), each directional, on evenly spaced Speed thresholds.
    /// A null top clip keeps the forward clip of the old tree's fastest blend.
    /// </summary>
    private static void BuildLocomotion(AnimatorController controller, AnimatorState state, Motion topClip)
    {
        var oldTree = state.motion as BlendTree;
        Motion idle = oldTree.children[0].motion;

        if (topClip == null)
        {
            var fastest = (BlendTree)oldTree.children[oldTree.children.Length - 1].motion;
            topClip = fastest.children.First(c => c.position == new Vector2(0f, 1f)).motion;
        }

        var tree = NewTree(controller, "Melee Locomotion", BlendTreeType.Simple1D, "Speed");
        tree.useAutomaticThresholds = false;
        tree.AddChild(idle, 0f);
        tree.AddChild(Directional(controller, "Walk", Sword("Walk_Forward"), Sword("Walk_Back"), Sword("Walk_Left"), Sword("Walk_Right")), 1f / 3f);
        tree.AddChild(Directional(controller, "Run", Sword("Run_Forward"), Sword("Run_Back"), Sword("Run_Left"), Sword("Run_Right")), 2f / 3f);
        tree.AddChild(Directional(controller, "Top", topClip, Sword("Run_Back"), Sword("Run_Left"), Sword("Run_Right")), 1f);

        state.motion = tree;
        // The clips are matched to the real speed, so they play at their own rate.
        state.speed = 1f;

        if (AssetDatabase.GetAssetPath(oldTree) == AssetDatabase.GetAssetPath(controller))
            RemoveTree(oldTree);
    }

    private static BlendTree Directional(AnimatorController controller, string name, Motion forward, Motion back, Motion left, Motion right)
    {
        var tree = NewTree(controller, name, BlendTreeType.SimpleDirectional2D, "XDir");
        tree.blendParameterY = "ZDir";
        tree.AddChild(forward, new Vector2(0f, 1f));
        tree.AddChild(back, new Vector2(0f, -1f));
        tree.AddChild(left, new Vector2(-1f, 0f));
        tree.AddChild(right, new Vector2(1f, 0f));
        return tree;
    }

    private static BlendTree NewTree(AnimatorController controller, string name, BlendTreeType type, string parameter)
    {
        var tree = new BlendTree { name = name, blendType = type, blendParameter = parameter, hideFlags = HideFlags.HideInHierarchy };
        AssetDatabase.AddObjectToAsset(tree, controller);
        return tree;
    }

    private static void RemoveTree(BlendTree tree)
    {
        foreach (var child in tree.children)
            if (child.motion is BlendTree inner)
                RemoveTree(inner);

        AssetDatabase.RemoveObjectFromAsset(tree);
        Object.DestroyImmediate(tree, true);
    }

    private static void AddParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
    {
        if (controller.parameters.All(p => p.name != name))
            controller.AddParameter(name, type);
    }

    private static AnimatorState FindState(AnimatorStateMachine machine, string name)
    {
        return machine.states.Select(s => s.state).First(s => s.name == name);
    }

    private static AnimatorState GetOrAddState(AnimatorStateMachine machine, AnimationClip clip, Vector3 position)
    {
        var state = machine.states.Select(s => s.state).FirstOrDefault(s => s.motion == clip);
        if (state != null)
            return state;

        state = machine.AddState(clip.name, position);
        state.motion = clip;
        return state;
    }

    private static AnimatorStateTransition GetOrAddTransition(AnimatorState from, AnimatorState to)
    {
        return from.transitions.FirstOrDefault(t => t.destinationState == to) ?? from.AddTransition(to);
    }

    private static void SetTiming(AnimatorStateTransition transition, bool hasExitTime, float exitTime, float duration)
    {
        transition.hasExitTime = hasExitTime;
        transition.exitTime = exitTime;
        transition.hasFixedDuration = true;
        transition.duration = duration;
        transition.offset = 0f;
    }

    private static void SetConditions(AnimatorStateTransition transition, params (string Parameter, AnimatorConditionMode Mode, float Threshold)[] conditions)
    {
        foreach (var condition in transition.conditions)
            transition.RemoveCondition(condition);

        foreach (var (parameter, mode, threshold) in conditions)
            transition.AddCondition(mode, threshold, parameter);
    }

    #endregion

    #region Melee prefab

    private static void SetupMeleePrefab(GameObject root, Melee setup)
    {
        Transform body = root.transform.Find("State Mashine");
        Transform ai = root.transform.Find("AI");

        var clipSpeeds = new[] { 0f, WalkSpeed, RunSpeed, setup.TopSpeed };
        foreach (var move in root.GetComponentsInChildren<MoveAnimationSubstate>(true))
            SetArray(move, "_clipSpeeds", clipSpeeds);

        var attack = body.GetComponentInChildren<AnimationMeleeState>(true);
        var attackType = GetOrAdd<AnimationRandomizerSubstate>(attack.transform, "Attack Type Randomizer [Substate]");
        Set(attackType, "_animationParamName", "AttackType");
        Set(attackType, "_minValue", 0);
        Set(attackType, "_maxValue", Combos.Length);

        var hit = body.Find("Hit");
        var hitType = GetOrAdd<AnimationRandomizerSubstate>(hit, "Hit Type Randomizer [Substate]");
        Set(hitType, "_animationParamName", "HitType");
        Set(hitType, "_minValue", 0);
        Set(hitType, "_maxValue", Hits.Length);

        var moveToTarget = ai.GetComponentInChildren<MoveToTargetState>(true);
        var charge = GetOrAdd<MeleeChargeSubstate>(moveToTarget.transform, "Charge [Substate]");
        Set(charge, "_chargeDistance", setup.ChargeDistance);
        Set(charge, "_chargeSpeed", setup.ChargeSpeed);

        Transform wait = ai.GetComponentsInChildren<MainState>(true).First(s => s.name == "Wait Between Attacks").transform;
        var waitBehaviour = GetOrAdd<MeleeWaitSubstate>(wait, "Sidestep Or Taunt [Substate]");
        Set(waitBehaviour, "_tauntCount", Taunts.Length);
        Set(waitBehaviour, "_sidestepSpeed", WalkSpeed);
        Set(waitBehaviour, "_startDelay", 0.8f);

        // Patrols walk instead of jogging.
        foreach (var patrol in ai.GetComponentsInChildren<PatrolState>(true))
            foreach (var speed in patrol.GetComponentsInChildren<SpeedModifierSubState>(true))
                Set(speed, "_newSpeed", 1.2f);
    }

    #endregion

    #region Helpers

    private static void EditPrefab(string name, System.Action<GameObject> edit)
    {
        string path = Prefabs + name + ".prefab";
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
            case int i: property.intValue = i; break;
            case bool b: property.boolValue = b; break;
            case Vector2 v: property.vector2Value = v; break;
            case string s: property.stringValue = s; break;
            case Object o: property.objectReferenceValue = o; break;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetArray(Object target, string field, float[] values)
    {
        var so = new SerializedObject(target);
        var property = so.FindProperty(field);
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            property.GetArrayElementAtIndex(i).floatValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    #endregion
}
