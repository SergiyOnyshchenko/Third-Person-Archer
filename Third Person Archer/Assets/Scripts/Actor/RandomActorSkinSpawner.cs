using System;
using System.Linq;
using UnityEngine;
using Actor;

[DefaultExecutionOrder(-5000)]
public class RandomActorSkinSpawner : MonoBehaviour, IActorAwaker
{
    [Serializable]
    public class SkinPrefabEntry
    {
        [Tooltip("Prefab reference for this skin.")]
        public GameObject Prefab;

        [Tooltip("Relative weight for random selection. Higher = more likely.")]
        public float RandomWeight = 1f;

    }

    [Header("Skins")]
    [SerializeField] private SkinPrefabEntry[] _skins;
    [Header("References")]
    [SerializeField] private Actor.Animator _animator;
    private GameObject _currentSkin;

    public void AwakeActor(ActorController actor)
    {
        if (_skins == null || _skins.Length == 0)
        {
            Debug.LogWarning($"{nameof(RandomActorSkinSpawner)}: No skins assigned!");
            return;
        }

        SkinPrefabEntry selected = GetRandomSkin();
        if (selected == null || selected.Prefab == null)
        {
            Debug.LogWarning($"{nameof(RandomActorSkinSpawner)}: Invalid prefab selected.");
            return;
        }

        _currentSkin = Instantiate(selected.Prefab, transform);
        Init(_currentSkin);
    }

    private SkinPrefabEntry GetRandomSkin()
    {
        float totalWeight = _skins.Sum(s => Mathf.Max(0, s.RandomWeight));
        if (totalWeight <= 0f) return null;

        float randomValue = UnityEngine.Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var entry in _skins)
        {
            cumulative += Mathf.Max(0, entry.RandomWeight);
            if (randomValue <= cumulative)
                return entry;
        }

        return _skins.Last();
    }

    private void Init(GameObject skin)
    {
        UnityEngine.Animator animator = skin.GetComponentInChildren<UnityEngine.Animator>();
        _animator.SetAnimator(animator);

        AnimationEventReciever eventReciever = skin.GetComponentInChildren<AnimationEventReciever>();
        _animator.SetEventReciever(eventReciever);
    }
}