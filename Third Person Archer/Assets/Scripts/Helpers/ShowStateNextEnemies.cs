using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ShowStateNextEnemies : MonoBehaviour
{
    [SerializeField] private ActorController[] _enemies;
    [SerializeField] private MainState _state;

    public void Init(ActorController[] enemies, MainState state, bool isShow)
    {
        _enemies = enemies;
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
        {
            enemy.gameObject.SetActive(value);
        }
    }
}
