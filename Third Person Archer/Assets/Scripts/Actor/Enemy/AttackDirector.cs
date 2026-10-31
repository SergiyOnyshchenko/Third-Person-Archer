using System.Collections.Generic;
using System.Text.RegularExpressions;
using Actor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Hands out the right to attack, so enemies take turns instead of firing all at once:
/// at most N attacks run at the same time and two attacks never start closer than a gap (both per zone).
/// Enemies get their turn in the order they started waiting.
/// </summary>
public static class AttackDirector
{
    private class Slot
    {
        public ActorController Actor;
        public float GrantedAt;
        public bool Started;
    }

    private static readonly List<Slot> Holders = new List<Slot>();
    private static readonly List<ActorController> Queue = new List<ActorController>();
    private static float _lastGrant = float.NegativeInfinity;
    private static AttackDirectorSettings _settings;
    private static AttackDirectorSettings.Zone? _zone;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        _settings = null;
        Clear();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single)
            Clear();
    }

    private static void Clear()
    {
        Holders.Clear();
        Queue.Clear();
        _lastGrant = float.NegativeInfinity;
        _zone = null;
    }

    private static AttackDirectorSettings Settings
    {
        get
        {
            if (_settings == null)
            {
                _settings = Resources.Load<AttackDirectorSettings>(AttackDirectorSettings.ResourceName);
                if (_settings == null)
                    _settings = ScriptableObject.CreateInstance<AttackDirectorSettings>();
            }
            return _settings;
        }
    }

    private static AttackDirectorSettings.Zone Zone => _zone ??= ZoneFor(SceneManager.GetActiveScene().name);

    private static AttackDirectorSettings.Zone ZoneFor(string sceneName)
    {
        // "Level2_4 (Night)", "Level 1-Boss"
        Match match = Regex.Match(sceneName, @"Level\s?(\d)");
        int zone = match.Success ? int.Parse(match.Groups[1].Value) - 1 : 0;
        var zones = Settings.Zones;
        return zones[Mathf.Clamp(zone, 0, zones.Length - 1)];
    }

    /// <summary>Call every frame while waiting; true once the actor may attack.</summary>
    public static bool TryAcquire(ActorController actor)
    {
        Prune();

        if (Find(actor) != null)
            return true;

        if (!Queue.Contains(actor))
            Queue.Add(actor);

        if (Queue[0] != actor || Holders.Count >= Zone.MaxAttackers || Time.time - _lastGrant < Zone.MinGap)
            return false;

        Queue.RemoveAt(0);
        Holders.Add(new Slot { Actor = actor, GrantedAt = Time.time });
        _lastGrant = Time.time;
        return true;
    }

    /// <summary>The actor stopped waiting without attacking.</summary>
    public static void Cancel(ActorController actor)
    {
        Queue.Remove(actor);
    }

    /// <summary>The attack animation has begun: the slot is held until <see cref="Release"/>.</summary>
    public static void NotifyStarted(ActorController actor)
    {
        Slot slot = Find(actor);
        if (slot != null)
            slot.Started = true;
    }

    public static bool HasStarted(ActorController actor)
    {
        Slot slot = Find(actor);
        return slot != null && slot.Started;
    }

    public static void Release(ActorController actor)
    {
        Holders.RemoveAll(s => s.Actor == actor);
    }

    private static Slot Find(ActorController actor)
    {
        foreach (var slot in Holders)
            if (slot.Actor == actor)
                return slot;
        return null;
    }

    private static void Prune()
    {
        float now = Time.time;
        var settings = Settings;

        Holders.RemoveAll(s => !IsAlive(s.Actor)
            || (!s.Started && now - s.GrantedAt > settings.StartTimeout)
            || now - s.GrantedAt > settings.MaxHold);

        Queue.RemoveAll(a => !IsAlive(a));
    }

    private static bool IsAlive(ActorController actor)
    {
        return actor != null && actor.isActiveAndEnabled && !actor.IsDead;
    }
}
