using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Entering combat: what each enemy does before it notices the player (IdleActivitySubstate on the AI's Stand)
/// and how it reacts when it does (AlertReactionSubstate on the AI's Alert).
/// Activity states loop until code blends out of them; reaction states play once, fitted into the type's alert time,
/// and go back to the idle.
/// </summary>
public static partial class EnemyBehaviourTool
{
    private const string Anims = "Assets/Animations/";

    private static readonly string LookAround = SwordClips + "Idle_LookingAround.fbx";
    private static readonly string BowLookAround = Anims + "Pro Longbow Pack/standing idle 02 looking.fbx";
    private static readonly string BowExamine = Anims + "Pro Longbow Pack/standing idle 03 examine.fbx";
    private static readonly string ArmsWide = SwordClips + "Idle_Taunt_ArmsWide.fbx";
    private static readonly string Signal = Anims + "Viking Archer/Erika Archer@Military Signaling.fbx";
    private static readonly string Yell = Anims + "Viking Archer/Erika Archer@Yelling Out.fbx";
    private static readonly string Cocky = Anims + "Viking Archer/Erika Archer@Being Cocky.fbx";
    private static readonly string Pray = Anims + "NPC/Y Bot@Praying.fbx";
    private static readonly string NinjaIdle = Anims + "Ninja_AnimSet/Animation/Humanoid/idle.fbx";
    private static readonly string Backflip = Anims + "Ninja/Erika Archer@Backflip.fbx";
    private static readonly string Gunplay = Anims + "Western/Erika Archer@Gunplay.fbx";
    private static readonly string[] Talks =
    {
        Anims + "NPC/Y Bot@Talking.fbx", Anims + "NPC/Y Bot@Talking (1).fbx", Anims + "NPC/Y Bot@Talking (2).fbx",
    };

    private const string ArcherReady = "standing equip bow";
    // The signal is long: only its first part (the arm sweep) plays.
    private const float SignalPart = 0.6f;

    private struct Act
    {
        public string State;
        public string Clip;
        public float Weight;
        public bool Company;
        public string Alert;
    }

    private struct React
    {
        public string State;
        public string Clip;
        public float Weight;
        // Share of the clip that plays; long shouts are cut after their peak.
        public float Part;
    }

    private struct Entrance
    {
        public string Controller;
        public string[] Prefabs;
        public string Locomotion;
        // Seconds the AI spends in its alert before it moves or attacks: the reaction must be done by then.
        public float AlertTime;
        public Act[] Activities;
        public React[] Reactions;
        public bool Signals;
    }

    private static Act Idle(float weight) => new Act { Weight = weight };
    private static Act Do(string state, string clip, float weight, bool company = false, string alert = null) =>
        new Act { State = state, Clip = clip, Weight = weight, Company = company, Alert = alert };
    private static React Usual(float weight) => new React { Weight = weight };
    private static React Play(string state, string clip, float weight, float part = 0.85f) =>
        new React { State = state, Clip = clip, Weight = weight, Part = part };

    private static readonly Act[] Chat =
    {
        Idle(0.55f),
        Do("Activity Talk 1", Talks[0], 0.15f, true), Do("Activity Talk 2", Talks[1], 0.15f, true), Do("Activity Talk 3", Talks[2], 0.15f, true),
    };

    private static readonly Entrance[] Entrances =
    {
        new Entrance
        {
            Controller = "Samurai", Prefabs = new[] { "Enemy Samurai Variant", "Enemy Warrior Variant" },
            Locomotion = "Movement 0", AlertTime = 2f, Signals = true,
            Activities = new[] { Idle(0.45f), Do("Activity Look Around", LookAround, 0.55f) },
            Reactions = new[] { Usual(0.6f), Play("Reaction Arms Wide", ArmsWide, 0.4f) },
        },
        new Entrance
        {
            Controller = "Samurai Runner", Prefabs = new[] { "Enemy Samurai (Runner) Variant" },
            Locomotion = "Movement 0", AlertTime = 2f, Signals = true,
            Activities = new[] { Idle(0.45f), Do("Activity Look Around", LookAround, 0.55f) },
            Reactions = new[] { Usual(0.6f), Play("Reaction Arms Wide", ArmsWide, 0.4f) },
        },
        // Sitting archers keep their backflip; standing ones just ready the bow.
        new Entrance
        {
            Controller = "Archer", Prefabs = new[] { "Enemy Archer Variant" },
            Locomotion = "Standing Idle 01", AlertTime = 3f,
            Activities = new[]
            {
                Idle(0.4f),
                Do("Activity Look Around", BowLookAround, 0.3f, alert: ArcherReady),
                Do("Activity Examine", BowExamine, 0.3f, alert: ArcherReady),
            },
            Reactions = new React[0],
        },
        // Some scenes give archers this copy of their controller: it needs the same states (the prefab is set up above).
        new Entrance
        {
            Controller = "Archer Baloon", Prefabs = new string[0],
            Locomotion = "Standing Idle 01", AlertTime = 3f,
            Activities = new[]
            {
                Idle(0.4f),
                Do("Activity Look Around", BowLookAround, 0.3f, alert: ArcherReady),
                Do("Activity Examine", BowExamine, 0.3f, alert: ArcherReady),
            },
            Reactions = new React[0],
        },
        new Entrance
        {
            Controller = "Archer Viking", Prefabs = new[] { "Enemy Viking Archer Variant" },
            Locomotion = "Movement", AlertTime = 2f, Signals = true,
            Activities = new[] { Idle(0.4f), Do("Activity Look Around", BowLookAround, 0.3f), Do("Activity Examine", BowExamine, 0.3f) },
            Reactions = new[] { Usual(0.5f), Play("Reaction Yell", Yell, 0.25f, 0.65f), Play("Reaction Cocky", Cocky, 0.25f, 0.7f) },
        },
        new Entrance
        {
            Controller = "Ninja", Prefabs = new[] { "Enemy Ninja Variant" },
            Locomotion = "Movement", AlertTime = 1f,
            Activities = new[] { Idle(0.4f), Do("Activity Pray", Pray, 0.3f), Do("Activity Ninja Idle", NinjaIdle, 0.3f) },
            Reactions = new[] { Usual(0.6f), Play("Reaction Backflip", Backflip, 0.4f) },
        },
        new Entrance
        {
            Controller = "Berserker", Prefabs = new[] { "Enemy Berserker Variant" },
            Locomotion = "Movement", AlertTime = 1.5f, Signals = true,
            Activities = new[] { Idle(0.5f), Do("Activity Look Around", LookAround, 0.5f) },
            Reactions = new[] { Usual(0.6f), Play("Reaction Yell", Yell, 0.4f, 0.65f) },
        },
        new Entrance
        {
            Controller = "Western Pistol 1", Prefabs = new[] { "Enemy Cowboy Revolver 1 Variant" },
            Locomotion = "Movement", AlertTime = 1f, Activities = Chat,
            Reactions = new[] { Usual(0.65f), Play("Reaction Gunplay", Gunplay, 0.35f) },
        },
        new Entrance
        {
            Controller = "Western Pistol 2", Prefabs = new[] { "Enemy Cowboy Revolver 2 Variant" },
            Locomotion = "Movement", AlertTime = 1f, Activities = Chat, Reactions = new React[0],
        },
        new Entrance
        {
            Controller = "Western Rifle", Prefabs = new[] { "Enemy Cowboy Rifle Variant" },
            Locomotion = "Movement", AlertTime = 1f, Activities = Chat, Reactions = new React[0],
        },
        new Entrance
        {
            Controller = "Fireworks", Prefabs = new[] { "Enemy Jap Firework" },
            Locomotion = "Movement", AlertTime = 2f,
            Activities = new[] { Idle(0.5f), Do("Activity Pray", Pray, 0.5f) },
            Reactions = new[] { Usual(0.6f), Play("Reaction Yell", Yell, 0.4f, 0.65f) },
        },
    };

    private static void ApplyEntrance()
    {
        foreach (var entrance in Entrances)
        {
            SetupEntranceController(entrance);

            foreach (var prefab in entrance.Prefabs)
                EditPrefab(prefab, root => SetupEntrancePrefab(root, entrance));
        }
    }

    private static void SetupEntranceController(Entrance entrance)
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Controllers + entrance.Controller + ".controller");
        var machine = controller.layers[0].stateMachine;
        AnimatorState locomotion = FindState(machine, entrance.Locomotion);

        float y = -200f;
        foreach (var activity in entrance.Activities.Where(a => a.State != null))
            AddActivityState(machine, activity.State, LoadClip(activity.Clip), new Vector3(900f, y += 70f));

        foreach (var reaction in entrance.Reactions.Where(r => r.State != null))
            AddReactionState(machine, reaction.State, LoadClip(reaction.Clip), locomotion, entrance.AlertTime, reaction.Part, new Vector3(1150f, y += 70f));

        if (entrance.Signals)
            AddReactionState(machine, "Reaction Signal", LoadClip(Signal), locomotion, entrance.AlertTime, SignalPart, new Vector3(1150f, y += 70f));

        EditorUtility.SetDirty(controller);
    }

    /// <summary>Loops (a clip that doesn't loop by itself restarts through a self transition); left only from code.</summary>
    private static void AddActivityState(AnimatorStateMachine machine, string name, AnimationClip clip, Vector3 position)
    {
        AnimatorState state = GetOrAddNamedState(machine, name, clip, position);
        state.speed = 1f;

        if (clip.isLooping)
            return;

        var loop = GetOrAddTransition(state, state);
        SetTiming(loop, true, 0.95f, 0.3f);
        SetConditions(loop);
    }

    /// <summary>Plays once (up to <paramref name="part"/> of the clip) within the alert time, then back to the idle.</summary>
    private static void AddReactionState(AnimatorStateMachine machine, string name, AnimationClip clip, AnimatorState locomotion,
        float alertTime, float part, Vector3 position)
    {
        AnimatorState state = GetOrAddNamedState(machine, name, clip, position);
        state.speed = Mathf.Max(1f, clip.length * part / (alertTime * 0.9f));

        var back = GetOrAddTransition(state, locomotion);
        SetTiming(back, true, part, 0.25f);
        SetConditions(back);
    }

    private static AnimatorState GetOrAddNamedState(AnimatorStateMachine machine, string name, Motion clip, Vector3 position)
    {
        var state = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == name) ?? machine.AddState(name, position);
        state.motion = clip;
        return state;
    }

    private static AnimationClip LoadClip(string path)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().First(c => !c.name.StartsWith("__preview"));
    }

    /// <summary>
    /// A patrolling enemy is walking when it notices the player, and the walk state had no (or an empty) alert transition:
    /// it skipped its alert animation and "!" icon. Every alert transition of the body now leads to its Alert state.
    /// </summary>
    private static void FixMovingAlert(Transform body)
    {
        MainState alertState = body.Find("Alert").GetComponent<MainState>();
        var move = body.GetComponentsInChildren<MoveToDestinationState>(true).FirstOrDefault();

        if (move != null)
        {
            var onAlert = move.GetComponentsInChildren<OnActorActionTransition>(true).FirstOrDefault(t => IsAlertTransition(t));
            if (onAlert == null)
            {
                onAlert = GetOrAdd<OnActorActionTransition>(move.transform, "On Alert Action [Transition]");
                Set(onAlert, "_actionName", "Alert");
            }

            // The patrol stops on the same frame the alert comes: the alert must be checked before "move finished".
            // (A prefab variant inherits the order from its base, and may not reorder inherited objects itself.)
            if (onAlert.transform.parent != move.transform)
                onAlert.transform.SetParent(move.transform, false);
            if (onAlert.transform.GetSiblingIndex() != 0)
                onAlert.transform.SetSiblingIndex(0);
        }

        foreach (var transition in body.GetComponentsInChildren<OnActorActionTransition>(true))
            if (IsAlertTransition(transition) && new SerializedObject(transition).FindProperty("_nextState").objectReferenceValue == null)
                Set(transition, "_nextState", alertState);
    }

    private static bool IsAlertTransition(OnActorActionTransition transition) =>
        new SerializedObject(transition).FindProperty("_actionName").stringValue == "Alert";

    private static void SetupEntrancePrefab(GameObject root, Entrance entrance)
    {
        FixMovingAlert(root.transform.Find("State Mashine"));

        var states = root.transform.Find("AI").GetComponentsInChildren<MainState>(true);
        Transform stand = states.First(s => s.name == "Stand").transform;
        Transform alert = states.First(s => s.name == "Alert").transform;

        var activity = GetOrAdd<IdleActivitySubstate>(stand, "Idle Activity [Substate]");
        var so = new SerializedObject(activity);
        so.FindProperty("_locomotionState").stringValue = entrance.Locomotion;
        var list = so.FindProperty("_activities");
        list.arraySize = entrance.Activities.Length;
        for (int i = 0; i < entrance.Activities.Length; i++)
        {
            var element = list.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("State").stringValue = entrance.Activities[i].State ?? "";
            element.FindPropertyRelative("Weight").floatValue = entrance.Activities[i].Weight;
            element.FindPropertyRelative("NeedsCompany").boolValue = entrance.Activities[i].Company;
            element.FindPropertyRelative("AlertState").stringValue = entrance.Activities[i].Alert ?? "";
        }
        so.ApplyModifiedPropertiesWithoutUndo();

        var reaction = GetOrAdd<AlertReactionSubstate>(alert, "Alert Reaction [Substate]");
        so = new SerializedObject(reaction);
        so.FindProperty("_activity").objectReferenceValue = activity;
        so.FindProperty("_signalState").stringValue = entrance.Signals ? "Reaction Signal" : "";
        so.FindProperty("_signalRadius").floatValue = 30f;
        list = so.FindProperty("_variants");
        list.arraySize = entrance.Reactions.Length;
        for (int i = 0; i < entrance.Reactions.Length; i++)
        {
            var element = list.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("State").stringValue = entrance.Reactions[i].State ?? "";
            element.FindPropertyRelative("Weight").floatValue = entrance.Reactions[i].Weight;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
