using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    [SerializeField] private List<GameData> _dataAssets;
    private Dictionary<Type, GameData> _dataMap;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _dataMap = new Dictionary<Type, GameData>();

        foreach (var data in _dataAssets)
        {
            if (data == null) continue;

            var type = data.GetType();
            if (!_dataMap.ContainsKey(type))
            {
                _dataMap.Add(type, data);
                data.Initialize();
            }
            else
            {
                Debug.LogWarning($"Duplicate GameData type in DataManager: {type}");
            }
        }
    }

    public bool TryGetData<T>(out T data) where T : GameData
    {
        if (_dataMap.TryGetValue(typeof(T), out var obj))
        {
            data = obj as T;
            return data != null;
        }

        data = null;
        return false;
    }
}