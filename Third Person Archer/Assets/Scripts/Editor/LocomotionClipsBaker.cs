using System.Collections.Generic;
using System.Linq;
using Actor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Adds start / stop states (CLazyRunner in-place clips) to the Player controller and bakes their
/// root-motion speed curves, turn yaw and hand-over phase into the player's LocomotionClipsDriver.
/// Also re-measures the run blend clip speeds for MoveAnimationSubstate.
/// </summary>
public static class LocomotionClipsBaker
{
    private const string ControllerPath = "Assets/Animations/!Animator Controllers/Player.controller";
    private const string PlayerPrefab = "Assets/Prefabs/Actors/Player Actor.prefab";
    private const string ClipsRoot = "Assets/Animations/CLazyRunnerActionAnimPack/Animations/P1_CLazyMovement";
    private const string RunState = "Movement";

    private static readonly (string field, string state, string clip)[] Clips =
    {
        ("_startForward", "Start Fwd", "Mvm_Jog_Start_Fwd"),
        ("_startLeft90", "Start L90", "Mvm_Jog_Start_L90"),
        ("_startRight90", "Start R90", "Mvm_Jog_Start_R90"),
        ("_startLeft180", "Start L180", "Mvm_Jog_Start_L180"),
        ("_startRight180", "Start R180", "Mvm_Jog_Start_R180"),
        ("_stopLeft", "Stop L", "Mvm_DashEnd_L"),
        ("_stopRight", "Stop R", "Mvm_DashEnd_R"),
    };

    // Speed thresholds of the Movement blend tree: Idle, Walk, Jog, Dash, Boost.
    private static readonly string[] RunClips = { "Mvm_Walk", "Mvm_Jog", "Mvm_Dash", "Mvm_Boost" };

    [MenuItem("Tools/Animation/Bake Locomotion Clips")]
    public static void Bake()
    {
        var clips = LoadClips();
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        var machine = controller.layers[0].stateMachine;
        var run = machine.states.First(s => s.state.name == RunState).state;

        // CameraIndependentScale runs in edit mode and would bake a camera-dependent scale into the prefab.
        var keptScales = new Dictionary<string, Vector3>();
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefab);
        foreach (var c in asset.GetComponentsInChildren<MonoBehaviour>(true))
            if (c != null && c.GetType().Name == "CameraIndependentScale")
                keptScales[AnimationUtility.CalculateTransformPath(c.transform, asset.transform)] = c.transform.localScale;

        var prefabRoot = PrefabUtility.LoadPrefabContents(PlayerPrefab);
        try
        {
            var unityAnimator = prefabRoot.GetComponentsInChildren<UnityEngine.Animator>(true)[0];
            float humanScale = unityAnimator.humanScale;

            var sampler = Object.Instantiate(unityAnimator.gameObject);
            sampler.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                var pose = new PoseSampler(sampler);
                float forwardEndYaw = pose.HipsYaw(clips["Mvm_Jog_Start_Fwd"], clips["Mvm_Jog_Start_Fwd"].length);
                var jog = clips["Mvm_Jog"];

                var driver = prefabRoot.GetComponentInChildren<LocomotionClipsDriver>(true);
                if (driver == null)
                {
                    var mover = prefabRoot.GetComponentInChildren<NavmeshMover>(true);
                    driver = mover.gameObject.AddComponent<LocomotionClipsDriver>();
                }

                var so = new SerializedObject(driver);
                so.FindProperty("_runState").stringValue = RunState;

                for (int i = 0; i < Clips.Length; i++)
                {
                    var (field, stateName, clipName) = Clips[i];
                    var clip = clips[clipName];
                    var rootClip = clips[clipName + "_Root"];
                    bool turn = stateName.Contains("90") || stateName.Contains("180");

                    var state = EnsureState(machine, run, stateName, clip, i);
                    CopyTransitions(run, state, clip.length, turn);

                    var p = so.FindProperty(field);
                    p.FindPropertyRelative("State").stringValue = stateName;
                    p.FindPropertyRelative("Duration").floatValue = clip.length;
                    p.FindPropertyRelative("Speed").animationCurveValue = SpeedCurve(rootClip, humanScale);
                    p.FindPropertyRelative("TurnYaw").floatValue = turn
                        ? Mathf.Round(Mathf.DeltaAngle(forwardEndYaw, pose.HipsYaw(clip, clip.length)) / 90f) * 90f
                        : 0f;
                    p.FindPropertyRelative("ExitPhase").floatValue = pose.BestPhase(clip, jog);
                }

                // Left foot leads around the phase where it is furthest ahead of the right one.
                float leftLead = pose.LeftLeadPhase(jog);
                so.FindProperty("_leftLeadPhase").vector2Value =
                    new Vector2(Mathf.Repeat(leftLead - 0.25f, 1f), Mathf.Repeat(leftLead + 0.25f, 1f));
                so.ApplyModifiedPropertiesWithoutUndo();

                // Real ground speed of the run blend clips (root motion), so the feet match the velocity.
                var speeds = new List<float> { 0f };
                speeds.AddRange(RunClips.Select(n => AverageSpeed(clips[n + "_Root"], humanScale)));
                foreach (var substate in prefabRoot.GetComponentsInChildren<MoveAnimationSubstate>(true))
                {
                    var sso = new SerializedObject(substate);
                    var arr = sso.FindProperty("_clipSpeeds");
                    arr.arraySize = speeds.Count;
                    for (int i = 0; i < speeds.Count; i++)
                        arr.GetArrayElementAtIndex(i).floatValue = (float)System.Math.Round(speeds[i], 2);
                    sso.ApplyModifiedPropertiesWithoutUndo();
                }

                Debug.Log($"LocomotionClipsBaker: baked {Clips.Length} clips, run speeds {string.Join(", ", speeds.Select(s => s.ToString("0.00")))}");
            }
            finally
            {
                Object.DestroyImmediate(sampler);
            }

            foreach (var kept in keptScales)
            {
                var t = string.IsNullOrEmpty(kept.Key) ? prefabRoot.transform : prefabRoot.transform.Find(kept.Key);
                if (t != null)
                    t.localScale = kept.Value;
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PlayerPrefab);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
    }

    private static Dictionary<string, AnimationClip> LoadClips()
    {
        var result = new Dictionary<string, AnimationClip>();
        foreach (var guid in AssetDatabase.FindAssets("t:AnimationClip", new[] { ClipsRoot }))
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(guid)))
                if (asset is AnimationClip clip && !clip.name.StartsWith("__preview"))
                    result[clip.name] = clip;
        return result;
    }

    private static AnimatorState EnsureState(AnimatorStateMachine machine, AnimatorState run, string name, AnimationClip clip, int index)
    {
        var state = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == name);
        if (state == null)
        {
            var runPosition = machine.states.First(s => s.state == run).position;
            state = machine.AddState(name, runPosition + new Vector3(250f + 220f * (index / 4), -160f + 70f * (index % 4), 0f));
        }

        state.motion = clip;
        state.speed = 1f;
        state.writeDefaultValues = run.writeDefaultValues;
        return state;
    }

    /// <summary>Same gameplay exits as the run state (aim, jump, roll…) plus a fallback back into it.</summary>
    private static void CopyTransitions(AnimatorState run, AnimatorState state, float length, bool turn)
    {
        foreach (var t in state.transitions.ToArray())
            state.RemoveTransition(t);

        foreach (var source in run.transitions)
        {
            var t = source.isExit ? state.AddExitTransition()
                : source.destinationStateMachine != null ? state.AddTransition(source.destinationStateMachine)
                : state.AddTransition(source.destinationState);
            t.hasExitTime = source.hasExitTime;
            t.exitTime = source.exitTime;
            t.hasFixedDuration = source.hasFixedDuration;
            t.duration = source.duration;
            t.offset = source.offset;
            t.interruptionSource = source.interruptionSource;
            t.canTransitionToSelf = source.canTransitionToSelf;
            foreach (var c in source.conditions)
                t.AddCondition(c.mode, c.threshold, c.parameter);
        }

        // Fallback if the driver doesn't hand over (e.g. interrupted run). Turn starts would pop anyway.
        const float blend = 0.15f;
        var back = state.AddTransition(run);
        back.hasExitTime = true;
        back.hasFixedDuration = true;
        back.duration = turn ? 0f : blend;
        back.exitTime = turn ? 1f : Mathf.Max(0f, 1f - blend / length);
    }

    private static AnimationCurve SpeedCurve(AnimationClip rootClip, float humanScale)
    {
        GetRootCurves(rootClip, out var x, out var z);
        const float dt = 1f / 30f;
        var raw = new List<Vector2>();
        for (float t = 0f; t < rootClip.length; t += dt)
        {
            float t1 = Mathf.Min(rootClip.length, t + dt);
            float dx = x.Evaluate(t1) - x.Evaluate(t), dz = z.Evaluate(t1) - z.Evaluate(t);
            raw.Add(new Vector2(t + (t1 - t) * 0.5f, Mathf.Sqrt(dx * dx + dz * dz) / (t1 - t) * humanScale));
        }

        // Light smoothing: hips bob per step makes the raw speed jitter.
        var curve = new AnimationCurve();
        for (int i = 0; i < raw.Count; i++)
        {
            float v = (raw[Mathf.Max(0, i - 1)].y + raw[i].y * 2f + raw[Mathf.Min(raw.Count - 1, i + 1)].y) / 4f;
            curve.AddKey(new Keyframe(raw[i].x, v));
        }
        for (int i = 0; i < curve.length; i++)
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
        for (int i = 0; i < curve.length; i++)
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
        return curve;
    }

    private static float AverageSpeed(AnimationClip rootClip, float humanScale)
    {
        GetRootCurves(rootClip, out var x, out var z);
        float distance = 0f, px = x.Evaluate(0f), pz = z.Evaluate(0f);
        const int steps = 40;
        for (int i = 1; i <= steps; i++)
        {
            float t = rootClip.length * i / steps;
            float cx = x.Evaluate(t), cz = z.Evaluate(t);
            distance += Mathf.Sqrt((cx - px) * (cx - px) + (cz - pz) * (cz - pz));
            px = cx;
            pz = cz;
        }
        return distance / rootClip.length * humanScale;
    }

    private static void GetRootCurves(AnimationClip clip, out AnimationCurve x, out AnimationCurve z)
    {
        x = z = null;
        foreach (var binding in AnimationUtility.GetCurveBindings(clip))
        {
            if (binding.propertyName == "RootT.x") x = AnimationUtility.GetEditorCurve(clip, binding);
            if (binding.propertyName == "RootT.z") z = AnimationUtility.GetEditorCurve(clip, binding);
        }
        if (x == null || z == null)
            throw new System.InvalidOperationException($"{clip.name}: no RootT curves");
    }

    /// <summary>Samples clips on a copy of the player model (hips-yaw frame foot positions).</summary>
    private class PoseSampler
    {
        private readonly GameObject _model;
        private readonly Transform _hips, _left, _right;

        public PoseSampler(GameObject model)
        {
            _model = model;
            _model.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            var animator = model.GetComponent<UnityEngine.Animator>();
            _hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            _left = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            _right = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }

        public float HipsYaw(AnimationClip clip, float time)
        {
            clip.SampleAnimation(_model, time);
            Vector3 f = _hips.forward;
            return Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg;
        }

        private Vector3[] Feet(AnimationClip clip, float time)
        {
            clip.SampleAnimation(_model, time);
            Vector3 f = _hips.forward;
            var yaw = Quaternion.Inverse(Quaternion.Euler(0f, Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg, 0f));
            return new[] { yaw * (_left.position - _hips.position), yaw * (_right.position - _hips.position) };
        }

        /// <summary>Run-loop phase whose feet best match the clip's last frame.</summary>
        public float BestPhase(AnimationClip clip, AnimationClip loop)
        {
            var target = Feet(clip, clip.length);
            float best = 0f, bestError = float.MaxValue;
            const int steps = 64;
            for (int i = 0; i < steps; i++)
            {
                float phase = i / (float)steps;
                var feet = Feet(loop, loop.length * phase);
                float error = (feet[0] - target[0]).sqrMagnitude + (feet[1] - target[1]).sqrMagnitude;
                if (error < bestError)
                {
                    bestError = error;
                    best = phase;
                }
            }
            return best;
        }

        public float LeftLeadPhase(AnimationClip loop)
        {
            float best = 0f, bestLead = float.MinValue;
            const int steps = 64;
            for (int i = 0; i < steps; i++)
            {
                float phase = i / (float)steps;
                var feet = Feet(loop, loop.length * phase);
                float lead = feet[0].z - feet[1].z;
                if (lead > bestLead)
                {
                    bestLead = lead;
                    best = phase;
                }
            }
            return best;
        }
    }
}
