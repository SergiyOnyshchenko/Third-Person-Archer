using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ShowStateNextEnemies : MonoBehaviour
{
    [SerializeField] private ActorController[] _enemies;
    [SerializeField] private ActorController[] _hostages;
    [SerializeField] private GameObject[] _additional;
    [SerializeField] private MainState _state;

    public void Init(ActorController[] enemies, ActorController[] hostages, GameObject[] additional, MainState state, bool isShow)
    {
        _enemies = enemies;
        _hostages = hostages;
        _additional = additional;

        _state = state;

        if(isShow)
            ShowEnemies(false);

        if (isShow)
            _state.EnteredState.AddListener(ShowEnemies);
        else
            _state.OutOfState.AddListener(HideEnemies);
    }

    private void ShowEnemies()
    {
        _state.EnteredState.RemoveListener(ShowEnemies);
        ShowEnemies(true);
    }

    private void HideEnemies()
    {
        _state.OutOfState.RemoveListener(HideEnemies);
        ShowEnemies(false);
    }

    private void ShowEnemies(bool value)
    {
        foreach (var enemy in _enemies)
            enemy.gameObject.SetActive(value);

        foreach (var hostage in _hostages)
            hostage.gameObject.SetActive(value);

        foreach (var additional in _additional)
            additional.SetActive(value);
    }
}
